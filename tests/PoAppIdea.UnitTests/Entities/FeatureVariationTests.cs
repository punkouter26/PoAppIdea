using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for FeatureVariation entity domain logic.
/// Focus: Pure logic validation, no external dependencies.
/// </summary>
public sealed class FeatureVariationTests
{
    private static FeatureVariation CreateValidFeatureVariation() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        MutationId = Guid.NewGuid(),
        Features =
        [
            new Feature { Name = "User Authentication", Description = "OAuth 2.0 login", Priority = FeaturePriority.Must },
            new Feature { Name = "Dashboard", Description = "Analytics overview", Priority = FeaturePriority.Should },
            new Feature { Name = "Export Data", Description = "CSV export", Priority = FeaturePriority.Could }
        ],
        ServiceIntegrations = ["Auth0", "Stripe", "SendGrid"],
        VariationTheme = "Enterprise-Ready",
        Score = 0f,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void FeatureVariation_ShouldInitialize_WithValidDefaults()
    {
        // Arrange & Act
        var variation = CreateValidFeatureVariation();

        // Assert
        variation.Id.Should().NotBeEmpty();
        variation.SessionId.Should().NotBeEmpty();
        variation.MutationId.Should().NotBeEmpty();
        variation.Features.Should().NotBeEmpty();
        variation.ServiceIntegrations.Should().NotBeEmpty();
        variation.VariationTheme.Should().NotBeNullOrWhiteSpace();
        variation.Score.Should().Be(0f);
    }

    [Theory]
    [InlineData("Minimalist MVP")]
    [InlineData("Enterprise-Ready")]
    [InlineData("Privacy-First")]
    [InlineData("Social-Heavy")]
    [InlineData("AI-Powered")]
    public void FeatureVariation_VariationTheme_ShouldAcceptValidThemes(string theme)
    {
        // Arrange & Act
        var variation = new FeatureVariation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            MutationId = Guid.NewGuid(),
            Features = [new Feature { Name = "Test", Description = "Test", Priority = FeaturePriority.Must }],
            ServiceIntegrations = ["TestAPI"],
            VariationTheme = theme,
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        variation.VariationTheme.Should().Be(theme);
    }

    [Fact]
    public void FeatureVariation_Score_ShouldBeUpdateable()
    {
        // Arrange
        var variation = CreateValidFeatureVariation();

        // Act
        variation.Score = 4.5f;

        // Assert
        variation.Score.Should().Be(4.5f);
    }

    [Theory]
    [InlineData(1f)]
    [InlineData(2.5f)]
    [InlineData(3f)]
    [InlineData(4f)]
    [InlineData(5f)]
    public void FeatureVariation_Score_ShouldAcceptValidRatings(float score)
    {
        // Arrange
        var variation = CreateValidFeatureVariation();

        // Act
        variation.Score = score;

        // Assert
        variation.Score.Should().Be(score);
    }

    [Fact]
    public void FeatureVariation_Features_ShouldContainMoSCoWPriorities()
    {
        // Arrange
        var variation = CreateValidFeatureVariation();

        // Act
        var mustFeatures = variation.Features.Where(f => f.Priority == FeaturePriority.Must).ToList();
        var shouldFeatures = variation.Features.Where(f => f.Priority == FeaturePriority.Should).ToList();
        var couldFeatures = variation.Features.Where(f => f.Priority == FeaturePriority.Could).ToList();

        // Assert
        mustFeatures.Should().NotBeEmpty();
        shouldFeatures.Should().NotBeEmpty();
        couldFeatures.Should().NotBeEmpty();
    }

    [Fact]
    public void FeatureVariation_ServiceIntegrations_ShouldMaintainList()
    {
        // Arrange
        var integrations = new List<string> { "Auth0", "Stripe", "SendGrid", "Twilio" };

        // Act
        var variation = new FeatureVariation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            MutationId = Guid.NewGuid(),
            Features = [new Feature { Name = "Test", Description = "Test", Priority = FeaturePriority.Must }],
            ServiceIntegrations = integrations,
            VariationTheme = "Enterprise-Ready",
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        variation.ServiceIntegrations.Should().HaveCount(4);
        variation.ServiceIntegrations.Should().Contain("Auth0");
        variation.ServiceIntegrations.Should().Contain("Stripe");
    }

    [Fact]
    public void Feature_ShouldInitialize_WithRequiredProperties()
    {
        // Arrange & Act
        var feature = new Feature
        {
            Name = "User Authentication",
            Description = "OAuth 2.0 and SSO support",
            Priority = FeaturePriority.Must
        };

        // Assert
        feature.Name.Should().NotBeNullOrWhiteSpace();
        feature.Description.Should().NotBeNullOrWhiteSpace();
        feature.Priority.Should().Be(FeaturePriority.Must);
    }

    [Theory]
    [InlineData(FeaturePriority.Must)]
    [InlineData(FeaturePriority.Should)]
    [InlineData(FeaturePriority.Could)]
    [InlineData(FeaturePriority.Wont)]
    public void Feature_Priority_ShouldAcceptAllMoSCoWValues(FeaturePriority priority)
    {
        // Arrange & Act
        var feature = new Feature
        {
            Name = "Test Feature",
            Description = "Test Description",
            Priority = priority
        };

        // Assert
        feature.Priority.Should().Be(priority);
    }

    [Fact]
    public void FeatureVariation_Features_ShouldHaveBetween3And10Items()
    {
        // Arrange
        var variation = CreateValidFeatureVariation();

        // Assert - per data-model.md, Features should have 3-10 items
        variation.Features.Count.Should().BeGreaterThanOrEqualTo(3);
        variation.Features.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void FeatureVariation_ShouldTrack_ParentMutation()
    {
        // Arrange
        var mutationId = Guid.NewGuid();

        // Act
        var variation = new FeatureVariation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            MutationId = mutationId,
            Features = [new Feature { Name = "Test", Description = "Test", Priority = FeaturePriority.Must }],
            ServiceIntegrations = ["TestAPI"],
            VariationTheme = "Minimalist MVP",
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        variation.MutationId.Should().Be(mutationId);
    }

    [Fact]
    public void FeatureVariation_CreatedAt_ShouldBe_RecentTimestamp()
    {
        // Arrange & Act
        var now = DateTimeOffset.UtcNow;
        var variation = CreateValidFeatureVariation();

        // Assert
        variation.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
    }
}
