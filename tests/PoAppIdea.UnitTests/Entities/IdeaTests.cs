using PoAppIdea.Core.Entities;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Idea entity domain logic.
/// Focus: Pure logic validation, no external dependencies.
/// </summary>
public sealed class IdeaTests
{
    private static Idea CreateValidIdea() => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        Title = "Test App Idea",
        Description = "A mobile app for tracking habits",
        BatchNumber = 1,
        GenerationPrompt = "Generate a habit tracking app idea",
        DnaKeywords = ["tracking", "habits", "productivity"],
        Score = 0f,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void Idea_ShouldInitialize_WithValidDefaults()
    {
        // Arrange & Act
        var idea = CreateValidIdea();

        // Assert
        idea.Id.Should().NotBeEmpty();
        idea.SessionId.Should().NotBeEmpty();
        idea.Title.Should().NotBeNullOrWhiteSpace();
        idea.Description.Should().NotBeNullOrWhiteSpace();
        idea.BatchNumber.Should().BeInRange(1, 5);
        idea.Score.Should().Be(0f);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Idea_BatchNumber_ShouldBeValid(int batchNumber)
    {
        // Arrange & Act
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Title = "Batch Test",
            Description = "Testing batch numbers",
            BatchNumber = batchNumber,
            GenerationPrompt = "Test prompt",
            DnaKeywords = ["test"],
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        idea.BatchNumber.Should().Be(batchNumber);
    }

    [Fact]
    public void Idea_Score_ShouldBe_Updateable()
    {
        // Arrange
        var idea = CreateValidIdea();

        // Act
        idea.Score = 85.5f;

        // Assert
        idea.Score.Should().Be(85.5f);
    }

    [Fact]
    public void Idea_DnaKeywords_ShouldMaintain_KeywordList()
    {
        // Arrange
        var idea = new Idea
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Title = "Keyword-rich App",
            Description = "App with DNA keywords",
            BatchNumber = 1,
            GenerationPrompt = "Generate app with keywords",
            DnaKeywords = ["ai", "machine-learning", "automation", "productivity"],
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        idea.DnaKeywords.Should().HaveCount(4);
        idea.DnaKeywords.Should().Contain("machine-learning");
    }

    [Fact]
    public void Idea_Title_ShouldBe_Updateable()
    {
        // Arrange
        var idea = CreateValidIdea();
        var newTitle = "Updated Title";

        // Act
        idea.Title = newTitle;

        // Assert
        idea.Title.Should().Be(newTitle);
    }

    [Fact]
    public void Idea_Description_ShouldBe_Updateable()
    {
        // Arrange
        var idea = CreateValidIdea();
        var newDescription = "Updated description for the app";

        // Act
        idea.Description = newDescription;

        // Assert
        idea.Description.Should().Be(newDescription);
    }
}
