namespace PoAppIdea.Core.Entities;

/// <summary>
/// A generated app concept.
/// </summary>
public sealed class Idea
{
    /// <summary>
    /// Unique idea identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session this idea belongs to.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Short idea title (1-100 chars).
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 1-2 sentence description (1-500 chars).
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Which batch for Phase 1 learning (1-5).
    /// </summary>
    public required int BatchNumber { get; init; }

    /// <summary>
    /// LLM prompt used to generate.
    /// </summary>
    public required string GenerationPrompt { get; init; }

    /// <summary>
    /// Extracted trait keywords (max 20).
    /// </summary>
    public required List<string> DnaKeywords { get; set; }

    /// <summary>
    /// Weighted rating score.
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
