using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Viral2Anime.Models.Video;

namespace Viral2Anime.Services.VideoAnalysis;

public class VideoMetadataService
{
    public async Task PopulateMetadataAsync(
        VideoProject project,
        string filePath)
    {
        var arguments =
            $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = processStartInfo
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"ffprobe failed: {error}");
        }

        using var document = JsonDocument.Parse(output);

        var root = document.RootElement;

        if (root.TryGetProperty("format", out var format))
        {
            if (format.TryGetProperty("duration", out var duration))
            {
                if (double.TryParse(
                    duration.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedDuration))
                {
                    project.DurationSeconds = parsedDuration;
                }
            }

            if (format.TryGetProperty("format_name", out var formatName))
            {
                project.FormatName =
                    formatName.GetString() ?? string.Empty;
            }
        }

        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (!stream.TryGetProperty(
                    "codec_type",
                    out var codecType))
                {
                    continue;
                }

                var type = codecType.GetString();

                if (type == "video")
                {
                    if (stream.TryGetProperty("width", out var width))
                    {
                        project.Width = width.GetInt32();
                    }

                    if (stream.TryGetProperty("height", out var height))
                    {
                        project.Height = height.GetInt32();
                    }

                    if (stream.TryGetProperty(
                        "avg_frame_rate",
                        out var frameRate))
                    {
                        project.FramesPerSecond =
                            ParseFrameRate(
                                frameRate.GetString());
                    }
                }

                if (type == "audio")
                {
                    project.HasAudio = true;
                }
            }
        }
    }

    private static double ParseFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (!value.Contains('/'))
        {
            return double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var simple)
                ? simple
                : 0;
        }

        var parts = value.Split('/');

        if (parts.Length != 2)
        {
            return 0;
        }

        if (!double.TryParse(
            parts[0],
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var numerator))
        {
            return 0;
        }

        if (!double.TryParse(
            parts[1],
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var denominator))
        {
            return 0;
        }

        if (denominator == 0)
        {
            return 0;
        }

        return numerator / denominator;
    }
}


