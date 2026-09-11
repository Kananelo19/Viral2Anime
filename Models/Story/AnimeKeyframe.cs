namespace Viral2Anime.Models.Story;

public class AnimeKeyframe
{
    public int ShotNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string SourceFrameUrl { get; set; } = string.Empty;

    public string GeneratedImagePath { get; set; } = string.Empty;

    public string GeneratedImageUrl { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public bool GeneratedSuccessfully { get; set; }
}
