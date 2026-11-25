namespace Clevora.Domain.Entities;

public class MessageDraft
{
    public int Id { get; set; }
    public int? TemplateId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    /// <summary>JSON-serialized dictionary of placeholder values: { "name": "Budi" }</summary>
    public string? PlaceholderValues { get; set; }
    public string? RecipientPhone { get; set; }
    public long Version { get; set; } = 1;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public MessageTemplate? Template { get; set; }
}
