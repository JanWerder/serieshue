using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace serieshue.Migrations
{
    public partial class Vector : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Titles",
                type: "tsvector",
                nullable: false)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "PrimaryTitle", "OriginalTitle" });

            migrationBuilder.CreateIndex(
                name: "IX_Titles_SearchVector",
                table: "Titles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Titles_SearchVector",
                table: "Titles");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Titles");
        }
    }
}
