using Clevora.Shared.Models;

namespace Clevora.Application.Interfaces;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadAsync(int templateId, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<AttachmentLinkResponse> GenerateLinkAsync(int attachmentId, TimeSpan? duration = null, CancellationToken ct = default);
    Task DeleteAsync(int attachmentId, CancellationToken ct = default);
    Task<IReadOnlyList<AttachmentDto>> GetByTemplateIdAsync(int templateId, CancellationToken ct = default);
}
