namespace Viral2Anime.Models.Story;

public class AnimeStoryboard
{
    public string Title { get; set; } = string.Empty;

    public string Concept { get; set; } = string.Empty;

    public double TotalDurationSeconds { get; set; }

    public List<AnimeStoryboardShot> Shots { get; set; } = new();
}
