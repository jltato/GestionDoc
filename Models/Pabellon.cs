using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Table("Pabellon")]
public partial class Pabellon
{
    [Key]
    [Column("idPabellon")]
    public int IdPabellon { get; set; }

    [Column("codigopabellon")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Codigopabellon { get; set; }

    [Column("nombre")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Nombre { get; set; }

    [Column("descripcion")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [Column("idCelda")]
    public int? IdCelda { get; set; }

    [Column("idEstablecimiento")]
    public int? IdEstablecimiento { get; set; }

    [Column("capacidadDeAlojadosDelPabellon")]
    public int? CapacidadDeAlojadosDelPabellon { get; set; }

    [Column("alojadosPorPabellon")]
    public int? AlojadosPorPabellon { get; set; }

    [Column("totalAlojados")]
    public int? TotalAlojados { get; set; }

    [Column("idNucleo")]
    [StringLength(20)]
    public string? IdNucleo { get; set; }

    [InverseProperty("IdPabellonNavigation")]
    public virtual ICollection<InternoXPabellonXEstablecimiento> InternoXPabellonXEstablecimientos { get; set; } = new List<InternoXPabellonXEstablecimiento>();
}
