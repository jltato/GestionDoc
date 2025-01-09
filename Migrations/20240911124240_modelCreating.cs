using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class modelCreating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Oficios_OficioId",
                table: "Observation");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_AspNetUsers_UserId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Scopes_ScopeId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area");

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
                name: "EstadoIdEstado",
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

            migrationBuilder.RenameColumn(
                name: "OficioId",
                table: "Observation",
                newName: "IdOficio");

            migrationBuilder.RenameIndex(
                name: "IX_Observation_OficioId",
                table: "Observation",
                newName: "IX_Observation_IdOficio");

            migrationBuilder.AlterColumn<bool>(
                name: "conocimiento",
                table: "Oficios_x_Area",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Visto",
                table: "Oficios_x_Area",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDerivado",
                table: "Oficios_x_Area",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "EstadoId",
                table: "Oficios_x_Area",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Oficios",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaIngreso",
                table: "Oficios",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "OficiosIdOficio",
                table: "Interno_x_Oficio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficios_IdEstabACargo",
                table: "Oficios",
                column: "IdEstabACargo");

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
                name: "IX_Interno_x_Oficio_OficiosIdOficio",
                table: "Interno_x_Oficio",
                column: "OficiosIdOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosIdOficio",
                table: "Interno_x_Oficio",
                column: "OficiosIdOficio",
                principalTable: "Oficios",
                principalColumn: "IdOficio");

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_Oficios_IdOficio",
                table: "Observation",
                column: "IdOficio",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_AspNetUsers_UserId",
                table: "Oficios",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Estado_IdEstado",
                table: "Oficios",
                column: "IdEstado",
                principalTable: "Estado",
                principalColumn: "IdEstado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios",
                column: "IdEstabACargo",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios",
                column: "IdTipoOficio",
                principalTable: "TipoOficios",
                principalColumn: "IdTipoOficio",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_AspNetUsers_UserId",
                table: "Oficios_x_Area",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Scopes_ScopeId",
                table: "Oficios_x_Area",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interno_x_Oficio_Oficios_OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Oficios_IdOficio",
                table: "Observation");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_AspNetUsers_UserId",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Estado_IdEstado",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_Scopes_IdEstabACargo",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_TipoOficios_IdTipoOficio",
                table: "Oficios");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_AspNetUsers_UserId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Scopes_ScopeId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area");

            migrationBuilder.DropIndex(
                name: "IX_Oficios_IdEstabACargo",
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

            migrationBuilder.DropIndex(
                name: "IX_Interno_x_Oficio_OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.DropColumn(
                name: "OficiosIdOficio",
                table: "Interno_x_Oficio");

            migrationBuilder.RenameColumn(
                name: "IdOficio",
                table: "Observation",
                newName: "OficioId");

            migrationBuilder.RenameIndex(
                name: "IX_Observation_IdOficio",
                table: "Observation",
                newName: "IX_Observation_OficioId");

            migrationBuilder.AlterColumn<bool>(
                name: "conocimiento",
                table: "Oficios_x_Area",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Visto",
                table: "Oficios_x_Area",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDerivado",
                table: "Oficios_x_Area",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "EstadoId",
                table: "Oficios_x_Area",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Oficios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaIngreso",
                table: "Oficios",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "EstadoIdEstado",
                table: "Oficios",
                type: "int",
                nullable: true);

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
                name: "FK_Observation_Oficios_OficioId",
                table: "Observation",
                column: "OficioId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_AspNetUsers_UserId",
                table: "Oficios_x_Area",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Scopes_ScopeId",
                table: "Oficios_x_Area",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Sections_SectionId",
                table: "Oficios_x_Area",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
