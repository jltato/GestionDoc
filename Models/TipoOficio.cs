using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models
{
    public class TipoOficio
    {
        [Key]
        public int IdTipoOficio { get; set; }
        [Required]
        public string TipoOficioNombre { get; set; } = string.Empty;
    }
}