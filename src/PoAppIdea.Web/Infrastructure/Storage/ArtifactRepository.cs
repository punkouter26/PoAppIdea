using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IArtifactRepository.
/// </summary>
public sealed class ArtifactRepository : IArtifactRepository
{
    private readonly TableStorageClient _tableClient;

    public ArtifactRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Artifact> CreateAsync(Artifact artifact, CancellationToken cancellationToken = default)
    {
        var rowKey = $"{artifact.SessionId}_{artifact.Type}";
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Artifacts,
            artifact.UserId.ToString(),
            rowKey,
            artifact,
            cancellationToken);
    }

    public async Task<Artifact?> GetByIdAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        var allArtifacts = await _tableClient.QueryAsync<Artifact>(
            AppConstants.TableNames.Artifacts,
            $"",
            cancellationToken);
        return allArtifacts.FirstOrDefault(a => a.Id == artifactId);
    }

    public async Task<Artifact?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var allArtifacts = await _tableClient.QueryAsync<Artifact>(
            AppConstants.TableNames.Artifacts,
            $"",
            cancellationToken);
        return allArtifacts.FirstOrDefault(a => a.HumanReadableSlug == slug);
    }

    public async Task<IReadOnlyList<Artifact>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var allArtifacts = await _tableClient.QueryAsync<Artifact>(
            AppConstants.TableNames.Artifacts,
            $"",
            cancellationToken);
        return allArtifacts.Where(a => a.SessionId == sessionId).ToList();
    }

    public async Task<IReadOnlyList<Artifact>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.QueryByPartitionKeyAsync<Artifact>(
            AppConstants.TableNames.Artifacts,
            userId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Artifact>> GetPublishedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var allArtifacts = await _tableClient.QueryAsync<Artifact>(
            AppConstants.TableNames.Artifacts,
            $"",
            cancellationToken);
        return allArtifacts
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public async Task<Artifact> UpdateAsync(Artifact artifact, CancellationToken cancellationToken = default)
    {
        var rowKey = $"{artifact.SessionId}_{artifact.Type}";
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Artifacts,
            artifact.UserId.ToString(),
            rowKey,
            artifact,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        var artifact = await GetByIdAsync(artifactId, cancellationToken);
        if (artifact is not null)
        {
            var rowKey = $"{artifact.SessionId}_{artifact.Type}";
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Artifacts,
                artifact.UserId.ToString(),
                rowKey,
                cancellationToken);
        }
    }
}
