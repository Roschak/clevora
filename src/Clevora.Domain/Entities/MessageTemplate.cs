namespace Clevora.Domain.Entities;

public class MessageTemplate
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TemplateCategory? Category { get; set; }
    public ICollection<AttachmentItem> Attachments { get; set; } = new List<AttachmentItem>();
}
