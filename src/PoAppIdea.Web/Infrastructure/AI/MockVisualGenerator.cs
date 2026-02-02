using PoAppIdea.Core.Entities;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Mock implementation of IVisualGenerator for testing without DALL-E API costs.
/// Pattern: Null Object Pattern - Provides deterministic mock data for testing.
/// 
/// Enable by setting environment variable: MOCK_AI=true
/// Or in appsettings: "MockAI": true
/// </summary>
public sealed class MockVisualGenerator : IVisualGenerator
{
    private readonly ILogger<MockVisualGenerator> _logger;

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

    public MockVisualGenerator(ILogger<MockVisualGenerator> logger)
    {
        _logger = logger;
    }

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

    public Task<byte[]?> GenerateImageAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating placeholder image for prompt: {Prompt}", 
            prompt.Length > 100 ? prompt[..100] + "..." : prompt);

        // Return a placeholder PNG
        // This is a simple 1x1 pixel PNG - in production tests you might want larger placeholders
        byte[] placeholderImage =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
            0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB4, 0x00, 0x00,
            0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ];

        return Task.FromResult<byte[]?>(placeholderImage);
    }

    public int GetStyleCount() => VisualStyles.Count;
}
