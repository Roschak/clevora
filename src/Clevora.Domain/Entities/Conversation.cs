namespace Clevora.Domain.Entities;

public class Conversation
{
    public int Id { get; set; }
    public string? Name { get; set; } // null = 1-on-1 chat
    public bool IsGroup { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
