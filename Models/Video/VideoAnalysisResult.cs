namespace Viral2Anime.Models.Video;

public class VideoAnalysisResult
{
    public string Summary { get; set; } = string.Empty;

    public int HighlightScore { get; set; }

    public string BestMomentDescription { get; set; } = string.Empty;

    public double BestMomentStartSeconds { get; set; }

    public double BestMomentEndSeconds { get; set; }

    public string AnimeConcept { get; set; } = string.Empty;

    public List<HighlightMoment> Moments { get; set; } = new();
}
