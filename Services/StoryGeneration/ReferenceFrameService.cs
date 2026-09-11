using System.Globalization;
using Viral2Anime.Models.Story;

namespace Viral2Anime.Services.StoryGeneration;

public class ReferenceFrameService
{
    public List<StoryboardReferenceFrame> MatchFrames(
        AnimeStoryboard storyboard,
        List<string> extractedFramePaths,
        string frameFolderName)
    {
        var references = new List<StoryboardReferenceFrame>();

        if (storyboard.Shots.Count == 0 ||
            extractedFramePaths.Count == 0)
        {
            return references;
        }

        var frames = extractedFramePaths
            .Select(path => new
            {
                Path = path,
                Index = GetFrameIndex(path)
            })
            .Where(x => x.Index > 0)
            .OrderBy(x => x.Index)
            .ToList();

        if (frames.Count == 0)
        {
            return references;
        }

        const double frameIntervalSeconds = 3.0;

        foreach (var shot in storyboard.Shots)
        {
            var targetSeconds = shot.ReferenceFrameSeconds;

            if (targetSeconds <= 0)
            {
                targetSeconds =
                    (shot.SourceStartSeconds +
                     shot.SourceEndSeconds) / 2.0;
            }

            var targetFrameNumber =
                (int)Math.Round(
                    targetSeconds / frameIntervalSeconds,
                    MidpointRounding.AwayFromZero);

            if (targetFrameNumber < 1)
            {
                targetFrameNumber = 1;
            }

            var closestFrame = frames
                .OrderBy(frame =>
                    Math.Abs(frame.Index - targetFrameNumber))
                .First();

            references.Add(new StoryboardReferenceFrame
            {
                ShotNumber = shot.ShotNumber,
                TargetSeconds = targetSeconds,
                FramePath = closestFrame.Path,
                FrameUrl =
                    "/generated/frames/" +
                    frameFolderName +
                    "/" +
                    Path.GetFileName(closestFrame.Path)
            });
        }

        return references;
    }

    private static int GetFrameIndex(string path)
    {
        var fileName =
            Path.GetFileNameWithoutExtension(path);

        var parts = fileName.Split('_');

        if (parts.Length == 0)
        {
            return 0;
        }

        return int.TryParse(
            parts[^1],
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var index)
            ? index
            : 0;
    }
}
