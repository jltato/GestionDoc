using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class OficiosPendientesDTO
    {
        [Key]
        public int IdOficio { get; set; }
        public DateTime FechaIng { get; set; } = DateTime.Now;
        public int? Legajo { get; set; }
        public string? Apellido { get; set; } = string.Empty;
        public string? Nombre  { get; set; } = string.Empty ;
        public string? Tribunal { get; set; } = string.Empty;
        public string? Estado { get; set; } = string.Empty;
        public int? ScopeId{ get; set; }
        public string? Plazo { get; set;} = string.Empty;
        public string? Tipo { get; set; } = string.Empty;
        public string? EstabACargo { get; set; } = string.Empty;
        public string? SAC {  get; set; } = string.Empty;

    }

}
