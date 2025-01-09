using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models
{
    public class MedioIng
    {
        [Key]
        public int IdMedio { get; set; }
        [Required]
        public string MedioName { get; set; } = string.Empty;    
    }
}
