using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUAP_PortalOficios.Data
{
    public class Scopes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScopeId { get; set; }
        [Required]
        public string ScopeName { get; set; } = string.Empty;


    }
}