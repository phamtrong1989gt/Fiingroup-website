using Microsoft.EntityFrameworkCore.Migrations;

namespace PT.Infrastructure.Migrations
{
    public partial class v200202020 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TourType",
                table: "Tour");

            migrationBuilder.AddColumn<int>(
                name: "TourTypeId",
                table: "Tour",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Acction",
                table: "Link",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Link",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "Link",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parrams",
                table: "Link",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Banner = table.Column<string>(nullable: true),
                    BannerHeader = table.Column<string>(nullable: true),
                    BannerFooter = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Language = table.Column<string>(maxLength: 10, nullable: true),
                    Delete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourType", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourType");

            migrationBuilder.DropColumn(
                name: "TourTypeId",
                table: "Tour");

            migrationBuilder.DropColumn(
                name: "Acction",
                table: "Link");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Link");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "Link");

            migrationBuilder.DropColumn(
                name: "Parrams",
                table: "Link");

            migrationBuilder.AddColumn<int>(
                name: "TourType",
                table: "Tour",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
