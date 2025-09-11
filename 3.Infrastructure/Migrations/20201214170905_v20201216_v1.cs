using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v20201216_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkId",
                table: "MenuItem",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkId",
                table: "MenuItem");
        }
    }
}
