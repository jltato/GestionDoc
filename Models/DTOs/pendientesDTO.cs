using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class pendientesDTO
    {
        [Key]
        public int IdOficio { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public int? Legajo { get; set; }
        public string? Apellido { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
        public string? Plazo { get; set; } = string.Empty;
        public string TipoOficioNombre { get; set; } = string.Empty;
        public string? Tribunal {  get; set; } = string.Empty;
        public string? EstabACargo { get; set; } = string.Empty;
    }
}
