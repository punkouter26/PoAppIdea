using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of ISynthesisRepository.
/// Partition key: SessionId, Row key: SynthesisId
/// </summary>
public sealed class SynthesisRepository : ISynthesisRepository
{
    private readonly TableStorageClient _tableClient;

    public SynthesisRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Synthesis> CreateAsync(Synthesis synthesis, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Syntheses,
            synthesis.SessionId.ToString(),
            synthesis.Id.ToString(),
            synthesis,
            cancellationToken);
    }

    public async Task<Synthesis?> GetByIdAsync(Guid synthesisId, CancellationToken cancellationToken = default)
    {
        var filter = $"RowKey eq '{synthesisId}'";
        var results = await _tableClient.QueryAsync<Synthesis>(
            AppConstants.TableNames.Syntheses, 
            filter, 
            cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<Synthesis?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var results = await _tableClient.QueryByPartitionKeyAsync<Synthesis>(
            AppConstants.TableNames.Syntheses,
            sessionId.ToString(),
            cancellationToken);
        return results.FirstOrDefault(); // One synthesis per session max
    }

    public async Task<Synthesis> UpdateAsync(Synthesis synthesis, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Syntheses,
            synthesis.SessionId.ToString(),
            synthesis.Id.ToString(),
            synthesis,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid synthesisId, CancellationToken cancellationToken = default)
    {
        var synthesis = await GetByIdAsync(synthesisId, cancellationToken);
        if (synthesis is not null)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Syntheses,
                synthesis.SessionId.ToString(),
                synthesisId.ToString(),
                cancellationToken);
        }
    }
}
