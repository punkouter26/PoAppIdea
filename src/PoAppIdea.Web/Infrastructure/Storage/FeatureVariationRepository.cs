using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IFeatureVariationRepository.
/// </summary>
public sealed class FeatureVariationRepository : IFeatureVariationRepository
{
    private readonly TableStorageClient _tableClient;

    public FeatureVariationRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<FeatureVariation> CreateAsync(FeatureVariation variation, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.FeatureVariations,
            variation.SessionId.ToString(),
            variation.Id.ToString(),
            variation,
            cancellationToken);
    }

    public async Task<IReadOnlyList<FeatureVariation>> CreateBatchAsync(IEnumerable<FeatureVariation> variations, CancellationToken cancellationToken = default)
    {
        var results = new List<FeatureVariation>();
        foreach (var variation in variations)
        {
            var created = await CreateAsync(variation, cancellationToken);
            results.Add(created);
        }
        return results;
    }

    public async Task<FeatureVariation?> GetByIdAsync(Guid variationId, CancellationToken cancellationToken = default)
    {
        var filter = $"RowKey eq '{variationId}'";
        var results = await _tableClient.QueryAsync<FeatureVariation>(AppConstants.TableNames.FeatureVariations, filter, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<FeatureVariation>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.QueryByPartitionKeyAsync<FeatureVariation>(
            AppConstants.TableNames.FeatureVariations,
            sessionId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<FeatureVariation>> GetByMutationIdAsync(Guid mutationId, CancellationToken cancellationToken = default)
    {
        // Query all variations and filter by mutation ID
        var filter = $"PartitionKey ne ''";
        var allVariations = await _tableClient.QueryAsync<FeatureVariation>(AppConstants.TableNames.FeatureVariations, filter, cancellationToken);
        return allVariations.Where(v => v.MutationId == mutationId).ToList();
    }

    public async Task<IReadOnlyList<FeatureVariation>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default)
    {
        var allVariations = await GetBySessionIdAsync(sessionId, cancellationToken);
        return allVariations.OrderByDescending(v => v.Score).Take(count).ToList();
    }

    public async Task<FeatureVariation> UpdateAsync(FeatureVariation variation, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.FeatureVariations,
            variation.SessionId.ToString(),
            variation.Id.ToString(),
            variation,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid variationId, CancellationToken cancellationToken = default)
    {
        var variation = await GetByIdAsync(variationId, cancellationToken);
        if (variation is not null)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.FeatureVariations,
                variation.SessionId.ToString(),
                variationId.ToString(),
                cancellationToken);
        }
    }
}
