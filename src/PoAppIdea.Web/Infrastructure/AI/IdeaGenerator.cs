using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Shared.Constants;
using System.Text.Json;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Generates ideas using Azure OpenAI GPT-4o.
/// </summary>
public sealed class IdeaGenerator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly JsonSerializerOptions _jsonOptions;

    public IdeaGenerator(Kernel kernel)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Generates initial batch of ideas based on app type and complexity.
    /// </summary>
    public async Task<IReadOnlyList<Idea>> GenerateInitialIdeasAsync(
        Guid sessionId,
        AppType appType,
        int complexityLevel,
        ProductPersonality? personality,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildInitialPrompt(appType, complexityLevel, personality);
        return await GenerateIdeasFromPromptAsync(sessionId, prompt, batchNumber: 1, cancellationToken);
    }

    /// <summary>
    /// Generates mutated ideas based on swiped ideas using crossover or repurposing.
    /// </summary>
    public async Task<IReadOnlyList<Idea>> GenerateMutatedIdeasAsync(
        Guid sessionId,
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        int batchNumber,
        MutationType mutationType,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildMutationPrompt(likedIdeas, dislikedIdeas, mutationType);
        return await GenerateIdeasFromPromptAsync(sessionId, prompt, batchNumber, cancellationToken);
    }

    /// <summary>
    /// Generates synthesized idea from top candidates.
    /// </summary>
    public async Task<Synthesis> GenerateSynthesisAsync(
        Guid sessionId,
        IReadOnlyList<Idea> topIdeas,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildSynthesisPrompt(topIdeas);
        var history = new ChatHistory();
        history.AddSystemMessage(GetSynthesisSystemPrompt());
        history.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
        return ParseSynthesis(sessionId, topIdeas, response.Content ?? "");
    }

    private async Task<IReadOnlyList<Idea>> GenerateIdeasFromPromptAsync(
        Guid sessionId,
        string prompt,
        int batchNumber,
        CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(GetIdeaSystemPrompt());
        history.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
        return ParseIdeas(sessionId, response.Content ?? "", batchNumber);
    }

    private static string GetIdeaSystemPrompt() => """
        You are an innovative product ideation assistant. Generate exactly 10 unique app ideas in JSON format.
        Each idea should have:
        - title: A catchy, memorable name (3-5 words)
        - description: A compelling one-paragraph pitch (50-100 words)
        - dnaKeywords: 5-7 keywords capturing the idea's essence
        
        Output format (JSON array only, no markdown):
        [{"title": "...", "description": "...", "dnaKeywords": ["...", "..."]}]
        """;

    private static string GetSynthesisSystemPrompt() => """
        You are a product synthesis expert. Combine the best elements of multiple app ideas into one cohesive concept.
        Focus on creating a unique value proposition that retains the strongest elements while maintaining coherence.
        
        Output format (JSON only, no markdown):
        {"mergedTitle": "...", "thematicBridge": "...", "retainedElements": ["...", "..."]}
        """;

    private static string BuildInitialPrompt(AppType appType, int complexityLevel, ProductPersonality? personality)
    {
        var complexityDesc = complexityLevel switch
        {
            1 => "simple, weekend project",
            2 => "moderate, 1-2 week project",
            3 => "complex, month-long project",
            4 => "advanced, multi-month project",
            5 => "enterprise-grade, extensive project",
            _ => "moderate complexity"
        };

        var preferenceSection = "";
        if (personality is not null)
        {
            // Build product preferences (top 5 positive biases)
            var topProductPrefs = personality.ProductBiases
                .Where(kvp => kvp.Value > 0.2f)
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => kvp.Key);

            // Build technical preferences (top 3 positive biases)
            var topTechPrefs = personality.TechnicalBiases
                .Where(kvp => kvp.Value > 0.2f)
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key);

            // Build avoidance list
            var avoidPatterns = personality.DislikedPatterns.Take(5);

            var prefsBuilder = new System.Text.StringBuilder();
            
            if (topProductPrefs.Any())
            {
                prefsBuilder.AppendLine($"User PREFERS themes like: {string.Join(", ", topProductPrefs)}");
            }

            if (topTechPrefs.Any())
            {
                prefsBuilder.AppendLine($"User PREFERS technical approaches: {string.Join(", ", topTechPrefs)}");
            }

            if (avoidPatterns.Any())
            {
                prefsBuilder.AppendLine($"User DISLIKES patterns like: {string.Join(", ", avoidPatterns)} - avoid these!");
            }

            if (prefsBuilder.Length > 0)
            {
                preferenceSection = $"\n\nUSER PREFERENCES (learned from previous sessions):\n{prefsBuilder}";
            }
        }

        return $"""
            Generate 10 innovative {appType} app ideas.
            Complexity: {complexityDesc}
            {preferenceSection}
            
            Focus on unique, implementable concepts that solve real problems.
            """;
    }

    private static string BuildMutationPrompt(
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        MutationType mutationType)
    {
        var likedSummary = string.Join("\n", likedIdeas.Select(i => $"- {i.Title}: {i.Description}"));
        var dislikedPatterns = dislikedIdeas.Any()
            ? $"Avoid patterns from: {string.Join(", ", dislikedIdeas.SelectMany(i => i.DnaKeywords).Distinct().Take(10))}"
            : "";

        var mutationInstruction = mutationType switch
        {
            MutationType.Crossover => "Combine elements from multiple liked ideas to create hybrid concepts.",
            MutationType.Repurposing => "Take core mechanics from liked ideas and apply them to different domains.",
            _ => "Evolve the ideas creatively."
        };

        return $"""
            Generate 10 new app ideas based on these preferences:
            
            LIKED IDEAS:
            {likedSummary}
            
            {dislikedPatterns}
            
            Strategy: {mutationInstruction}
            """;
    }

    private static string BuildSynthesisPrompt(IReadOnlyList<Idea> topIdeas)
    {
        var ideaSummary = string.Join("\n", topIdeas.Select(i => 
            $"- {i.Title}: {i.Description} [Keywords: {string.Join(", ", i.DnaKeywords)}]"));

        return $"""
            Synthesize these top ideas into one cohesive product concept:
            
            {ideaSummary}
            
            Create a unified vision that captures the best elements.
            """;
    }

    private IReadOnlyList<Idea> ParseIdeas(Guid sessionId, string content, int batchNumber)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<List<IdeaJson>>(content, _jsonOptions);
            if (parsed is null) return GenerateFallbackIdeas(sessionId, batchNumber);

            return parsed.Select(p => new Idea
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Title = p.Title ?? "Untitled Idea",
                Description = p.Description ?? "No description available.",
                BatchNumber = batchNumber,
                GenerationPrompt = "AI-generated idea batch",
                DnaKeywords = p.DnaKeywords ?? [],
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
        }
        catch
        {
            return GenerateFallbackIdeas(sessionId, batchNumber);
        }
    }

    private static IReadOnlyList<Idea> GenerateFallbackIdeas(Guid sessionId, int batchNumber)
    {
        return Enumerable.Range(1, AppConstants.IdeasPerBatch)
            .Select(i => new Idea
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Title = $"Generated Idea {i}",
                Description = "An innovative app concept.",
                BatchNumber = batchNumber,
                GenerationPrompt = "Fallback idea generation",
                DnaKeywords = ["innovative", "app"],
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
    }

    private Synthesis ParseSynthesis(Guid sessionId, IReadOnlyList<Idea> sourceIdeas, string content)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<SynthesisJson>(content, _jsonOptions);
            var retainedElementsDict = sourceIdeas.ToDictionary(
                i => i.Id,
                i => i.DnaKeywords.Take(3).ToList());
            
            return new Synthesis
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SourceIdeaIds = sourceIdeas.Select(i => i.Id).ToList(),
                MergedTitle = parsed?.MergedTitle ?? "Synthesized Concept",
                MergedDescription = "A unified product concept combining the best elements of selected ideas.",
                ThematicBridge = parsed?.ThematicBridge ?? "A unified product vision.",
                RetainedElements = retainedElementsDict,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        catch
        {
            var retainedElementsDict = sourceIdeas.ToDictionary(
                i => i.Id,
                i => i.DnaKeywords.Take(3).ToList());
            
            return new Synthesis
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SourceIdeaIds = sourceIdeas.Select(i => i.Id).ToList(),
                MergedTitle = "Synthesized Concept",
                MergedDescription = "A unified product concept combining the best elements of selected ideas.",
                ThematicBridge = "A unified product vision combining the best elements.",
                RetainedElements = retainedElementsDict,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
    }

    private sealed record IdeaJson(string? Title, string? Description, List<string>? DnaKeywords);
    private sealed record SynthesisJson(string? MergedTitle, string? ThematicBridge, List<string>? RetainedElements);
}
