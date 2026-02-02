using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Contracts;
using PoAppIdea.Web.Infrastructure.AI;

using SessionEntity = PoAppIdea.Core.Entities.Session;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Service for managing idea generation and swipe processing.
/// Implements batch generation with learning logic per FR-002, FR-003, FR-004, FR-006.
/// </summary>
public sealed class SparkService(
    ISessionRepository sessionRepository,
    IIdeaRepository ideaRepository,
    ISwipeRepository swipeRepository,
    IIdeaGenerator ideaGenerator,
    IConfiguration configuration,
    ILogger<SparkService> logger)
{
    // Configurable settings from appsettings.json
    private readonly int _defaultBatchSize = configuration.GetValue("IdeaGeneration:IdeasPerBatch", 10);
    private readonly int _maxBatches = configuration.GetValue("IdeaGeneration:MaxBatches", 2);
    
    private const int FastSwipeThresholdMs = 1000;
    private const int HesitationThresholdMs = 3000;
    private const float FastSwipeWeight = 0.5f;
    private const float NormalSwipeWeight = 1.0f;
    private const float HesitationWeight = 1.5f;
    private const int TopIdeasCount = 3;

    /// <summary>
    /// Generates a batch of ideas for the given session.
    /// Uses learning from previous swipes to improve suggestions.
    /// </summary>
    public async Task<GenerateIdeasResponse> GenerateIdeasAsync(
        Guid sessionId,
        GenerateIdeasRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot generate ideas for a completed session");
        }

        var batchSize = request.BatchSize > 0 ? request.BatchSize : _defaultBatchSize;

        // Determine batch number from existing ideas
        var existingIdeas = await ideaRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var currentMaxBatch = existingIdeas.Count > 0 
            ? existingIdeas.Max(i => i.BatchNumber) 
            : 0;
        var batchNumber = currentMaxBatch + 1;

        if (batchNumber > _maxBatches)
        {
            throw new InvalidOperationException($"Maximum batch limit ({_maxBatches}) reached");
        }

        // Get learning context from previous swipes
        var learningContext = await BuildLearningContextAsync(sessionId, cancellationToken);

        // Generate ideas using AI
        var generatedIdeas = await GenerateIdeasWithAIAsync(
            session,
            batchSize,
            batchNumber,
            learningContext,
            request.SeedPhrase,
            cancellationToken);

        // Save ideas to repository
        foreach (var idea in generatedIdeas)
        {
            await ideaRepository.CreateAsync(idea, cancellationToken);
        }

        logger.LogInformation(
            "Generated {Count} ideas for session {SessionId}, batch {BatchNumber}",
            generatedIdeas.Count,
            sessionId,
            batchNumber);

        return new GenerateIdeasResponse
        {
            SessionId = sessionId,
            BatchNumber = batchNumber,
            Ideas = generatedIdeas.Select(MapToDto).ToList(),
            HasMore = batchNumber < _maxBatches
        };
    }

    /// <summary>
    /// Records a swipe action with timing-based weighting per FR-004.
    /// </summary>
    public async Task<Swipe> RecordSwipeAsync(
        Guid sessionId,
        Guid userId,
        RecordSwipeRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this session");
        }

        var idea = await ideaRepository.GetByIdAsync(request.IdeaId, cancellationToken)
            ?? throw new InvalidOperationException($"Idea {request.IdeaId} not found");

        // Calculate speed category based on swipe duration (FR-004)
        var speedCategory = CalculateSpeedCategory(request.DurationMs);

        var swipe = new Swipe
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            IdeaId = request.IdeaId,
            UserId = userId,
            Direction = request.Direction,
            DurationMs = request.DurationMs,
            Timestamp = DateTimeOffset.UtcNow,
            SpeedCategory = speedCategory
        };

        await swipeRepository.CreateAsync(swipe, cancellationToken);

        // Update idea score based on swipe
        await UpdateIdeaScoreAsync(idea, swipe, cancellationToken);

        logger.LogDebug(
            "Recorded swipe {Direction} for idea {IdeaId}, duration {DurationMs}ms, speed {SpeedCategory}",
            request.Direction,
            request.IdeaId,
            request.DurationMs,
            speedCategory);

        return swipe;
    }

    /// <summary>
    /// Gets the top-rated ideas for a session per FR-006.
    /// Edge Case T170: When all ideas are liked, use swipe speed for ranking.
    /// Edge Case T171: When all ideas are disliked, offer restart guidance.
    /// </summary>
    public async Task<IReadOnlyList<IdeaDto>> GetTopIdeasAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var ideas = await ideaRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var swipes = await swipeRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        // Analyze swipe patterns
        var likeSwipes = swipes.Where(s => s.Direction is SwipeDirection.Right or SwipeDirection.Up).ToList();
        var dislikeSwipes = swipes.Where(s => s.Direction == SwipeDirection.Left).ToList();
        var swipedIdeaIds = swipes.Select(s => s.IdeaId).ToHashSet();

        // Edge Case T171: All dislikes - no liked ideas found
        if (likeSwipes.Count == 0 && dislikeSwipes.Count > 0)
        {
            logger.LogWarning(
                "Session {SessionId}: All {Count} swiped ideas were disliked. Suggesting restart.",
                sessionId,
                dislikeSwipes.Count);

            // Return empty list with logging - the endpoint/UI can handle offering a restart
            return new List<IdeaDto>();
        }

        var likedIdeaIds = likeSwipes.Select(s => s.IdeaId).ToHashSet();

        // Edge Case T170: All likes - use swipe speed for differentiated ranking
        if (dislikeSwipes.Count == 0 && likeSwipes.Count > 0)
        {
            logger.LogInformation(
                "Session {SessionId}: All {Count} ideas liked. Using swipe speed for ranking.",
                sessionId,
                likeSwipes.Count);

            // Create speed-adjusted scores: slower swipes = more deliberate = higher confidence
            var swipeSpeedMap = likeSwipes.ToDictionary(
                s => s.IdeaId,
                s => CalculateSpeedConfidenceScore(s));

            var topIdeas = ideas
                .Where(i => likedIdeaIds.Contains(i.Id))
                .OrderByDescending(i => 
                    swipeSpeedMap.TryGetValue(i.Id, out var speedScore) 
                        ? i.Score * speedScore 
                        : i.Score)
                .Take(TopIdeasCount)
                .Select(MapToDto)
                .ToList();

            return topIdeas;
        }

        // Normal case: mixed likes/dislikes - standard scoring
        var topIdeasNormal = ideas
            .Where(i => likedIdeaIds.Contains(i.Id))
            .OrderByDescending(i => i.Score)
            .Take(TopIdeasCount)
            .Select(MapToDto)
            .ToList();

        return topIdeasNormal;
    }

    /// <summary>
    /// Calculates a confidence score based on swipe speed.
    /// Slower swipes indicate more deliberate consideration.
    /// Pattern: Strategy - different scoring strategies based on user behavior.
    /// </summary>
    private static float CalculateSpeedConfidenceScore(Swipe swipe)
    {
        // Hesitation (slow swipe) = high confidence in the decision
        // Fast swipe = quick gut reaction, lower confidence
        // Super-like (up) = maximum confidence
        if (swipe.Direction == SwipeDirection.Up)
        {
            return 2.0f; // Super-likes always rank highest
        }

        return swipe.SpeedCategory switch
        {
            SwipeSpeed.Slow => 1.5f,  // Deliberate consideration
            SwipeSpeed.Medium => 1.0f, // Normal confidence
            SwipeSpeed.Fast => 0.75f,  // Quick reaction
            _ => 1.0f
        };
    }

    /// <summary>
    /// Checks if all swiped ideas in the session were disliked.
    /// Used by UI to offer restart options (T171).
    /// </summary>
    public async Task<SwipeAnalysis> AnalyzeSwipePatternsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var swipes = await swipeRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        if (swipes.Count == 0)
        {
            return new SwipeAnalysis
            {
                TotalSwipes = 0,
                LikeCount = 0,
                DislikeCount = 0,
                SuperLikeCount = 0,
                AllLikes = false,
                AllDislikes = false,
                ShouldOfferRestart = false
            };
        }

        var likes = swipes.Count(s => s.Direction == SwipeDirection.Right);
        var dislikes = swipes.Count(s => s.Direction == SwipeDirection.Left);
        var superLikes = swipes.Count(s => s.Direction == SwipeDirection.Up);

        var allLikes = dislikes == 0 && (likes + superLikes) > 0;
        var allDislikes = (likes + superLikes) == 0 && dislikes > 0;

        return new SwipeAnalysis
        {
            TotalSwipes = swipes.Count,
            LikeCount = likes,
            DislikeCount = dislikes,
            SuperLikeCount = superLikes,
            AllLikes = allLikes,
            AllDislikes = allDislikes,
            ShouldOfferRestart = allDislikes && dislikes >= 5 // Offer restart after 5+ dislikes with no likes
        };
    }

    /// <summary>
    /// Gets all ideas for a session.
    /// </summary>
    public async Task<IReadOnlyList<IdeaDto>> GetSessionIdeasAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var ideas = await ideaRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return ideas.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Calculates swipe speed category based on viewing duration per FR-004.
    /// </summary>
    private static SwipeSpeed CalculateSpeedCategory(int durationMs)
    {
        if (durationMs < FastSwipeThresholdMs)
        {
            return SwipeSpeed.Fast;
        }
        else if (durationMs > HesitationThresholdMs)
        {
            return SwipeSpeed.Slow;
        }
        else
        {
            return SwipeSpeed.Medium;
        }
    }

    /// <summary>
    /// Calculates swipe weight based on viewing duration per FR-004.
    /// </summary>
    private static float CalculateSwipeWeight(int durationMs)
    {
        if (durationMs < FastSwipeThresholdMs)
        {
            return FastSwipeWeight;
        }
        else if (durationMs > HesitationThresholdMs)
        {
            return HesitationWeight;
        }
        else
        {
            return NormalSwipeWeight;
        }
    }

    /// <summary>
    /// Builds learning context from previous swipes to influence next batch.
    /// </summary>
    private async Task<LearningContext> BuildLearningContextAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var swipes = await swipeRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (swipes.Count == 0)
        {
            return new LearningContext();
        }

        var ideas = await ideaRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var ideaLookup = ideas.ToDictionary(i => i.Id);

        var likedThemes = new List<string>();
        var dislikedThemes = new List<string>();
        var superLikedThemes = new List<string>();

        foreach (var swipe in swipes)
        {
            if (!ideaLookup.TryGetValue(swipe.IdeaId, out var idea))
                continue;

            var themes = idea.DnaKeywords.ToList();

            switch (swipe.Direction)
            {
                case SwipeDirection.Right:
                    likedThemes.AddRange(themes);
                    break;
                case SwipeDirection.Up:
                    superLikedThemes.AddRange(themes);
                    break;
                case SwipeDirection.Left:
                    dislikedThemes.AddRange(themes);
                    break;
            }
        }

        return new LearningContext
        {
            LikedThemes = likedThemes.Distinct().ToList(),
            DislikedThemes = dislikedThemes.Distinct().ToList(),
            SuperLikedThemes = superLikedThemes.Distinct().ToList(),
            SwipeCount = swipes.Count
        };
    }

    /// <summary>
    /// Generates ideas using AI with learning context applied.
    /// </summary>
    private async Task<List<Idea>> GenerateIdeasWithAIAsync(
        SessionEntity session,
        int batchSize,
        int batchNumber,
        LearningContext learningContext,
        string? seedPhrase,
        CancellationToken cancellationToken)
    {
        // Use IdeaGenerator to create ideas
        IReadOnlyList<Idea> generatedIdeas;

        if (batchNumber == 1)
        {
            // First batch - use initial generation
            generatedIdeas = await ideaGenerator.GenerateInitialIdeasAsync(
                session.Id,
                session.AppType,
                session.ComplexityLevel,
                personality: null,
                cancellationToken);
        }
        else
        {
            // Subsequent batches - use learning from previous swipes
            var existingIdeas = await ideaRepository.GetBySessionIdAsync(session.Id, cancellationToken);
            var swipes = await swipeRepository.GetBySessionIdAsync(session.Id, cancellationToken);

            var likedIdeas = existingIdeas
                .Where(i => swipes.Any(s => s.IdeaId == i.Id && s.Direction is SwipeDirection.Right or SwipeDirection.Up))
                .ToList();
            var dislikedIdeas = existingIdeas
                .Where(i => swipes.Any(s => s.IdeaId == i.Id && s.Direction == SwipeDirection.Left))
                .ToList();

            generatedIdeas = await ideaGenerator.GenerateMutatedIdeasAsync(
                session.Id,
                likedIdeas,
                dislikedIdeas,
                batchNumber,
                Core.Enums.MutationType.Crossover,
                cancellationToken);
        }

        return generatedIdeas.Take(batchSize).ToList();
    }

    /// <summary>
    /// Builds the generation prompt incorporating learning context.
    /// </summary>
    private static string BuildGenerationPrompt(
        AppType appType,
        int complexityLevel,
        int batchNumber,
        LearningContext learningContext,
        string? seedPhrase)
    {
        var complexityDesc = complexityLevel switch
        {
            1 => "a simple weekend project",
            2 => "a small 1-2 week project",
            3 => "a medium month-long project",
            4 => "a complex multi-month project",
            5 => "an enterprise-grade extensive project",
            _ => "a project"
        };

        var prompt = $"Generate a unique {appType} app idea for {complexityDesc}. " +
                     $"This is batch {batchNumber} of ideas.";

        if (!string.IsNullOrWhiteSpace(seedPhrase))
        {
            prompt += $" The user is interested in: {seedPhrase}.";
        }

        if (learningContext.SuperLikedThemes.Count > 0)
        {
            prompt += $" The user LOVES ideas involving: {string.Join(", ", learningContext.SuperLikedThemes)}.";
        }

        if (learningContext.LikedThemes.Count > 0)
        {
            prompt += $" The user likes ideas involving: {string.Join(", ", learningContext.LikedThemes)}.";
        }

        if (learningContext.DislikedThemes.Count > 0)
        {
            prompt += $" AVOID themes like: {string.Join(", ", learningContext.DislikedThemes)}.";
        }

        return prompt;
    }

    /// <summary>
    /// Updates idea score based on swipe action.
    /// </summary>
    private async Task UpdateIdeaScoreAsync(
        Idea idea,
        Swipe swipe,
        CancellationToken cancellationToken)
    {
        var weight = CalculateSwipeWeight(swipe.DurationMs);
        var scoreAdjustment = swipe.Direction switch
        {
            SwipeDirection.Right => 1.0f * weight,
            SwipeDirection.Up => 2.0f * weight,
            SwipeDirection.Left => -0.5f * weight,
            _ => 0f
        };

        idea.Score = Math.Max(0, idea.Score + scoreAdjustment);
        await ideaRepository.UpdateAsync(idea, cancellationToken);
    }

    private static IdeaDto MapToDto(Idea entity) => new()
    {
        Id = entity.Id,
        SessionId = entity.SessionId,
        Title = entity.Title,
        Description = entity.Description,
        BatchNumber = entity.BatchNumber,
        DnaKeywords = entity.DnaKeywords,
        Score = entity.Score,
        CreatedAt = entity.CreatedAt
    };

    /// <summary>
    /// Internal context for learning from user swipes.
    /// </summary>
    private sealed class LearningContext
    {
        public List<string> LikedThemes { get; init; } = [];
        public List<string> DislikedThemes { get; init; } = [];
        public List<string> SuperLikedThemes { get; init; } = [];
        public int SwipeCount { get; init; }
    }
}
