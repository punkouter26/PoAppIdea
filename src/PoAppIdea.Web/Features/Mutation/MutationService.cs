using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;
using PoAppIdea.Shared.Contracts;

using SessionEntity = PoAppIdea.Core.Entities.Session;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Service for managing mutation generation and rating.
/// Implements directed evolution per FR-007, FR-008.
/// </summary>
public sealed class MutationService(
    ISessionRepository sessionRepository,
    IIdeaRepository ideaRepository,
    IMutationRepository mutationRepository,
    IChatCompletionService chatService,
    ILogger<MutationService> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const int DefaultMutationsPerIdea = 4;
    private const int TopIdeasCount = 2;
    private const int TopMutationsCount = 10;

    /// <summary>
    /// Generates mutations from top ideas using Crossover and Repurposing strategies.
    /// Per FR-009: Top 3 ideas → 9 evolved variations (3 per idea with mixed strategies + feature integration).
    /// </summary>
    public async Task<MutateIdeasResponse> GenerateMutationsAsync(
        Guid sessionId,
        MutateIdeasRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot generate mutations for a completed session");
        }

        // Get top ideas to mutate
        var topIdeas = await GetTopIdeasToMutateAsync(sessionId, request.TopIdeaIds, cancellationToken);
        if (topIdeas.Count == 0)
        {
            throw new InvalidOperationException("No ideas available for mutation. Complete Phase 1 first.");
        }

        var mutationsPerIdea = request.MutationsPerIdea > 0 
            ? request.MutationsPerIdea 
            : DefaultMutationsPerIdea;

        var allMutations = new List<Core.Entities.Mutation>();
        var crossoverCount = 0;
        var repurposingCount = 0;

        // Generate mutations for each top idea
        foreach (var idea in topIdeas)
        {
            // Split mutations: 5 crossover + 5 repurposing (or proportional split)
            var crossoverMutations = mutationsPerIdea / 2;
            var repurposingMutations = mutationsPerIdea - crossoverMutations;

            // Generate crossover mutations (combine with other top ideas)
            var crossovers = await GenerateCrossoverMutationsAsync(
                sessionId, idea, topIdeas, crossoverMutations, cancellationToken);
            allMutations.AddRange(crossovers);
            crossoverCount += crossovers.Count;

            // Generate repurposing mutations (transform single idea to new domain)
            var repurposings = await GenerateRepurposingMutationsAsync(
                sessionId, idea, repurposingMutations, cancellationToken);
            allMutations.AddRange(repurposings);
            repurposingCount += repurposings.Count;
        }

        // Save all mutations
        await mutationRepository.CreateBatchAsync(allMutations, cancellationToken);

        logger.LogInformation(
            "Generated {Count} mutations for session {SessionId} ({Crossover} crossover, {Repurposing} repurposing)",
            allMutations.Count,
            sessionId,
            crossoverCount,
            repurposingCount);

        return new MutateIdeasResponse
        {
            SessionId = sessionId,
            TotalMutations = allMutations.Count,
            Mutations = allMutations.Select(MapToDto).ToList(),
            CrossoverCount = crossoverCount,
            RepurposingCount = repurposingCount
        };
    }

    /// <summary>
    /// Gets all mutations for a session.
    /// </summary>
    public async Task<IReadOnlyList<MutationDto>> GetMutationsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var mutations = await mutationRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return mutations.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets top mutations by score per FR-008.
    /// </summary>
    public async Task<IReadOnlyList<MutationDto>> GetTopMutationsAsync(
        Guid sessionId,
        int count = TopMutationsCount,
        CancellationToken cancellationToken = default)
    {
        var mutations = await mutationRepository.GetTopByScoreAsync(sessionId, count, cancellationToken);
        return mutations.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Rates a mutation.
    /// </summary>
    public async Task<MutationDto> RateMutationAsync(
        Guid sessionId,
        Guid mutationId,
        RateMutationRequest request,
        CancellationToken cancellationToken = default)
    {
        var mutation = await mutationRepository.GetByIdAsync(mutationId, cancellationToken)
            ?? throw new InvalidOperationException($"Mutation {mutationId} not found");

        if (mutation.SessionId != sessionId)
        {
            throw new InvalidOperationException($"Mutation {mutationId} does not belong to session {sessionId}");
        }

        mutation.Score = request.Score;
        var updated = await mutationRepository.UpdateAsync(mutation, cancellationToken);

        logger.LogInformation(
            "Rated mutation {MutationId} with score {Score}",
            mutationId,
            request.Score);

        return MapToDto(updated);
    }

    private async Task<IReadOnlyList<Idea>> GetTopIdeasToMutateAsync(
        Guid sessionId,
        IReadOnlyList<Guid>? specificIds,
        CancellationToken cancellationToken)
    {
        if (specificIds is { Count: > 0 })
        {
            var ideas = new List<Idea>();
            foreach (var id in specificIds)
            {
                var idea = await ideaRepository.GetByIdAsync(id, cancellationToken);
                if (idea is not null && idea.SessionId == sessionId)
                {
                    ideas.Add(idea);
                }
            }
            return ideas;
        }

        return await ideaRepository.GetTopByScoreAsync(sessionId, TopIdeasCount, cancellationToken);
    }

    private async Task<List<Core.Entities.Mutation>> GenerateCrossoverMutationsAsync(
        Guid sessionId,
        Idea primaryIdea,
        IReadOnlyList<Idea> allTopIdeas,
        int count,
        CancellationToken cancellationToken)
    {
        // For crossover, combine with other top ideas
        var otherIdeas = allTopIdeas.Where(i => i.Id != primaryIdea.Id).ToList();
        if (otherIdeas.Count == 0)
        {
            // If only one idea, fall back to repurposing
            return await GenerateRepurposingMutationsAsync(sessionId, primaryIdea, count, cancellationToken);
        }

        var prompt = BuildCrossoverPrompt(primaryIdea, otherIdeas, count);
        var mutations = await GenerateMutationsFromPromptAsync(
            sessionId,
            [primaryIdea.Id, otherIdeas[0].Id], // Primary crossover pair
            MutationType.Crossover,
            prompt,
            cancellationToken);

        return mutations.Take(count).ToList();
    }

    private async Task<List<Core.Entities.Mutation>> GenerateRepurposingMutationsAsync(
        Guid sessionId,
        Idea idea,
        int count,
        CancellationToken cancellationToken)
    {
        var prompt = BuildRepurposingPrompt(idea, count);
        var mutations = await GenerateMutationsFromPromptAsync(
            sessionId,
            [idea.Id],
            MutationType.Repurposing,
            prompt,
            cancellationToken);

        return mutations.Take(count).ToList();
    }

    private async Task<List<Core.Entities.Mutation>> GenerateMutationsFromPromptAsync(
        Guid sessionId,
        List<Guid> parentIdeaIds,
        MutationType mutationType,
        string prompt,
        CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(GetMutationSystemPrompt(mutationType));
        history.AddUserMessage(prompt);

        try
        {
            var response = await chatService.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
            return ParseMutations(sessionId, parentIdeaIds, mutationType, response.Content ?? "");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to generate mutations, using fallback");
            return GenerateFallbackMutations(sessionId, parentIdeaIds, mutationType, 5);
        }
    }

    private static string GetMutationSystemPrompt(MutationType mutationType)
    {
        var strategyDesc = mutationType switch
        {
            MutationType.Crossover => "Combine elements from multiple parent ideas to create hybrid concepts that leverage the best of both.",
            MutationType.Repurposing => "Take the core mechanics or unique value proposition and apply it to completely different domains or user segments.",
            _ => "Evolve the ideas creatively."
        };

        return $$"""
            You are an expert product ideation assistant specializing in {{mutationType.ToString().ToLower()}} mutations.
            {{strategyDesc}}
            
            Generate mutations in JSON format. Each mutation should have:
            - title: A catchy, memorable name (3-5 words)
            - description: A compelling one-paragraph pitch (50-100 words)
            - mutationRationale: Explanation of how/why this mutation was derived (30-50 words)
            
            Output format (JSON array only, no markdown):
            [{"title": "...", "description": "...", "mutationRationale": "..."}]
            """;
    }

    private static string BuildCrossoverPrompt(Idea primary, IReadOnlyList<Idea> others, int count)
    {
        var otherSummaries = string.Join("\n", others.Select(i => 
            $"- {i.Title}: {i.Description} [Keywords: {string.Join(", ", i.DnaKeywords)}]"));

        return $$"""
            Generate {{count}} crossover mutations by combining the primary idea with elements from secondary ideas.
            
            PRIMARY IDEA:
            Title: {{primary.Title}}
            Description: {{primary.Description}}
            Keywords: {{string.Join(", ", primary.DnaKeywords)}}
            
            SECONDARY IDEAS TO CROSSOVER WITH:
            {{otherSummaries}}
            
            Create hybrid concepts that combine the best features of the primary with elements from the secondary ideas.
            """;
    }

    private static string BuildRepurposingPrompt(Idea idea, int count)
    {
        return $$"""
            Generate {{count}} repurposing mutations by applying the core concept to new domains.
            
            ORIGINAL IDEA:
            Title: {{idea.Title}}
            Description: {{idea.Description}}
            Keywords: {{string.Join(", ", idea.DnaKeywords)}}
            
            Apply the core mechanics or unique value proposition to completely different:
            - Industries (e.g., healthcare, education, finance, entertainment)
            - User segments (e.g., B2B, kids, elderly, professionals)
            - Platforms (e.g., wearables, AR/VR, IoT, voice assistants)
            
            Each repurposing should feel like a fresh product while retaining the essence of the original.
            """;
    }

    private List<Core.Entities.Mutation> ParseMutations(
        Guid sessionId,
        List<Guid> parentIdeaIds,
        MutationType mutationType,
        string content)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<List<MutationJson>>(content, _jsonOptions);
            if (parsed is null) return GenerateFallbackMutations(sessionId, parentIdeaIds, mutationType, 5);

            return parsed.Select(p => new Core.Entities.Mutation
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                ParentIdeaIds = parentIdeaIds,
                MutationType = mutationType,
                Title = p.Title ?? "Evolved Concept",
                Description = p.Description ?? "A mutated product concept.",
                MutationRationale = p.MutationRationale ?? $"{mutationType} mutation applied.",
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
        }
        catch
        {
            return GenerateFallbackMutations(sessionId, parentIdeaIds, mutationType, 5);
        }
    }

    private static List<Core.Entities.Mutation> GenerateFallbackMutations(
        Guid sessionId,
        List<Guid> parentIdeaIds,
        MutationType mutationType,
        int count)
    {
        var typeDesc = mutationType == MutationType.Crossover ? "Hybrid" : "Repurposed";
        return Enumerable.Range(1, count)
            .Select(i => new Core.Entities.Mutation
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                ParentIdeaIds = parentIdeaIds,
                MutationType = mutationType,
                Title = $"{typeDesc} Concept {i}",
                Description = $"An evolved product concept through {mutationType.ToString().ToLower()}.",
                MutationRationale = $"{mutationType} applied to create a new variation.",
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList();
    }

    private static MutationDto MapToDto(Core.Entities.Mutation mutation) => new()
    {
        Id = mutation.Id,
        SessionId = mutation.SessionId,
        ParentIdeaIds = mutation.ParentIdeaIds,
        MutationType = mutation.MutationType,
        Title = mutation.Title,
        Description = mutation.Description,
        MutationRationale = mutation.MutationRationale,
        Score = mutation.Score,
        CreatedAt = mutation.CreatedAt
    };

    private sealed record MutationJson(string? Title, string? Description, string? MutationRationale);
}
