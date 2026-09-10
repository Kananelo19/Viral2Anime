using Microsoft.AspNetCore.Mvc;
using Viral2Anime.Models.Video;
using Viral2Anime.Services.VideoAnalysis;

namespace Viral2Anime.Controllers;

public class CreateController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly VideoMetadataService _metadataService;

    public CreateController(
        IWebHostEnvironment environment,
        VideoMetadataService metadataService)
    {
        _environment = environment;
        _metadataService = metadataService;
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

        var storedFileName =
            $"{Guid.NewGuid()}{extension}";

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

        return View("Uploaded", project);
    }
}
