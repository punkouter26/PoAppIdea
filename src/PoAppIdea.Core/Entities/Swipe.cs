using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// User interaction with an idea (swipe).
/// </summary>
public sealed class Swipe
{
    /// <summary>
    /// Unique swipe identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Idea being swiped.
    /// </summary>
    public required Guid IdeaId { get; init; }

    /// <summary>
    /// User who swiped.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Like or Dislike.
    /// </summary>
    public required SwipeDirection Direction { get; init; }

    /// <summary>
    /// Time from display to swipe (ms). Must be positive.
    /// </summary>
    public required int DurationMs { get; init; }

    /// <summary>
    /// When swipe occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Speed category: Fast (&lt;1s), Medium (1-3s), Slow (&gt;3s).
    /// </summary>
    public required SwipeSpeed SpeedCategory { get; init; }
}
