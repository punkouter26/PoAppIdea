using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IVisualAssetRepository.
/// Uses TableStorageClient for metadata and BlobStorageClient for image storage.
/// Partition key: SessionId, Row key: AssetId
/// </summary>
public sealed class VisualAssetRepository : IVisualAssetRepository
{
    private readonly TableStorageClient _tableClient;
    private readonly BlobStorageClient _blobClient;

    public VisualAssetRepository(TableStorageClient tableClient, BlobStorageClient blobClient)
    {
        _tableClient = tableClient;
        _blobClient = blobClient;
    }

    public async Task<VisualAsset> CreateAsync(VisualAsset asset, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.VisualAssets,
            asset.SessionId.ToString(),
            asset.Id.ToString(),
            asset,
            cancellationToken);
    }

    public async Task CreateBatchAsync(IEnumerable<VisualAsset> assets, CancellationToken cancellationToken = default)
    {
        foreach (var asset in assets)
        {
            await _tableClient.UpsertAsync(
                AppConstants.TableNames.VisualAssets,
                asset.SessionId.ToString(),
                asset.Id.ToString(),
                asset,
                cancellationToken);
        }
    }

    public async Task<VisualAsset?> GetByIdAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.GetAsync<VisualAsset>(
            AppConstants.TableNames.VisualAssets,
            sessionId.ToString(),
            assetId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<VisualAsset>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var assets = await _tableClient.QueryByPartitionKeyAsync<VisualAsset>(
            AppConstants.TableNames.VisualAssets,
            sessionId.ToString(),
            cancellationToken);

        return assets.OrderBy(a => a.CreatedAt).ToList();
    }

    public async Task<VisualAsset?> GetSelectedAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var assets = await GetBySessionIdAsync(sessionId, cancellationToken);
        return assets.FirstOrDefault(a => a.IsSelected);
    }

    public async Task<VisualAsset> UpdateAsync(VisualAsset asset, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.VisualAssets,
            asset.SessionId.ToString(),
            asset.Id.ToString(),
            asset,
            cancellationToken);
    }

    public async Task SelectAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default)
    {
        var assets = await GetBySessionIdAsync(sessionId, cancellationToken);

        foreach (var asset in assets)
        {
            asset.IsSelected = asset.Id == assetId;
            await UpdateAsync(asset, cancellationToken);
        }
    }

    public async Task<int> CountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var assets = await GetBySessionIdAsync(sessionId, cancellationToken);
        return assets.Count;
    }

    public async Task DeleteAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default)
    {
        await _tableClient.DeleteAsync(
            AppConstants.TableNames.VisualAssets,
            sessionId.ToString(),
            assetId.ToString(),
            cancellationToken);
    }

    /// <summary>
    /// Uploads an image to blob storage and returns the URL.
    /// </summary>
    public async Task<string> UploadImageAsync(Guid assetId, byte[] imageData, CancellationToken cancellationToken = default)
    {
        var blobName = $"{assetId}.png";
        return await _blobClient.UploadAsync(
            AppConstants.ContainerNames.VisualAssets,
            blobName,
            imageData,
            "image/png",
            cancellationToken);
    }

    /// <summary>
    /// Uploads a thumbnail to blob storage and returns the URL.
    /// </summary>
    public async Task<string> UploadThumbnailAsync(Guid assetId, byte[] imageData, CancellationToken cancellationToken = default)
    {
        var blobName = $"{assetId}_thumb.png";
        return await _blobClient.UploadAsync(
            AppConstants.ContainerNames.VisualAssets,
            blobName,
            imageData,
            "image/png",
            cancellationToken);
    }
}
