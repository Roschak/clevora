namespace Clevora.Shared.Models;

public record AttachmentDto(
    int Id,
    int TemplateId,
    string FileName,
    long FileSizeBytes,
    string? ContentType,
    string? DownloadUrl,
    DateTime UploadedAt);

public record AttachmentLinkResponse(
    int AttachmentId,
    string FileName,
    string SecureUrl,
    DateTime ExpiresAtUtc);
