using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Table("establecimiento")]
public partial class Establecimiento
{
    [Key]
    [Column("idEstablecimiento")]
    public int IdEstablecimiento { get; set; }

    [Column("nombre")]
    [Unicode(false)]
    public string? Nombre { get; set; }

    [Column("descripcion")]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [Column("estado")]
    public bool? Estado { get; set; }

    [Column("idcomplejo")]
    public int? Idcomplejo { get; set; }

    [Column("cantidadOcupacional")]
    public int? CantidadOcupacional { get; set; }

    [Column("cantidadNivelMaximo")]
    public double? CantidadNivelMaximo { get; set; }

    [Column("visible")]
    public bool? Visible { get; set; }

    [InverseProperty("IdEstablecimientoNavigation")]
    public virtual ICollection<InternoXPabellonXEstablecimiento> InternoXPabellonXEstablecimientos { get; set; } = new List<InternoXPabellonXEstablecimiento>();
}
