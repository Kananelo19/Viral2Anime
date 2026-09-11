using System.Collections.Concurrent;
using Viral2Anime.Models.Story;
using Viral2Anime.Models.Video;

namespace Viral2Anime.Services.ProjectState;

public class ProjectProcessingStore
{
    private readonly ConcurrentDictionary<Guid, ProjectProcessingSession>
        _sessions = new();

    public void Save(ProjectProcessingSession session)
    {
        _sessions[session.Project.Id] = session;
    }

    public ProjectProcessingSession? Get(Guid projectId)
    {
        return _sessions.TryGetValue(
            projectId,
            out var session)
            ? session
            : null;
    }

    public bool Remove(Guid projectId)
    {
        return _sessions.TryRemove(
            projectId,
            out _);
    }
}

public class ProjectProcessingSession
{
    public VideoProject Project { get; set; } = new();

    public VideoAnalysisResult Analysis { get; set; } = new();

    public AnimeStoryboard Storyboard { get; set; } = new();

    public List<StoryboardReferenceFrame> ReferenceFrames { get; set; }
        = new();

    public List<AnimeKeyframe> GeneratedKeyframes { get; set; }
        = new();

    public List<AnimeShotClip> AnimatedClips { get; set; }
        = new();

    public string FinalVideoUrl { get; set; } = string.Empty;
}
