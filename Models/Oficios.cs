using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SUAP_PortalOficios.Data;

namespace SUAP_PortalOficios.Models
{
    public class Oficios  
    {
        [Key]
        public int IdOficio { get; set; }

        // Foráneas
        [ForeignKey("IdTipoOficio")]
        public int? IdTipoOficio { get; set; } = 1;
        public int? IdTribunal { get; set; }

        [ForeignKey("IdEstado")]
        public int IdEstado { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        [ForeignKey("IdPlazo")]
        public int? IdPlazo { get; set; }
        [ForeignKey("IdMedio")]
        public int? IdMedio { get; set; } = 1;


        // Propiedades adicionales
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }       
        public DateTime? Modificado { get; set; }
        public bool? EliminadoLogico { get; set; } = false;
        public int IdEstabACargo { get; set; }
        public string? SAC { get; set; }

        // Propiedades de Navegación       
        public TipoOficio? TipoOficio { get; set; }     
        public Estado? Estado { get; set; }        
        public MyUser? MyUser { get; set; }
        public Scopes? Scope { get; set; }
        public Plazo? Plazo { get; set; }
        public MedioIng? MedioIng { get; set; }
        [NotMapped]
        public Tribunal? Tribunal { get; set; }

        public ICollection<Observation>? Observations { get; set; }
        public ICollection<Oficios_x_Area>? oficios_X_Areas{ get; set; }
        public ICollection<DocumentPdf>? DocumentPdfs{ get; set; }
        public ICollection<Interno_x_Oficio>? interno_X_Oficios { get; set; }


    }
}
