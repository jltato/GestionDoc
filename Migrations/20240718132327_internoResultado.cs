using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class internoResultado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectioId",
                table: "Oficios_x_Area");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_x_Area_SectioId",
                table: "Oficios_x_Area");

            migrationBuilder.DropColumn(
                name: "SectioId",
                table: "Oficios_x_Area");

            migrationBuilder.AlterColumn<int>(
                name: "IdTipoOficio",
                table: "Oficios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_x_Area_SectionId",
                table: "Oficios_x_Area",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios",
                column: "IdTipoOficio",
                principalTable: "TipoOficios",
                principalColumn: "IdTipoOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_x_Area_SectionId",
                table: "Oficios_x_Area");

            migrationBuilder.AddColumn<int>(
                name: "SectioId",
                table: "Oficios_x_Area",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "IdTipoOficio",
                table: "Oficios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_x_Area_SectioId",
                table: "Oficios_x_Area",
                column: "SectioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios",
                column: "IdTipoOficio",
                principalTable: "TipoOficios",
                principalColumn: "IdTipoOficio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectioId",
                table: "Oficios_x_Area",
                column: "SectioId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
