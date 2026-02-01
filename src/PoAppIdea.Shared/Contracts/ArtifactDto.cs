using PoAppIdea.Core.Enums;

namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for artifact data transfer.
/// </summary>
public sealed record ArtifactDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required ArtifactType Type { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public string? BlobUrl { get; init; }
    public required bool IsPublished { get; init; }
    public required string HumanReadableSlug { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}

/// <summary>
/// Summary DTO for artifact listing in gallery.
/// </summary>
public sealed record ArtifactSummaryDto
{
    public required Guid Id { get; init; }
    public required ArtifactType Type { get; init; }
    public required string Title { get; init; }
    public required string HumanReadableSlug { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
}
