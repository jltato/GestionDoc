using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class masDatosOficio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Plazo",
                table: "Oficios",
                newName: "SAC");

            migrationBuilder.AddColumn<int>(
                name: "IdMedio",
                table: "Oficios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdPlazo",
                table: "Oficios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedioIng",
                columns: table => new
                {
                    IdMedio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedioName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedioIng", x => x.IdMedio);
                });

            migrationBuilder.CreateTable(
                name: "Plazo",
                columns: table => new
                {
                    IdPlazo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlazoName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plazo", x => x.IdPlazo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_IdMedio",
                table: "Oficios",
                column: "IdMedio");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_IdPlazo",
                table: "Oficios",
                column: "IdPlazo");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_MedioIng_IdMedio",
                table: "Oficios",
                column: "IdMedio",
                principalTable: "MedioIng",
                principalColumn: "IdMedio",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Plazo_IdPlazo",
                table: "Oficios",
                column: "IdPlazo",
                principalTable: "Plazo",
                principalColumn: "IdPlazo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_MedioIng_IdMedio",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Plazo_IdPlazo",
                table: "Oficios");

            migrationBuilder.DropTable(
                name: "MedioIng");

            migrationBuilder.DropTable(
                name: "Plazo");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_IdMedio",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_IdPlazo",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "IdMedio",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "IdPlazo",
                table: "Oficios");

            migrationBuilder.RenameColumn(
                name: "SAC",
                table: "Oficios",
                newName: "Plazo");
        }
    }
}
