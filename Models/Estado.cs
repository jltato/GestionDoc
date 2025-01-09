
using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models
{
    public class Estado
    {
        [Key]
        public int IdEstado { get; set; }
        [Required]
        public string EstadoNombre { get; set; } = string.Empty;

    }
}