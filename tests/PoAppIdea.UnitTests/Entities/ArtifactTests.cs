using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Artifact entity domain logic.
/// Focus: Property validation, slug formatting.
/// </summary>
public sealed class ArtifactTests
{
    private static Artifact CreateValidArtifact() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Type = ArtifactType.PRD,
        Title = "Sample Product Requirements Document",
        Content = "# PRD Content\n\n## Overview\n...",
        IsPublished = false,
        HumanReadableSlug = "sample-prd-document",
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void Artifact_ShouldInitialize_WithRequiredProperties()
    {
        // Arrange & Act
        var artifact = CreateValidArtifact();

        // Assert
        artifact.Id.Should().NotBeEmpty();
        artifact.SessionId.Should().NotBeEmpty();
        artifact.UserId.Should().NotBeEmpty();
        artifact.Type.Should().Be(ArtifactType.PRD);
        artifact.Title.Should().NotBeNullOrEmpty();
        artifact.Content.Should().NotBeNullOrEmpty();
        artifact.IsPublished.Should().BeFalse();
        artifact.HumanReadableSlug.Should().NotBeNullOrEmpty();
        artifact.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(ArtifactType.PRD)]
    [InlineData(ArtifactType.TechnicalDeepDive)]
    [InlineData(ArtifactType.VisualAssetPack)]
    public void Artifact_ShouldSupport_AllArtifactTypes(ArtifactType artifactType)
    {
        // Arrange & Act
        var artifact = new Artifact
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = artifactType,
            Title = $"Test {artifactType}",
            Content = "Test content",
            IsPublished = false,
            HumanReadableSlug = $"test-{artifactType.ToString().ToLowerInvariant()}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        artifact.Type.Should().Be(artifactType);
    }

    [Fact]
    public void Artifact_BlobUrl_ShouldBeOptional()
    {
        // Arrange
        var artifact = CreateValidArtifact();

        // Assert - BlobUrl is null by default for non-visual artifacts
        artifact.BlobUrl.Should().BeNull();
    }

    [Fact]
    public void Artifact_Publish_ShouldUpdateTimestamp()
    {
        // Arrange
        var artifact = CreateValidArtifact();

        // Act
        artifact.IsPublished = true;
        artifact.PublishedAt = DateTimeOffset.UtcNow;

        // Assert
        artifact.IsPublished.Should().BeTrue();
        artifact.PublishedAt.Should().NotBeNull();
        artifact.PublishedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Artifact_HumanReadableSlug_ShouldBeLowercaseWithHyphens()
    {
        // Arrange
        var slug = "my-product-requirements-doc";

        // Act
        var artifact = new Artifact
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ArtifactType.PRD,
            Title = "My Product Requirements Doc",
            Content = "Content",
            IsPublished = false,
            HumanReadableSlug = slug,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        artifact.HumanReadableSlug.Should().Be(slug);
        artifact.HumanReadableSlug.Should().MatchRegex("^[a-z0-9-]+$");
    }
}
