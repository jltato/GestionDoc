using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class modificoDocumentPdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentPdf_Oficios_OficioId",
                table: "DocumentPdf");

            migrationBuilder.AlterColumn<string>(
                name: "src",
                table: "DocumentPdf",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fechaCarga",
                table: "DocumentPdf",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DocumentPdf",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EliminadoLogico",
                table: "DocumentPdf",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentPdf_Oficios_OficioId",
                table: "DocumentPdf",
                column: "OficioId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentPdf_Oficios_OficioId",
                table: "DocumentPdf");

            migrationBuilder.AlterColumn<string>(
                name: "src",
                table: "DocumentPdf",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fechaCarga",
                table: "DocumentPdf",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "DocumentPdf",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EliminadoLogico",
                table: "DocumentPdf",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentPdf_Oficios_OficioId",
                table: "DocumentPdf",
                column: "OficioId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
