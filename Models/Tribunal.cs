using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SUAP_PortalOficios.Models;

[Table("tribunal")]
public partial class Tribunal
{
    [Key]
    [Column("idTribunal")]
    public int IdTribunal { get; set; }

    [Column("nombre")]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    [Column("idTipoTribunal")]
    public int? IdTipoTribunal { get; set; }

    [Column("legfox")]
    public int? Legfox { get; set; }

    [Column("estadoTribu")]
    public int? EstadoTribu { get; set; }

    [Column("esNacional")]
    public bool? EsNacional { get; set; }
}
