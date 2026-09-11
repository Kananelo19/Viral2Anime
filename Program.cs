var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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
