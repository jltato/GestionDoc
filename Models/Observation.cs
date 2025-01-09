using SUAP_PortalOficios.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace SUAP_PortalOficios.Models
{
    public class Observation
    {
        [Key]
        public int IdObservacion { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        [Required]
        public int IdOficio { get; set; }

        [ForeignKey("UserId")]
        public MyUser user { get; set; }

        [ForeignKey("IdOficio")]
        public Oficios oficios { get; set; }
    }
}