using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Mutation entity and evolution logic.
/// Focus: Mutation types, scoring.
/// </summary>
public sealed class MutationTests
{
    private static Mutation CreateValidMutation(
        MutationType type = MutationType.Crossover,
        List<Guid>? parentIds = null) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        ParentIdeaIds = parentIds ?? [Guid.NewGuid(), Guid.NewGuid()],
        MutationType = type,
        Title = "Mutated Idea",
        Description = "A mutation combining two ideas",
        MutationRationale = "Combined best features from both parent ideas",
        Score = 0f,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Theory]
    [InlineData(MutationType.Crossover)]
    [InlineData(MutationType.Repurposing)]
    public void Mutation_ShouldSupport_AllMutationTypes(MutationType mutationType)
    {
        // Arrange & Act
        var mutation = CreateValidMutation(type: mutationType);

        // Assert
        mutation.MutationType.Should().Be(mutationType);
    }

    [Fact]
    public void Mutation_Crossover_ShouldHave_TwoParents()
    {
        // Arrange - Crossover requires two parent ideas
        var parentA = Guid.NewGuid();
        var parentB = Guid.NewGuid();

        var mutation = new Mutation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            ParentIdeaIds = [parentA, parentB],
            MutationType = MutationType.Crossover,
            Title = "Hybrid App",
            Description = "Combining features from both parents",
            MutationRationale = "Merged AI capabilities with social features",
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        mutation.ParentIdeaIds.Should().HaveCount(2);
        mutation.ParentIdeaIds.Should().Contain(parentA);
        mutation.ParentIdeaIds.Should().Contain(parentB);
        mutation.MutationType.Should().Be(MutationType.Crossover);
    }

    [Fact]
    public void Mutation_Repurposing_ShouldHave_SingleSource()
    {
        // Arrange - Repurposing uses single idea for different domain
        var sourceIdea = Guid.NewGuid();

        var mutation = new Mutation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            ParentIdeaIds = [sourceIdea],
            MutationType = MutationType.Repurposing,
            Title = "Repurposed Concept",
            Description = "Same core idea applied to healthcare",
            MutationRationale = "Applied productivity concepts to healthcare domain",
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        mutation.ParentIdeaIds.Should().HaveCount(1);
        mutation.ParentIdeaIds.Should().Contain(sourceIdea);
        mutation.MutationType.Should().Be(MutationType.Repurposing);
    }

    [Fact]
    public void Mutation_Score_ShouldBe_Updateable()
    {
        // Arrange
        var mutation = CreateValidMutation();

        // Act
        mutation.Score = 92.5f;

        // Assert
        mutation.Score.Should().Be(92.5f);
    }

    [Fact]
    public void Mutation_Title_ShouldBe_Updateable()
    {
        // Arrange
        var mutation = CreateValidMutation();
        var newTitle = "Updated Mutation Title";

        // Act
        mutation.Title = newTitle;

        // Assert
        mutation.Title.Should().Be(newTitle);
    }

    [Fact]
    public void Mutation_Description_ShouldBe_Updateable()
    {
        // Arrange
        var mutation = CreateValidMutation();
        var newDescription = "Updated description explaining the mutation";

        // Act
        mutation.Description = newDescription;

        // Assert
        mutation.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Mutation_MutationRationale_ShouldExplain_WhyMutationOccurred()
    {
        // Arrange & Act
        var mutation = new Mutation
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            ParentIdeaIds = [Guid.NewGuid()],
            MutationType = MutationType.Repurposing,
            Title = "Healthcare App",
            Description = "Healthcare tracking application",
            MutationRationale = "Repurposed fitness tracking for patient monitoring",
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        mutation.MutationRationale.Should().NotBeNullOrWhiteSpace();
        mutation.MutationRationale.Should().Contain("Repurposed");
    }
}
