using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// A single idea evolution journey through phases 0-6.
/// </summary>
public sealed class Session
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Owner of this session.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Application type category.
    /// </summary>
    public required AppType AppType { get; set; }

    /// <summary>
    /// Complexity slider value (1-5).
    /// </summary>
    public required int ComplexityLevel { get; set; }

    /// <summary>
    /// Current phase of the session.
    /// </summary>
    public required SessionPhase CurrentPhase { get; set; }

    /// <summary>
    /// Session lifecycle status.
    /// </summary>
    public required SessionStatus Status { get; set; }

    /// <summary>
    /// Session start timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Session completion timestamp.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Top 5 from Phase 1, Top 10 from Phase 2.
    /// </summary>
    public required List<Guid> TopIdeaIds { get; set; }

    /// <summary>
    /// User's final selection (1-10 ideas).
    /// </summary>
    public required List<Guid> SelectedIdeaIds { get; set; }

    /// <summary>
    /// Merged concept if multiple selected.
    /// </summary>
    public Guid? SynthesisId { get; set; }
}
