using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serieshue.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Tconst = table.Column<string>(type: "text", nullable: false),
                    PrimaryTitle = table.Column<string>(type: "text", nullable: true),
                    OriginalTitle = table.Column<string>(type: "text", nullable: true),
                    IsAdult = table.Column<bool>(type: "boolean", nullable: true),
                    StartYear = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    RuntimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    Genres = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Tconst);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    TConst = table.Column<string>(type: "text", nullable: false),
                    TitleTconst = table.Column<string>(type: "text", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    EpisodeTitle = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: true),
                    Votes = table.Column<int>(type: "integer", nullable: true),
                    RuntimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    Genres = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.TConst);
                    table.ForeignKey(
                        name: "FK_Episodes_Titles_TitleTconst",
                        column: x => x.TitleTconst,
                        principalTable: "Titles",
                        principalColumn: "Tconst",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_TitleTconst",
                table: "Episodes",
                column: "TitleTconst");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
