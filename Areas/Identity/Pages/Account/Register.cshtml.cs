// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Blazorise;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Packaging.Signing;
using SUAP_PortalOficios.Data;
using SUAP_PortalOficios.Extensions;
using System.ComponentModel.DataAnnotations;


namespace PruebaIdentity.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Administrador, Coordinador, Asesor")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<MyUser> _signInManager;
        private readonly UserManager<MyUser> _userManager;
        private readonly IUserStore<MyUser> _userStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly MyDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public RegisterModel(
            UserManager<MyUser> userManager,
            IUserStore<MyUser> userStore,
            SignInManager<MyUser> signInManager,
            MyDbContext context,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> role)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _roleManager = role;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        public string ExistingUserId { get; set; } // Propiedad para pasar el userId al frontend

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>      

       
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
            [Display(Name = "Nombre")]
            public string UserName { get; set; }


            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [StringLength(100, ErrorMessage = "La {0} debe tener como minimo {2} y maximo {1} caracteres de largo.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage ="El nombre y apellido es obligatorio")]
            public string Nombre { get; set; }

        }
        [BindProperty]
        public List<UserList> Usuarios { get; set; }
       
        public async Task OnGetAsync(string returnUrl = null)
        {
            var thisUser = await _userManager.GetUserAsync(User);

            var roles = await _userManager.GetRolesAsync(thisUser);

            if (!roles.Contains("Administrador") && !roles.Contains("coordinador"))
            {
                RedirectToPage("/");
            }

            bool TotalScopes = await _context.UserPermissions
                                   .AnyAsync(up => up.UserId == thisUser.Id && up.ScopeId == 1);
            if (TotalScopes)
            {
                Usuarios = _userManager.Users
                   .Where(user => user.Id != thisUser.Id)
                   .Select(user => new UserList
                   {
                       UserId = user.Id,
                       UserName = user.NormalizedUserName,
                       Nombre = user.Nombre,
                       IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now // Indica si está bloqueado
                   })
                   .ToList();
            }
            else
            {

                var sql = @"                   
                    SELECT DISTINCT U.Id, U.NormalizedUserName, U.Nombre, U.LockoutEnd
                    FROM AspNetUsers U
                    INNER JOIN UserPermissions UP ON UP.UserId = U.Id
                    WHERE U.Id != @UserId
                    AND NOT EXISTS (
                        SELECT 1
                        FROM UserPermissions subUP
                        WHERE subUP.UserId = U.Id
                        AND subUP.ScopeId NOT IN (
                            SELECT ScopeId 
                            FROM UserPermissions 
                            WHERE UserId = @UserId
                        )
                    )
                    AND EXISTS (
                        SELECT 1
                        FROM UserPermissions subUP
                        WHERE subUP.UserId = U.Id
                        AND subUP.ScopeId IN (
                            SELECT ScopeId 
                            FROM UserPermissions 
                            WHERE UserId = @UserId
                        )
                    )";
                var parametro = new SqlParameter("@UserId", thisUser.Id);

                //var users = await _context.Users.FromSqlRaw(sql, parametro).ToListAsync();
                Usuarios = await _context.Users
                            .FromSqlRaw(sql, parametro)
                            .Select(user => new UserList
                            {
                                UserId = user.Id,
                                UserName = user.NormalizedUserName,
                                Nombre = user.Nombre,
                                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now
                            })
                            .ToListAsync();
            }

            ReturnUrl = returnUrl;  

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Nombre = Input.Nombre.ToUpper();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    

                    var userId = await _userManager.GetUserIdAsync(user);

                    var nombreRol = await _roleManager.FindByNameAsync("Usuario");

                    var res = await _userManager.AddToRoleAsync(user, nombreRol.Name);

                    if (res.Succeeded)
                    {
                        var thisUser = await _userManager.GetUserAsync(User);
                        var scope = await _context.UserPermissions
                                    .Where(up =>up.UserId == thisUser.Id)
                                    .Select(a => a.ScopeId)
                                    .FirstOrDefaultAsync();


                        var userPermission = new UserPermissions
                        {
                            UserId = user.Id,
                            SectionId = 1,
                            ScopeId = scope
                        };
                        _context.UserPermissions.Add(userPermission);
                        var result2 = await _context.SaveChangesAsync();
                        if (result2>0)
                        {
                            _logger.Modificacion(this.User.Identity?.Name, $"Ha creado al usuario {user.Nombre} exitosamente");
                            return RedirectToPage("/Account/Details", new { id = user.Id });
                        }
                        else
                        {
                            _logger.LogError("El susuario {0} no se pudo guardar los userpermissions.", user.NormalizedUserName);
                            return RedirectToPage("/Account/Details", new { id = user.Id });
                        }                       
                    }
                    else
                    {
                        _logger.LogError("El susuario {0} no se pudo guardar el Rol.", user.NormalizedUserName);
                        return RedirectToPage("/Account/Details", new { id = userId });
                    }
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        // Buscar el usuario existente
                        var existingUser = await _userManager.FindByNameAsync(Input.UserName);
                        if (existingUser != null)
                        {
                            if(existingUser.LockoutEnd != null && existingUser.LockoutEnd.Value > DateTime.Now )
                            {
                                ExistingUserId = existingUser.Id; // Almacenar el ID del usuario existente
                                ModelState.AddModelError("Input.UserName", "El nombre de usuario ya está en uso y se encuentra bloqueado.");
                            }
                            else
                            {
                                ModelState.AddModelError("Input.UserName", "El nombre de usuario ya está en uso y activo.");
                            }                                                       
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
           
            return Page();
        }

        private MyUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<MyUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(MyUser)}'. " +
                    $"Ensure that '{nameof(MyUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        public class UserList()
        {
            public string UserId { get; set; }
            public string UserName { get; set; } = string.Empty;     
            public string Nombre { get; set; } = string.Empty;
            public bool IsLockedOut { get; set; }
        }
    }
}
