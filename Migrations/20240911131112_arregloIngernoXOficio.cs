using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class arregloIngernoXOficio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosId",
                table: "Interno_x_Oficio");

            migrationBuilder.DropForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Interno_x_Oficio_OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.DropColumn(
                name: "OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosId",
                table: "Interno_x_Oficio",
                column: "OficiosId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios",
                column: "IdEstabACargo",
                principalTable: "Scopes",
                principalColumn: "ScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosId",
                table: "Interno_x_Oficio");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios");

            migrationBuilder.AddColumn<int>(
                name: "OficiosIdOficio",
                table: "Interno_x_Oficio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interno_x_Oficio_OficiosIdOficio",
                table: "Interno_x_Oficio",
                column: "OficiosIdOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosId",
                table: "Interno_x_Oficio",
                column: "OficiosId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosIdOficio",
                table: "Interno_x_Oficio",
                column: "OficiosIdOficio",
                principalTable: "Oficios",
                principalColumn: "IdOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios",
                column: "IdEstabACargo",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
