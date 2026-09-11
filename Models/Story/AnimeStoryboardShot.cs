namespace Viral2Anime.Models.Story;

public class AnimeStoryboardShot
{
    public int ShotNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    // Original uploaded video timeline
    public double SourceStartSeconds { get; set; }

    public double SourceEndSeconds { get; set; }

    public double ReferenceFrameSeconds { get; set; }

    // Final anime video timeline
    public double OutputStartSeconds { get; set; }

    public double OutputEndSeconds { get; set; }

    public double DurationSeconds { get; set; }

    public string CameraDirection { get; set; } = string.Empty;

    public string SceneDescription { get; set; } = string.Empty;

    public string VisualEffects { get; set; } = string.Empty;

    public string CharacterAction { get; set; } = string.Empty;

    public string GenerationPrompt { get; set; } = string.Empty;
}
