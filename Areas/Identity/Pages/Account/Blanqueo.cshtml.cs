using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using SUAP_PortalOficios.Data;
using System.ComponentModel.DataAnnotations;


namespace SUAP_PortalOficios.Areas.Identity.Pages.Account
{
    public class BlanqueoModel : PageModel
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly MyDbContext _context;
        private readonly ILogger<DetailsModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;


        public BlanqueoModel(
             UserManager<MyUser> userManager
            , MyDbContext context
            , ILogger<DetailsModel> logger,
             RoleManager<IdentityRole> roleManager
             )
        {
            _userManager = userManager; 
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }
        [BindProperty]
        public PasswordModel Password { get; set; }

        public string StatusMessage { get; set; }
        public string ReturnUrl {  get; set; }



        public async Task<ActionResult> OnGetAsync(string id)
        {
            UserId = id;           

            return Page();
        }

        public async Task<IActionResult> OnPostForm2(string id)
        {
            ModelState.Clear();
            TryValidateModel(Password, nameof(Password));
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"No se puede cargar el usuario con el ID '{_userManager.GetUserId(User)}'.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var changePasswordResult = await _userManager.ResetPasswordAsync(user, token, Password.Password);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return RedirectToPage("/Account/Details", new { id = user.Id });
            }

            user.PaswordChange = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                StatusMessage = "La contraseña ha sido blanqueada correctamente. El Usuario debera cambiar la contraseña en el proximo inicio de sesión";
                
                return Page();
            }
            ModelState.AddModelError(string.Empty, "Este Usuario ya posee este alcance. Por favor elija otro");
            return Page();
        }

        public class PasswordModel
        {
            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [StringLength(100, ErrorMessage = "La {0} debe tener como minimo {2} y maximo {1} caracteres de largo.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }
        }
    }
}
