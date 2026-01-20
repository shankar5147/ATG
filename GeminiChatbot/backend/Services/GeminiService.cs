using System.Text;
using System.Text.Json;
using ChatbotApi.Models;

namespace ChatbotApi.Services;

public interface IGeminiService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request);
}

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = "Gemini API key is not configured. Please set 'Gemini:ApiKey' in appsettings.json"
                };
            }

            var geminiRequest = BuildGeminiRequest(request);
            var jsonContent = JsonSerializer.Serialize(geminiRequest, _jsonOptions);

            _logger.LogInformation("Sending request to Gemini API");

            // Using gemini-1.5-flash - you can also try: gemini-pro, gemini-1.5-pro
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={apiKey}";

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(jsonContent, Encoding.UTF8, "application/json")
            );

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, responseContent);

                // Parse error message from response if possible
                var errorMsg = $"Gemini API error: {response.StatusCode}";
                if ((int)response.StatusCode == 429)
                {
                    errorMsg = "Rate limit exceeded. Please wait a moment and try again.";
                }

                return new ChatResponse
                {
                    Success = false,
                    Error = errorMsg
                };
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, _jsonOptions);

            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(text))
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = "No response received from Gemini"
                };
            }

            return new ChatResponse
            {
                Success = true,
                Response = text
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with Gemini API");
            return new ChatResponse
            {
                Success = false,
                Error = $"Error: {ex.Message}"
            };
        }
    }

    private GeminiRequest BuildGeminiRequest(ChatRequest request)
    {
        var geminiRequest = new GeminiRequest();

        // Add conversation history if provided
        if (request.History != null && request.History.Count > 0)
        {
            foreach (var message in request.History)
            {
                geminiRequest.Contents.Add(new GeminiContent
                {
                    Role = message.Role == "user" ? "user" : "model",
                    Parts = new List<GeminiPart> { new GeminiPart { Text = message.Content } }
                });
            }
        }

        // Add current message
        geminiRequest.Contents.Add(new GeminiContent
        {
            Role = "user",
            Parts = new List<GeminiPart> { new GeminiPart { Text = request.Message } }
        });

        return geminiRequest;
    }
}
