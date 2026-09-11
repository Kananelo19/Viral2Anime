using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viral2Anime.Migrations
{
    /// <inheritdoc />
    public partial class InitialViralProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViralProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    OutputType = table.Column<string>(type: "TEXT", nullable: false),
                    AnimationStyle = table.Column<string>(type: "TEXT", nullable: false),
                    Stage = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationSeconds = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    FramesPerSecond = table.Column<double>(type: "REAL", nullable: false),
                    HasAudio = table.Column<bool>(type: "INTEGER", nullable: false),
                    FormatName = table.Column<string>(type: "TEXT", nullable: false),
                    AnalysisJson = table.Column<string>(type: "TEXT", nullable: false),
                    StoryboardJson = table.Column<string>(type: "TEXT", nullable: false),
                    ReferenceFramesJson = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedKeyframesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViralProjects", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViralProjects");
        }
    }
}
