using Microsoft.AspNetCore.Mvc;
using ChatbotApi.Models;
using ChatbotApi.Services;

namespace ChatbotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IGeminiService geminiService, ILogger<ChatController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ChatResponse
            {
                Success = false,
                Error = "Message cannot be empty"
            });
        }

        _logger.LogInformation("Received chat request: {Message}", request.Message);

        var response = await _geminiService.SendMessageAsync(request);

        if (!response.Success)
        {
            return StatusCode(500, response);
        }

        return Ok(response);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
