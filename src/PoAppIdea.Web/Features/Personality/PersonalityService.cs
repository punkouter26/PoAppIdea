using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;

namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Service for managing user Product Personality profiles.
/// Aggregates swipe behavior to learn user preferences across sessions.
/// </summary>
public sealed class PersonalityService
{
    private readonly IPersonalityRepository _personalityRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISwipeRepository _swipeRepository;
    private readonly IIdeaRepository _ideaRepository;
    private readonly ILogger<PersonalityService> _logger;

    /// <summary>
    /// Bias decay factor for unused preferences (10% per 30 days of inactivity).
    /// </summary>
    private const float BiasDecayFactor = 0.1f;
    private static readonly TimeSpan DecayPeriod = TimeSpan.FromDays(30);

    /// <summary>
    /// Maximum number of disliked patterns to store.
    /// </summary>
    private const int MaxDislikedPatterns = 50;

    public PersonalityService(
        IPersonalityRepository personalityRepository,
        ISessionRepository sessionRepository,
        ISwipeRepository swipeRepository,
        IIdeaRepository ideaRepository,
        ILogger<PersonalityService> logger)
    {
        _personalityRepository = personalityRepository;
        _sessionRepository = sessionRepository;
        _swipeRepository = swipeRepository;
        _ideaRepository = ideaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets the personality profile for a user.
    /// </summary>
    public async Task<GetPersonalityResponse?> GetPersonalityAsync(
        Guid userId,
        GetPersonalityRequest request,
        CancellationToken cancellationToken = default)
    {
        var personality = await _personalityRepository.GetByUserIdAsync(userId, cancellationToken);

        if (personality is null)
        {
            _logger.LogDebug("No personality profile found for user {UserId}", userId);
            return null;
        }

        // Apply decay if needed
        personality = await ApplyBiasDecayIfNeededAsync(personality, cancellationToken);

        var response = new GetPersonalityResponse
        {
            UserId = personality.UserId,
            ProductBiases = personality.ProductBiases,
            TechnicalBiases = personality.TechnicalBiases,
            DislikedPatterns = personality.DislikedPatterns,
            SwipeSpeedProfile = new SwipeSpeedProfileDto
            {
                AverageFastMs = personality.SwipeSpeedProfile.AverageFastMs,
                AverageMediumMs = personality.SwipeSpeedProfile.AverageMediumMs,
                AverageSlowMs = personality.SwipeSpeedProfile.AverageSlowMs
            },
            TotalSessions = personality.TotalSessions,
            LastUpdatedAt = personality.LastUpdatedAt,
            TopPreferences = ExtractTopPreferences(personality)
        };

        if (request.IncludeSessionHistory)
        {
            var sessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);
            response = response with
            {
                RecentSessions = sessions
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(10)
                    .Select(s => new SessionSummaryDto
                    {
                        SessionId = s.Id,
                        AppType = s.AppType.ToString(),
                        ComplexityLevel = s.ComplexityLevel,
                        CurrentPhase = s.CurrentPhase.ToString(),
                        CreatedAt = s.CreatedAt,
                        CompletedAt = s.CompletedAt
                    })
                    .ToList()
            };
        }

        return response;
    }

    /// <summary>
    /// Gets or creates a personality profile for a user.
    /// </summary>
    public async Task<ProductPersonality> GetOrCreatePersonalityAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var personality = await _personalityRepository.GetByUserIdAsync(userId, cancellationToken);

        if (personality is null)
        {
            personality = CreateDefaultPersonality(userId);
            personality = await _personalityRepository.UpsertAsync(personality, cancellationToken);
            _logger.LogInformation("Created new personality profile for user {UserId}", userId);
        }

