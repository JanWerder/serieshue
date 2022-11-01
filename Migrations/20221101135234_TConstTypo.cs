using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serieshue.Migrations
{
    public partial class TConstTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TConst",
                table: "Episodes",
                newName: "Tconst");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tconst",
                table: "Episodes",
                newName: "TConst");
        }
    }
}
