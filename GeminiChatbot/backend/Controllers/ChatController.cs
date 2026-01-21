using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatbotApi.Models;
using ChatbotApi.Services;

namespace ChatbotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IChatHistoryService _chatHistoryService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGeminiService geminiService, IChatHistoryService chatHistoryService, ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _chatHistoryService = chatHistoryService;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ChatResponseWithSession>> Chat([FromBody] ChatRequestWithSession request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ChatResponseWithSession
            {
                Success = false,
                Error = "Message cannot be empty"
            });
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized(new ChatResponseWithSession
            {
                Success = false,
                Error = "User not authenticated"
            });
        }

        _logger.LogInformation("Received chat request from user {UserId}: {Message}", userId, request.Message);

        // Create or get session
        int sessionId;
        if (request.SessionId.HasValue)
        {
            var session = await _chatHistoryService.GetSessionAsync(request.SessionId.Value, userId.Value);
            if (session == null)
            {
                return NotFound(new ChatResponseWithSession
                {
                    Success = false,
                    Error = "Chat session not found"
                });
            }
            sessionId = session.Id;
        }
        else
        {
            var newSession = await _chatHistoryService.CreateSessionAsync(userId.Value);
            sessionId = newSession.Id;
        }

        // Save user message
        await _chatHistoryService.AddMessageAsync(sessionId, "user", request.Message);

        // Get response from Gemini
        var chatRequest = new ChatRequest
        {
            Message = request.Message,
            History = request.History
        };

        var response = await _geminiService.SendMessageAsync(chatRequest);

        if (!response.Success)
        {
            return StatusCode(500, new ChatResponseWithSession
            {
                Success = false,
                Error = response.Error,
                SessionId = sessionId
            });
        }

        // Save assistant response
        await _chatHistoryService.AddMessageAsync(sessionId, "assistant", response.Response);

        return Ok(new ChatResponseWithSession
        {
            Success = true,
            Response = response.Response,
            SessionId = sessionId
        });
    }

    // Get all sessions for the current user
    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<ChatSessionDto>>> GetSessions()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var sessions = await _chatHistoryService.GetUserSessionsAsync(userId.Value);
        return Ok(sessions);
    }

    // Get messages for a specific session
    [HttpGet("sessions/{sessionId}/messages")]
    [Authorize]
    public async Task<ActionResult<List<ChatMessageDto>>> GetSessionMessages(int sessionId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var messages = await _chatHistoryService.GetSessionMessagesAsync(sessionId, userId.Value);
        return Ok(messages);
    }

    // Create a new session
    [HttpPost("sessions")]
    [Authorize]
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequest? request)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var session = await _chatHistoryService.CreateSessionAsync(userId.Value, request?.Title);
        return Ok(new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title ?? "New Chat",
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = 0
        });
    }

    // Delete a session
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<ActionResult> DeleteSession(int sessionId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        await _chatHistoryService.DeleteSessionAsync(sessionId, userId.Value);
        return NoContent();
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

// Extended models
public class ChatRequestWithSession
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage>? History { get; set; }
    public int? SessionId { get; set; }
}

public class ChatResponseWithSession
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int? SessionId { get; set; }
}

public class CreateSessionRequest
{
    public string? Title { get; set; }
}
