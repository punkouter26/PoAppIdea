namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Request to generate mutations from top ideas.
/// </summary>
public sealed record MutateIdeasRequest
{
    /// <summary>
    /// Optional list of specific idea IDs to mutate.
    /// If not provided, top 5 ideas by score will be used.
    /// </summary>
    public IReadOnlyList<Guid>? TopIdeaIds { get; init; }

    /// <summary>
    /// Number of mutations to generate per idea.
    /// Defaults to 4 (generates 8 total with 2 top ideas).
    /// </summary>
    public int MutationsPerIdea { get; init; } = 4;
}
