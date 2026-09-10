using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Viral2Anime.Services.VideoAnalysis;

public class GeminiVideoAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiVideoAnalysisService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> AnalyzeVideoAsync(
        string videoPath,
        string contentType = "Football Match")
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Gemini API key is missing.");
        }

        var videoBytes = await File.ReadAllBytesAsync(videoPath);
        var base64Video = Convert.ToBase64String(videoBytes);

        var mimeType = GetMimeType(videoPath);

        var prompt = $"""
You are the video analysis engine for Viral2Anime.

Analyze this uploaded video carefully.

Content type:
{contentType}

Return a concise but useful analysis containing:

1. What is happening in the video.
2. The important moments in chronological order.
3. Approximate timestamps for each important moment.
4. The most exciting or meaningful moment.
5. A highlight score from 0 to 100.
6. A short explanation of why that moment matters.
7. How the scene could be transformed into a cinematic anime sequence.

For sports footage, pay attention to:
- attacking moves
- defending
- shots
- saves
- goals
- tackles
- passes
- celebrations
- momentum changes
- player reactions
- crowd or sideline reactions

Do not invent events that are not visible.
""";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mimeType,
                                data = base64Video
                            }
                        },
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={apiKey}");

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Gemini request failed: {response.StatusCode}\n{responseJson}");
        }

        using var document = JsonDocument.Parse(responseJson);

        var root = document.RootElement;

        var text =
            root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

        return text ?? "No analysis returned.";
    }

    private static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".m4v" => "video/x-m4v",
            ".webm" => "video/webm",
            ".mpeg" => "video/mpeg",
            ".mpg" => "video/mpeg",
            _ => "video/mp4"
        };
    }
}
