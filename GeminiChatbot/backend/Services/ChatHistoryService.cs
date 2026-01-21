using Microsoft.EntityFrameworkCore;
using ChatbotApi.Data;
using ChatbotApi.Models.Entities;

namespace ChatbotApi.Services;

public interface IChatHistoryService
{
    Task<ChatSession> CreateSessionAsync(int userId, string? title = null);
    Task<ChatSession?> GetSessionAsync(int sessionId, int userId);
    Task<List<ChatSessionDto>> GetUserSessionsAsync(int userId);
    Task<List<ChatMessageDto>> GetSessionMessagesAsync(int sessionId, int userId);
    Task<ChatMessageEntity> AddMessageAsync(int sessionId, string role, string content);
    Task<bool> UpdateSessionTitleAsync(int sessionId, int userId, string title);
    Task<bool> DeleteSessionAsync(int sessionId, int userId);
    Task<string> GenerateSessionTitleAsync(string firstMessage);
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatHistoryService> _logger;

    public ChatHistoryService(ApplicationDbContext context, ILogger<ChatHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatSession> CreateSessionAsync(int userId, string? title = null)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = title ?? "New Chat",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(int sessionId, int userId)
    {
        return await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<List<ChatSessionDto>> GetUserSessionsAsync(int userId)
    {
        return await _context.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new ChatSessionDto
            {
                Id = s.Id,
                Title = s.Title ?? "New Chat",
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                MessageCount = s.Messages.Count
            })
            .ToListAsync();
    }

    public async Task<List<ChatMessageDto>> GetSessionMessagesAsync(int sessionId, int userId)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return new List<ChatMessageDto>();
        }

        return session.Messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            Role = m.Role,
            Content = m.Content,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<ChatMessageEntity> AddMessageAsync(int sessionId, string role, string content)
    {
        var message = new ChatMessageEntity
        {
            ChatSessionId = sessionId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);

        // Update session's UpdatedAt
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;

            // Auto-generate title from first user message if title is default
            if (role == "user" && (session.Title == "New Chat" || string.IsNullOrEmpty(session.Title)))
            {
                session.Title = await GenerateSessionTitleAsync(content);
            }
        }

        await _context.SaveChangesAsync();

        return message;
    }

    public async Task<string> GenerateSessionTitleAsync(string firstMessage)
    {
        // Generate a smart title from the first message
        // Remove extra whitespace and newlines
        var cleaned = string.Join(" ", firstMessage.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries));

        // If message is short enough, use it as is
        if (cleaned.Length <= 40)
        {
            return cleaned;
        }

        // Find a good breaking point (end of word)
        var truncated = cleaned.Substring(0, 40);
        var lastSpace = truncated.LastIndexOf(' ');

        if (lastSpace > 20)
        {
            return truncated.Substring(0, lastSpace) + "...";
        }

        return truncated + "...";
    }

    public async Task<bool> UpdateSessionTitleAsync(int sessionId, int userId, string title)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return false;
        }

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, int userId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return false;
        }

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }
}

// DTOs
public class ChatSessionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
