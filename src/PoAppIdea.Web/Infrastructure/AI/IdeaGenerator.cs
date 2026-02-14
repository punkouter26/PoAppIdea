using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Shared.Constants;
using System.Text.Json;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Generates ideas using Azure OpenAI GPT-4o.
/// Pattern: Strategy Pattern - Encapsulates AI generation algorithm, allowing different prompting strategies.
/// </summary>
public sealed class IdeaGenerator : IIdeaGenerator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly AiResponseCache _cache;
    private readonly ILogger<IdeaGenerator> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public IdeaGenerator(Kernel kernel, AiResponseCache cache, ILogger<IdeaGenerator> logger)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _cache = cache;
        _logger = logger;
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

    // Optimization 9: Synthesis is now handled exclusively by SynthesisEngine
    // (removed duplicate GenerateSynthesisAsync to avoid redundant token spend)

    private async Task<IReadOnlyList<Idea>> GenerateIdeasFromPromptAsync(
        Guid sessionId,
        string prompt,
        int batchNumber,
        CancellationToken cancellationToken)
    {
        // CANCELLATION TOKEN FIX: Check for cancellation early
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Calling Azure OpenAI for session {SessionId}, batch {BatchNumber}", sessionId, batchNumber);
            
            var history = new ChatHistory();
            history.AddSystemMessage(GetIdeaSystemPrompt());
            history.AddUserMessage(prompt);

            // Optimization 8: Reduced from 10→5 ideas, max_tokens 4000→2000
            // Optimization 10: JSON mode guarantees valid JSON, eliminating repair passes
            var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.9,
                    ["max_tokens"] = 2000,
                    ["top_p"] = 0.95,
                    ["frequency_penalty"] = 0.3,
                    ["presence_penalty"] = 0.3
                }
            };
            
            // Optimization 3: Check cache before calling AI
            var cacheKey = $"{GetIdeaSystemPrompt()}|{prompt}";
            var responseContent = await _cache.GetOrGenerateAsync(
                "ideas",
                cacheKey,
                async () =>
                {
                    var result = await _chatService.GetChatMessageContentAsync(history, executionSettings, cancellationToken: cancellationToken);
                    return result.Content ?? "";
                },
                cancellationToken: cancellationToken);
            
            // CANCELLATION TOKEN FIX: Check after async operation
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogDebug("AI response received (possibly cached), content length: {Length}", responseContent?.Length ?? 0);
            
            if (string.IsNullOrEmpty(responseContent))
            {
                _logger.LogWarning("Azure OpenAI returned empty content for session {SessionId}", sessionId);
                return GenerateFallbackIdeas(sessionId, batchNumber);
            }

            // Optimization 5: Local JSON repair instead of expensive AI repair pass
            var parsedIdeas = TryParseIdeas(sessionId, responseContent, batchNumber, "AI-generated idea batch");
            if (parsedIdeas is not null)
            {
                return parsedIdeas;
            }

            _logger.LogWarning("Initial AI output was not valid JSON for session {SessionId}; attempting local repair", sessionId);
            
            // Local JSON repair: fix common issues without calling AI again
            var locallyRepaired = TryLocalJsonRepair(responseContent);
            if (!string.IsNullOrWhiteSpace(locallyRepaired))
            {
                parsedIdeas = TryParseIdeas(sessionId, locallyRepaired, batchNumber, "locally-repaired idea batch");
                if (parsedIdeas is not null)
                {
                    return parsedIdeas;
                }
            }

            _logger.LogWarning("JSON repair failed for session {SessionId}; using fallback ideas", sessionId);
            return GenerateFallbackIdeas(sessionId, batchNumber);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Idea generation cancelled for session {SessionId}", sessionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI call failed for session {SessionId}, batch {BatchNumber}. Using fallback ideas.", sessionId, batchNumber);
            return GenerateFallbackIdeas(sessionId, batchNumber);
        }
    }

    /// <summary>
    /// Optimization 5: Local JSON repair — fixes common AI JSON issues without an expensive AI call.
    /// Handles: markdown fences, trailing commas, incomplete arrays, JSON wrapped in objects.
    /// </summary>
    private static string? TryLocalJsonRepair(string rawContent)
    {
        try
        {
            var content = rawContent.Trim();

            // Strip markdown code fences
            if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                content = content[7..];
            else if (content.StartsWith("```"))
                content = content[3..];
            if (content.EndsWith("```"))
                content = content[..^3];
            content = content.Trim();

            // If wrapped in an object (JSON mode), extract the array property
            if (content.StartsWith('{'))
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return prop.Value.GetRawText();
                    }
                }
            }

            // Extract JSON array
            var start = content.IndexOf('[');
            var end = content.LastIndexOf(']');
            if (start >= 0 && end > start)
            {
                content = content[start..(end + 1)];
            }

            // Fix trailing commas before ] or }
            content = System.Text.RegularExpressions.Regex.Replace(content, @",\s*([\]\}])", "$1");

            // Validate it parses
            var options = new System.Text.Json.JsonDocumentOptions { AllowTrailingCommas = true };
            using var _ = System.Text.Json.JsonDocument.Parse(content, options);
            return content;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Trimmed system prompt (~300 tokens vs ~1200 tokens). Removed verbose framework explanations,
    /// anti-pattern examples, and good examples. The model generates creative ideas without those.
    /// </summary>
    private static string GetIdeaSystemPrompt() => """
        You are an expert product ideation assistant. Generate INNOVATIVE app ideas.
        Each idea must target a SPECIFIC user persona, solve a CONCRETE pain point, and offer a UNIQUE approach.
        Avoid generic ideas (todo apps, social media, e-commerce, fitness trackers).
        
        Each idea: title (2-4 words), description (WHO/WHAT/HOW/WHY, 60-120 words), dnaKeywords (5-7 precise keywords).
        
        Return ONLY a JSON array: [{"title":"...","description":"...","dnaKeywords":["...","..."]}]
        """;

    // Synthesis is now exclusively handled by SynthesisEngine (Optimization 9)

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

        var appTypeGuidance = appType switch
        {
            AppType.WebApp => "Focus on browser-based solutions with responsive UIs. Consider SaaS, dashboards, collaborative tools, or content platforms.",
            AppType.MobileApp => "Focus on mobile-first experiences. Consider location-based, camera/sensor integration, push notifications, or on-the-go workflows.",
            AppType.ConsoleApp => "Focus on developer tools, automation scripts, CLI utilities, or batch processing. Consider DevOps, data pipelines, or system administration.",
            AppType.UnityApp => "Focus on interactive 3D/2D experiences. Consider games, simulations, AR/VR, visualization, or educational experiences.",
            _ => "Focus on practical, user-centered solutions."
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
                prefsBuilder.AppendLine($"User STRONGLY PREFERS these themes: {string.Join(", ", topProductPrefs)}");
                prefsBuilder.AppendLine("Weight these heavily in your ideas.");
            }

            if (topTechPrefs.Any())
            {
                prefsBuilder.AppendLine($"User PREFERS these technical approaches: {string.Join(", ", topTechPrefs)}");
                prefsBuilder.AppendLine("Incorporate these where relevant.");
            }

            if (avoidPatterns.Any())
            {
                prefsBuilder.AppendLine($"User DISLIKES these patterns: {string.Join(", ", avoidPatterns)}");
                prefsBuilder.AppendLine("AVOID these completely!");
            }

            if (prefsBuilder.Length > 0)
            {
                preferenceSection = $"\n\n🎯 USER PREFERENCES (from previous sessions - IMPORTANT):\n{prefsBuilder}";
            }
        }

        return $"""
            Generate 5 innovative {appType} app ideas.
            Complexity: {complexityDesc}. {appTypeGuidance}
            Each idea must be distinct, specific to a niche user group, and technically feasible.
            {preferenceSection}
            """;
    }

    private static string BuildMutationPrompt(
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        MutationType mutationType)
    {
        // Optimization 4: Send only titles + top keywords instead of full descriptions
        var likedSummary = string.Join("; ", likedIdeas.Select(i => $"{i.Title} [{string.Join(", ", i.DnaKeywords.Take(4))}]"));
        var dislikedKeywords = dislikedIdeas.SelectMany(i => i.DnaKeywords).Distinct().Take(10).ToList();
        var dislikedPatterns = dislikedKeywords.Any()
            ? $"Avoid: {string.Join(", ", dislikedKeywords)}"
            : "";

        var strategy = mutationType switch
        {
            MutationType.Crossover => "Crossover: merge complementary features from liked ideas into hybrids.",
            MutationType.Repurposing => "Repurposing: apply liked ideas' core mechanics to different domains/users.",
            _ => "Evolve creatively."
        };

        return $"""
            Generate 5 evolved app ideas. Strategy: {strategy}
            Liked: {likedSummary}
            {dislikedPatterns}
            Build on liked patterns, add fresh twists, keep each idea distinct and specific.
            """;
    }

    private static string BuildSynthesisPrompt(IReadOnlyList<Idea> topIdeas)
    {
        // Kept for potential future use but synthesis now goes through SynthesisEngine
        var ideaSummary = string.Join("; ", topIdeas.Select(i => 
            $"{i.Title} [{string.Join(", ", i.DnaKeywords.Take(3))}]"));
        return $"Synthesize into one concept: {ideaSummary}";
    }

    private IReadOnlyList<Idea>? TryParseIdeas(Guid sessionId, string content, int batchNumber, string generationPrompt)
    {
        try
        {
            // Strip markdown code fences if present
            var cleanContent = content.Trim();
            if (cleanContent.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanContent = cleanContent[7..];
            }
            else if (cleanContent.StartsWith("```"))
            {
                cleanContent = cleanContent[3..];
            }
            if (cleanContent.EndsWith("```"))
            {
                cleanContent = cleanContent[..^3];
            }
            cleanContent = cleanContent.Trim();

            var extractedArray = ExtractJsonArray(cleanContent);
            if (!string.IsNullOrWhiteSpace(extractedArray))
            {
                cleanContent = extractedArray;
            }
            
            var parsed = JsonSerializer.Deserialize<List<IdeaJson>>(cleanContent, _jsonOptions);
            if (parsed is null || parsed.Count == 0)
            {
                _logger.LogWarning("Failed to parse AI response as idea list for session {SessionId}. Content: {Content}", 
                    sessionId, content.Length > 500 ? content[..500] : content);
                return null;
            }

            _logger.LogInformation("Successfully parsed {Count} ideas from AI response for session {SessionId}", parsed.Count, sessionId);
            
            return parsed.Select(p => new Idea
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Title = p.Title ?? "Untitled Idea",
                Description = p.Description ?? "No description available.",
                BatchNumber = batchNumber,
                GenerationPrompt = generationPrompt,
                DnaKeywords = p.DnaKeywords ?? [],
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON parsing failed for session {SessionId}. Content: {Content}", 
                sessionId, content.Length > 500 ? content[..500] : content);
            return null;
        }
    }

    private static string? ExtractJsonArray(string content)
    {
        var startIndex = content.IndexOf('[');
        var endIndex = content.LastIndexOf(']');

        if (startIndex < 0 || endIndex <= startIndex)
        {
            return null;
        }

        return content[startIndex..(endIndex + 1)];
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

    private sealed record IdeaJson(string? Title, string? Description, List<string>? DnaKeywords);
}
