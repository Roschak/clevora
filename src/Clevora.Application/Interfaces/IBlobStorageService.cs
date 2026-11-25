namespace Clevora.Application.Interfaces;

public record BlobUploadResult(string BlobName, string ContainerName, long FileSizeBytes, string DownloadUrl);
public record BlobFileInfo(string BlobName, string ContainerName, string FileName, long FileSizeBytes, DateTimeOffset? LastModified);

public interface IBlobStorageService
{
    Task<BlobUploadResult> UploadAsync(Stream content, string fileName, string containerName, CancellationToken ct = default);
    Task<IReadOnlyList<BlobFileInfo>> ListAsync(string containerName, CancellationToken ct = default);
    Task DeleteAsync(string blobName, string containerName, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(string blobName, string containerName, TimeSpan expiry, CancellationToken ct = default);
}
