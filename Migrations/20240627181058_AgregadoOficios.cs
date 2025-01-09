using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoOficios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estado",
                columns: table => new
                {
                    IdEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstadoNombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estado", x => x.IdEstado);
                });

            migrationBuilder.CreateTable(
                name: "TipoOficios",
                columns: table => new
                {
                    IdTipoOficio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoOficioNombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoOficios", x => x.IdTipoOficio);
                });

            migrationBuilder.CreateTable(
                name: "Oficios",
                columns: table => new
                {
                    IdOficio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTipoOficio = table.Column<int>(type: "int", nullable: false),
                    IdTribunal = table.Column<int>(type: "int", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Plazo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdEstado = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Modificado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficios", x => x.IdOficio);
                    table.ForeignKey(
                        name: "FK_Oficios_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Oficios_Estado_IdEstado",
                        column: x => x.IdEstado,
                        principalTable: "Estado",
                        principalColumn: "IdEstado");
                    table.ForeignKey(
                        name: "FK_Oficios_TipoOficios_IdTipoOficio",
                        column: x => x.IdTipoOficio,
                        principalTable: "TipoOficios",
                        principalColumn: "IdTipoOficio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPdf",
                columns: table => new
                {
                    DocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    src = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    OficioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPdf", x => x.DocId);
                    table.ForeignKey(
                        name: "FK_DocumentPdf_Oficios_OficioId",
                        column: x => x.OficioId,
                        principalTable: "Oficios",
                        principalColumn: "IdOficio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interno_x_Oficio",
                columns: table => new
                {
                    OficiosId = table.Column<int>(type: "int", nullable: false),
                    Legajo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interno_x_Oficio", x => new { x.OficiosId, x.Legajo });
                    table.ForeignKey(
                        name: "FK_Interno_x_Oficio_Oficios_OficiosId",
                        column: x => x.OficiosId,
                        principalTable: "Oficios",
                        principalColumn: "IdOficio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Observation",
                columns: table => new
                {
                    IdObservacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OficioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observation", x => x.IdObservacion);
                    table.ForeignKey(
                        name: "FK_Observation_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observation_Oficios_OficioId",
                        column: x => x.OficioId,
                        principalTable: "Oficios",
                        principalColumn: "IdOficio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oficios_x_Area",
                columns: table => new
                {
                    OficiosId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ScopeId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false),
                    FechaDerivado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    conocimiento = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SectioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficios_x_Area", x => new { x.OficiosId, x.SectionId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_Oficios_x_Area_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Oficios_x_Area_Oficios_OficiosId",
                        column: x => x.OficiosId,
                        principalTable: "Oficios",
                        principalColumn: "IdOficio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Oficios_x_Area_Scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Scopes",
                        principalColumn: "ScopeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Oficios_x_Area_Sections_SectioId",
                        column: x => x.SectioId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPdf_OficioId",
                table: "DocumentPdf",
                column: "OficioId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_OficioId",
                table: "Observation",
                column: "OficioId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_UserId",
                table: "Observation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_IdEstado",
                table: "Oficios",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_IdTipoOficio",
                table: "Oficios",
                column: "IdTipoOficio");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_UserId",
                table: "Oficios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_x_Area_ScopeId",
                table: "Oficios_x_Area",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_x_Area_SectioId",
                table: "Oficios_x_Area",
                column: "SectioId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_x_Area_UserId",
                table: "Oficios_x_Area",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentPdf");

            migrationBuilder.DropTable(
                name: "Interno_x_Oficio");

            migrationBuilder.DropTable(
                name: "Observation");

            migrationBuilder.DropTable(
                name: "Oficios_x_Area");

            migrationBuilder.DropTable(
                name: "Oficios");

            migrationBuilder.DropTable(
                name: "Estado");

            migrationBuilder.DropTable(
                name: "TipoOficios");
        }
    }
}
