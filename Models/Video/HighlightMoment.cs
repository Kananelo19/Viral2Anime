namespace Viral2Anime.Models.Video;

public class HighlightMoment
{
    public double StartSeconds { get; set; }

    public double EndSeconds { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ImportanceScore { get; set; }

    public string AnimeDirection { get; set; } = string.Empty;
}
