using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class userPermissionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Oficios_OficiosId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_AspNetUsers_UserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Sections_SectionId",
                table: "UserPermissions");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Oficios_OficiosId",
                table: "Oficios_x_Area",
                column: "OficiosId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_AspNetUsers_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Scopes_ScopeId",
                table: "UserPermissions",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Sections_SectionId",
                table: "UserPermissions",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficios_x_Area_Oficios_OficiosId",
                table: "Oficios_x_Area");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_AspNetUsers_UserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Scopes_ScopeId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Sections_SectionId",
                table: "UserPermissions");

          
            migrationBuilder.AddForeignKey(
                name: "FK_Oficios_x_Area_Oficios_OficiosId",
                table: "Oficios_x_Area",
                column: "OficiosId",
                principalTable: "Oficios",
                principalColumn: "IdOficio",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_AspNetUsers_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Scopes_ScopeId",
                table: "UserPermissions",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Sections_SectionId",
                table: "UserPermissions",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
