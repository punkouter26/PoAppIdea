using System.Reflection;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Infrastructure.AI;

/// <summary>
/// Unit tests for IdeaGenerator prompt construction.
/// Focus: Validate that prompts contain key innovation guidance and constraints.
/// 
/// Note: These tests use reflection to access private static methods for testing prompt
/// construction logic. This approach is acceptable here because:
/// 1. The prompt building logic is implementation detail that doesn't need public exposure
/// 2. We want to ensure prompts contain specific keywords/patterns for AI effectiveness
/// 3. Alternative approaches (making methods public/internal) would expose implementation details
/// </summary>
public sealed class IdeaGeneratorPromptTests
{
    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_InnovationFrameworks()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert - Prompt uses streamlined keywords for AI effectiveness
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("INNOVATIVE", "should emphasize innovation");
        systemPrompt.Should().Contain("SPECIFIC", "should reference specificity for user personas");
        systemPrompt.Should().Contain("UNIQUE", "should reference uniqueness");
    }

    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_QualityCriteria()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert - Streamlined prompt uses focused quality keywords
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("SPECIFIC", "should emphasize specificity");
        systemPrompt.Should().Contain("UNIQUE", "should emphasize uniqueness");
        systemPrompt.Should().Contain("CONCRETE", "should emphasize concrete pain points");
        systemPrompt.Should().Contain("INNOVATIVE", "should emphasize innovation");
    }

    [Fact]
    public void GetIdeaSystemPrompt_ShouldContain_AvoidanceGuidance()
    {
        // Arrange & Act
        var systemPrompt = GetPrivateStaticMethod("GetIdeaSystemPrompt")?.Invoke(null, null) as string;

        // Assert - Prompt includes anti-pattern guidance inline
        systemPrompt.Should().NotBeNullOrWhiteSpace();
        systemPrompt.Should().Contain("Avoid generic ideas", "should include avoidance guidance");
        systemPrompt.Should().Contain("JSON array", "should specify output format");
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

        // Assert - Streamlined prompt embeds innovation requirements directly
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("innovative", "should emphasize innovation");
        prompt.Should().Contain("niche", "should emphasize niche targeting");
        prompt.Should().Contain("distinct", "should emphasize distinctness");
        prompt.Should().Contain("feasible", "should emphasize feasibility");
    }

    [Fact]
    public void BuildInitialPrompt_ShouldContain_AppTypeAndComplexity()
    {
        // Arrange & Act
        var prompt = InvokeBuildInitialPrompt(AppType.WebApp, 2, null);

        // Assert - Prompt includes app type guidance and complexity description
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("WebApp", "should reference app type");
        prompt.Should().Contain("1-2 week project", "should reference complexity");
        prompt.Should().Contain("Generate 5", "should request 5 ideas");
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
        prompt.Should().Contain("DISLIKES", "should include dislike avoidance guidance");
    }

    [Theory]
    [InlineData(MutationType.Crossover, "Crossover")]
    [InlineData(MutationType.Repurposing, "Repurposing")]
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
        prompt.Should().Contain("Avoid", "should include avoidance section");
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
