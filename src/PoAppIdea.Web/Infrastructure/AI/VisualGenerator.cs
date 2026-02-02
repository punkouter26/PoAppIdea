using Azure.AI.OpenAI;
using OpenAI.Images;
using PoAppIdea.Core.Entities;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Service for generating visual mockups via Azure OpenAI DALL-E.
/// Implements Strategy Pattern for different visual styles.
/// </summary>
public sealed class VisualGenerator : IVisualGenerator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VisualGenerator> _logger;

    /// <summary>
    /// Predefined visual styles for variety.
    /// </summary>
    private static readonly IReadOnlyList<(string LayoutStyle, string Vibe, string[] Colors)> VisualStyles =
    [
        ("Dashboard", "Professional", ["#1E40AF", "#3B82F6", "#93C5FD", "#F8FAFC", "#1F2937"]),
        ("Card-based", "Modern", ["#7C3AED", "#A78BFA", "#EDE9FE", "#FFFFFF", "#4B5563"]),
        ("Minimal", "Clean", ["#059669", "#34D399", "#ECFDF5", "#F9FAFB", "#374151"]),
        ("Split-screen", "Bold", ["#DC2626", "#F87171", "#FEE2E2", "#FFFFFF", "#111827"]),
        ("Hero-focused", "Playful", ["#F59E0B", "#FBBF24", "#FEF3C7", "#FFFBEB", "#1F2937"]),
        ("List-based", "Functional", ["#0891B2", "#22D3EE", "#CFFAFE", "#F0FDFA", "#334155"]),
        ("Grid", "Dynamic", ["#DB2777", "#F472B6", "#FCE7F3", "#FDF2F8", "#1E293B"]),
        ("Tabbed", "Organized", ["#4F46E5", "#818CF8", "#E0E7FF", "#EEF2FF", "#111827"]),
        ("Wizard/Steps", "Guided", ["#059669", "#10B981", "#D1FAE5", "#ECFDF5", "#064E3B"]),
        ("Mobile-first", "Compact", ["#6366F1", "#A5B4FC", "#E0E7FF", "#FFFFFF", "#1E1B4B"])
    ];

    public VisualGenerator(IConfiguration configuration, ILogger<VisualGenerator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generates a DALL-E prompt for a visual mockup based on the app concept.
    /// </summary>
    public string GeneratePrompt(
        string appTitle,
        string appDescription,
        string appType,
        int styleIndex,
        string? userStyleHint = null)
    {
        var style = VisualStyles[styleIndex % VisualStyles.Count];
        var colors = string.Join(", ", style.Colors.Take(3));

        var basePrompt = $"""
            UI mockup for a {appType.ToLowerInvariant()} app called "{appTitle}".
            App description: {appDescription}
            Layout style: {style.LayoutStyle}
            Visual vibe: {style.Vibe}
            Color palette: {colors}
            High fidelity, clean design, modern UI/UX, no text labels, app interface screenshot.
            """;

        if (!string.IsNullOrEmpty(userStyleHint))
        {
            basePrompt += $"\nUser style preference: {userStyleHint}";
        }

        return basePrompt.Trim();
    }

    /// <summary>
    /// Gets the style info for a given style index.
    /// </summary>
    public StyleInfo GetStyleInfo(int styleIndex)
    {
        var style = VisualStyles[styleIndex % VisualStyles.Count];
        return new StyleInfo
        {
            ColorPalette = [.. style.Colors],
            LayoutStyle = style.LayoutStyle,
            Vibe = style.Vibe
        };
    }

    /// <summary>
    /// Generates a visual mockup using Azure OpenAI DALL-E.
    /// Returns the image bytes if successful, null if generation fails.
    /// </summary>
    public async Task<byte[]?> GenerateImageAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var apiKey = _configuration["AzureOpenAI:ApiKey"];
            var deploymentName = _configuration["AzureOpenAI:DalleDeployment"] ?? "dall-e-3";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Azure OpenAI DALL-E not configured, returning placeholder");
                return GeneratePlaceholderImage();
            }

            var client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey));
            var imageClient = client.GetImageClient(deploymentName);

            // Use configurable image size for cost optimization (512x512 is ~60% cheaper)
            var imageSizeConfig = _configuration["AzureOpenAI:ImageSize"] ?? "1024x1024";
            var imageSize = imageSizeConfig == "512x512" 
                ? GeneratedImageSize.W512xH512 
                : GeneratedImageSize.W1024xH1024;

            var options = new ImageGenerationOptions
            {
                Size = imageSize,
                Quality = GeneratedImageQuality.Standard,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var result = await imageClient.GenerateImageAsync(prompt, options, cancellationToken);

            if (result?.Value?.ImageBytes is not null)
            {
                return result.Value.ImageBytes.ToArray();
            }

            _logger.LogWarning("DALL-E returned no image data for prompt: {Prompt}", prompt[..Math.Min(100, prompt.Length)]);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image with DALL-E for prompt: {Prompt}", prompt[..Math.Min(100, prompt.Length)]);
            return null;
        }
    }

    /// <summary>
    /// Generates a placeholder image for development/testing when DALL-E is not configured.
    /// </summary>
    private static byte[] GeneratePlaceholderImage()
    {
        // Return a simple 1x1 PNG placeholder
        // In a real scenario, you might return a more meaningful placeholder
        return [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
            0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB4, 0x00, 0x00,
            0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }

    /// <summary>
    /// Gets the total number of available visual styles.
    /// </summary>
    public int GetStyleCount() => VisualStyles.Count;
}
