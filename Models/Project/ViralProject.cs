namespace Viral2Anime.Models.Project;

public class ViralProject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "Football Match";

    public string OutputType { get; set; } = "30 Second Short";

    public string AnimationStyle { get; set; } = "Cinematic Anime";

    public string AudioMode { get; set; } = "Mute";

    public string AspectRatio { get; set; } = "9:16";

    public string OutputResolution { get; set; } = "1080x1920";

    public int TargetDurationSeconds { get; set; } = 30;

    public ProjectStage Stage { get; set; } =
        ProjectStage.Uploaded;

    public double DurationSeconds { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double FramesPerSecond { get; set; }

    public bool HasAudio { get; set; }

    public string FormatName { get; set; } = string.Empty;

    public string AnalysisJson { get; set; } = string.Empty;

    public string StoryboardJson { get; set; } = string.Empty;

    public string ReferenceFramesJson { get; set; } = string.Empty;

    public string GeneratedKeyframesJson { get; set; } = string.Empty;

    public string AnimatedClipsJson { get; set; } = string.Empty;

    public string FinalVideoFileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } =
        DateTime.UtcNow;
}
