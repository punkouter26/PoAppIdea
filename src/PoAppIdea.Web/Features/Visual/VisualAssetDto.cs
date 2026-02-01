namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Style information for a visual asset.
/// </summary>
public sealed record StyleInfoDto
{
    /// <summary>
    /// Hex color codes.
    /// </summary>
    public required List<string> ColorPalette { get; init; }

    /// <summary>
    /// Layout style (e.g., "Dashboard", "Card-based").
    /// </summary>
    public required string LayoutStyle { get; init; }

    /// <summary>
    /// Vibe (e.g., "Professional", "Playful").
    /// </summary>
    public required string Vibe { get; init; }
}

/// <summary>
/// Response DTO for a visual asset.
/// </summary>
public sealed record VisualAssetDto
{
    /// <summary>
    /// Unique asset identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Azure Blob Storage URL for the full image.
    /// </summary>
    public required string BlobUrl { get; init; }

    /// <summary>
    /// Thumbnail URL for faster loading.
    /// </summary>
    public required string ThumbnailUrl { get; init; }

    /// <summary>
    /// DALL-E prompt used to generate this visual.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Color palette, layout, and vibe attributes.
    /// </summary>
    public required StyleInfoDto StyleAttributes { get; init; }

    /// <summary>
    /// Whether this visual is the user's selected direction.
    /// </summary>
    public required bool IsSelected { get; init; }

    /// <summary>
    /// When this visual was generated.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Response DTO for getting visuals.
/// </summary>
public sealed record GetVisualsResponse
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// List of visual assets.
    /// </summary>
    public required IReadOnlyList<VisualAssetDto> Visuals { get; init; }

    /// <summary>
    /// Whether a visual has been selected.
    /// </summary>
    public required bool HasSelection { get; init; }

    /// <summary>
    /// ID of the selected visual, if any.
    /// </summary>
    public Guid? SelectedVisualId { get; init; }
}
