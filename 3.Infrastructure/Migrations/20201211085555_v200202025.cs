using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v200202025 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AltId",
                table: "FileData",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltId",
                table: "FileData");
        }
    }
}
