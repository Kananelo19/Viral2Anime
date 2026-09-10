namespace Viral2Anime.Models.Video;

public class VideoProject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "Football Match";
    public string OutputType { get; set; } = "30 Second Short";
    public string AnimationStyle { get; set; } = "Cinematic Anime";

    public double DurationSeconds { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double FramesPerSecond { get; set; }

    public bool HasAudio { get; set; }

    public string FormatName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
