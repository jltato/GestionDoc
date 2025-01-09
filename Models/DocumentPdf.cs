using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SUAP_PortalOficios.Models
{
    public class DocumentPdf
    {
        [Key]
        public int DocId {  get; set; }

        [AllowNull]
        public string src { get; set; }= string.Empty;
        public string? FileName { get; set; } 
        public string? FileSize { get; set; }
        public DateTime fechaCarga { get; set; }= DateTime.Now;

        public bool EliminadoLogico { get; set; } = false;

        [Required]
        public int OficioId { get; set; }

        [ForeignKey("OficioId")]
        public Oficios? Oficio { get; set; }
        
    }
}