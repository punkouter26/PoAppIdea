using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Request DTO for recording a swipe action.
/// </summary>
public sealed class RecordSwipeRequest
{
    /// <summary>
    /// The ID of the idea being swiped on.
    /// </summary>
    public required Guid IdeaId { get; init; }

    /// <summary>
    /// The direction of the swipe (Left = discard, Right = keep, Up = super-like).
    /// </summary>
    public required SwipeDirection Direction { get; init; }

    /// <summary>
    /// Duration in milliseconds the user spent viewing the idea before swiping.
    /// Used for hesitation detection per FR-004.
    /// </summary>
    public required int DurationMs { get; init; }
}
