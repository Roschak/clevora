using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Clevora.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Clevora.Infrastructure.Services;

public class AzureBlobStorageService(IConfiguration config) : IBlobStorageService
{
    private BlobServiceClient CreateClient() =>
        new(config["AzureBlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureBlobStorage:ConnectionString not configured."));

    public async Task<BlobUploadResult> UploadAsync(
        Stream content,
        string fileName,
        string containerName,
        CancellationToken ct = default)
    {
        var serviceClient = CreateClient();
        var container = serviceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid():N}_{fileName}";
        var blobClient = container.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders
        {
            ContentDisposition = $"attachment; filename=\"{fileName}\""
        };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers }, ct);
        var props = await blobClient.GetPropertiesAsync(cancellationToken: ct);
        var downloadUrl = await GetDownloadUrlAsync(blobName, containerName, TimeSpan.FromHours(1), ct);

        return new BlobUploadResult(blobName, containerName, props.Value.ContentLength, downloadUrl);
    }

    public async Task<IReadOnlyList<BlobFileInfo>> ListAsync(string containerName, CancellationToken ct = default)
    {
        var container = CreateClient().GetBlobContainerClient(containerName);
        var items = new List<BlobFileInfo>();

        await foreach (var blob in container.GetBlobsAsync(cancellationToken: ct))
        {
            var parts = blob.Name.Split('_', 2);
            var friendlyName = parts.Length == 2 ? parts[1] : blob.Name;
            items.Add(new BlobFileInfo(
                blob.Name,
                containerName,
                friendlyName,
                blob.Properties.ContentLength ?? 0,
                blob.Properties.LastModified));
        }
        return items;
    }

    public async Task DeleteAsync(string blobName, string containerName, CancellationToken ct = default)
    {
        var blob = CreateClient().GetBlobContainerClient(containerName).GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public Task<string> GetDownloadUrlAsync(
        string blobName,
        string containerName,
        TimeSpan expiry,
        CancellationToken ct = default)
    {
        var serviceClient = CreateClient();
        var container = serviceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
            return Task.FromResult(blobClient.Uri.ToString());

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sasUri.ToString());
    }
}
