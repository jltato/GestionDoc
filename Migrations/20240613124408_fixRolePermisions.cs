using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class fixRolePermisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_AspNetRoles_RoleNavigationId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Scopes_ScopeNavigationScopeId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Sections_SectionNavigationSectionId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleNavigationId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_ScopeNavigationScopeId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_SectionNavigationSectionId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "RoleNavigationId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "ScopeNavigationScopeId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "SectionNavigationSectionId",
                table: "RolePermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleNavigationId",
                table: "RolePermissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeNavigationScopeId",
                table: "RolePermissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionNavigationSectionId",
                table: "RolePermissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleNavigationId",
                table: "RolePermissions",
                column: "RoleNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_ScopeNavigationScopeId",
                table: "RolePermissions",
                column: "ScopeNavigationScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_SectionNavigationSectionId",
                table: "RolePermissions",
                column: "SectionNavigationSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_AspNetRoles_RoleNavigationId",
                table: "RolePermissions",
                column: "RoleNavigationId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Scopes_ScopeNavigationScopeId",
                table: "RolePermissions",
                column: "ScopeNavigationScopeId",
                principalTable: "Scopes",
                principalColumn: "ScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Sections_SectionNavigationSectionId",
                table: "RolePermissions",
                column: "SectionNavigationSectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");
        }
    }
}
