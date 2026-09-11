namespace Viral2Anime.Models.Story;

public class FinalAnimeVideo
{
    public Guid ProjectId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public double DurationSeconds { get; set; }

    public int ShotCount { get; set; }

    public bool GeneratedSuccessfully { get; set; }
}
