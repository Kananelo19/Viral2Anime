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

        var sourceDuration =
            analysis.Moments.Count > 0
                ? analysis.Moments.Max(m => m.EndSeconds)
                : analysis.BestMomentEndSeconds;

        var analysisJson = JsonSerializer.Serialize(
            analysis,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var prompt = $$"""
You are the storyboard generation engine for Viral2Anime.

Transform the factual source-video analysis into an ORIGINAL
cinematic anime storyboard.

Output type:
{{outputType}}

Animation style:
{{animationStyle}}

Approximate source-video duration:
{{sourceDuration}} seconds

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

      "sourceStartSeconds": 0.0,
      "sourceEndSeconds": 0.0,
      "referenceFrameSeconds": 0.0,

      "outputStartSeconds": 0.0,
      "outputEndSeconds": 0.0,
      "durationSeconds": 0.0,

      "cameraDirection": "camera instructions",
      "sceneDescription": "what the scene visually contains",
      "visualEffects": "visual treatment",
      "characterAction": "what characters are doing",
      "generationPrompt": "complete prompt for generating this shot"
    }
  ]
}

CRITICAL TIMELINE RULES:

- sourceStartSeconds and sourceEndSeconds refer ONLY to the
  ORIGINAL uploaded source video.
- sourceStartSeconds and sourceEndSeconds MUST remain inside
  the actual source-video timeline.
- Never create source timestamps beyond the source video.
- referenceFrameSeconds must fall between sourceStartSeconds
  and sourceEndSeconds.
- referenceFrameSeconds should represent the most visually
  useful source frame for that anime shot.

- outputStartSeconds and outputEndSeconds refer to the NEW
  anime video's timeline.
- The output timeline may be longer than the source timeline.
- It is acceptable to expand a short real-world action into
  several seconds of dramatic anime presentation.
- durationSeconds must equal:
  outputEndSeconds - outputStartSeconds.

CONTENT RULES:

- Create a clear beginning, build-up, climax and reaction.
- Focus heavily on the strongest real highlight.
- Keep events grounded in the factual video analysis.
- Do not invent goals, saves, collisions or actions that the
  analysis does not support.
- If the shot missed, preserve the miss.
- Preserve visible team/player details when known.
- Do not invent jersey colors when the analysis does not
  establish them.
- When visual appearance is uncertain, tell the image
  generator to preserve appearance from the source
  reference frame instead of guessing.
- Generation prompts should explicitly say to preserve
  clothing colors, player placement and environment from
  the supplied source reference image.
- You may dramatize framing, lighting, timing, camera
  movement, motion, energy effects and reactions.
- Maintain continuity between shots.
- Use original anime-inspired cinematic language.
- Do not name or imitate an existing anime, manga, artist,
  studio or franchise.
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

        var requestJson =
            JsonSerializer.Serialize(requestBody);

        var responseJson =
            await SendWithRetryAsync(
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

        foreach (var shot in storyboard.Shots)
        {
            shot.SourceStartSeconds =
                Math.Clamp(
                    shot.SourceStartSeconds,
                    0,
                    sourceDuration);

            shot.SourceEndSeconds =
                Math.Clamp(
                    shot.SourceEndSeconds,
                    shot.SourceStartSeconds,
                    sourceDuration);

            shot.ReferenceFrameSeconds =
                Math.Clamp(
                    shot.ReferenceFrameSeconds,
                    shot.SourceStartSeconds,
                    shot.SourceEndSeconds);
        }

        return storyboard;
    }

    private async Task<string> SendWithRetryAsync(
        string apiKey,
        string requestJson)
    {
        const int maxAttempts = 3;

        for (var attempt = 1;
             attempt <= maxAttempts;
             attempt++)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={apiKey}");

            request.Content =
                new StringContent(
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
                response.StatusCode ==
                    System.Net.HttpStatusCode.ServiceUnavailable ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.BadGateway ||
                response.StatusCode ==
                    System.Net.HttpStatusCode.GatewayTimeout;

            if (!retryable ||
                attempt == maxAttempts)
            {
                throw new Exception(
                    $"Storyboard request failed after {attempt} attempt(s): " +
                    $"{response.StatusCode}\n{responseJson}");
            }

            var delaySeconds =
                attempt == 1 ? 2 : 4;

            await Task.Delay(
                TimeSpan.FromSeconds(delaySeconds));
        }

        throw new Exception(
            "Storyboard request failed unexpectedly.");
    }
}
