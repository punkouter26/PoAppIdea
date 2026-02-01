using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Response DTO for gallery browsing.
/// </summary>
public sealed record BrowseGalleryResponse
{
    /// <summary>
    /// List of gallery items.
    /// </summary>
    public required IReadOnlyList<GalleryItemDto> Items { get; init; }

    /// <summary>
    /// Cursor for next page (null if no more results).
    /// </summary>
    public string? NextCursor { get; init; }

    /// <summary>
    /// Total count of matching items (if available).
    /// </summary>
    public int? TotalCount { get; init; }
}

/// <summary>
/// DTO for a gallery item (artifact summary).
/// </summary>
public sealed record GalleryItemDto
{
    /// <summary>
    /// Artifact ID.
    /// </summary>
    public required Guid ArtifactId { get; init; }

    /// <summary>
    /// Artifact title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Artifact type.
    /// </summary>
    public required ArtifactType ArtifactType { get; init; }

    /// <summary>
    /// App type from the session.
    /// </summary>
    public required AppType AppType { get; init; }

    /// <summary>
    /// Author's display name.
    /// </summary>
    public required string AuthorDisplayName { get; init; }

    /// <summary>
    /// URL-friendly slug.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// When the artifact was published.
    /// </summary>
    public required DateTimeOffset PublishedAt { get; init; }

    /// <summary>
    /// Thumbnail URL for visual assets.
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Brief description or excerpt.
    /// </summary>
    public string? Description { get; init; }
}
