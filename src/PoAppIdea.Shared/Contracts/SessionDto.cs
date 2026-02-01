using PoAppIdea.Core.Enums;

namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for session data transfer.
/// </summary>
public sealed record SessionDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required SessionPhase CurrentPhase { get; init; }
    public required SessionStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public required IReadOnlyList<Guid> TopIdeaIds { get; init; }
    public required IReadOnlyList<Guid> SelectedIdeaIds { get; init; }
    public Guid? SynthesisId { get; init; }
}
