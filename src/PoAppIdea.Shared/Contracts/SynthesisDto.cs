namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for synthesis data transfer.
/// </summary>
public sealed record SynthesisDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required IReadOnlyList<Guid> SourceIdeaIds { get; init; }
    public required string MergedTitle { get; init; }
    public required string MergedDescription { get; init; }
    public required string ThematicBridge { get; init; }
    public required IReadOnlyDictionary<Guid, IReadOnlyList<string>> RetainedElements { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
