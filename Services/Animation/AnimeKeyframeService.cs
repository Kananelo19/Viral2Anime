using System.Diagnostics;
using Viral2Anime.Models.Story;

namespace Viral2Anime.Services.Animation;

public class AnimeKeyframeService
{
    private const string ModelName =
        "dreamshaper_v8_q6p_q8p.ckpt";

    public async Task<AnimeKeyframe> GenerateKeyframeAsync(
        AnimeStoryboardShot shot,
        StoryboardReferenceFrame referenceFrame,
        string outputFolder)
    {
        if (!File.Exists(referenceFrame.FramePath))
        {
            throw new Exception(
                $"Reference frame does not exist: {referenceFrame.FramePath}");
        }

        Directory.CreateDirectory(outputFolder);

        var fileName =
            $"shot_{shot.ShotNumber:000}.png";

        var outputPath =
            Path.Combine(
                outputFolder,
                fileName);

        var prompt = $"""
original cinematic anime-inspired soccer scene,
preserve the real players visible in the source reference,
preserve the real uniform colors,
preserve yellow and black uniforms when visible,
preserve opponent uniform colors,
preserve field layout,
preserve ball location when visible,
preserve player positions,
preserve direction of play,
preserve stadium and environment,

storyboard shot: {shot.Title},

character action:
{shot.CharacterAction},

scene:
{shot.SceneDescription},

camera direction:
{shot.CameraDirection},

visual effects:
{shot.VisualEffects},

cinematic sports composition,
dynamic action,
expressive lighting,
speed lines where appropriate,
dramatic perspective,
polished animation keyframe,
high detail,
original anime-inspired visual treatment,
no captions,
no text,
no logos,
no watermark
""";

        const string negativePrompt = """
wrong uniform colors,
blue uniforms when not present,
red uniforms when not present,
extra unnecessary players,
duplicate players,
extra limbs,
missing limbs,
distorted anatomy,
deformed hands,
deformed legs,
duplicate ball,
multiple soccer balls,
floating ball,
blurry,
low quality,
text,
captions,
watermark,
logo
""";

        var startInfo = new ProcessStartInfo
        {
            FileName = "draw-things-cli",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("generate");

        startInfo.ArgumentList.Add("--model");
        startInfo.ArgumentList.Add(ModelName);

        startInfo.ArgumentList.Add("--image");
        startInfo.ArgumentList.Add(
            referenceFrame.FramePath);

        startInfo.ArgumentList.Add("--prompt");
        startInfo.ArgumentList.Add(prompt);

        startInfo.ArgumentList.Add(
            "--negative-prompt");
        startInfo.ArgumentList.Add(
            negativePrompt);

        startInfo.ArgumentList.Add("--strength");
        startInfo.ArgumentList.Add("0.45");

        startInfo.ArgumentList.Add("--steps");
        startInfo.ArgumentList.Add("20");

        startInfo.ArgumentList.Add("--cfg");
        startInfo.ArgumentList.Add("7");

        startInfo.ArgumentList.Add("--width");
        startInfo.ArgumentList.Add("768");

        startInfo.ArgumentList.Add("--height");
        startInfo.ArgumentList.Add("448");

        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(outputPath);

        startInfo.ArgumentList.Add("--offline");
        startInfo.ArgumentList.Add("--disable-preview");

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var standardOutputTask =
            process.StandardOutput.ReadToEndAsync();

        var standardErrorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var standardOutput =
            await standardOutputTask;

        var standardError =
            await standardErrorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception(
                "Local anime generation failed.\n" +
                $"Exit code: {process.ExitCode}\n" +
                $"Output:\n{standardOutput}\n" +
                $"Error:\n{standardError}");
        }

        if (!File.Exists(outputPath))
        {
            throw new Exception(
                "Draw Things finished but did not create " +
                $"the expected image: {outputPath}");
        }

        return new AnimeKeyframe
        {
            ShotNumber = shot.ShotNumber,
            Title = shot.Title,
            SourceFrameUrl =
                referenceFrame.FrameUrl,
            GeneratedImagePath =
                outputPath,
            Prompt =
                prompt,
            GeneratedSuccessfully =
                true
        };
    }
}
