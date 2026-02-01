using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IRefinementAnswerRepository.
/// Partition key: SessionId, Row key: AnswerId
/// </summary>
public sealed class RefinementAnswerRepository : IRefinementAnswerRepository
{
    private readonly TableStorageClient _tableClient;

    public RefinementAnswerRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<RefinementAnswer> CreateAsync(RefinementAnswer answer, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.RefinementAnswers,
            answer.SessionId.ToString(),
            answer.Id.ToString(),
            answer,
            cancellationToken);
    }

    public async Task CreateBatchAsync(IEnumerable<RefinementAnswer> answers, CancellationToken cancellationToken = default)
    {
        var answerList = answers.ToList();
        if (answerList.Count == 0) return;

        // Group by partition key (SessionId) for batch operations
        var groups = answerList.GroupBy(a => a.SessionId);
        foreach (var group in groups)
        {
            foreach (var answer in group)
            {
                await _tableClient.InsertAsync(
                    AppConstants.TableNames.RefinementAnswers,
                    answer.SessionId.ToString(),
                    answer.Id.ToString(),
                    answer,
                    cancellationToken);
            }
        }
    }

    public async Task<RefinementAnswer?> GetByIdAsync(Guid sessionId, Guid answerId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.GetAsync<RefinementAnswer>(
            AppConstants.TableNames.RefinementAnswers,
            sessionId.ToString(),
            answerId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<RefinementAnswer>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var results = await _tableClient.QueryByPartitionKeyAsync<RefinementAnswer>(
            AppConstants.TableNames.RefinementAnswers,
            sessionId.ToString(),
            cancellationToken);
        return results.OrderBy(a => a.Phase).ThenBy(a => a.QuestionNumber).ToList();
    }

    public async Task<IReadOnlyList<RefinementAnswer>> GetBySessionAndPhaseAsync(
        Guid sessionId,
        RefinementPhase phase,
        CancellationToken cancellationToken = default)
    {
        var all = await GetBySessionIdAsync(sessionId, cancellationToken);
        return all.Where(a => a.Phase == phase)
            .OrderBy(a => a.QuestionNumber)
            .ToList();
    }

    public async Task<RefinementAnswer> UpdateAsync(RefinementAnswer answer, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.RefinementAnswers,
            answer.SessionId.ToString(),
            answer.Id.ToString(),
            answer,
            cancellationToken);
    }

    public async Task DeleteBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var answers = await GetBySessionIdAsync(sessionId, cancellationToken);
        foreach (var answer in answers)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.RefinementAnswers,
                sessionId.ToString(),
                answer.Id.ToString(),
                cancellationToken);
        }
    }

    public async Task<int> CountBySessionAndPhaseAsync(
        Guid sessionId,
        RefinementPhase phase,
        CancellationToken cancellationToken = default)
    {
        var answers = await GetBySessionAndPhaseAsync(sessionId, phase, cancellationToken);
        return answers.Count;
    }
}
