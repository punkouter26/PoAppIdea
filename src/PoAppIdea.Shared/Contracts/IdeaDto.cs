namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for idea data transfer.
/// </summary>
public sealed record IdeaDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required int BatchNumber { get; init; }
    public required IReadOnlyList<string> DnaKeywords { get; init; }
    public required float Score { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
