using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Client for Azure Blob Storage operations.
/// </summary>
public sealed class BlobStorageClient
{
    private readonly BlobServiceClient _serviceClient;

    public BlobStorageClient(BlobServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    /// <summary>
    /// Gets or creates a container client.
    /// </summary>
    public async Task<BlobContainerClient> GetContainerClientAsync(string containerName, CancellationToken cancellationToken = default)
    {
        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }

    /// <summary>
    /// Uploads a blob from a stream.
    /// </summary>
    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, options, cancellationToken);
        return blobClient.Uri.ToString();
    }

    /// <summary>
    /// Uploads a blob from bytes.
    /// </summary>
    public async Task<string> UploadAsync(string containerName, string blobName, byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(content);
        return await UploadAsync(containerName, blobName, stream, contentType, cancellationToken);
    }

    /// <summary>
    /// Downloads a blob to a stream.
    /// </summary>
    public async Task<Stream?> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var response = await blobClient.DownloadAsync(cancellationToken);
        return response.Value.Content;
    }

    /// <summary>
    /// Gets a SAS URL for a blob with read access.
    /// </summary>
    public async Task<string?> GetSasUrlAsync(string containerName, string blobName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var sasUri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return sasUri.ToString();
    }

    /// <summary>
    /// Deletes a blob.
    /// </summary>
    public async Task DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Checks if a blob exists.
    /// </summary>
    public async Task<bool> ExistsAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }
}
