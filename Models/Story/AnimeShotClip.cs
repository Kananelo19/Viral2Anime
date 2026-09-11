namespace Viral2Anime.Models.Story;

public class AnimeShotClip
{
    public int ShotNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public double DurationSeconds { get; set; }

    public string SourceKeyframeUrl { get; set; } = string.Empty;

    public string ClipPath { get; set; } = string.Empty;

    public string ClipUrl { get; set; } = string.Empty;

    public bool GeneratedSuccessfully { get; set; }
}
