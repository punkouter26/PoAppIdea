using PoAppIdea.Core.Enums;

namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for mutation data transfer.
/// </summary>
public sealed record MutationDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required IReadOnlyList<Guid> ParentIdeaIds { get; init; }
    public required MutationType MutationType { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string MutationRationale { get; init; }
    public required float Score { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
