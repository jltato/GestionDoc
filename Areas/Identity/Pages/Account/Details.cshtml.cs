// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SUAP_PortalOficios.Data;
using SUAP_PortalOficios.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Areas.Identity.Pages.Account
{
    public class DetailsModel : PageModel
    {

        private readonly UserManager<MyUser> _userManager;
        private readonly MyDbContext _context;
        private readonly ILogger<DetailsModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;

        public DetailsModel (
             UserManager<MyUser> userManager
            , MyDbContext context
            , ILogger<DetailsModel> logger,
             RoleManager<IdentityRole> roleManager,
             IMemoryCache cache
             )
        {

            _userManager = userManager;
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _cache = cache;
        }

        [BindProperty]
        public InputModel Input { get; set; }
       
        [BindProperty]
        public RolModel Rol { get; set; }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        public string ReturnUrl { get; set; }
        [BindProperty(SupportsGet = true)]
        public List<Sections> Sections { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<Scopes> Scopes { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<IdentityRole> Roles { get; set; }

        [BindProperty (SupportsGet = true)]
        public string UserRole { get; set; }

        public string UserName { get; set; }

        public bool IsLocked { get; set; }

        public List<RoleList> Alcance { get; set; }
      
        public string StatusMessage { get; set; }

        public async Task<ActionResult> OnGetAsync(string id)
        {
            UserId = id;

            await inicializador(UserId);
         
            return Page();
        }

        public async Task<ActionResult> OnPostForm1(string id)
        {
            UserId = id;
            var user = await _userManager.FindByIdAsync(id);
                   
            if (user != null)
            { 
                ModelState.Clear();
                TryValidateModel(Input, nameof(Input));
                if (ModelState.IsValid)
                {
                    var userPermission = new UserPermissions
                    {
                        UserId = user.Id,
                        SectionId = Input.SelectedSection,
                        ScopeId = Input.SelectedScope
                    };
                    _context.UserPermissions.Add(userPermission);
               
                    try
                    {
                        var result = await _context.SaveChangesAsync();
                        if (result > 0)
                        {
                            _logger.LogInformation("User modify the {0} account with succsess.", user.NormalizedUserName);
                            _logger.Modificacion(this.User.Identity?.Name, $"Agregó el alcance: SectionId: {Input.SelectedSection}, ScopeId{Input.SelectedScope} a la cuenta de {user.NormalizedUserName}");
                            string cacheKey = $"userPermissions_{user.Id}";
                            _cache.Remove(cacheKey);
                            return RedirectToPage("/Account/Details", new { id = user.Id });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Ha ocurrido un error, intente nuevamente");
                            return Page();
                        }
                    }
                    catch (Exception ex) {
                        if (ex.InnerException.HResult == -2146232060)
                        {
                            ModelState.AddModelError(string.Empty, "Este Usuario ya posee este alcance. Por favor elija otro");
                            await inicializador(UserId);
                            return Page();
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.Message);
                            await inicializador(UserId);
                            return Page();
                        }                        
                    }
                }
            }
            await inicializador(UserId);
            return Page();
        }

        public async Task<ActionResult> OnPostForm3(string id)
            {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                var rolename = await _userManager.GetRolesAsync(user);

                var nombreRol = await _roleManager.FindByIdAsync(Rol.SelectedRolId);
                if (rolename.Count == 0)
                {
                    var res = await _userManager.AddToRoleAsync(user, nombreRol.Name);
                    if (res.Succeeded)
                    {
                        _logger.LogInformation("User modify the {0} account with succsess.", user.NormalizedUserName);
                        _logger.Modificacion(this.User.Identity?.Name, $"Agregó el rol{nombreRol.Name} al usuario {user.NormalizedUserName}");
                        string cacheKey = $"userPermissions_{user.Id}";
                        _cache.Remove(cacheKey);
                        return RedirectToPage("/Account/Details", new { id = user.Id });
                    }
                }
                else
                {
                    var rol = await _roleManager.FindByNameAsync(rolename.FirstOrDefault());
                    if (rol.Id != Rol.SelectedRolId)
                    {
                        var resultRemove = await _userManager.RemoveFromRolesAsync(user, rolename);
                        if (resultRemove.Succeeded)
                        {
                            var resultAdd = await _userManager.AddToRoleAsync(user, nombreRol.Name);

                            if (resultAdd.Succeeded)
                            {
                                string cacheKey = $"userPermissions_{user.Id}";
                                _cache.Remove(cacheKey);
                                _logger.LogInformation("User modify the {0} account with succsess.", user.NormalizedUserName);
                                return RedirectToPage("/Account/Details", new { id = user.Id });
                            }
                        }
                    }
                    else
                    {
                        string cacheKey = $"userPermissions_{user.Id}";
                        _cache.Remove(cacheKey);
                        _logger.LogInformation("User modify the {0} account with succsess.", user.NormalizedUserName);
                        return RedirectToPage("/Account/Details", new { id = user.Id });
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            var userRole = await _context.UserRoles
                               .Where(ur => ur.UserId == UserId)
                               .Select(ur => ur.RoleId)
                               .FirstOrDefaultAsync();
           if (userRole != null)
           {
                UserRole = userRole;
           }
           return Page();         
        }

        public async Task<ActionResult> OnPostDeleteRole(string userId, int scopeId, int sectionId)
        {
            try
            {
                var itemToDelete = await _context.UserPermissions.FirstOrDefaultAsync(u => u.UserId == userId && u.ScopeId == scopeId && u.SectionId == sectionId);
                var roles = await _context.UserPermissions.Where(a => a.UserId == userId).ToListAsync();
                if (roles.Count > 1)
                {

                    if (itemToDelete != null)
                    {
                        _context.UserPermissions.Remove(itemToDelete);
                        await _context.SaveChangesAsync();
                        string cacheKey = $"userPermissions_{userId}";
                        _cache.Remove(cacheKey);
                        _logger.Modificacion(this.User.Identity?.Name, $"Ha eliminado el alcance: SectionId{sectionId}, ScopeId{scopeId} del usuario {this.UserName}");
                        return RedirectToPage("/Account/Details", new { id = userId });
                    }

                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await inicializador(userId);
                return Page();
            }
            return RedirectToPage("/Account/Details", new { id = userId });
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required.");
            }
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (user.LockoutEnd > DateTime.Now)
                {
                    user.LockoutEnd = null;
                    _logger.Modificacion(this.User.Identity?.Name, $"Ha Desbloqueado al usuario {UserName}");
                }
                else
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue; // Bloquear indefinidamente (no genera problemas de eliminado en cascada)
                }
                var res = await _userManager.UpdateAsync(user);
                string cacheKey = $"userPermissions_{user.Id}";
                _cache.Remove(cacheKey);
                if (res.Succeeded)
                {
                    _logger.Modificacion(this.User.Identity?.Name, $"Ha bloqueado al usuario {UserName}");
                    return RedirectToPage("/Account/Register");

                }
                else
                {
                    _logger.LogError("No se ha podido bloquear al usuario " + UserName);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.LogError("No se ha podido bloquear al usuario " + UserName);
                await inicializador(userId);
                return Page();
            }
            await inicializador(userId);
            return Page();
        }

        public async Task inicializador(string id )
        {          
            var thisUser = await _userManager.GetUserAsync(User);
            var thisUserId = thisUser.Id;          

            UserId = id;

            Sections = await _context.Sections.ToListAsync();

            bool hasSpecificScope = await _context.UserPermissions
                                    .AnyAsync(up => up.UserId == thisUserId && up.ScopeId == 1);
            if (hasSpecificScope)
            {
                Scopes = await _context.Scopes.ToListAsync();
            }
            else
            {
                Scopes = await _context.UserPermissions
                            .Where(up => up.UserId == thisUserId)
                            .Join(_context.Scopes,
                                   up => up.ScopeId,
                                   sec => sec.ScopeId,
                                   (up, sec) => sec)
                            .Distinct() 
                            .ToListAsync();
            }

            if (User.IsInRole("Administrador"))
            {
                Roles = await _context.Roles.ToListAsync();
            }
            else if (User.IsInRole("Coordinador"))
            {
                Roles = await _context.Roles
                         .Where(r => r.Name != "Administrador")
                         .ToListAsync();
            }
            else if (User.IsInRole("Asesor"))
            {
                Roles = await _context.Roles
                        .Where(r => r.Name != "Administrador" && r.Name != "Coordinador")
                        .ToListAsync();
            }

            var userRole = await _context.UserRoles
                          .Where(ur => ur.UserId == UserId)
                          .Select(ur => ur.RoleId)
                          .FirstOrDefaultAsync();
            if (userRole != null)
            {
                UserRole = userRole;
            }
            Alcance = await _context.UserPermissions
                        .Where(up => up.UserId == UserId )
                        .Join(_context.Sections,
                              up => up.SectionId,
                              sec => sec.SectionId,
                              (up, sec) => new { up, sec })
                        .Join(_context.Scopes,
                              upSecJoint => upSecJoint.up.ScopeId,
                              sco => sco.ScopeId,
                              (upSecJoint, sco) => new RoleList
                              {
                                  ScopeName = sco.ScopeName,
                                  SectionName = upSecJoint.sec.Name,
                                  UserId = UserId,
                                  ScopeId = sco.ScopeId,
                                  SectionId = upSecJoint.sec.SectionId
                              })
                        .ToListAsync();

            var res = await _context.Users
                   .Where(us => UserId == null || us.Id == UserId)
                   .FirstOrDefaultAsync();
            UserName = res.Nombre;
            IsLocked = res.LockoutEnd > DateTime.Now;
        }

        public class InputModel
        {           
            [Required(ErrorMessage = "La sección es obligatoria.")]
            [Display(Name = "Sección")]
            public int SelectedSection { get; set; }

            [Required(ErrorMessage = "El Ámbito es obligatorio.")]
            [Display(Name = "Ámbito")]
            public int SelectedScope { get; set; }        
        }

        public class RolModel
        {
            [Required(ErrorMessage = "El Rol es obligatorio.")]
            [Display(Name = "Rol")]
            public string SelectedRolId { get; set; }
        }      
    }

    public class RoleList
    {
        public string UserId { get; set; }
        public int? SectionId { get; set; }
        public int? ScopeId { get; set; }
        public string ScopeName { get; set; }
        public string SectionName {  get; set; }  
    }
}
