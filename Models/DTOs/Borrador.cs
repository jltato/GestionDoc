using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class Borrador
    {
        [Key]
        public int IdOficio { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? FileName { get; set; } = string.Empty;
        public string? FileSize { get; set; }

    }
}
