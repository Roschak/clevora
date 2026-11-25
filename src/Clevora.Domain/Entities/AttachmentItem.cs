namespace Clevora.Domain.Entities;

public class AttachmentItem
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ContentType { get; set; }
    public long Version { get; set; } = 1;
    public bool IsDeleted { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public MessageTemplate? Template { get; set; }
}
