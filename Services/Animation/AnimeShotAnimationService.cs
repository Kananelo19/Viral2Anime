using System.Diagnostics;
using System.Globalization;
using Viral2Anime.Models.Story;

namespace Viral2Anime.Services.Animation;

public class AnimeShotAnimationService
{
    public async Task<AnimeShotClip> GenerateClipAsync(
        AnimeStoryboardShot shot,
        AnimeKeyframe keyframe,
        string outputFolder,
        string outputResolution)
    {
        if (!File.Exists(keyframe.GeneratedImagePath))
        {
            throw new Exception(
                $"Anime keyframe does not exist: " +
                $"{keyframe.GeneratedImagePath}");
        }

        Directory.CreateDirectory(outputFolder);

        var duration =
            shot.DurationSeconds > 0
                ? shot.DurationSeconds
                : 4.0;

        var (width, height) =
            ParseResolution(outputResolution);

        var fileName =
            $"shot_{shot.ShotNumber:000}.mp4";

        var outputPath =
            Path.Combine(
                outputFolder,
                fileName);

        const int fps = 30;

        var frames =
            Math.Max(
                1,
                (int)Math.Round(
                    duration * fps));

        var filter =
            $"scale={width}:{height}:" +
            $"force_original_aspect_ratio=increase," +
            $"crop={width}:{height}," +
            $"zoompan=" +
            $"z='min(zoom+0.0015,1.10)':" +
            $"x='iw/2-(iw/zoom/2)':" +
            $"y='ih/2-(ih/zoom/2)':" +
            $"d={frames}:" +
            $"s={width}x{height}:" +
            $"fps={fps}," +
            $"format=yuv420p";

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
        startInfo.ArgumentList.Add("-loop");
        startInfo.ArgumentList.Add("1");
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(
            keyframe.GeneratedImagePath);
        startInfo.ArgumentList.Add("-vf");
        startInfo.ArgumentList.Add(filter);
        startInfo.ArgumentList.Add("-t");
        startInfo.ArgumentList.Add(
            duration.ToString(
                "0.000",
                CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("-r");
        startInfo.ArgumentList.Add(
            fps.ToString());
        startInfo.ArgumentList.Add("-c:v");
        startInfo.ArgumentList.Add("libx264");
        startInfo.ArgumentList.Add("-pix_fmt");
        startInfo.ArgumentList.Add("yuv420p");
        startInfo.ArgumentList.Add("-movflags");
        startInfo.ArgumentList.Add("+faststart");
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
                "FFmpeg anime shot animation failed:\n" +
                error);
        }

        if (!File.Exists(outputPath))
        {
            throw new Exception(
                "FFmpeg finished but no shot clip " +
                "was created.");
        }

        return new AnimeShotClip
        {
            ShotNumber = shot.ShotNumber,
            Title = shot.Title,
            DurationSeconds = duration,
            SourceKeyframeUrl =
                keyframe.GeneratedImageUrl,
            ClipPath = outputPath,
            GeneratedSuccessfully = true
        };
    }

    private static (int Width, int Height)
        ParseResolution(string resolution)
    {
        return resolution switch
        {
            "1080x1920" => (1080, 1920),
            "1920x1080" => (1920, 1080),
            "1080x1080" => (1080, 1080),
            "720x1280" => (720, 1280),
            "1280x720" => (1280, 720),

            _ => (1080, 1920)
        };
    }
}
