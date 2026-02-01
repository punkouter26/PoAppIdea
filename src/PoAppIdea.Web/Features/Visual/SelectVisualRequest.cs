namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Request DTO for selecting a visual direction.
/// The asset ID is provided in the route.
/// </summary>
public sealed record SelectVisualRequest
{
    /// <summary>
    /// Optional notes about why this visual was selected.
    /// </summary>
    public string? SelectionNotes { get; init; }
}
