using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Table("interno")]
public partial class Interno
{
    [Key]
    [Column("legajo")]
    public int Legajo { get; set; }

    [Column("conducta")]
    [StringLength(50)]
    public string? Conducta { get; set; }

    [Column("fecIngSpc")]
    public DateOnly? FecIngSpc { get; set; }

    [Column("fecIngEp")]
    public DateOnly? FecIngEp { get; set; }

    [Column("fechaDetencion")]
    public DateOnly? FechaDetencion { get; set; }

    [Column("fecCamPab")]
    public DateOnly? FecCamPab { get; set; }

    [Column("idTipoDetencion")]
    public int? IdTipoDetencion { get; set; }

    [Column("art11")]
    [StringLength(2)]
    public string? Art11 { get; set; }

    public int? IdDomicilio { get; set; }

    public int? Idhuellas { get; set; }

    [Column("Fec_computo_provisorio")]
    public DateOnly? FecComputoProvisorio { get; set; }

    [Column("idReincidencia")]
    public int? IdReincidencia { get; set; }

    [Column("idTipoConAlojado")]
    public int? IdTipoConAlojado { get; set; }

    [Column("IDConcepto")]
    [StringLength(10)]
    public string? Idconcepto { get; set; }

    [Column("tipoViolencia")]
    public int? TipoViolencia { get; set; }

    [Column("idNombreAntFox")]
    public int? IdNombreAntFox { get; set; }

    [Column("nombre")]
    [StringLength(150)]
    public string? Nombre { get; set; }

    [Column("apellido")]
    [StringLength(150)]
    public string? Apellido { get; set; }

    [Column("nombre2")]
    [StringLength(150)]
    public string? Nombre2 { get; set; }

    [Column("apellido2")]
    [StringLength(150)]
    public string? Apellido2 { get; set; }

    [Column("nombre3")]
    [StringLength(150)]
    public string? Nombre3 { get; set; }

    [Column("apellido3")]
    [StringLength(150)]
    public string? Apellido3 { get; set; }

    [Column("nombre4")]
    [StringLength(150)]
    public string? Nombre4 { get; set; }

    [Column("apellido4")]
    [StringLength(150)]
    public string? Apellido4 { get; set; }

    [Column("alias")]
    [StringLength(150)]
    public string? Alias { get; set; }

    [Column("estadoBaja")]
    public bool? EstadoBaja { get; set; }

    [Column("esCargaRapida")]
    public bool? EsCargaRapida { get; set; }

    [Column("art140")]
    public bool? Art140 { get; set; }

    [Column("victima")]
    public int? Victima { get; set; }

    [Column("documento1")]
    public int? Documento1 { get; set; }

    [Column("documento2")]
    public int? Documento2 { get; set; }

    [Column("documento3")]
    public int? Documento3 { get; set; }

    [Column("embarazo")]
    public bool? Embarazo { get; set; }

    [Column("condenadoSinTTO")]
    public bool? CondenadoSinTto { get; set; }

    public bool? ProcSinAlojamiento { get; set; }

    [Column("fechaCalificacion")]
    public DateOnly? FechaCalificacion { get; set; }

    [Column("fechaControlLegajo")]
    public DateOnly? FechaControlLegajo { get; set; }

    [InverseProperty("IdLegajoNavigation")]
    public virtual ICollection<InternoXPabellonXEstablecimiento> InternoXPabellonXEstablecimientos { get; set; } = new List<InternoXPabellonXEstablecimiento>();
}
