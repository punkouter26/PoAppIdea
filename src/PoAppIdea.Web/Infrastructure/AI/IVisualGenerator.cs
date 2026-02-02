using PoAppIdea.Core.Entities;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Interface for visual (DALL-E) generation services.
/// Pattern: Strategy Pattern - Allows swapping between real AI and mock implementations.
/// </summary>
public interface IVisualGenerator
{
    /// <summary>
    /// Generates a DALL-E prompt for a visual mockup based on the app concept.
    /// </summary>
    string GeneratePrompt(
        string appTitle,
        string appDescription,
        string appType,
        int styleIndex,
        string? userStyleHint = null);

    /// <summary>
    /// Gets the style info for a given style index.
    /// </summary>
    StyleInfo GetStyleInfo(int styleIndex);

    /// <summary>
    /// Generates a visual mockup using Azure OpenAI DALL-E.
    /// Returns the image bytes if successful, null if generation fails.
    /// </summary>
    Task<byte[]?> GenerateImageAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total number of available visual styles.
    /// </summary>
    int GetStyleCount();
}
