using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Response containing generated mutations.
/// </summary>
public sealed record MutateIdeasResponse
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Total number of mutations generated.
    /// </summary>
    public required int TotalMutations { get; init; }

    /// <summary>
    /// Generated mutations grouped by parent idea.
    /// </summary>
    public required IReadOnlyList<MutationDto> Mutations { get; init; }

    /// <summary>
    /// Number of crossover mutations (combining 2 parent ideas).
    /// </summary>
    public required int CrossoverCount { get; init; }

    /// <summary>
    /// Number of repurposing mutations (single parent idea transformed).
    /// </summary>
    public required int RepurposingCount { get; init; }
}
