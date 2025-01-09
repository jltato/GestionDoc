using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using SUAP_PortalOficios.Data;
using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Data
{
    public class UserPermissions
    {
        // Foreign key to IdentityUser table
        [Required]
        [ForeignKey("UserId")]       
        public String UserId { get; set; }
        public MyUser User { get; set; }


        // Foreign key to Sections table
        [ForeignKey("SectionId")]
        public int SectionId { get; set; }
        public Sections? Section { get; set; }

        // Foreign key to Scopes table
        [ForeignKey("ScopeId")]
        public int ScopeId { get; set; }
        public Scopes? Scope { get; set; }

       
    }
}