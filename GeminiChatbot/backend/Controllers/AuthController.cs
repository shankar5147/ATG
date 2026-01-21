using Microsoft.AspNetCore.Mvc;
using ChatbotApi.Services;

namespace ChatbotApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Error = "Name, email, and password are required"
            });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Error = "Password must be at least 6 characters"
            });
        }

        _logger.LogInformation("Registration attempt for: {Email}", request.Email);

        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                Error = "Email and password are required"
            });
        }

        _logger.LogInformation("Login attempt for: {Email}", request.Email);

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpGet("validate")]
    public async Task<ActionResult<AuthResult>> ValidateToken()
    {
        var authHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new AuthResult
            {
                Success = false,
                Error = "No valid token provided"
            });
        }

        var token = authHeader.Substring("Bearer ".Length);
        var user = await _authService.ValidateTokenAsync(token);

        if (user == null)
        {
            return Unauthorized(new AuthResult
            {
                Success = false,
                Error = "Invalid or expired token"
            });
        }

        return Ok(new AuthResult
        {
            Success = true,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            }
        });
    }
}
