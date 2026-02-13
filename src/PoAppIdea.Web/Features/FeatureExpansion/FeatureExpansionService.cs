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

        // Generate variations for each top mutation
        foreach (var mutation in topMutations)
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

    private async Task<List<FeatureVariation>> GenerateVariationsForMutationAsync(
        Guid sessionId,
        MutationEntity mutation,
        int count,
        CancellationToken cancellationToken)
    {
        var variations = new List<FeatureVariation>();
        var themesToUse = VariationThemes.Take(count).ToArray();

        foreach (var theme in themesToUse)
        {
            var variation = await GenerateWithRetryAsync(
                sessionId, mutation, theme, cancellationToken);
            if (variation is not null)
            {
                variations.Add(variation);
            }

            // Small delay between calls to avoid rate limiting
            await Task.Delay(1500, cancellationToken);
        }

        return variations;
    }

    /// <summary>
    /// Retries a single AI call with exponential backoff on HTTP 429 (rate limit).
    /// </summary>
    private async Task<FeatureVariation?> GenerateWithRetryAsync(
        Guid sessionId,
        MutationEntity mutation,
        string theme,
        CancellationToken cancellationToken,
        int maxRetries = 3)
    {
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await GenerateSingleVariationAsync(
                    sessionId, mutation, theme, cancellationToken);
            }
            catch (Microsoft.SemanticKernel.HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                if (attempt == maxRetries)
                {
                    logger.LogWarning(
                        "Rate limit: giving up on {Theme} variation for mutation {MutationId} after {Retries} retries",
                        theme, mutation.Id, maxRetries);
                    return null;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt + 1) * 5); // 10s, 20s, 40s
                logger.LogInformation(
                    "Rate limited on {Theme} for mutation {MutationId}, retrying in {Delay}s (attempt {Attempt}/{Max})",
                    theme, mutation.Id, delay.TotalSeconds, attempt + 1, maxRetries);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to generate {Theme} variation for mutation {MutationId}",
                    theme,
                    mutation.Id);
                return null;
            }
        }

        return null;
    }

    private async Task<FeatureVariation> GenerateSingleVariationAsync(
        Guid sessionId,
        MutationEntity mutation,
        string theme,
        CancellationToken cancellationToken)
    {
        var prompt = BuildFeatureGenerationPrompt(mutation, theme);

        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        chatHistory.AddSystemMessage("""
            You are a product designer generating feature sets for app concepts.
            Generate a focused set of 5-7 features with MoSCoW priorities.
            Include relevant service integrations based on the theme.
            
            Respond ONLY with valid JSON in this exact format:
            {
                "features": [
                    {"name": "Feature Name", "description": "Brief description", "priority": "Must|Should|Could|Wont"}
                ],
                "serviceIntegrations": ["Service 1", "Service 2"],
                "variationTheme": "Theme Name"
            }
            """);
        chatHistory.AddUserMessage(prompt);

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            cancellationToken: cancellationToken);

        var responseContent = response.Content ?? throw new InvalidOperationException("Empty response from AI");

        // Parse JSON response
        var jsonStart = responseContent.IndexOf('{');
        var jsonEnd = responseContent.LastIndexOf('}') + 1;
        if (jsonStart < 0 || jsonEnd <= jsonStart)
        {
            throw new InvalidOperationException($"Invalid JSON in response: {responseContent}");
        }

        var jsonContent = responseContent[jsonStart..jsonEnd];
        var generated = JsonSerializer.Deserialize<GeneratedFeatureVariation>(jsonContent, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to parse feature variation response");

        return new FeatureVariation
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            MutationId = mutation.Id,
            Features = generated.Features.Select(f => new Feature
            {
                Name = f.Name,
                Description = f.Description,
                Priority = ParsePriority(f.Priority)
            }).ToList(),
            ServiceIntegrations = generated.ServiceIntegrations.ToList(),
            VariationTheme = theme,
            Score = 0f,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static string BuildFeatureGenerationPrompt(MutationEntity mutation, string theme)
    {
        return $"""
            Generate a "{theme}" feature variation for this app concept:
            
            Title: {mutation.Title}
            Description: {mutation.Description}
            Mutation Type: {mutation.MutationType}
            Rationale: {mutation.MutationRationale}
            
            Create 5-7 features with MoSCoW priorities (Must/Should/Could/Wont) that align with the "{theme}" approach.
            Include 2-4 relevant service integrations (APIs, third-party services).
            
            Theme Guidelines:
            - Minimalist MVP: Focus on essential features only, minimal integrations
            - Enterprise-Ready: Include admin features, SSO, audit logging
            - Privacy-First: Emphasize data protection, encryption, user consent
            - Social-Heavy: Include sharing, collaboration, community features
            - AI-Powered: Leverage AI/ML for core functionality
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
    /// </summary>
    private sealed record GeneratedFeatureVariation
    {
        public required IReadOnlyList<GeneratedFeature> Features { get; init; }
        public required IReadOnlyList<string> ServiceIntegrations { get; init; }
        public required string VariationTheme { get; init; }
    }

    private sealed record GeneratedFeature
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Priority { get; init; }
    }
}
