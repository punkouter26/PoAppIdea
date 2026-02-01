namespace PoAppIdea.Core.Entities;

/// <summary>
/// Style attributes for a visual asset.
/// </summary>
public sealed class StyleInfo
{
    /// <summary>
    /// Hex color codes.
    /// </summary>
    public required List<string> ColorPalette { get; set; }

    /// <summary>
    /// Layout style (e.g., "Dashboard", "Card-based").
    /// </summary>
    public required string LayoutStyle { get; set; }

    /// <summary>
    /// Vibe (e.g., "Professional", "Playful").
    /// </summary>
    public required string Vibe { get; set; }
}

/// <summary>
/// A DALL-E generated mockup or mood board.
/// </summary>
public sealed class VisualAsset
{
    /// <summary>
    /// Unique asset identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Azure Blob Storage URL.
    /// </summary>
    public required string BlobUrl { get; set; }

    /// <summary>
    /// Thumbnail URL.
    /// </summary>
    public required string ThumbnailUrl { get; set; }

    /// <summary>
    /// DALL-E prompt used.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Color palette, layout, vibe.
    /// </summary>
    public required StyleInfo StyleAttributes { get; set; }

    /// <summary>
    /// User's chosen visual.
    /// </summary>
    public required bool IsSelected { get; set; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
