using System.ComponentModel.DataAnnotations;

namespace SUAP_PortalOficios.Models.DTOs
{
    public class FormCargaDTO
    {
        [Required]
        public Oficios Oficios {  get; set; }
        public List<RegistroDTO> RegistroList { get; set; } = new List<RegistroDTO>();
    }
}
