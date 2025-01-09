using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class EstACargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_AspNetUsers_UserId",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Estado_IdEstado",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_IdEstado",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_IdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_UserId",
                table: "Oficios");

            migrationBuilder.AddColumn<bool>(
                name: "Visto",
                table: "Oficios_x_Area",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Oficios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdEstado",
                table: "Oficios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoIdEstado",
                table: "Oficios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdEstabACargo",
                table: "Oficios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "Oficios",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeId",
                table: "Oficios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoOficioIdTipoOficio",
                table: "Oficios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_EstadoIdEstado",
                table: "Oficios",
                column: "EstadoIdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_MyUserId",
                table: "Oficios",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_ScopeId",
                table: "Oficios",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_TipoOficioIdTipoOficio",
                table: "Oficios",
                column: "TipoOficioIdTipoOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_AspNetUsers_MyUserId",
                table: "Oficios",
                column: "MyUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Estado_EstadoIdEstado",
                table: "Oficios",
                column: "EstadoIdEstado",
                principalTable: "Estado",
                principalColumn: "IdEstado");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Scopes_ScopeId",
                table: "Oficios",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_TipoOficios_TipoOficioIdTipoOficio",
                table: "Oficios",
                column: "TipoOficioIdTipoOficio",
                principalTable: "TipoOficios",
                principalColumn: "IdTipoOficio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_AspNetUsers_MyUserId",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Estado_EstadoIdEstado",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Scopes_ScopeId",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_TipoOficios_TipoOficioIdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_EstadoIdEstado",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_MyUserId",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_ScopeId",
                table: "Oficios");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_TipoOficioIdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "Visto",
                table: "Oficios_x_Area");

            migrationBuilder.DropColumn(
                name: "EstadoIdEstado",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "IdEstabACargo",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                table: "Oficios");

            migrationBuilder.DropColumn(
                name: "TipoOficioIdTipoOficio",
                table: "Oficios");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Oficios",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "IdEstado",
                table: "Oficios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_AspNetUsers_UserId",
                table: "Oficios",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Estado_IdEstado",
                table: "Oficios",
                column: "IdEstado",
                principalTable: "Estado",
                principalColumn: "IdEstado");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios",
                column: "IdTipoOficio",
                principalTable: "TipoOficios",
                principalColumn: "IdTipoOficio");
        }
    }
}
