using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;
using PoAppIdea.Shared.Contracts;

using MutationEntity = PoAppIdea.Core.Entities.Mutation;
using SessionEntity = PoAppIdea.Core.Entities.Session;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Service for managing feature variation generation and rating.
/// Implements directed feature expansion per FR-011, FR-012.
/// Uses Strategy pattern for different variation themes.
/// </summary>
public sealed class FeatureExpansionService(
    ISessionRepository sessionRepository,
    IMutationRepository mutationRepository,
    IFeatureVariationRepository featureVariationRepository,
    IChatCompletionService chatService,
    ILogger<FeatureExpansionService> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const int DefaultVariationsPerMutation = 3;
    private const int TopMutationsCount = 3;
    private const int TopFeaturesCount = 3;

    /// <summary>
    /// Variation themes to generate different feature perspectives.
    /// Each mutation gets variations with different themes.
    /// </summary>
    private static readonly string[] VariationThemes =
    [
        "Minimalist MVP",
        "Enterprise-Ready",
        "Privacy-First",
        "Social-Heavy",
        "AI-Powered"
    ];

    /// <summary>
    /// Generates feature variations from top mutations using different themes.
    /// Per FR-011: Top 10 mutations → 50 feature variations (5 per mutation).
    /// </summary>
    public async Task<ExpandFeaturesResponse> GenerateFeatureVariationsAsync(
        Guid sessionId,
        ExpandFeaturesRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot generate feature variations for a completed session");
        }

        // Get top mutations to expand
        var topMutations = await GetTopMutationsToExpandAsync(sessionId, request.TopMutationIds, cancellationToken);
        if (topMutations.Count == 0)
        {
            throw new InvalidOperationException("No mutations available for feature expansion. Complete Phase 2 first.");
        }

        var variationsPerMutation = request.VariationsPerMutation > 0
            ? request.VariationsPerMutation
            : DefaultVariationsPerMutation;

        var allVariations = new List<FeatureVariation>();
        var mutationsProcessed = 0;

        // Generate variations for each top mutation — collect errors but continue
        var errors = new List<string>();
        foreach (var mutation in topMutations)
        {
            try
            {
                var variations = await GenerateVariationsForMutationAsync(
                    sessionId, mutation, variationsPerMutation, cancellationToken);
                allVariations.AddRange(variations);
                mutationsProcessed++;

                logger.LogDebug(
                    "Generated {Count} feature variations for mutation {MutationId} ({Title})",
                    variations.Count,
                    mutation.Id,
                    mutation.Title);
            }
            catch (Exception ex)
            {
                errors.Add($"{mutation.Title}: {ex.Message}");
                mutationsProcessed++;
                logger.LogError(ex,
                    "Feature variation generation failed for mutation {MutationId} ({Title})",
                    mutation.Id, mutation.Title);
            }
        }

        // If ALL mutations failed, throw so the UI shows an error
        if (allVariations.Count == 0 && errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Feature generation failed for all mutations. Errors: {string.Join("; ", errors)}");
        }

        // Save all variations
        await featureVariationRepository.CreateBatchAsync(allVariations, cancellationToken);

        logger.LogInformation(
            "Generated {Count} feature variations for session {SessionId} from {MutationCount} mutations",
            allVariations.Count,
            sessionId,
            mutationsProcessed);

        return new ExpandFeaturesResponse
        {
            SessionId = sessionId,
            TotalVariations = allVariations.Count,
            Variations = allVariations.Select(MapToDto).ToList(),
            MutationsProcessed = mutationsProcessed
        };
    }

    /// <summary>
    /// Gets all feature variations for a session.
    /// </summary>
    public async Task<IReadOnlyList<FeatureVariationDto>> GetFeatureVariationsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var variations = await featureVariationRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return variations.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets feature variations for a specific mutation.
    /// </summary>
    public async Task<IReadOnlyList<FeatureVariationDto>> GetVariationsByMutationAsync(
        Guid mutationId,
        CancellationToken cancellationToken = default)
    {
        var variations = await featureVariationRepository.GetByMutationIdAsync(mutationId, cancellationToken);
        return variations.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Rates a feature variation.
    /// Per FR-012: User rating system identifies top candidates for submission.
    /// </summary>
    public async Task<FeatureVariationDto> RateVariationAsync(
        Guid sessionId,
        Guid variationId,
        RateFeatureVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        var variation = await featureVariationRepository.GetByIdAsync(variationId, cancellationToken)
            ?? throw new InvalidOperationException($"Feature variation {variationId} not found");

        if (variation.SessionId != sessionId)
        {
            throw new InvalidOperationException("Feature variation does not belong to the specified session");
        }

        if (request.Score < 1 || request.Score > 5)
        {
            throw new ArgumentException("Score must be between 1 and 5");
        }

        variation.Score = request.Score;

        var updated = await featureVariationRepository.UpdateAsync(variation, cancellationToken);

        logger.LogDebug(
            "Rated feature variation {VariationId} with score {Score} in session {SessionId}",
            variationId,
            request.Score,
            sessionId);

        return MapToDto(updated);
    }

    /// <summary>
    /// Gets top feature variations by score.
    /// Returns top 10 proto-apps for submission phase.
    /// </summary>
    public async Task<IReadOnlyList<FeatureVariationDto>> GetTopVariationsAsync(
        Guid sessionId,
        int? count = null,
        CancellationToken cancellationToken = default)
    {
        var topCount = count ?? TopFeaturesCount;
        var variations = await featureVariationRepository.GetTopByScoreAsync(sessionId, topCount, cancellationToken);
        return variations.Select(MapToDto).ToList();
    }

    private async Task<IReadOnlyList<MutationEntity>> GetTopMutationsToExpandAsync(
        Guid sessionId,
        IReadOnlyList<Guid>? specificIds,
        CancellationToken cancellationToken)
    {
        if (specificIds is { Count: > 0 })
        {
            var result = new List<MutationEntity>();
            foreach (var id in specificIds)
            {
                var mutation = await mutationRepository.GetByIdAsync(id, cancellationToken);
                if (mutation is not null && mutation.SessionId == sessionId)
                {
                    result.Add(mutation);
                }
            }
            return result;
        }

        return await mutationRepository.GetTopByScoreAsync(sessionId, TopMutationsCount, cancellationToken);
    }

    /// <summary>
    /// Generates all theme variations for a mutation in a SINGLE batched AI call to reduce token usage.
    /// Instead of N calls (one per theme), sends one call requesting all themes at once.
    /// </summary>
    private async Task<List<FeatureVariation>> GenerateVariationsForMutationAsync(
        Guid sessionId,
        MutationEntity mutation,
        int count,
        CancellationToken cancellationToken)
    {
        var themesToUse = VariationThemes.Take(count).ToArray();

        // Batched: single AI call for all themes
        Exception? lastException = null;
        for (var attempt = 0; attempt <= 3; attempt++)
        {
            try
            {
                return await GenerateBatchedVariationsAsync(sessionId, mutation, themesToUse, cancellationToken);
            }
            catch (Microsoft.SemanticKernel.HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                lastException = ex;
                if (attempt == 3)
                {
                    logger.LogWarning(
                        "Rate limit: giving up on batched variations for mutation {MutationId} after 3 retries",
                        mutation.Id);
                    break;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt + 1) * 5);
                logger.LogInformation(
                    "Rate limited on batched variations for mutation {MutationId}, retrying in {Delay}s (attempt {Attempt}/3)",
                    mutation.Id, delay.TotalSeconds, attempt + 1);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                logger.LogError(ex,
                    "Failed to generate batched variations for mutation {MutationId} ({Title}). Response parsing may have failed.",
                    mutation.Id, mutation.Title);
                break;
            }
        }

        // Throw so the caller knows this mutation failed — don't silently return []
        throw new InvalidOperationException(
            $"Failed to generate feature variations for mutation '{mutation.Title}': {lastException?.Message}",
            lastException);
    }

    /// <summary>
    /// Single AI call that generates all theme variations at once (saves ~80% of feature expansion tokens).
    /// </summary>
    private async Task<List<FeatureVariation>> GenerateBatchedVariationsAsync(
        Guid sessionId,
        MutationEntity mutation,
        string[] themes,
        CancellationToken cancellationToken)
    {
        var prompt = BuildBatchedFeaturePrompt(mutation, themes);

        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a product designer generating feature sets for app concepts.
            Generate multiple themed variations in a single response. Each variation has 5-7 features with MoSCoW priorities plus service integrations.
            
            Respond ONLY with valid JSON — a JSON array of variation objects:
            [
              {
                "variationTheme": "Theme Name",
                "features": [{"name": "Feature", "description": "Brief desc", "priority": "Must|Should|Could|Wont"}],
                "serviceIntegrations": ["Service 1", "Service 2"]
              }
            ]
            """);
        chatHistory.AddUserMessage(prompt);

        var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.7,
                ["max_tokens"] = 3000
            }
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory, executionSettings, cancellationToken: cancellationToken);

        var responseContent = response.Content ?? throw new InvalidOperationException("Empty response from AI");

        logger.LogDebug("Raw AI response for mutation {MutationId}: {Response}",
            mutation.Id, responseContent.Length > 500 ? responseContent[..500] + "..." : responseContent);

        // Parse the batched JSON response — could be array or wrapped in object
        var variations = new List<FeatureVariation>();
        var cleanContent = responseContent.Trim();

        // Try to extract JSON array
        List<GeneratedFeatureVariation>? parsed = null;

        // If wrapped in an object like {"variations": [...]}
        if (cleanContent.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(cleanContent);
                var root = doc.RootElement;
                // Find the first array property
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        parsed = JsonSerializer.Deserialize<List<GeneratedFeatureVariation>>(prop.Value.GetRawText(), _jsonOptions);
                        break;
                    }
                }
            }
            catch { /* fall through to array parse */ }
        }

        // Direct array
        if (parsed is null)
        {
            var arrayStart = cleanContent.IndexOf('[');
            var arrayEnd = cleanContent.LastIndexOf(']');
            if (arrayStart >= 0 && arrayEnd > arrayStart)
            {
                cleanContent = cleanContent[arrayStart..(arrayEnd + 1)];
            }
            parsed = JsonSerializer.Deserialize<List<GeneratedFeatureVariation>>(cleanContent, _jsonOptions);
        }

        if (parsed is null || parsed.Count == 0)
        {
            throw new InvalidOperationException(
                $"Failed to parse batched feature variation response. Content starts with: {cleanContent[..Math.Min(200, cleanContent.Length)]}");
        }

        foreach (var generated in parsed)
        {
            var features = generated.Features;
            if (features is null || features.Count == 0)
            {
                logger.LogWarning("Skipping variation with no features for mutation {MutationId}", mutation.Id);
                continue;
            }

            var theme = generated.VariationTheme ?? generated.Theme ?? "General";
            variations.Add(new FeatureVariation
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                MutationId = mutation.Id,
                Features = features.Where(f => f.Name is not null).Select(f => new Feature
                {
                    Name = f.Name!,
                    Description = f.Description ?? "",
                    Priority = ParsePriority(f.Priority ?? "Could")
                }).ToList(),
                ServiceIntegrations = generated.ServiceIntegrations?.ToList() ?? [],
                VariationTheme = theme,
                Score = 0f,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        logger.LogDebug("Batched AI call generated {Count} variations for mutation {MutationId} in single request",
            variations.Count, mutation.Id);

        return variations;
    }

    private static string BuildBatchedFeaturePrompt(MutationEntity mutation, string[] themes)
    {
        var themeList = string.Join(", ", themes.Select(t => $"\"{t}\""));
        return $"""
            Generate feature variations for these themes: [{themeList}] for this app concept:
            
            Title: {mutation.Title}
            Description: {mutation.Description}
            Type: {mutation.MutationType}
            
            For EACH theme, create 5-7 features (MoSCoW: Must/Should/Could/Wont) + 2-4 service integrations.
            Theme guidelines: Minimalist MVP=essentials only; Enterprise-Ready=admin/SSO/audit; Privacy-First=encryption/consent; Social-Heavy=sharing/community; AI-Powered=ML core features.
            """;
    }

    private static FeaturePriority ParsePriority(string priority)
    {
        return priority.ToUpperInvariant() switch
        {
            "MUST" => FeaturePriority.Must,
            "SHOULD" => FeaturePriority.Should,
            "COULD" => FeaturePriority.Could,
            "WONT" or "WON'T" => FeaturePriority.Wont,
            _ => FeaturePriority.Could
        };
    }

    private static FeatureVariationDto MapToDto(FeatureVariation variation)
    {
        return new FeatureVariationDto
        {
            Id = variation.Id,
            SessionId = variation.SessionId,
            MutationId = variation.MutationId,
            Features = variation.Features.Select(f => new FeatureDto
            {
                Name = f.Name,
                Description = f.Description,
                Priority = f.Priority
            }).ToList(),
            ServiceIntegrations = variation.ServiceIntegrations.AsReadOnly(),
            VariationTheme = variation.VariationTheme,
            Score = variation.Score,
            CreatedAt = variation.CreatedAt
        };
    }

    /// <summary>
    /// Internal model for parsing AI response.
    /// Uses lenient deserialization — no 'required' to avoid JsonException on missing props.
    /// </summary>
    private sealed record GeneratedFeatureVariation
    {
        [System.Text.Json.Serialization.JsonPropertyName("features")]
        public IReadOnlyList<GeneratedFeature>? Features { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceIntegrations")]
        public IReadOnlyList<string>? ServiceIntegrations { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("variationTheme")]
        public string? VariationTheme { get; init; }

        // AI sometimes uses "theme" instead of "variationTheme"
        [System.Text.Json.Serialization.JsonPropertyName("theme")]
        public string? Theme { get; init; }
    }

    private sealed record GeneratedFeature
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string? Description { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("priority")]
        public string? Priority { get; init; }
    }
}
