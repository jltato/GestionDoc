using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models
{
    public class Plazo
    {
        [Key]
        public int IdPlazo { get; set; }
        [Required]
        public string PlazoName { get; set; } = string.Empty;
    }
}
