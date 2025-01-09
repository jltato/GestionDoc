using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SUAP_PortalOficios.Models
{
    public class Interno_x_Oficio
    {

        [ForeignKey("OficiosId")]
        [Key, Column(Order =1)]
        public int OficiosId { get; set; }

        [Key, Column(Order =2)]
        public int Legajo { get; set; }

        
    }
}