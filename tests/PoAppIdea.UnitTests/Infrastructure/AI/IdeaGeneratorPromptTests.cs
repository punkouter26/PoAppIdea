using System.Reflection;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Infrastructure.AI;

/// <summary>
/// Unit tests for IdeaGenerator prompt construction.
/// Focus: Validate that prompts contain key innovation guidance and constraints.
/// </summary>
public sealed class IdeaGeneratorPromptTests
{
    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_InnovationFrameworks()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("SCAMPER", "should reference SCAMPER framework");
        systemPrompt.Should().Contain("Jobs-to-be-Done", "should reference Jobs-to-be-Done framework");
        systemPrompt.Should().Contain("Blue Ocean", "should reference Blue Ocean Strategy");
    }

    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_QualityCriteria()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("SPECIFIC", "should emphasize specificity");
        systemPrompt.Should().Contain("UNIQUE", "should emphasize uniqueness");
        systemPrompt.Should().Contain("FEASIBLE", "should emphasize feasibility");
        systemPrompt.Should().Contain("VALUABLE", "should emphasize value");
    }

    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_Examples()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("GOOD EXAMPLES", "should include good examples section");
        systemPrompt.Should().Contain("AVOID GENERIC IDEAS", "should include bad examples section");
    }

    [Theory]
    [InlineData(AppType.WebApp, "browser-based")]
    [InlineData(AppType.MobileApp, "mobile-first")]
    [InlineData(AppType.ConsoleApp, "developer tools")]
    [InlineData(AppType.UnityApp, "interactive 3D")]
    public void BuildInitialPrompt_ShouldContain_AppTypeGuidance(AppType appType, string expectedKeyword)
    {
        // Arrange & Act
        var prompt = InvokeBuildInitialPrompt(appType, 2, null);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain(expectedKeyword, $"should contain guidance for {appType}");
    }

    [Theory]
    [InlineData(1, "weekend project")]
    [InlineData(2, "1-2 week project")]
    [InlineData(3, "month-long project")]
    [InlineData(4, "multi-month project")]
    [InlineData(5, "enterprise-grade")]
    public void BuildInitialPrompt_ShouldContain_ComplexityLevel(int complexity, string expectedDescription)
    {
        // Arrange & Act
        var prompt = InvokeBuildInitialPrompt(AppType.WebApp, complexity, null);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain(expectedDescription, $"should describe complexity level {complexity}");
    }

    [Fact]
    public void BuildInitialPrompt_ShouldContain_InnovationRequirements()
    {
        // Arrange & Act
        var prompt = InvokeBuildInitialPrompt(AppType.WebApp, 2, null);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("INNOVATION REQUIREMENTS", "should include innovation section");
        prompt.Should().Contain("SPECIFIC niche", "should emphasize niche targeting");
        prompt.Should().Contain("CONCRETE pain point", "should emphasize concrete problems");
        prompt.Should().Contain("UNIQUE approach", "should emphasize uniqueness");
    }

    [Fact]
    public void BuildInitialPrompt_ShouldContain_CreativityTechniques()
    {
        // Arrange & Act
        var prompt = InvokeBuildInitialPrompt(AppType.WebApp, 2, null);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("CREATIVITY TECHNIQUES", "should include creativity section");
        prompt.Should().Contain("SCAMPER", "should reference SCAMPER");
        prompt.Should().Contain("Lateral Thinking", "should reference lateral thinking");
    }

    [Fact]
    public void BuildInitialPrompt_WithPersonality_ShouldInclude_UserPreferences()
    {
        // Arrange
        var personality = new ProductPersonality
        {
            UserId = Guid.NewGuid(),
            ProductBiases = new Dictionary<string, float>
            {
                ["productivity"] = 0.8f,
                ["ai-powered"] = 0.6f,
                ["collaboration"] = 0.4f
            },
            TechnicalBiases = new Dictionary<string, float>
            {
                ["cloud-native"] = 0.7f,
                ["real-time"] = 0.5f
            },
            DislikedPatterns = new List<string> { "social media", "e-commerce" },
            SwipeSpeedProfile = new SpeedProfile
            {
                AverageFastMs = 500,
                AverageMediumMs = 1500,
                AverageSlowMs = 3000
            },
            TotalSessions = 1,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var prompt = InvokeBuildInitialPrompt(AppType.WebApp, 2, personality);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("USER PREFERENCES", "should include user preferences section");
        prompt.Should().Contain("productivity", "should include top product preference");
        prompt.Should().Contain("AVOID", "should include avoidance guidance");
    }

    [Theory]
    [InlineData(MutationType.Crossover, "CROSSOVER")]
    [InlineData(MutationType.Repurposing, "REPURPOSING")]
    public void BuildMutationPrompt_ShouldContain_MutationStrategy(MutationType mutationType, string expectedKeyword)
    {
        // Arrange
        var likedIdeas = new List<Idea>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                Title = "AI Fitness Coach",
                Description = "Personal AI coach for adaptive workouts",
                DnaKeywords = new List<string> { "fitness", "ai", "coaching" },
                BatchNumber = 1,
                GenerationPrompt = "test",
                Score = 0.8f,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        var dislikedIdeas = new List<Idea>();

        // Act
        var prompt = InvokeBuildMutationPrompt(likedIdeas, dislikedIdeas, mutationType);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain(expectedKeyword, $"should describe {mutationType} strategy");
        prompt.Should().Contain("AI Fitness Coach", "should reference liked ideas");
    }

    [Fact]
    public void BuildMutationPrompt_WithDislikedIdeas_ShouldInclude_AvoidancePatterns()
    {
        // Arrange
        var likedIdeas = new List<Idea>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                Title = "Liked Idea",
                Description = "A good idea",
                DnaKeywords = new List<string> { "good", "innovation" },
                BatchNumber = 1,
                GenerationPrompt = "test",
                Score = 0.8f,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        var dislikedIdeas = new List<Idea>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                Title = "Disliked Idea",
                Description = "A bad idea",
                DnaKeywords = new List<string> { "social-media", "generic" },
                BatchNumber = 1,
                GenerationPrompt = "test",
                Score = 0.2f,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        // Act
        var prompt = InvokeBuildMutationPrompt(likedIdeas, dislikedIdeas, MutationType.Crossover);

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("AVOID", "should include avoidance section");
    }

    // Helper methods to invoke private static methods using reflection
    private static MethodInfo? GetPrivateStaticMethod(string methodName)
    {
        var ideaGeneratorType = typeof(PoAppIdea.Web.Infrastructure.AI.IdeaGenerator);
        return ideaGeneratorType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
    }

    private static string InvokeBuildInitialPrompt(AppType appType, int complexity, ProductPersonality? personality)
    {
        var method = GetPrivateStaticMethod("BuildInitialPrompt");
        var result = method?.Invoke(null, new object?[] { appType, complexity, personality });
        return result as string ?? string.Empty;
    }

    private static string InvokeBuildMutationPrompt(
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        MutationType mutationType)
    {
        var method = GetPrivateStaticMethod("BuildMutationPrompt");
        var result = method?.Invoke(null, new object?[] { likedIdeas, dislikedIdeas, mutationType });
        return result as string ?? string.Empty;
    }
}
