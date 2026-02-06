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
    private readonly ILogger<IdeaGenerator> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public IdeaGenerator(Kernel kernel, ILogger<IdeaGenerator> logger)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
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
        try
        {
            _logger.LogDebug("Calling Azure OpenAI for session {SessionId}, batch {BatchNumber}", sessionId, batchNumber);
            
            var history = new ChatHistory();
            history.AddSystemMessage(GetIdeaSystemPrompt());
            history.AddUserMessage(prompt);

            // Use higher temperature for more creative, diverse ideas
            var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = 0.9,  // High creativity
                    ["max_tokens"] = 4000,  // Allow detailed responses
                    ["top_p"] = 0.95,       // Nucleus sampling for diversity
                    ["frequency_penalty"] = 0.3,  // Reduce repetition
                    ["presence_penalty"] = 0.3    // Encourage topic diversity
                }
            };
            
            var response = await _chatService.GetChatMessageContentAsync(history, executionSettings, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Azure OpenAI response received, content length: {Length}", response.Content?.Length ?? 0);
            
            if (string.IsNullOrEmpty(response.Content))
            {
                _logger.LogWarning("Azure OpenAI returned empty content for session {SessionId}", sessionId);
                return GenerateFallbackIdeas(sessionId, batchNumber);
            }
            
            return ParseIdeas(sessionId, response.Content, batchNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI call failed for session {SessionId}, batch {BatchNumber}. Using fallback ideas.", sessionId, batchNumber);
            return GenerateFallbackIdeas(sessionId, batchNumber);
        }
    }

    private static string GetIdeaSystemPrompt() => """
        You are an elite product ideation expert trained in SCAMPER, Jobs-to-be-Done, and Blue Ocean Strategy.
        
        Generate exactly 10 INNOVATIVE app ideas that are:
        ✓ SPECIFIC - Target clear user personas and pain points (not "productivity app for everyone")
        ✓ UNIQUE - Offer novel combinations or unexplored niches (avoid "another todo app")
        ✓ FEASIBLE - Technically achievable within the specified complexity
        ✓ VALUABLE - Solve real, urgent problems with measurable impact
        
        QUALITY CRITERIA (each idea must have):
        1. A SPECIFIC target user (e.g., "freelance graphic designers" not "professionals")
        2. A CLEAR problem they face (not generic pain points)
        3. A UNIQUE solution mechanism (novel approach or combination)
        4. CONCRETE features that differentiate it from existing solutions
        
        AVOID GENERIC IDEAS LIKE:
        ❌ "Task management app with AI"
        ❌ "Social media platform for connecting people"
        ❌ "E-commerce marketplace"
        ❌ "Fitness tracking app"
        
        GOOD EXAMPLES:
        ✓ "VoiceFlow Studio" - Voice-controlled DAW for musicians with RSI/carpal tunnel, using gesture+voice for hands-free mixing
        ✓ "GhostWrite Legal" - AI contract analyzer for small landlords, flags risky clauses in tenant agreements vs. local laws
        ✓ "CodePair Mentor" - Async pair programming for remote teams; records coding sessions, AI suggests refactoring moments
        
        Each idea must include:
        - title: Memorable, descriptive name (2-4 words)
        - description: Specific pitch covering WHO (target user), WHAT (problem), HOW (unique solution), WHY (value) in 60-120 words
        - dnaKeywords: 5-7 precise keywords (include target user, problem domain, key tech/approach)
        
        Output format (JSON array only, no markdown, no code fences):
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
            Generate 10 innovative {appType} app ideas.
            
            🎯 TARGET COMPLEXITY: {complexityDesc}
            
            📱 APP TYPE GUIDANCE:
            {appTypeGuidance}
            
            💡 INNOVATION REQUIREMENTS:
            1. Identify a SPECIFIC niche or underserved user group (not "general users")
            2. Address a CONCRETE pain point with measurable impact
            3. Propose a UNIQUE approach or novel combination of features
            4. Ensure technical feasibility within the complexity level
            5. Each idea should be distinct from the others (diverse domains/approaches)
            
            🎨 CREATIVITY TECHNIQUES (use at least 2-3):
            - SCAMPER: Substitute, Combine, Adapt, Modify, Put to other use, Eliminate, Reverse
            - Jobs-to-be-Done: What "job" is the user really hiring this app for?
            - Lateral Thinking: Cross-pollinate ideas from unrelated industries
            - Constraint-based: What if you removed a key assumption?
            {preferenceSection}
            
            Remember: AVOID generic ideas. Be specific about WHO uses it, WHAT problem it solves, and HOW it's different.
            """;
    }

    private static string BuildMutationPrompt(
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        MutationType mutationType)
    {
        var likedSummary = string.Join("\n", likedIdeas.Select(i => $"✓ {i.Title}: {i.Description}\n  Keywords: {string.Join(", ", i.DnaKeywords)}"));
        var dislikedKeywords = dislikedIdeas.SelectMany(i => i.DnaKeywords).Distinct().ToList();
        var dislikedPatterns = dislikedKeywords.Any()
            ? $"\n❌ AVOID these patterns/keywords: {string.Join(", ", dislikedKeywords.Take(15))}\n"
            : "";

        var mutationInstruction = mutationType switch
        {
            MutationType.Crossover => """
                🧬 CROSSOVER MUTATION:
                - Identify the strongest elements from 2-3 liked ideas
                - Create HYBRID concepts that merge complementary features
                - Find synergies between different domains (e.g., fitness + gamification + social)
                - Ensure the combination creates MORE value than individual parts
                - Example: Liked "AI fitness coach" + "social challenges" → "Competitive AI-coached fitness leagues"
                """,
            MutationType.Repurposing => """
                🔄 REPURPOSING MUTATION:
                - Take the core mechanics/value proposition from liked ideas
                - Apply them to COMPLETELY DIFFERENT domains or user groups
                - Ask "What if [liked mechanism] was used for [different problem]?"
                - Example: Liked "voice-controlled music DAW" → "Voice-controlled CAD for architects with injuries"
                """,
            _ => "Evolve the ideas in creative, unexpected directions."
        };

        return $"""
            Generate 10 new evolved app ideas based on user preferences from swiping.
            
            💚 USER LIKED THESE IDEAS (learn what resonates):
            {likedSummary}
            {dislikedPatterns}
            
            🎯 EVOLUTION STRATEGY:
            {mutationInstruction}
            
            📋 REQUIREMENTS:
            1. Build on patterns from liked ideas (user personas, problem spaces, approaches)
            2. Add fresh twists - don't just copy, evolve meaningfully
            3. Maintain or increase specificity (narrow, not broader)
            4. Each idea should be DISTINCT from others in this batch
            5. Stay within the same complexity/app type as original session
            
            Remember: The user swiped right on these for a reason. Learn from what made them compelling!
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
            
            var parsed = JsonSerializer.Deserialize<List<IdeaJson>>(cleanContent, _jsonOptions);
            if (parsed is null || parsed.Count == 0)
            {
                _logger.LogWarning("Failed to parse AI response as idea list for session {SessionId}. Content: {Content}", 
                    sessionId, content.Length > 500 ? content[..500] : content);
                return GenerateFallbackIdeas(sessionId, batchNumber);
            }

            _logger.LogInformation("Successfully parsed {Count} ideas from AI response for session {SessionId}", parsed.Count, sessionId);
            
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
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed for session {SessionId}. Content: {Content}", 
                sessionId, content.Length > 500 ? content[..500] : content);
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
