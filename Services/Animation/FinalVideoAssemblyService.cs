using System.Diagnostics;
using System.Globalization;
using Viral2Anime.Models.Story;

namespace Viral2Anime.Services.Animation;

public class FinalVideoAssemblyService
{
    public async Task<FinalAnimeVideo> AssembleAsync(
        Guid projectId,
        List<AnimeShotClip> clips,
        string outputFolder,
        string audioMode,
        string sourceVideoPath,
        bool sourceHasAudio)
    {
        var usableClips =
            clips
                .Where(clip =>
                    clip.GeneratedSuccessfully &&
                    File.Exists(clip.ClipPath))
                .OrderBy(clip => clip.ShotNumber)
                .ToList();

        if (usableClips.Count == 0)
        {
            throw new Exception(
                "No completed anime shot clips are available.");
        }

        Directory.CreateDirectory(outputFolder);

        var concatFile =
            Path.Combine(
                outputFolder,
                $"concat_{projectId}.txt");

        var silentVideoPath =
            Path.Combine(
                outputFolder,
                $"silent_{projectId}.mp4");

        var outputFileName =
            $"viral2anime_{projectId}.mp4";

        var outputPath =
            Path.Combine(
                outputFolder,
                outputFileName);

        var concatLines =
            usableClips.Select(
                clip =>
                    $"file '{EscapeForConcat(clip.ClipPath)}'");

        await File.WriteAllLinesAsync(
            concatFile,
            concatLines);

        var duration =
            usableClips.Sum(
                clip => clip.DurationSeconds);

        await CreateSilentVideoAsync(
            concatFile,
            silentVideoPath);

        var useOriginalAudio =
            audioMode == "Original Audio" &&
            sourceHasAudio &&
            File.Exists(sourceVideoPath);

        if (useOriginalAudio)
        {
            await AddOriginalAudioAsync(
                silentVideoPath,
                sourceVideoPath,
                outputPath,
                duration);
        }
        else
        {
            File.Copy(
                silentVideoPath,
                outputPath,
                overwrite: true);
        }

        CleanupFile(concatFile);
        CleanupFile(silentVideoPath);

        if (!File.Exists(outputPath))
        {
            throw new Exception(
                "Final video was not created.");
        }

        return new FinalAnimeVideo
        {
            ProjectId = projectId,
            FilePath = outputPath,
            DurationSeconds = duration,
            ShotCount = usableClips.Count,
            GeneratedSuccessfully = true
        };
    }

    private static async Task CreateSilentVideoAsync(
        string concatFile,
        string outputPath)
    {
        var startInfo =
            CreateProcess();

        startInfo.ArgumentList.Add("-y");
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add("concat");
        startInfo.ArgumentList.Add("-safe");
        startInfo.ArgumentList.Add("0");
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(concatFile);

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("0:v:0");

        startInfo.ArgumentList.Add("-c:v");
        startInfo.ArgumentList.Add("libx264");

        startInfo.ArgumentList.Add("-pix_fmt");
        startInfo.ArgumentList.Add("yuv420p");

        startInfo.ArgumentList.Add("-an");

        startInfo.ArgumentList.Add("-movflags");
        startInfo.ArgumentList.Add("+faststart");

        startInfo.ArgumentList.Add(outputPath);

        await RunFfmpegAsync(
            startInfo,
            "Final silent video assembly failed");
    }

    private static async Task AddOriginalAudioAsync(
        string silentVideoPath,
        string sourceVideoPath,
        string outputPath,
        double durationSeconds)
    {
        var startInfo =
            CreateProcess();

        startInfo.ArgumentList.Add("-y");

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(
            silentVideoPath);

        // Loop the original source if the anime output
        // lasts longer than the original clip.
        startInfo.ArgumentList.Add(
            "-stream_loop");

        startInfo.ArgumentList.Add("-1");

        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(
            sourceVideoPath);

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("0:v:0");

        startInfo.ArgumentList.Add("-map");
        startInfo.ArgumentList.Add("1:a:0");

        startInfo.ArgumentList.Add("-c:v");
        startInfo.ArgumentList.Add("copy");

        startInfo.ArgumentList.Add("-c:a");
        startInfo.ArgumentList.Add("aac");

        startInfo.ArgumentList.Add("-b:a");
        startInfo.ArgumentList.Add("192k");

        startInfo.ArgumentList.Add("-t");
        startInfo.ArgumentList.Add(
            durationSeconds.ToString(
                "0.000",
                CultureInfo.InvariantCulture));

        startInfo.ArgumentList.Add("-movflags");
        startInfo.ArgumentList.Add("+faststart");

        startInfo.ArgumentList.Add(outputPath);

        await RunFfmpegAsync(
            startInfo,
            "Adding original audio failed");
    }

    private static ProcessStartInfo CreateProcess()
    {
        return new ProcessStartInfo
        {
            FileName = "ffmpeg",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    private static async Task RunFfmpegAsync(
        ProcessStartInfo startInfo,
        string errorPrefix)
    {
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
                $"{errorPrefix}:\n{error}");
        }
    }

    private static string EscapeForConcat(
        string path)
    {
        return path.Replace(
            "'",
            "'\\''");
    }

    private static void CleanupFile(
        string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}
