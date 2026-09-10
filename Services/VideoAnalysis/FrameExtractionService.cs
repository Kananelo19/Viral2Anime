using System.Diagnostics;

namespace Viral2Anime.Services.VideoAnalysis;

public class FrameExtractionService
{
    public async Task<List<string>> ExtractFramesAsync(
        string videoPath,
        string outputFolder,
        int intervalSeconds = 3)
    {
        Directory.CreateDirectory(outputFolder);

        var outputPattern = Path.Combine(
            outputFolder,
            "frame_%03d.jpg");

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-y");
        startInfo.ArgumentList.Add("-i");
        startInfo.ArgumentList.Add(videoPath);

        startInfo.ArgumentList.Add("-vf");
        startInfo.ArgumentList.Add($"fps=1/{intervalSeconds}");

        startInfo.ArgumentList.Add("-q:v");
        startInfo.ArgumentList.Add("2");

        startInfo.ArgumentList.Add(outputPattern);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg frame extraction failed: {error}");
        }

        return Directory
            .GetFiles(outputFolder, "frame_*.jpg")
            .OrderBy(path => path)
            .ToList();
    }
}
