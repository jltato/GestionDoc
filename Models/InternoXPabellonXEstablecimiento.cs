using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Table("interno_x_pabellon_x_establecimiento")]
public partial class InternoXPabellonXEstablecimiento
{
    [Key]
    [Column("idMovimiento")]
    public int IdMovimiento { get; set; }

    [Column("idLegajo")]
    public int IdLegajo { get; set; }

    [Column("idPabellon")]
    public int IdPabellon { get; set; }

    [Column("idEstablecimiento")]
    public int IdEstablecimiento { get; set; }

    [Column("fechaDesde", TypeName = "datetime")]
    public DateTime FechaDesde { get; set; }

    [Column("fechaHasta", TypeName = "datetime")]
    public DateTime? FechaHasta { get; set; }

    [Column("bitMIG")]
    public bool? BitMig { get; set; }

    [Column("idUsuario")]
    public int? IdUsuario { get; set; }

    [Column("idMotivoCambio")]
    public int? IdMotivoCambio { get; set; }

    [Column("observaciones")]
    [StringLength(250)]
    [Unicode(false)]
    public string? Observaciones { get; set; }

    [Column("idHospital")]
    public int? IdHospital { get; set; }

    [ForeignKey("IdEstablecimiento")]
    [InverseProperty("InternoXPabellonXEstablecimientos")]
    public virtual Establecimiento IdEstablecimientoNavigation { get; set; } = null!;

    [ForeignKey("IdLegajo")]
    [InverseProperty("InternoXPabellonXEstablecimientos")]
    public virtual Interno IdLegajoNavigation { get; set; } = null!;

    [ForeignKey("IdPabellon")]
    [InverseProperty("InternoXPabellonXEstablecimientos")]
    public virtual Pabellon IdPabellonNavigation { get; set; } = null!;
}
