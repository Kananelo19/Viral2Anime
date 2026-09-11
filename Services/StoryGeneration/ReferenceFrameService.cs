using System.Diagnostics;
using System.Globalization;
using Viral2Anime.Models.Story;

namespace Viral2Anime.Services.StoryGeneration;

public class ReferenceFrameService
{
    public async Task<List<StoryboardReferenceFrame>>
        ExtractExactFramesAsync(
            AnimeStoryboard storyboard,
            string videoPath,
            string outputFolder,
            string frameFolderName)
    {
        var references =
            new List<StoryboardReferenceFrame>();

        Directory.CreateDirectory(outputFolder);

        foreach (var shot in storyboard.Shots
            .OrderBy(item => item.ShotNumber))
        {
            var targetSeconds =
                shot.ReferenceFrameSeconds;

            if (targetSeconds <= 0)
            {
                targetSeconds =
                    (shot.SourceStartSeconds +
                     shot.SourceEndSeconds) / 2.0;
            }

            if (targetSeconds < 0)
            {
                targetSeconds = 0;
            }

            var fileName =
                $"shot_{shot.ShotNumber:000}_reference.jpg";

            var outputPath =
                Path.Combine(
                    outputFolder,
                    fileName);

            await ExtractFrameAsync(
                videoPath,
                targetSeconds,
                outputPath);

            references.Add(
                new StoryboardReferenceFrame
                {
                    ShotNumber =
                        shot.ShotNumber,

                    TargetSeconds =
                        targetSeconds,

                    FramePath =
                        outputPath,

                    FrameUrl =
                        "/generated/references/" +
                        frameFolderName +
                        "/" +
                        fileName
                });
        }

        return references;
    }

    private static async Task ExtractFrameAsync(
        string videoPath,
        double seconds,
        string outputPath)
    {
        var startInfo =
            new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

        startInfo.ArgumentList.Add("-y");

        startInfo.ArgumentList.Add("-ss");

        startInfo.ArgumentList.Add(
            seconds.ToString(
                "0.000",
                CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(videoPath);

        startInfo.ArgumentList.Add("-frames:v");
        startInfo.ArgumentList.Add("1");

        startInfo.ArgumentList.Add("-q:v");
        startInfo.ArgumentList.Add("2");

        startInfo.ArgumentList.Add(outputPath);

        using var process =
            new Process
            {
                StartInfo = startInfo
            };

        process.Start();

        var outputTask =
            process.StandardOutput.ReadToEndAsync();

        var errorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        await outputTask;

        var error =
            await errorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"FFmpeg exact frame extraction failed at " +
                $"{seconds:0.000}s:\n{error}");
        }

        if (!File.Exists(outputPath))
        {
            throw new Exception(
                $"FFmpeg did not create reference frame " +
                $"for {seconds:0.000}s.");
        }
    }
}
