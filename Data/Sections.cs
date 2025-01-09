using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUAP_PortalOficios.Data
{
    public class Sections
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SectionId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty ;

    }
}