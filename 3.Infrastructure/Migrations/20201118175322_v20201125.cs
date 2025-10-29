using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20201125 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerFooter",
                table: "Tour",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerHeader",
                table: "Tour",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerFooter",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "BannerHeader",
                table: "Tour");
        }
    }
}
