using PoAppIdea.Core.Entities;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for VisualAsset entity domain logic.
/// Focus: Blob URLs, style attributes, selection state.
/// </summary>
public sealed class VisualAssetTests
{
    private static VisualAsset CreateValidVisualAsset() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        BlobUrl = "https://stpoappidea.blob.core.windows.net/visual-assets/123.png",
        ThumbnailUrl = "https://stpoappidea.blob.core.windows.net/visual-assets/123-thumb.png",
        Prompt = "A modern dashboard UI with card-based layout, purple accent colors, professional vibe",
        StyleAttributes = new StyleInfo
        {
            ColorPalette = ["#6366F1", "#8B5CF6", "#EC4899", "#F9FAFB"],
            LayoutStyle = "Dashboard",
            Vibe = "Professional"
        },
        IsSelected = false,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void VisualAsset_ShouldInitialize_WithRequiredProperties()
    {
        // Arrange & Act
        var asset = CreateValidVisualAsset();

        // Assert
        asset.Id.Should().NotBeEmpty();
        asset.SessionId.Should().NotBeEmpty();
        asset.BlobUrl.Should().NotBeNullOrEmpty();
        asset.ThumbnailUrl.Should().NotBeNullOrEmpty();
        asset.Prompt.Should().NotBeNullOrEmpty();
        asset.StyleAttributes.Should().NotBeNull();
        asset.IsSelected.Should().BeFalse();
        asset.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void VisualAsset_BlobUrl_ShouldBeValidAzureUrl()
    {
        // Arrange
        var asset = CreateValidVisualAsset();

        // Assert
        asset.BlobUrl.Should().StartWith("https://");
        asset.BlobUrl.Should().Contain("blob.core.windows.net");
    }

    [Fact]
    public void VisualAsset_Selection_ShouldBeToggleable()
    {
        // Arrange
        var asset = CreateValidVisualAsset();

        // Act
        asset.IsSelected = true;

        // Assert
        asset.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void StyleInfo_ColorPalette_ShouldContainHexCodes()
    {
        // Arrange
        var asset = CreateValidVisualAsset();

        // Assert
        asset.StyleAttributes.ColorPalette.Should().NotBeEmpty();
        asset.StyleAttributes.ColorPalette.Should().AllSatisfy(color =>
        {
            color.Should().MatchRegex("^#[0-9A-Fa-f]{6}$");
        });
    }

    [Theory]
    [InlineData("Dashboard")]
    [InlineData("Card-based")]
    [InlineData("Minimalist")]
    [InlineData("Data-heavy")]
    public void StyleInfo_LayoutStyle_ShouldAllowVariousStyles(string layoutStyle)
    {
        // Arrange & Act
        var asset = new VisualAsset
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            BlobUrl = "https://example.com/image.png",
            ThumbnailUrl = "https://example.com/thumb.png",
            Prompt = "Test prompt",
            StyleAttributes = new StyleInfo
            {
                ColorPalette = ["#000000"],
                LayoutStyle = layoutStyle,
                Vibe = "Professional"
            },
            IsSelected = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        asset.StyleAttributes.LayoutStyle.Should().Be(layoutStyle);
    }

    [Theory]
    [InlineData("Professional")]
    [InlineData("Playful")]
    [InlineData("Minimalist")]
    [InlineData("Bold")]
    public void StyleInfo_Vibe_ShouldAllowVariousVibes(string vibe)
    {
        // Arrange & Act
        var asset = new VisualAsset
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            BlobUrl = "https://example.com/image.png",
            ThumbnailUrl = "https://example.com/thumb.png",
            Prompt = "Test prompt",
            StyleAttributes = new StyleInfo
            {
                ColorPalette = ["#000000"],
                LayoutStyle = "Dashboard",
                Vibe = vibe
            },
            IsSelected = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        asset.StyleAttributes.Vibe.Should().Be(vibe);
    }

    [Fact]
    public void VisualAsset_Prompt_ShouldDescribeDesiredImage()
    {
        // Arrange
        var asset = CreateValidVisualAsset();

        // Assert
        asset.Prompt.Should().NotBeNullOrEmpty();
        asset.Prompt.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public void VisualAsset_BlobUrl_ShouldBeUpdateable()
    {
        // Arrange
        var asset = CreateValidVisualAsset();
        var newUrl = "https://stpoappidea.blob.core.windows.net/visual-assets/updated.png";

        // Act
        asset.BlobUrl = newUrl;

        // Assert
        asset.BlobUrl.Should().Be(newUrl);
    }
}
