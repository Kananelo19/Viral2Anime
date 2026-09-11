using Microsoft.EntityFrameworkCore;
using Viral2Anime.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));

builder.Services.AddScoped<
    Viral2Anime.Services.VideoAnalysis.VideoMetadataService>();

builder.Services.AddScoped<
    Viral2Anime.Services.VideoAnalysis.FrameExtractionService>();

builder.Services.AddScoped<
    Viral2Anime.Services.StoryGeneration.ReferenceFrameService>();

builder.Services.AddHttpClient<
    Viral2Anime.Services.VideoAnalysis.GeminiVideoAnalysisService>();

builder.Services.AddHttpClient<
    Viral2Anime.Services.StoryGeneration.AnimeStoryboardService>();

builder.Services.AddScoped<
    Viral2Anime.Services.Animation.AnimeKeyframeService>();


builder.Services.AddScoped<
    Viral2Anime.Services.Animation.AnimeShotAnimationService>();


builder.Services.AddScoped<
    Viral2Anime.Services.Animation.FinalVideoAssemblyService>();


builder.Services.AddSingleton<
    Viral2Anime.Services.ProjectState.ProjectProcessingStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
