namespace PoAppIdea.Core.Entities;

/// <summary>
/// A merged concept from multiple selected ideas.
/// </summary>
public sealed class Synthesis
{
    /// <summary>
    /// Unique synthesis identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Ideas being merged (2-10 entries).
    /// </summary>
    public required List<Guid> SourceIdeaIds { get; init; }

    /// <summary>
    /// Unified concept title (1-100 chars).
    /// </summary>
    public required string MergedTitle { get; set; }

    /// <summary>
    /// Unified description (1-1000 chars).
    /// </summary>
    public required string MergedDescription { get; set; }

    /// <summary>
    /// Explanation of how ideas connect.
    /// </summary>
    public required string ThematicBridge { get; set; }

    /// <summary>
    /// Which elements kept per source idea.
    /// </summary>
    public required Dictionary<Guid, List<string>> RetainedElements { get; set; }

    /// <summary>
    /// Synthesis timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
