using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viral2Anime.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimationAndFinalRender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnimatedClipsJson",
                table: "ViralProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FinalVideoFileName",
                table: "ViralProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnimatedClipsJson",
                table: "ViralProjects");

            migrationBuilder.DropColumn(
                name: "FinalVideoFileName",
                table: "ViralProjects");
        }
    }
}
