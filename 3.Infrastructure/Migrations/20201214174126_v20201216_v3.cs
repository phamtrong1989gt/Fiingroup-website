using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20201216_v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryType",
                table: "MenuItem",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "MenuItem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryType",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "MenuItem");
        }
    }
}
