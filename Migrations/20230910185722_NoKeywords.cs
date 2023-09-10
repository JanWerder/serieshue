using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serieshue.Migrations
{
    /// <inheritdoc />
    public partial class NoKeywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "AdditionalInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "AdditionalInfos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
