using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUAP_PortalOficios.Migrations
{
    /// <inheritdoc />
    public partial class agregoMyUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PaswordChange",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaswordChange",
                table: "AspNetUsers");
        }
    }
}
