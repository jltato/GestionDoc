using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SUAP_PortalOficios.Controllers;
using SUAP_PortalOficios.Data.Repository.Interfaces;
using SUAP_PortalOficios.Models.DTOs;
using System.Security.Claims;

namespace SUAP_PortalOficios.Data.Repository.Implementations
{
    public class UserPermissionsService : IUserPermissionsService
    {
        private readonly MyDbContext _context;
        private readonly UserManager<MyUser> _UserManager;
        private readonly ILogger<UserPermissionsService> _logger;
        private readonly IMemoryCache _cache;

        public UserPermissionsService(IMemoryCache cache, MyDbContext context, UserManager<MyUser> userManager, ILogger<UserPermissionsService> logger)
        {
            _context = context;
            _UserManager = userManager;
            _logger = logger;
            _cache = cache;
        }
        async Task<UserPermissionsDTO> IUserPermissionsService.GetPermissionsAsync(ClaimsPrincipal user)
        {
            // Obtén el userId desde los claims
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                throw new InvalidOperationException("User ID not found in the claims.");
            }
            string cacheKey = $"userPermissions_{userId}";

            // Verifica si los permisos ya están en la caché
            if (!_cache.TryGetValue(cacheKey, out UserPermissionsDTO userPermissions))
            {
                // Si no están en la caché, los traigo de la base de datos
                
                var rolesList = user.FindAll(ClaimTypes.Role).Select(roleClaim => roleClaim.Value).ToList();

                var upList = await _context.UserPermissions
                                .Where(s => s.UserId == userId)
                                .Select(up => new { up.SectionId, up.ScopeId })
                                .ToListAsync();

                userPermissions = new UserPermissionsDTO
                {
                    UserId = userId,
                    Roles = rolesList,
                    Scopes = upList.Select(up => up.ScopeId).ToList(),
                    Sections = upList.Select(up => up.SectionId).ToList()
                };

                // Configura la caché con expiración absoluta de 30 minutos y expiración por inactividad
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));                    

                // Agregar a la caché
                _cache.Set(cacheKey, userPermissions, cacheEntryOptions);
            }
            else
            {
                // Si los permisos ya están en la caché, reinicia el contador de expiración
                _cache.Set(cacheKey, userPermissions, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))); // Resetea la expiración en cada acceso
                   
            }

            return userPermissions;
        }

        void IUserPermissionsService.RemovePermissions(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                throw new InvalidOperationException("User ID not found in the claims.");
            }
            string cacheKey = $"userPermissions_{userId}";
            _cache.Remove(cacheKey);
        }
    }
}
