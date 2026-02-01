using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// Final deliverable document (PRD, Technical Deep-Dive, or Visual Asset Pack).
/// </summary>
public sealed class Artifact
{
    /// <summary>
    /// Unique artifact identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Owner.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// PRD, TechnicalDeepDive, or VisualAssetPack.
    /// </summary>
    public required ArtifactType Type { get; init; }

    /// <summary>
    /// Human-readable title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Markdown content (PRD, Tech) or manifest (Visual).
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// For Visual pack zip file.
    /// </summary>
    public string? BlobUrl { get; set; }

    /// <summary>
    /// Published to gallery.
    /// </summary>
    public required bool IsPublished { get; set; }

    /// <summary>
    /// URL-friendly identifier (lowercase, alphanumeric + hyphens, 5-100 chars).
    /// </summary>
    public required string HumanReadableSlug { get; init; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// When published to gallery.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }
}
