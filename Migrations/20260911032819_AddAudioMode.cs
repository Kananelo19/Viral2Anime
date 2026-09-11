using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viral2Anime.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioMode",
                table: "ViralProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioMode",
                table: "ViralProjects");
        }
    }
}
