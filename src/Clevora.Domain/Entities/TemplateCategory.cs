namespace Clevora.Domain.Entities;

public class TemplateCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MessageTemplate> Templates { get; set; } = new List<MessageTemplate>();
}
