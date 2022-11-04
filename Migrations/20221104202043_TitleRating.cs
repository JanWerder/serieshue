using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serieshue.Migrations
{
    public partial class TitleRating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Titles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Votes",
                table: "Titles",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Titles");

            migrationBuilder.DropColumn(
                name: "Votes",
                table: "Titles");
        }
    }
}
