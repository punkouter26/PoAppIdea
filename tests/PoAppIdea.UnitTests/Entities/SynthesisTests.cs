using PoAppIdea.Core.Entities;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Synthesis entity domain logic.
/// Focus: Pure logic validation, no external dependencies.
/// Phase 6 tests per FR-013, FR-014.
/// </summary>
public sealed class SynthesisTests
{
    private static Synthesis CreateValidSynthesis() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        SourceIdeaIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
        MergedTitle = "Unified Social Fitness Platform",
        MergedDescription = "A comprehensive platform combining social networking with fitness tracking, " +
                           "enabling users to connect, compete, and share their wellness journey.",
        ThematicBridge = "Both concepts share the core theme of building communities around personal goals " +
                        "and leveraging social motivation for individual improvement.",
        RetainedElements = new Dictionary<Guid, List<string>>
        {
            [Guid.NewGuid()] = ["Social feed", "Friend connections", "Activity sharing"],
            [Guid.NewGuid()] = ["Workout tracking", "Goal setting", "Progress charts"]
        },
        CreatedAt = DateTimeOffset.UtcNow
    };

    #region Entity Initialization Tests

    [Fact]
    public void Synthesis_ShouldInitialize_WithValidDefaults()
    {
        // Arrange & Act
        var synthesis = CreateValidSynthesis();

        // Assert
        synthesis.Id.Should().NotBeEmpty();
        synthesis.SessionId.Should().NotBeEmpty();
        synthesis.SourceIdeaIds.Should().NotBeEmpty();
        synthesis.MergedTitle.Should().NotBeNullOrWhiteSpace();
        synthesis.MergedDescription.Should().NotBeNullOrWhiteSpace();
        synthesis.ThematicBridge.Should().NotBeNullOrWhiteSpace();
        synthesis.RetainedElements.Should().NotBeEmpty();
        synthesis.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Synthesis_SourceIdeaIds_ShouldContainMultipleIds()
    {
        // Arrange & Act
        var synthesis = CreateValidSynthesis();

        // Assert - Synthesis requires 2-10 source ideas per spec
        synthesis.SourceIdeaIds.Count.Should().BeGreaterThanOrEqualTo(2);
        synthesis.SourceIdeaIds.Count.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void Synthesis_RetainedElements_ShouldMapToSourceIdeas()
    {
        // Arrange & Act
        var synthesis = CreateValidSynthesis();

        // Assert - Each source idea should have retained elements
        synthesis.RetainedElements.Should().NotBeEmpty();
        foreach (var kvp in synthesis.RetainedElements)
        {
            kvp.Key.Should().NotBeEmpty();
            kvp.Value.Should().NotBeEmpty();
        }
    }

    #endregion

    #region MergedTitle Tests

    [Theory]
    [InlineData("Simple Title")]
    [InlineData("A Longer Title With Multiple Words")]
    [InlineData("Title-With-Hyphens")]
    [InlineData("Title123 With Numbers")]
    public void Synthesis_MergedTitle_ShouldAcceptValidTitles(string title)
    {
        // Arrange
        var synthesis = CreateValidSynthesis();

        // Act
        synthesis.MergedTitle = title;

        // Assert
        synthesis.MergedTitle.Should().Be(title);
    }

    [Fact]
    public void Synthesis_MergedTitle_ShouldRespectMaxLength()
    {
        // Arrange - Max 100 chars per spec
        var synthesis = CreateValidSynthesis();
        var longTitle = new string('A', 100);

        // Act
        synthesis.MergedTitle = longTitle;

        // Assert
        synthesis.MergedTitle.Length.Should().BeLessThanOrEqualTo(100);
    }

    #endregion

    #region MergedDescription Tests

    [Fact]
    public void Synthesis_MergedDescription_ShouldBeUpdateable()
    {
        // Arrange
        var synthesis = CreateValidSynthesis();
        var newDescription = "Updated description explaining the unified concept.";

        // Act
        synthesis.MergedDescription = newDescription;

        // Assert
        synthesis.MergedDescription.Should().Be(newDescription);
    }

    [Fact]
    public void Synthesis_MergedDescription_ShouldRespectMaxLength()
    {
        // Arrange - Max 1000 chars per spec
        var synthesis = CreateValidSynthesis();
        var longDescription = new string('A', 1000);

        // Act
        synthesis.MergedDescription = longDescription;

        // Assert
        synthesis.MergedDescription.Length.Should().BeLessThanOrEqualTo(1000);
    }

    #endregion

    #region ThematicBridge Tests

    [Fact]
    public void Synthesis_ThematicBridge_ShouldExplainConnection()
    {
        // Arrange & Act
        var synthesis = CreateValidSynthesis();

        // Assert - Thematic bridge should be meaningful text
        synthesis.ThematicBridge.Should().NotBeNullOrWhiteSpace();
        synthesis.ThematicBridge.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Synthesis_ThematicBridge_ShouldBeUpdateable()
    {
        // Arrange
        var synthesis = CreateValidSynthesis();
        var newBridge = "The common thread is user empowerment through technology.";

        // Act
        synthesis.ThematicBridge = newBridge;

        // Assert
        synthesis.ThematicBridge.Should().Be(newBridge);
    }

    #endregion

    #region RetainedElements Tests

    [Fact]
    public void Synthesis_RetainedElements_ShouldBeUpdateable()
    {
        // Arrange
        var synthesis = CreateValidSynthesis();
        var newElements = new Dictionary<Guid, List<string>>
        {
            [Guid.NewGuid()] = ["New Element 1", "New Element 2"]
        };

        // Act
        synthesis.RetainedElements = newElements;

        // Assert
        synthesis.RetainedElements.Should().ContainSingle();
        synthesis.RetainedElements.Values.First().Should().HaveCount(2);
    }

    [Fact]
    public void Synthesis_RetainedElements_ShouldAllowMultipleSources()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        
        var synthesis = new Synthesis
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            SourceIdeaIds = [id1, id2, id3],
            MergedTitle = "Multi-source Synthesis",
            MergedDescription = "Combining three ideas into one.",
            ThematicBridge = "Common innovation theme",
            RetainedElements = new Dictionary<Guid, List<string>>
            {
                [id1] = ["Element A", "Element B"],
                [id2] = ["Element C"],
                [id3] = ["Element D", "Element E", "Element F"]
            },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        synthesis.RetainedElements.Should().HaveCount(3);
        synthesis.RetainedElements[id1].Should().HaveCount(2);
        synthesis.RetainedElements[id2].Should().HaveCount(1);
        synthesis.RetainedElements[id3].Should().HaveCount(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Synthesis_WithMinimumTwoSources_ShouldBeValid()
    {
        // Arrange - Minimum 2 sources required
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var synthesis = new Synthesis
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            SourceIdeaIds = [id1, id2],
            MergedTitle = "Two-Idea Synthesis",
            MergedDescription = "Combining exactly two ideas.",
            ThematicBridge = "Shared theme between both",
            RetainedElements = new Dictionary<Guid, List<string>>
            {
                [id1] = ["From first"],
                [id2] = ["From second"]
            },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        synthesis.SourceIdeaIds.Should().HaveCount(2);
        synthesis.RetainedElements.Should().HaveCount(2);
    }

    [Fact]
    public void Synthesis_WithMaximumTenSources_ShouldBeValid()
    {
        // Arrange - Maximum 10 sources allowed
        var sourceIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var retainedElements = sourceIds.ToDictionary(
            id => id,
            id => new List<string> { $"Element from {id.ToString()[..8]}" });

        // Act
        var synthesis = new Synthesis
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            SourceIdeaIds = sourceIds,
            MergedTitle = "Ten-Idea Mega Synthesis",
            MergedDescription = "Combining the maximum of ten ideas into one concept.",
            ThematicBridge = "Innovation and user value unite all ten concepts",
            RetainedElements = retainedElements,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        synthesis.SourceIdeaIds.Should().HaveCount(10);
        synthesis.RetainedElements.Should().HaveCount(10);
    }

    [Fact]
    public void Synthesis_EmptyRetainedElements_ShouldBeAllowed()
    {
        // Arrange - Edge case: synthesis with no retained elements tracked
        var synthesis = new Synthesis
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            SourceIdeaIds = [Guid.NewGuid(), Guid.NewGuid()],
            MergedTitle = "Minimal Synthesis",
            MergedDescription = "Simple merge",
            ThematicBridge = "Common theme",
            RetainedElements = new Dictionary<Guid, List<string>>(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        synthesis.RetainedElements.Should().BeEmpty();
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public void Synthesis_ShouldMapToDto_WithAllFields()
    {
        // Arrange
        var synthesis = CreateValidSynthesis();

        // Act - Simulate DTO mapping
        var dto = new
        {
            synthesis.Id,
            synthesis.SessionId,
            SourceIdeaIds = synthesis.SourceIdeaIds.AsReadOnly(),
            synthesis.MergedTitle,
            synthesis.MergedDescription,
            synthesis.ThematicBridge,
            RetainedElements = synthesis.RetainedElements.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly()),
            synthesis.CreatedAt
        };

        // Assert
        dto.Id.Should().Be(synthesis.Id);
        dto.SessionId.Should().Be(synthesis.SessionId);
        dto.SourceIdeaIds.Should().BeEquivalentTo(synthesis.SourceIdeaIds);
        dto.MergedTitle.Should().Be(synthesis.MergedTitle);
        dto.MergedDescription.Should().Be(synthesis.MergedDescription);
        dto.ThematicBridge.Should().Be(synthesis.ThematicBridge);
        dto.RetainedElements.Should().HaveCount(synthesis.RetainedElements.Count);
        dto.CreatedAt.Should().Be(synthesis.CreatedAt);
    }

    #endregion
}
