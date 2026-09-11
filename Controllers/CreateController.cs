using Microsoft.AspNetCore.Mvc;
using Viral2Anime.Models.Video;
using Viral2Anime.Services.Animation;
using Viral2Anime.Services.StoryGeneration;
using Viral2Anime.Services.VideoAnalysis;

namespace Viral2Anime.Controllers;

public class CreateController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly VideoMetadataService _metadataService;
    private readonly FrameExtractionService _frameExtractionService;
    private readonly GeminiVideoAnalysisService _geminiVideoAnalysisService;
    private readonly AnimeStoryboardService _animeStoryboardService;
    private readonly ReferenceFrameService _referenceFrameService;
    private readonly AnimeKeyframeService _animeKeyframeService;

    public CreateController(
        IWebHostEnvironment environment,
        VideoMetadataService metadataService,
        FrameExtractionService frameExtractionService,
        GeminiVideoAnalysisService geminiVideoAnalysisService,
        AnimeStoryboardService animeStoryboardService,
        ReferenceFrameService referenceFrameService,
        AnimeKeyframeService animeKeyframeService)
    {
        _environment = environment;
        _metadataService = metadataService;
        _frameExtractionService = frameExtractionService;
        _geminiVideoAnalysisService = geminiVideoAnalysisService;
        _animeStoryboardService = animeStoryboardService;
        _referenceFrameService = referenceFrameService;
        _animeKeyframeService = animeKeyframeService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Index(
        IFormFile video,
        string title,
        string outputType)
    {
        if (video == null || video.Length == 0)
        {
            ViewBag.Error = "Select a video first.";
            return View();
        }

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(video.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            storedFileName);

        await using (var stream =
            new FileStream(filePath, FileMode.Create))
        {
            await video.CopyToAsync(stream);
        }

        var project = new VideoProject
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? Path.GetFileNameWithoutExtension(video.FileName)
                : title,

            OriginalFileName = video.FileName,
            StoredFileName = storedFileName,

            OutputType = string.IsNullOrWhiteSpace(outputType)
                ? "30 Second Short"
                : outputType
        };

        await _metadataService.PopulateMetadataAsync(
            project,
            filePath);

        var frameFolderName =
            project.Id.ToString();

        var frameFolderPath = Path.Combine(
            _environment.WebRootPath,
            "generated",
            "frames",
            frameFolderName);

        var extractedFrames =
            await _frameExtractionService.ExtractFramesAsync(
                filePath,
                frameFolderPath,
                3);

        ViewBag.FrameUrls =
            extractedFrames
                .Select(path =>
                    "/generated/frames/" +
                    frameFolderName +
                    "/" +
                    Path.GetFileName(path))
                .ToList();

        try
        {
            var analysis =
                await _geminiVideoAnalysisService
                    .AnalyzeVideoAsync(
                        filePath,
                        project.ContentType);

            ViewBag.AiAnalysis = analysis;

            var storyboard =
                await _animeStoryboardService
                    .GenerateStoryboardAsync(
                        analysis,
                        project.OutputType,
                        project.AnimationStyle);

            ViewBag.Storyboard = storyboard;

            var referenceFrames =
                _referenceFrameService.MatchFrames(
                    storyboard,
                    extractedFrames,
                    frameFolderName);

            ViewBag.ReferenceFrames =
                referenceFrames;

            // TEST PHASE:
            // Generate only the strongest/climax shot.
            var climaxShot =
                storyboard.Shots
                    .OrderByDescending(
                        shot => shot.OutputStartSeconds)
                    .Skip(1)
                    .FirstOrDefault();

            if (climaxShot == null)
            {
                climaxShot =
                    storyboard.Shots
                        .OrderByDescending(
                            shot => shot.DurationSeconds)
                        .FirstOrDefault();
            }

            if (climaxShot != null)
            {
                var referenceFrame =
                    referenceFrames.FirstOrDefault(
                        frame =>
                            frame.ShotNumber ==
                            climaxShot.ShotNumber);

                if (referenceFrame != null)
                {
                    var keyframeFolderName =
                        project.Id.ToString();

                    var keyframeFolderPath =
                        Path.Combine(
                            _environment.WebRootPath,
                            "generated",
                            "keyframes",
                            keyframeFolderName);

                    var keyframe =
                        await _animeKeyframeService
                            .GenerateKeyframeAsync(
                                climaxShot,
                                referenceFrame,
                                keyframeFolderPath);

                    keyframe.GeneratedImageUrl =
                        "/generated/keyframes/" +
                        keyframeFolderName +
                        "/" +
                        Path.GetFileName(
                            keyframe.GeneratedImagePath);

                    ViewBag.AnimeKeyframe =
                        keyframe;
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.AiError = ex.Message;
        }

        return View(
            "Uploaded",
            project);
    }
}
