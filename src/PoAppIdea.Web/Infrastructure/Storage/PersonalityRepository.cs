using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IPersonalityRepository.
/// </summary>
public sealed class PersonalityRepository : IPersonalityRepository
{
    private readonly TableStorageClient _tableClient;
    private const string ProfileRowKey = "profile";

    public PersonalityRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<ProductPersonality?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.GetAsync<ProductPersonality>(
            AppConstants.TableNames.Personalities,
            userId.ToString(),
            ProfileRowKey,
            cancellationToken);
    }

    public async Task<ProductPersonality> UpsertAsync(ProductPersonality personality, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Personalities,
            personality.UserId.ToString(),
            ProfileRowKey,
            personality,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _tableClient.DeleteAsync(
            AppConstants.TableNames.Personalities,
            userId.ToString(),
            ProfileRowKey,
            cancellationToken);
    }
}
