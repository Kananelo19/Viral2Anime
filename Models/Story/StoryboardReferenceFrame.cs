namespace Viral2Anime.Models.Story;

public class StoryboardReferenceFrame
{
    public int ShotNumber { get; set; }

    public double TargetSeconds { get; set; }

    public string FramePath { get; set; } = string.Empty;

    public string FrameUrl { get; set; } = string.Empty;
}
