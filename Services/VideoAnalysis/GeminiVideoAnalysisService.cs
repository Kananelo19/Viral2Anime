using System.Net;
using System.Text;
using System.Text.Json;
using Viral2Anime.Models.Video;

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

    public async Task<VideoAnalysisResult> AnalyzeVideoAsync(
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

        var prompt = $$"""
You are the video understanding engine for Viral2Anime.

Analyze the uploaded video carefully.

Content type:
{{contentType}}

Return ONLY valid JSON.

Do not include markdown.
Do not include ```json.
Do not include commentary before or after the JSON.

Use exactly this structure:

{
  "summary": "short factual summary",
  "highlightScore": 0,
  "bestMomentDescription": "description of the strongest moment",
  "bestMomentStartSeconds": 0.0,
  "bestMomentEndSeconds": 0.0,
  "animeConcept": "original cinematic anime transformation concept",
  "moments": [
    {
      "startSeconds": 0.0,
      "endSeconds": 0.0,
      "type": "event type",
      "description": "what visibly happens",
      "importanceScore": 0,
      "animeDirection": "original cinematic treatment"
    }
  ]
}

Rules:

- highlightScore must be from 0 to 100.
- importanceScore must be from 0 to 100.
- Timestamps must be numeric seconds.
- Moments must be chronological.
- Focus on meaningful events.
- Do not invent events that are not visible.
- For sports, identify passes, tackles, dribbles, shots, goals,
  saves, reactions, celebrations, momentum changes and other
  meaningful actions when actually visible.
- Anime directions should describe original techniques such as
  dramatic framing, speed lines, slow motion, impact frames,
  camera movement, lighting, energy effects and reaction shots.
- Do not reference or imitate a named anime, manga, artist,
  studio, franchise or copyrighted visual style.
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
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody);

        var responseJson = await SendWithRetryAsync(
            apiKey,
            requestJson);

        using var responseDocument =
            JsonDocument.Parse(responseJson);

        var generatedText =
            responseDocument.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

        if (string.IsNullOrWhiteSpace(generatedText))
        {
            throw new Exception(
                "Gemini returned an empty analysis.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var analysis =
            JsonSerializer.Deserialize<VideoAnalysisResult>(
                generatedText,
                options);

        if (analysis == null)
        {
            throw new Exception(
                "Gemini analysis could not be parsed.");
        }

        analysis.HighlightScore =
            Math.Clamp(
                analysis.HighlightScore,
                0,
                100);

        foreach (var moment in analysis.Moments)
        {
            moment.ImportanceScore =
                Math.Clamp(
                    moment.ImportanceScore,
                    0,
                    100);
        }

        return analysis;
    }

    private async Task<string> SendWithRetryAsync(
        string apiKey,
        string requestJson)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={apiKey}");

            request.Content = new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

            using var response =
                await _httpClient.SendAsync(request);

            var responseJson =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseJson;
            }

            var retryable =
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.TooManyRequests ||
                response.StatusCode == HttpStatusCode.BadGateway ||
                response.StatusCode == HttpStatusCode.GatewayTimeout;

            if (!retryable || attempt == maxAttempts)
            {
                throw new Exception(
                    $"Gemini request failed after {attempt} attempt(s): " +
                    $"{response.StatusCode}\n{responseJson}");
            }

            var delaySeconds = attempt switch
            {
                1 => 2,
                2 => 4,
                _ => 6
            };

            await Task.Delay(
                TimeSpan.FromSeconds(delaySeconds));
        }

        throw new Exception(
            "Gemini request failed unexpectedly.");
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
