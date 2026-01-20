namespace ChatbotApi.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage>? History { get; set; }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}

// Gemini API Models
public class GeminiRequest
{
    public List<GeminiContent> Contents { get; set; } = new();
}

public class GeminiContent
{
    public string Role { get; set; } = string.Empty;
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    public string Text { get; set; } = string.Empty;
}

public class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    public GeminiContent? Content { get; set; }
}
