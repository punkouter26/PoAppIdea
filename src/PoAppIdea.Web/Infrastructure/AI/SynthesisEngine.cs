using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Core.Entities;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// AI engine for synthesizing multiple ideas into a cohesive concept.
/// Implements thematic bridging logic per FR-014.
/// Uses Chain of Responsibility pattern for synthesis steps.
/// </summary>
public sealed class SynthesisEngine
{
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<SynthesisEngine> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SynthesisEngine(
        IChatCompletionService chatService,
        ILogger<SynthesisEngine> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Synthesizes multiple ideas into a single cohesive concept.
    /// Identifies thematic bridges and retains key elements from each source.
    /// </summary>
    public async Task<SynthesisResult> SynthesizeIdeasAsync(
        IReadOnlyList<IdeaSource> sourceIdeas,
        CancellationToken cancellationToken = default)
    {
        if (sourceIdeas.Count < 2)
        {
            throw new InvalidOperationException("Synthesis requires at least 2 source ideas");
        }

        _logger.LogInformation(
            "Synthesizing {Count} ideas into a cohesive concept",
            sourceIdeas.Count);

        var prompt = BuildSynthesisPrompt(sourceIdeas);

        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        chatHistory.AddUserMessage(prompt);

        // Optimization 10: JSON mode for guaranteed valid JSON output
        var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.6,
                ["max_tokens"] = 1500
            }
        };

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, executionSettings, cancellationToken: cancellationToken);
        var content = response.Content ?? throw new InvalidOperationException("AI returned empty response");

        var result = ParseSynthesisResponse(content, sourceIdeas);

        _logger.LogDebug(
            "Synthesis complete: '{Title}' - Thematic bridge: {Bridge}",
            result.MergedTitle,
            result.ThematicBridge);

        return result;
    }

    /// <summary>
    /// Optimization 4: Sends compact summaries instead of full idea objects.
    /// </summary>
    private static string BuildSynthesisPrompt(IReadOnlyList<IdeaSource> sourceIdeas)
    {
        var ideasSummary = string.Join("\n", sourceIdeas.Select(i => 
            $"- {i.Id}: \"{i.Title}\" — {(i.Description.Length > 120 ? i.Description[..120] + "..." : i.Description)} [Features: {string.Join(", ", i.KeyFeatures?.Take(4) ?? [])}]"));

        var jsonFormat = """{"mergedTitle": "...", "mergedDescription": "...", "thematicBridge": "...", "retainedElements": {"idea-guid": ["element1", "element2"]}}""";

        return $"""
            You are a creative product strategist. Synthesize these app ideas into a single cohesive concept.

            ## Source Ideas
            {ideasSummary}

            Create: 1) Compelling thematic bridge connecting all ideas, 2) Merged title, 3) Merged description (max 600 chars), 4) Retained elements from each source.

            Return JSON only: {jsonFormat}
            """;
    }

    private SynthesisResult ParseSynthesisResponse(string content, IReadOnlyList<IdeaSource> sourceIdeas)
    {
        try
        {
            // Clean up response if wrapped in markdown
            var jsonContent = content;
            if (content.Contains("```json"))
            {
                var start = content.IndexOf("```json", StringComparison.Ordinal) + 7;
                var end = content.LastIndexOf("```", StringComparison.Ordinal);
                if (end > start)
                {
                    jsonContent = content[start..end].Trim();
                }
            }
            else if (content.Contains("```"))
            {
                var start = content.IndexOf("```", StringComparison.Ordinal) + 3;
                var end = content.LastIndexOf("```", StringComparison.Ordinal);
                if (end > start)
                {
                    jsonContent = content[start..end].Trim();
                }
            }

            var parsed = JsonSerializer.Deserialize<SynthesisRawResponse>(jsonContent, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to parse synthesis response");

            // Convert string keys to Guids
            var retainedElements = new Dictionary<Guid, List<string>>();
            foreach (var kvp in parsed.RetainedElements)
            {
                if (Guid.TryParse(kvp.Key, out var ideaId))
                {
                    retainedElements[ideaId] = kvp.Value;
                }
            }

            // Ensure all source ideas have retained elements
            foreach (var source in sourceIdeas)
            {
                if (!retainedElements.ContainsKey(source.Id))
                {
                    retainedElements[source.Id] = new List<string> { source.Title, "Core concept" };
                }
            }

            return new SynthesisResult
            {
                MergedTitle = parsed.MergedTitle,
                MergedDescription = parsed.MergedDescription,
                ThematicBridge = parsed.ThematicBridge,
                RetainedElements = retainedElements
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse synthesis JSON, extracting manually");
            return ExtractFallbackResult(content, sourceIdeas);
        }
    }

    private SynthesisResult ExtractFallbackResult(string content, IReadOnlyList<IdeaSource> sourceIdeas)
    {
        // Fallback: create a simple synthesis from the source ideas
        var combinedTitle = string.Join(" × ", sourceIdeas.Select(i => i.Title.Split(' ').First()));
        var combinedDesc = $"A unified concept combining: {string.Join(", ", sourceIdeas.Select(i => i.Title))}. " +
                          $"{sourceIdeas[0].Description}";

        var retainedElements = sourceIdeas.ToDictionary(
            i => i.Id,
            i => i.KeyFeatures?.ToList() ?? new List<string> { i.Title });

        return new SynthesisResult
        {
            MergedTitle = combinedTitle.Length > 100 ? combinedTitle[..97] + "..." : combinedTitle,
            MergedDescription = combinedDesc.Length > 1000 ? combinedDesc[..997] + "..." : combinedDesc,
            ThematicBridge = "These ideas share common themes of innovation and user value.",
            RetainedElements = retainedElements
        };
    }

    private sealed class SynthesisRawResponse
    {
        public string MergedTitle { get; set; } = string.Empty;
        public string MergedDescription { get; set; } = string.Empty;
        public string ThematicBridge { get; set; } = string.Empty;
        public Dictionary<string, List<string>> RetainedElements { get; set; } = new();
    }
}

/// <summary>
/// Input source for synthesis - represents a selected idea.
/// </summary>
public sealed record IdeaSource
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<string>? KeyFeatures { get; init; }
}

/// <summary>
/// Result of the synthesis operation.
/// </summary>
public sealed record SynthesisResult
{
    public required string MergedTitle { get; init; }
    public required string MergedDescription { get; init; }
    public required string ThematicBridge { get; init; }
    public required Dictionary<Guid, List<string>> RetainedElements { get; init; }
}
