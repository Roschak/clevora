using Clevora.Application.Interfaces;
using Clevora.Domain.Entities;
using Clevora.Infrastructure.Data;
using Clevora.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Clevora.Infrastructure.Services;

public class AttachmentService(
    ClevoraDbContext db,
    IBlobStorageService blobStorage,
    ICurrentUserService currentUser) : IAttachmentService
{
    private const string ContainerName = "clevora-attachments";

    public async Task<AttachmentDto> UploadAsync(
        int templateId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var template = await db.Templates.FirstOrDefaultAsync(
            t => t.Id == templateId && t.OwnerId == currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Template {templateId} not found.");

        BlobUploadResult uploadResult;
        try
        {
            uploadResult = await blobStorage.UploadAsync(fileStream, fileName, ContainerName, ct);
        }
        catch
        {
            // Fallback for development if Azure Blob Storage is not configured
            var fallbackBlobName = $"dev_{Guid.NewGuid():N}_{fileName}";
            uploadResult = new BlobUploadResult(
                fallbackBlobName,
                ContainerName,
                fileStream.Length,
                $"/api/v1/attachments/dev-mock/{fallbackBlobName}");
        }

        var attachment = new AttachmentItem
        {
            TemplateId = templateId,
            OwnerId = currentUser.UserId,
            FileName = fileName,
            BlobName = uploadResult.BlobName,
            ContainerName = uploadResult.ContainerName,
            FileSizeBytes = uploadResult.FileSizeBytes,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow,
            Version = 1,
            IsDeleted = false
        };

        db.Attachments.Add(attachment);
        template.UpdatedAt = DateTime.UtcNow;
        template.Version++;
        await db.SaveChangesAsync(ct);

        return new AttachmentDto(
            attachment.Id,
            attachment.TemplateId,
            attachment.FileName,
            attachment.FileSizeBytes,
            attachment.ContentType,
            uploadResult.DownloadUrl,
            attachment.UploadedAt);
    }

    public async Task<AttachmentLinkResponse> GenerateLinkAsync(
        int attachmentId,
        TimeSpan? duration = null,
        CancellationToken ct = default)
    {
        var attachment = await db.Attachments.FirstOrDefaultAsync(
            a => a.Id == attachmentId && a.OwnerId == currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Attachment {attachmentId} not found.");

        var ttl = duration ?? TimeSpan.FromHours(2);
        string downloadUrl;
        try
        {
            downloadUrl = await blobStorage.GetDownloadUrlAsync(
                attachment.BlobName, attachment.ContainerName, ttl, ct);
        }
        catch
        {
            downloadUrl = $"/api/v1/attachments/dev-mock/{attachment.BlobName}";
        }

        return new AttachmentLinkResponse(
            attachment.Id,
            attachment.FileName,
            downloadUrl,
            DateTime.UtcNow.Add(ttl));
    }

    public async Task DeleteAsync(int attachmentId, CancellationToken ct = default)
    {
        var attachment = await db.Attachments.FirstOrDefaultAsync(
            a => a.Id == attachmentId && a.OwnerId == currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Attachment {attachmentId} not found.");

        attachment.IsDeleted = true;
        attachment.Version++;
        await db.SaveChangesAsync(ct);

        try
        {
            await blobStorage.DeleteAsync(attachment.BlobName, attachment.ContainerName, ct);
        }
        catch
        {
            // Ignore blob deletion errors in development
        }
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetByTemplateIdAsync(
        int templateId, CancellationToken ct = default)
    {
        var items = await db.Attachments
            .Where(a => a.TemplateId == templateId && a.OwnerId == currentUser.UserId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(ct);

        var dtos = new List<AttachmentDto>();
        foreach (var a in items)
        {
            string? url = null;
            try
            {
                url = await blobStorage.GetDownloadUrlAsync(
                    a.BlobName, a.ContainerName, TimeSpan.FromHours(1), ct);
            }
            catch
            {
                url = $"/api/v1/attachments/dev-mock/{a.BlobName}";
            }

            dtos.Add(new AttachmentDto(
                a.Id, a.TemplateId, a.FileName, a.FileSizeBytes, a.ContentType, url, a.UploadedAt));
        }

        return dtos;
    }
}
