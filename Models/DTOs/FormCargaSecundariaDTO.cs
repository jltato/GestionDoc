using SUAP_PortalOficios.Data;
using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class FormCargaSecundariaDTO
    {

        [Required]
        public Oficios Oficios { get; set; }
        public List<Sections> RegistroList { get; set; } = new List<Sections>();
    }
}
