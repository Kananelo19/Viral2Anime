using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viral2Anime.Migrations
{
    /// <inheritdoc />
    public partial class AddOutputSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspectRatio",
                table: "ViralProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OutputResolution",
                table: "ViralProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TargetDurationSeconds",
                table: "ViralProjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AspectRatio",
                table: "ViralProjects");

            migrationBuilder.DropColumn(
                name: "OutputResolution",
                table: "ViralProjects");

            migrationBuilder.DropColumn(
                name: "TargetDurationSeconds",
                table: "ViralProjects");
        }
    }
}
