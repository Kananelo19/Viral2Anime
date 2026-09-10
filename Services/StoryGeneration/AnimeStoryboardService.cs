using System.Text;
using System.Text.Json;
using Viral2Anime.Models.Story;
using Viral2Anime.Models.Video;

namespace Viral2Anime.Services.StoryGeneration;

public class AnimeStoryboardService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AnimeStoryboardService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AnimeStoryboard> GenerateStoryboardAsync(
        VideoAnalysisResult analysis,
        string outputType,
        string animationStyle)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("Gemini API key is missing.");
        }

        var analysisJson = JsonSerializer.Serialize(
            analysis,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var prompt = $$"""
You are the storyboard generation engine for Viral2Anime.

Your job is to transform a factual video highlight analysis
into an ORIGINAL cinematic anime storyboard.

Output type:
{{outputType}}

Animation style:
{{animationStyle}}

Video analysis:
{{analysisJson}}

Return ONLY valid JSON.

Do not include markdown.
Do not include commentary.
Do not use ```json.

Use exactly this structure:

{
  "title": "short storyboard title",
  "concept": "overall cinematic concept",
  "totalDurationSeconds": 0.0,
  "shots": [
    {
      "shotNumber": 1,
      "title": "shot title",
      "startSeconds": 0.0,
      "endSeconds": 0.0,
      "durationSeconds": 0.0,
      "cameraDirection": "camera instructions",
      "sceneDescription": "what the scene visually contains",
      "visualEffects": "visual treatment",
      "characterAction": "what characters are doing",
      "generationPrompt": "complete prompt for generating this shot"
    }
  ]
}

Rules:

- Create a clear beginning, build-up, climax and reaction.
- Focus heavily on the strongest highlight.
- Keep shots chronological.
- Use the source events as factual grounding.
- Do not invent a goal if the source says the shot missed.
- Do not invent players, teams or actions not supported by the analysis.
- You may dramatize presentation, camera movement, lighting,
  energy effects and timing.
- Make the sequence feel exciting and cinematic.
- Each generationPrompt must work as a standalone visual prompt.
- Preserve continuity between shots.
- Describe clothing, field position, ball position and action
  consistently when possible.
- Use original anime-inspired cinematic language.
- Do not name or imitate any existing anime, manga, artist,
  studio, franchise or copyrighted visual style.
- Use speed lines, impact frames, dramatic perspective,
  atmospheric lighting, motion blur, energy trails,
  reaction shots and slow motion where appropriate.
""";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
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
                "Gemini returned an empty storyboard.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var storyboard =
            JsonSerializer.Deserialize<AnimeStoryboard>(
                generatedText,
                options);

        if (storyboard == null)
        {
            throw new Exception(
                "Storyboard could not be parsed.");
        }

        storyboard.Shots =
            storyboard.Shots
                .OrderBy(s => s.ShotNumber)
                .ToList();

        return storyboard;
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
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout;

            if (!retryable || attempt == maxAttempts)
            {
                throw new Exception(
                    $"Storyboard request failed after {attempt} attempt(s): " +
                    $"{response.StatusCode}\n{responseJson}");
            }

            var delaySeconds = attempt == 1 ? 2 : 4;

            await Task.Delay(
                TimeSpan.FromSeconds(delaySeconds));
        }

        throw new Exception(
            "Storyboard request failed unexpectedly.");
    }
}
