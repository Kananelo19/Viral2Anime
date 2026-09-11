using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Viral2Anime.Data;
using Viral2Anime.Models.Project;
using Viral2Anime.Models.Story;
using Viral2Anime.Models.Video;

namespace Viral2Anime.Controllers;

public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ProjectsController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects =
            await _dbContext.ViralProjects
                .OrderByDescending(
                    project => project.UpdatedAt)
                .ToListAsync();

        return View(projects);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var dbProject =
            await _dbContext.ViralProjects
                .FirstOrDefaultAsync(
                    project =>
                        project.Id == id);

        if (dbProject == null)
        {
            return NotFound();
        }

        var videoProject =
            new VideoProject
            {
                Id = dbProject.Id,
                Title = dbProject.Title,
                OriginalFileName = dbProject.OriginalFileName,
                StoredFileName = dbProject.StoredFileName,
                ContentType = dbProject.ContentType,
                OutputType = dbProject.OutputType,
                AnimationStyle = dbProject.AnimationStyle,

                AudioMode =
                    dbProject.AudioMode,

                AspectRatio =
                    dbProject.AspectRatio,

                OutputResolution =
                    dbProject.OutputResolution,

                TargetDurationSeconds =
                    dbProject.TargetDurationSeconds,
                DurationSeconds = dbProject.DurationSeconds,
                Width = dbProject.Width,
                Height = dbProject.Height,
                FramesPerSecond = dbProject.FramesPerSecond,
                HasAudio = dbProject.HasAudio,
                FormatName = dbProject.FormatName,
                CreatedAt = dbProject.CreatedAt
            };

        var options =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        var analysis =
            DeserializeOrDefault<VideoAnalysisResult>(
                dbProject.AnalysisJson,
                options);

        var storyboard =
            DeserializeOrDefault<AnimeStoryboard>(
                dbProject.StoryboardJson,
                options);

        var referenceFrames =
            DeserializeList<StoryboardReferenceFrame>(
                dbProject.ReferenceFramesJson,
                options);

        var animeKeyframes =
            DeserializeList<AnimeKeyframe>(
                dbProject.GeneratedKeyframesJson,
                options);

        var animatedClips =
            DeserializeList<AnimeShotClip>(
                dbProject.AnimatedClipsJson,
                options);

        ViewBag.Analysis = analysis;
        ViewBag.Storyboard = storyboard;
        ViewBag.ReferenceFrames = referenceFrames;
        ViewBag.AnimeKeyframes = animeKeyframes;
        ViewBag.AnimatedClips = animatedClips;
        ViewBag.ProjectStage = dbProject.Stage;
        ViewBag.AutoGenerateKeyframes = false;
        ViewBag.FrameUrls = new List<string>();

        ViewBag.FinalVideoUrl =
            string.IsNullOrWhiteSpace(
                dbProject.FinalVideoFileName)
                ? string.Empty
                : "/generated/final/" +
                  dbProject.Id +
                  "/" +
                  dbProject.FinalVideoFileName;

        return View(
            "~/Views/Create/Uploaded.cshtml",
            videoProject);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var project =
            await _dbContext.ViralProjects
                .FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        DeleteProjectFiles(project);

        _dbContext.ViralProjects.Remove(project);

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var source =
            await _dbContext.ViralProjects
                .FindAsync(id);

        if (source == null)
        {
            return NotFound();
        }

        var copy =
            new ViralProject
            {
                Id = Guid.NewGuid(),

                Title =
                    source.Title + " Copy",

                OriginalFileName =
                    source.OriginalFileName,

                StoredFileName =
                    source.StoredFileName,

                ContentType =
                    source.ContentType,

                OutputType =
                    source.OutputType,

                AnimationStyle =
                    source.AnimationStyle,

                Stage =
                    ProjectStage.ReferencesReady,

                DurationSeconds =
                    source.DurationSeconds,

                Width =
                    source.Width,

                Height =
                    source.Height,

                FramesPerSecond =
                    source.FramesPerSecond,

                HasAudio =
                    source.HasAudio,

                FormatName =
                    source.FormatName,

                AnalysisJson =
                    source.AnalysisJson,

                StoryboardJson =
                    source.StoryboardJson,

                ReferenceFramesJson =
                    source.ReferenceFramesJson,

                GeneratedKeyframesJson =
                    string.Empty,

                AnimatedClipsJson =
                    string.Empty,

                FinalVideoFileName =
                    string.Empty,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

        _dbContext.ViralProjects.Add(copy);

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(
            nameof(Details),
            new
            {
                id = copy.Id
            });
    }

    private void DeleteProjectFiles(
        ViralProject project)
    {
        var generatedRoot =
            Path.Combine(
                _environment.WebRootPath,
                "generated");

        var folders =
            new[]
            {
                Path.Combine(
                    generatedRoot,
                    "references",
                    project.Id.ToString()),

                Path.Combine(
                    generatedRoot,
                    "keyframes",
                    project.Id.ToString()),

                Path.Combine(
                    generatedRoot,
                    "clips",
                    project.Id.ToString()),

                Path.Combine(
                    generatedRoot,
                    "final",
                    project.Id.ToString())
            };

        foreach (var folder in folders)
        {
            if (Directory.Exists(folder))
            {
                try
                {
                    Directory.Delete(
                        folder,
                        recursive: true);
                }
                catch
                {
                    // File cleanup should not block
                    // database deletion.
                }
            }
        }
    }

    private static T DeserializeOrDefault<T>(
        string json,
        JsonSerializerOptions options)
        where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(
                json,
                options)
                ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    private static List<T> DeserializeList<T>(
        string json,
        JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(
                json,
                options)
                ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }
}
