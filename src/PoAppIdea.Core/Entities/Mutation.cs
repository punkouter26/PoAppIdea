using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// A derived idea from Crossover or Repurposing.
/// </summary>
public sealed class Mutation
{
    /// <summary>
    /// Unique mutation identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Source ideas (1 for Repurpose, 2 for Crossover).
    /// </summary>
    public required List<Guid> ParentIdeaIds { get; init; }

    /// <summary>
    /// Crossover or Repurposing.
    /// </summary>
    public required MutationType MutationType { get; init; }

    /// <summary>
    /// Mutated idea title (1-100 chars).
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Mutated description (1-500 chars).
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// How/why mutation was applied.
    /// </summary>
    public required string MutationRationale { get; init; }

    /// <summary>
    /// User rating score.
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