        return personality;
    }

    /// <summary>
    /// Updates the personality profile based on a completed session.
    /// </summary>
    public async Task<ProductPersonality> UpdateFromSessionAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var personality = await GetOrCreatePersonalityAsync(userId, cancellationToken);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        // Get swipes for this session
        var swipes = await _swipeRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        if (swipes.Count == 0)
        {
            _logger.LogDebug("No swipes found for session {SessionId}, skipping personality update", sessionId);
            return personality;
        }

        // Get ideas for keyword extraction
        var ideaIds = swipes.Select(s => s.IdeaId).Distinct().ToList();
        var ideas = await GetIdeasByIdsAsync(ideaIds, sessionId, cancellationToken);

        // Update biases based on swipes
        UpdateProductBiases(personality, swipes, ideas);
        UpdateTechnicalBiases(personality, swipes, ideas);
        UpdateDislikedPatterns(personality, swipes, ideas);
        UpdateSwipeSpeedProfile(personality, swipes);

        // Increment session count
        personality.TotalSessions++;
        personality.LastUpdatedAt = DateTimeOffset.UtcNow;

        // Persist changes
        personality = await _personalityRepository.UpsertAsync(personality, cancellationToken);

        _logger.LogInformation(
            "Updated personality for user {UserId} from session {SessionId}. Total sessions: {TotalSessions}",
            userId, sessionId, personality.TotalSessions);

        return personality;
    }

    /// <summary>
    /// Updates the personality profile with manual overrides.
    /// </summary>
    public async Task<GetPersonalityResponse> UpdatePersonalityAsync(
        Guid userId,
        UpdatePersonalityRequest request,
        CancellationToken cancellationToken = default)
    {
        var personality = await GetOrCreatePersonalityAsync(userId, cancellationToken);

        if (request.ResetProfile)
        {
            personality = CreateDefaultPersonality(userId);
            _logger.LogInformation("Reset personality profile for user {UserId}", userId);
        }
        else
        {
            // Apply product bias overrides
            if (request.ProductBiases is not null)
            {
                foreach (var (key, value) in request.ProductBiases)
                {
                    personality.ProductBiases[key] = Math.Clamp(value, -1.0f, 1.0f);
                }
            }

            // Apply technical bias overrides
            if (request.TechnicalBiases is not null)
            {
                foreach (var (key, value) in request.TechnicalBiases)
                {
                    personality.TechnicalBiases[key] = Math.Clamp(value, -1.0f, 1.0f);
                }
            }

            // Add disliked patterns
            if (request.AddDislikedPatterns is not null)
            {
                foreach (var pattern in request.AddDislikedPatterns)
                {
                    if (!personality.DislikedPatterns.Contains(pattern) &&
                        personality.DislikedPatterns.Count < MaxDislikedPatterns)
                    {
                        personality.DislikedPatterns.Add(pattern);
                    }
                }
            }

            // Remove disliked patterns
            if (request.RemoveDislikedPatterns is not null)
            {
                foreach (var pattern in request.RemoveDislikedPatterns)
                {
                    personality.DislikedPatterns.Remove(pattern);
                }
            }
        }

        personality.LastUpdatedAt = DateTimeOffset.UtcNow;
        personality = await _personalityRepository.UpsertAsync(personality, cancellationToken);

        return new GetPersonalityResponse
        {
            UserId = personality.UserId,
            ProductBiases = personality.ProductBiases,
            TechnicalBiases = personality.TechnicalBiases,
            DislikedPatterns = personality.DislikedPatterns,
            SwipeSpeedProfile = new SwipeSpeedProfileDto
            {
                AverageFastMs = personality.SwipeSpeedProfile.AverageFastMs,
                AverageMediumMs = personality.SwipeSpeedProfile.AverageMediumMs,
                AverageSlowMs = personality.SwipeSpeedProfile.AverageSlowMs
            },
            TotalSessions = personality.TotalSessions,
            LastUpdatedAt = personality.LastUpdatedAt,
            TopPreferences = ExtractTopPreferences(personality)
        };
    }

    private static ProductPersonality CreateDefaultPersonality(Guid userId) => new()
    {
        UserId = userId,
        ProductBiases = new Dictionary<string, float>(),
        TechnicalBiases = new Dictionary<string, float>(),
        DislikedPatterns = new List<string>(),
        SwipeSpeedProfile = new SpeedProfile
        {
            AverageFastMs = 500,
            AverageMediumMs = 2000,
            AverageSlowMs = 5000
        },
        TotalSessions = 0,
        LastUpdatedAt = DateTimeOffset.UtcNow
    };

    private async Task<ProductPersonality> ApplyBiasDecayIfNeededAsync(
        ProductPersonality personality,
        CancellationToken cancellationToken)
    {
        var timeSinceUpdate = DateTimeOffset.UtcNow - personality.LastUpdatedAt;
        var decayPeriods = (int)(timeSinceUpdate / DecayPeriod);

        if (decayPeriods <= 0)
        {
            return personality;
        }

        // Apply exponential decay
        var decayMultiplier = (float)Math.Pow(1 - BiasDecayFactor, decayPeriods);

        foreach (var key in personality.ProductBiases.Keys.ToList())
        {
            personality.ProductBiases[key] *= decayMultiplier;
        }

        foreach (var key in personality.TechnicalBiases.Keys.ToList())
        {
            personality.TechnicalBiases[key] *= decayMultiplier;
        }

        personality.LastUpdatedAt = DateTimeOffset.UtcNow;
        personality = await _personalityRepository.UpsertAsync(personality, cancellationToken);

        _logger.LogDebug(
            "Applied {Periods} decay periods to personality for user {UserId}",
            decayPeriods, personality.UserId);

        return personality;
    }

    private static void UpdateProductBiases(
        ProductPersonality personality,
        IReadOnlyList<Swipe> swipes,
        IReadOnlyDictionary<Guid, Idea> ideas)
    {
        foreach (var swipe in swipes)
        {
            if (!ideas.TryGetValue(swipe.IdeaId, out var idea))
            {
                continue;
            }

            var biasChange = swipe.Direction == SwipeDirection.Right ? 0.1f : -0.1f;
            var speedMultiplier = swipe.SpeedCategory switch
            {
                SwipeSpeed.Fast => 1.5f,
                SwipeSpeed.Medium => 1.0f,
                SwipeSpeed.Slow => 0.5f,
                _ => 1.0f
            };

            foreach (var keyword in idea.DnaKeywords)
            {
                if (!personality.ProductBiases.ContainsKey(keyword))
                {
                    personality.ProductBiases[keyword] = 0f;
                }

                personality.ProductBiases[keyword] = Math.Clamp(
                    personality.ProductBiases[keyword] + biasChange * speedMultiplier,
                    -1.0f, 1.0f);
            }
        }
    }

    private static void UpdateTechnicalBiases(
        ProductPersonality personality,
        IReadOnlyList<Swipe> swipes,
        IReadOnlyDictionary<Guid, Idea> ideas)
    {
        // Technical biases are derived from keywords containing technical terms
        var technicalKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "serverless", "monolith", "microservices", "cloud", "api",
            "realtime", "offline", "mobile", "web", "desktop",
            "ai", "ml", "blockchain", "iot", "saas", "subscription"
        };

        foreach (var swipe in swipes)
        {
            if (!ideas.TryGetValue(swipe.IdeaId, out var idea))
            {
                continue;
            }

            var biasChange = swipe.Direction == SwipeDirection.Right ? 0.1f : -0.1f;

            foreach (var keyword in idea.DnaKeywords.Where(k => technicalKeywords.Contains(k)))
            {
                if (!personality.TechnicalBiases.ContainsKey(keyword))
                {
                    personality.TechnicalBiases[keyword] = 0f;
                }

                personality.TechnicalBiases[keyword] = Math.Clamp(
                    personality.TechnicalBiases[keyword] + biasChange,
                    -1.0f, 1.0f);
            }
        }
    }

    private static void UpdateDislikedPatterns(
        ProductPersonality personality,
        IReadOnlyList<Swipe> swipes,
        IReadOnlyDictionary<Guid, Idea> ideas)
    {
        // Track patterns from strongly disliked ideas (fast dislikes)
        var strongDislikes = swipes
            .Where(s => s.Direction == SwipeDirection.Left && s.SpeedCategory == SwipeSpeed.Fast)
            .ToList();

        foreach (var swipe in strongDislikes)
        {
            if (!ideas.TryGetValue(swipe.IdeaId, out var idea))
            {
                continue;
            }

            // Add first keyword as disliked pattern (if not already present)
            var pattern = idea.DnaKeywords.FirstOrDefault();
            if (pattern is not null &&
                !personality.DislikedPatterns.Contains(pattern) &&
                personality.DislikedPatterns.Count < MaxDislikedPatterns)
            {
                personality.DislikedPatterns.Add(pattern);
            }
        }
    }

    private static void UpdateSwipeSpeedProfile(ProductPersonality personality, IReadOnlyList<Swipe> swipes)
    {
        var fastSwipes = swipes.Where(s => s.SpeedCategory == SwipeSpeed.Fast).Select(s => (double)s.DurationMs).ToList();
        var mediumSwipes = swipes.Where(s => s.SpeedCategory == SwipeSpeed.Medium).Select(s => (double)s.DurationMs).ToList();
        var slowSwipes = swipes.Where(s => s.SpeedCategory == SwipeSpeed.Slow).Select(s => (double)s.DurationMs).ToList();

        if (fastSwipes.Count > 0)
        {
            personality.SwipeSpeedProfile.AverageFastMs =
                (personality.SwipeSpeedProfile.AverageFastMs + fastSwipes.Average()) / 2;
        }

        if (mediumSwipes.Count > 0)
        {
            personality.SwipeSpeedProfile.AverageMediumMs =
                (personality.SwipeSpeedProfile.AverageMediumMs + mediumSwipes.Average()) / 2;
        }

        if (slowSwipes.Count > 0)
        {
            personality.SwipeSpeedProfile.AverageSlowMs =
                (personality.SwipeSpeedProfile.AverageSlowMs + slowSwipes.Average()) / 2;
        }
    }

    private static IReadOnlyList<PreferenceThemeDto> ExtractTopPreferences(ProductPersonality personality)
    {
        var preferences = new List<PreferenceThemeDto>();

        // Add top product biases
        preferences.AddRange(personality.ProductBiases
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => new PreferenceThemeDto
            {
                Theme = kvp.Key,
                Strength = kvp.Value,
                Category = "Product"
            }));

        // Add top technical biases
        preferences.AddRange(personality.TechnicalBiases
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => new PreferenceThemeDto
            {
                Theme = kvp.Key,
                Strength = kvp.Value,
                Category = "Technical"
            }));

        return preferences.OrderByDescending(p => p.Strength).Take(10).ToList();
    }

    private async Task<IReadOnlyDictionary<Guid, Idea>> GetIdeasByIdsAsync(
        IReadOnlyList<Guid> ideaIds,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var ideas = await _ideaRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return ideas.Where(i => ideaIds.Contains(i.Id)).ToDictionary(i => i.Id);
    }
}
