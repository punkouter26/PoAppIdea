using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// DTO for artifact details in API responses.
/// </summary>
public sealed record ArtifactResponseDto
{
    /// <summary>
    /// Artifact unique identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Artifact type (PRD, TechnicalDeepDive, VisualAssetPack).
    /// </summary>
    public required ArtifactType Type { get; init; }

    /// <summary>
    /// Human-readable title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Markdown content (for PRD and TechnicalDeepDive) or manifest JSON (for VisualAssetPack).
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Blob URL for downloadable content (primarily for VisualAssetPack).
    /// </summary>
    public string? BlobUrl { get; init; }

    /// <summary>
    /// URL-friendly identifier for public sharing.
    /// </summary>
    public required string HumanReadableSlug { get; init; }

    /// <summary>
    /// Whether this artifact is published to the public gallery.
    /// </summary>
    public required bool IsPublished { get; init; }

    /// <summary>
    /// When the artifact was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// When the artifact was published (null if not published).
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }
}
