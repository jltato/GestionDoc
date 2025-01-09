using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class UserPermissionsDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        public List<int> Sections { get; set; } = new List<int>();
        public List<int> Scopes { get; set; } = new List<int>();
        public List<string> Roles { get; set; } = new List<string>();
    }
}
