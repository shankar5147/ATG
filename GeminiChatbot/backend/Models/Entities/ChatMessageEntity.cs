using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatbotApi.Models.Entities;

public class ChatMessageEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChatSessionId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("ChatSessionId")]
    public virtual ChatSession ChatSession { get; set; } = null!;
}
