using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Keyless]
[Table("Interno_x_Tribunal")]
public partial class InternoXTribunal
{
    [Column("idMovimiento")]
    public int IdMovimiento { get; set; }

    [Column("legajo")]
    public int Legajo { get; set; }

    [Column("idTribunal")]
    public int IdTribunal { get; set; }

    [Column("fechaDesde")]
    public DateOnly FechaDesde { get; set; }

    [Column("fechaHasta")]
    public DateOnly? FechaHasta { get; set; }

    [Column("observaciones")]
    [StringLength(300)]
    [Unicode(false)]
    public string? Observaciones { get; set; }

    [Column("idTipoMovimiento")]
    public int? IdTipoMovimiento { get; set; }

    [Column("idUsuario")]
    public int? IdUsuario { get; set; }

    [Column("fechaSistemaMovimiento", TypeName = "datetime")]
    public DateTime? FechaSistemaMovimiento { get; set; }

    [Column("actuales")]
    public bool? Actuales { get; set; }

    [Column("disposicionAnterior")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DisposicionAnterior { get; set; }

    [Column("eliminadoLogico")]
    public bool? EliminadoLogico { get; set; }

    [ForeignKey("IdTribunal")]
    public virtual Tribunal IdTribunalNavigation { get; set; } = null!;

    [ForeignKey("Legajo")]
    public virtual Interno LegajoNavigation { get; set; } = null!;
}
