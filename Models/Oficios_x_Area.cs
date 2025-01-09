using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SUAP_PortalOficios.Data;
using System.Text.Json.Serialization;

namespace SUAP_PortalOficios.Models
{
    public class Oficios_x_Area
    {
        [Key, Column(Order = 1)]
        public int OficiosId { get; set; }

        [Key, Column(Order = 2)]
        public int SectionId { get; set; }

        [Key, Column(Order = 3)]
        public int ScopeId { get; set; }

        [Required]
        public int EstadoId { get; set; } = 1;
        public DateTime FechaDerivado { get; set; } = DateTime.Now;
        public DateTime? FechaFin {  get; set; }
        public bool conocimiento { get; set; } = false;
        public string? UserId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public bool Visto { get; set; } = false;

        [ForeignKey("EstadoId")]
        public Estado Estado { get; set; }
        [ForeignKey("OficiosId")]
        [JsonIgnore] // Evita el ciclo
        public Oficios oficios { get; set; }

        [ForeignKey("SectionId")]
        public Sections Sections { get; set; }

        [ForeignKey("ScopeId")]
        public Scopes Scopes { get; set; }

        [ForeignKey("UserId")]
        public MyUser MyUser { get; set; }
    }
}