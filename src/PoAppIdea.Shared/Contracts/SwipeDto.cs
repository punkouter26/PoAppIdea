using PoAppIdea.Core.Enums;

namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for swipe data transfer.
/// </summary>
public sealed record SwipeDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required Guid IdeaId { get; init; }
    public required Guid UserId { get; init; }
    public required SwipeDirection Direction { get; init; }
    public required int DurationMs { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required SwipeSpeed SpeedCategory { get; init; }
}
