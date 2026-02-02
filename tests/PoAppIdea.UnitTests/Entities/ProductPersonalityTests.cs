using PoAppIdea.Core.Entities;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for ProductPersonality entity domain logic.
/// Focus: Bias scoring, pattern tracking, speed profiles.
/// </summary>
public sealed class ProductPersonalityTests
{
    private static ProductPersonality CreateValidPersonality() => new()
    {
        UserId = Guid.NewGuid(),
        ProductBiases = new Dictionary<string, float>
        {
            { "social", 0.8f },
            { "gamification", 0.5f },
            { "enterprise", -0.3f }
        },
        TechnicalBiases = new Dictionary<string, float>
        {
            { "serverless", 0.6f },
            { "microservices", 0.4f },
            { "monolith", -0.5f }
        },
        DislikedPatterns = ["subscription-based", "ad-supported"],
        SwipeSpeedProfile = new SpeedProfile
        {
            AverageFastMs = 150.0,
            AverageMediumMs = 500.0,
            AverageSlowMs = 1200.0
        },
        TotalSessions = 5,
        LastUpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void ProductPersonality_ShouldInitialize_WithRequiredProperties()
    {
        // Arrange & Act
        var personality = CreateValidPersonality();

        // Assert
        personality.UserId.Should().NotBeEmpty();
        personality.ProductBiases.Should().NotBeEmpty();
        personality.TechnicalBiases.Should().NotBeEmpty();
        personality.DislikedPatterns.Should().NotBeEmpty();
        personality.SwipeSpeedProfile.Should().NotBeNull();
        personality.TotalSessions.Should().BePositive();
        personality.LastUpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("social", 0.8f)]
    [InlineData("enterprise", -0.3f)]
    [InlineData("gamification", 0.5f)]
    public void ProductPersonality_ProductBiases_ShouldStoreCorrectValues(string key, float expectedValue)
    {
        // Arrange
        var personality = CreateValidPersonality();

        // Assert
        personality.ProductBiases.Should().ContainKey(key);
        personality.ProductBiases[key].Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    public void ProductPersonality_BiasScore_ShouldAllowValidRange(float biasValue)
    {
        // Arrange
        var personality = CreateValidPersonality();

        // Act
        personality.ProductBiases["newTrait"] = biasValue;

        // Assert
        personality.ProductBiases["newTrait"].Should().BeInRange(-1.0f, 1.0f);
    }

    [Fact]
    public void ProductPersonality_DislikedPatterns_ShouldBeUpdateable()
    {
        // Arrange
        var personality = CreateValidPersonality();
        var initialCount = personality.DislikedPatterns.Count;

        // Act
        personality.DislikedPatterns.Add("pay-to-win");

        // Assert
        personality.DislikedPatterns.Should().HaveCount(initialCount + 1);
        personality.DislikedPatterns.Should().Contain("pay-to-win");
    }

    [Fact]
    public void SpeedProfile_ShouldHaveCorrectOrdering()
    {
        // Arrange
        var personality = CreateValidPersonality();

        // Assert - Fast < Medium < Slow
        personality.SwipeSpeedProfile.AverageFastMs.Should().BeLessThan(personality.SwipeSpeedProfile.AverageMediumMs);
        personality.SwipeSpeedProfile.AverageMediumMs.Should().BeLessThan(personality.SwipeSpeedProfile.AverageSlowMs);
    }

    [Fact]
    public void ProductPersonality_TotalSessions_ShouldBeIncrementable()
    {
        // Arrange
        var personality = CreateValidPersonality();
        var initialSessions = personality.TotalSessions;

        // Act
        personality.TotalSessions++;

        // Assert
        personality.TotalSessions.Should().Be(initialSessions + 1);
    }

    [Fact]
    public void ProductPersonality_TechnicalBiases_ShouldStoreMultipleValues()
    {
        // Arrange & Act
        var personality = CreateValidPersonality();

        // Assert
        personality.TechnicalBiases.Should().HaveCount(3);
        personality.TechnicalBiases.Should().ContainKey("serverless");
        personality.TechnicalBiases.Should().ContainKey("microservices");
        personality.TechnicalBiases.Should().ContainKey("monolith");
    }
}
