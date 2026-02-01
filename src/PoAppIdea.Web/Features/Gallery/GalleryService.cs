using System.Collections.Concurrent;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;

using SessionEntity = PoAppIdea.Core.Entities.Session;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Service for managing the public gallery of published artifacts.
/// T174: Implements response caching for gallery endpoints.
/// Pattern: Cache-Aside - cache is checked first, then populated on miss.
/// </summary>
public sealed class GalleryService
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<GalleryService> _logger;

    /// <summary>
    /// Maximum items per page.
    /// </summary>
    private const int MaxPageSize = 100;

    /// <summary>
    /// Cache duration for gallery browse results (T174).
    /// </summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// In-memory cache for gallery responses (T174).
    /// In production, use distributed cache (Redis) for multi-instance deployments.
    /// </summary>
    private static readonly ConcurrentDictionary<string, CachedGalleryResponse> _browseCache = new();

    public GalleryService(
        IArtifactRepository artifactRepository,
        ISessionRepository sessionRepository,
        ILogger<GalleryService> logger)
    {
        _artifactRepository = artifactRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Browses the public gallery with optional filters and pagination.
    /// T174: Results are cached for improved performance.
    /// </summary>
    public async Task<BrowseGalleryResponse> BrowseGalleryAsync(
        BrowseGalleryRequest request,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Min(request.Limit, MaxPageSize);
        var skip = DecodeCursor(request.Cursor);

        // T174: Check cache first
        var cacheKey = BuildCacheKey(request.Query, request.AppType, skip, limit);
        if (TryGetFromCache(cacheKey, out var cachedResponse))
        {
            _logger.LogDebug("Cache HIT for gallery browse: {CacheKey}", cacheKey);
            return cachedResponse!;
        }

        _logger.LogDebug(
            "Cache MISS - Browsing gallery: Query='{Query}', AppType={AppType}, Skip={Skip}, Limit={Limit}",
            request.Query, request.AppType, skip, limit);

        // Get published artifacts
        var artifacts = await _artifactRepository.GetPublishedAsync(skip, limit + 1, cancellationToken);

        // Get session info for app types
        var sessionIds = artifacts.Select(a => a.SessionId).Distinct().ToList();
        var sessions = new Dictionary<Guid, SessionEntity>();
        foreach (var sessionId in sessionIds)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
            if (session is not null)
            {
                sessions[sessionId] = session;
            }
        }

        // Apply filters
        var filteredArtifacts = artifacts.AsEnumerable();

        if (request.AppType.HasValue)
        {
            filteredArtifacts = filteredArtifacts.Where(a => 
                sessions.TryGetValue(a.SessionId, out var s) && s.AppType == request.AppType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var query = request.Query.ToLowerInvariant();
            filteredArtifacts = filteredArtifacts.Where(a =>
                a.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Content.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var resultList = filteredArtifacts.Take(limit).ToList();
        var hasMore = artifacts.Count > limit;

        // Map to DTOs
        var items = resultList.Select(a => new GalleryItemDto
        {
            ArtifactId = a.Id,
            Title = a.Title,
            ArtifactType = a.Type,
            AppType = sessions.TryGetValue(a.SessionId, out var s) ? s.AppType : AppType.WebApp,
            AuthorDisplayName = "Community Member", // In production, join with User table
            Slug = a.HumanReadableSlug,
            PublishedAt = a.PublishedAt ?? a.CreatedAt,
            ThumbnailUrl = a.BlobUrl,
            Description = GetExcerpt(a.Content)
        }).ToList();

        var response = new BrowseGalleryResponse
        {
            Items = items,
            NextCursor = hasMore ? EncodeCursor(skip + limit) : null,
            TotalCount = null // Expensive to compute for large datasets
        };

        // T174: Cache the response
        AddToCache(cacheKey, response);

        return response;
    }

    /// <summary>
    /// Invalidates gallery cache when artifacts are published/unpublished.
    /// T174: Call this when gallery content changes.
    /// </summary>
    public void InvalidateGalleryCache()
    {
        _browseCache.Clear();
        _logger.LogInformation("Gallery cache invalidated");
    }

    /// <summary>
    /// Builds a cache key for gallery browse requests.
    /// </summary>
    private static string BuildCacheKey(string? query, AppType? appType, int skip, int limit)
    {
        return $"gallery:{query ?? "all"}:{appType?.ToString() ?? "any"}:{skip}:{limit}";
    }

    /// <summary>
    /// Tries to get a cached response.
    /// </summary>
    private static bool TryGetFromCache(string cacheKey, out BrowseGalleryResponse? response)
    {
        if (_browseCache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
        {
            response = cached.Response;
            return true;
        }

        response = null;
        return false;
    }

    /// <summary>
    /// Adds a response to the cache.
    /// </summary>
    private static void AddToCache(string cacheKey, BrowseGalleryResponse response)
    {
        _browseCache[cacheKey] = new CachedGalleryResponse(response, DateTimeOffset.UtcNow.Add(CacheDuration));

        // Simple cleanup: remove expired entries when cache grows
        if (_browseCache.Count > 1000)
        {
            var expiredKeys = _browseCache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _browseCache.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// Gets a single gallery item by artifact ID.
    /// </summary>
    public async Task<GalleryItemDto?> GetGalleryItemAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        
        if (artifact is null || !artifact.IsPublished)
        {
            return null;
        }

        var session = await _sessionRepository.GetByIdAsync(artifact.SessionId, cancellationToken);

        return new GalleryItemDto
        {
            ArtifactId = artifact.Id,
            Title = artifact.Title,
            ArtifactType = artifact.Type,
            AppType = session?.AppType ?? AppType.WebApp,
            AuthorDisplayName = "Community Member",
            Slug = artifact.HumanReadableSlug,
            PublishedAt = artifact.PublishedAt ?? artifact.CreatedAt,
            ThumbnailUrl = artifact.BlobUrl,
            Description = GetExcerpt(artifact.Content)
        };
    }

    /// <summary>
    /// Publishes an artifact to the public gallery.
    /// </summary>
    public async Task<GalleryItemDto> PublishArtifactAsync(
        Guid artifactId,
        Guid userId,
        PublishArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        
        if (artifact is null)
        {
            throw new InvalidOperationException($"Artifact {artifactId} not found");
        }

        if (artifact.UserId != userId)
        {
            throw new InvalidOperationException("You can only publish your own artifacts");
        }

        if (artifact.IsPublished)
        {
            throw new InvalidOperationException("Artifact is already published");
        }

        // Update artifact to published
        artifact.IsPublished = true;
        artifact.PublishedAt = DateTimeOffset.UtcNow;
        
        await _artifactRepository.UpdateAsync(artifact, cancellationToken);

        _logger.LogInformation(
            "Published artifact {ArtifactId} to gallery by user {UserId}",
            artifactId, userId);

        var session = await _sessionRepository.GetByIdAsync(artifact.SessionId, cancellationToken);

        return new GalleryItemDto
        {
            ArtifactId = artifact.Id,
            Title = artifact.Title,
            ArtifactType = artifact.Type,
            AppType = session?.AppType ?? AppType.WebApp,
            AuthorDisplayName = "You",
            Slug = artifact.HumanReadableSlug,
            PublishedAt = artifact.PublishedAt.Value,
            ThumbnailUrl = artifact.BlobUrl,
            Description = GetExcerpt(artifact.Content)
        };
    }

    /// <summary>
    /// Unpublishes an artifact from the gallery.
    /// </summary>
    public async Task UnpublishArtifactAsync(
        Guid artifactId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        
        if (artifact is null)
        {
            throw new InvalidOperationException($"Artifact {artifactId} not found");
        }

        if (artifact.UserId != userId)
        {
            throw new InvalidOperationException("You can only unpublish your own artifacts");
        }

        if (!artifact.IsPublished)
        {
            return; // Already unpublished
        }

        artifact.IsPublished = false;
        artifact.PublishedAt = null;
        
        await _artifactRepository.UpdateAsync(artifact, cancellationToken);

        _logger.LogInformation(
            "Unpublished artifact {ArtifactId} from gallery by user {UserId}",
            artifactId, userId);
    }

    /// <summary>
    /// Imports an idea from the gallery as a starting point for a new session.
    /// </summary>
    public async Task<SessionEntity> ImportFromGalleryAsync(
        Guid artifactId,
        Guid userId,
        ImportIdeaRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        
        if (artifact is null || !artifact.IsPublished)
        {
            throw new InvalidOperationException($"Published artifact {artifactId} not found");
        }

        // Get the original session to copy app type and complexity
        var originalSession = await _sessionRepository.GetByIdAsync(artifact.SessionId, cancellationToken);
        
        if (originalSession is null)
        {
            throw new InvalidOperationException("Original session not found");
        }

        // Create a new session based on the imported artifact
        var newSession = new SessionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AppType = originalSession.AppType,
            ComplexityLevel = originalSession.ComplexityLevel,
            CurrentPhase = SessionPhase.Phase0_Scope, // Start fresh
            Status = SessionStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow,
            TopIdeaIds = new List<Guid>(),
            SelectedIdeaIds = new List<Guid>()
        };

        await _sessionRepository.CreateAsync(newSession, cancellationToken);

        _logger.LogInformation(
            "Created new session {SessionId} from imported artifact {ArtifactId} for user {UserId}",
            newSession.Id, artifactId, userId);

        return newSession;
    }

    /// <summary>
    /// Gets user's session history with their artifacts.
    /// </summary>
    public async Task<UserHistoryDto> GetUserHistoryAsync(
        Guid userId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);
        var sessionItems = new List<SessionHistoryItemDto>();

        foreach (var session in sessions.OrderByDescending(s => s.CreatedAt).Take(limit))
        {
            var artifacts = await _artifactRepository.GetBySessionIdAsync(session.Id, cancellationToken);

            // Try to get idea title from synthesis
            string? ideaTitle = null;
            if (session.SynthesisId.HasValue)
            {
                // For now, use session description or placeholder
                ideaTitle = $"{session.AppType} App Session";
            }

            sessionItems.Add(new SessionHistoryItemDto
            {
                SessionId = session.Id,
                AppType = session.AppType,
                ComplexityLevel = session.ComplexityLevel,
                CurrentPhase = session.CurrentPhase,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                CompletedAt = session.CompletedAt,
                IdeaTitle = ideaTitle,
                Artifacts = artifacts.Select(a => new SessionArtifactDto
                {
                    ArtifactId = a.Id,
                    Title = a.Title,
                    ArtifactType = a.Type.ToString(),
                    IsPublished = a.IsPublished,
                    ImportCount = 0, // Import count tracking is not yet implemented in the entity
                    CreatedAt = a.CreatedAt
                }).ToList()
            });
        }

        return new UserHistoryDto { Sessions = sessionItems };
    }

    private static int DecodeCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return 0;
        return int.TryParse(cursor, out var skip) ? skip : 0;
    }

    private static string EncodeCursor(int skip)
    {
        return skip.ToString();
    }

    private static string GetExcerpt(string content, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(content)) return "";
        
        // Skip markdown headers and get first paragraph
        var lines = content.Split('\n')
            .Where(l => !l.StartsWith('#') && !string.IsNullOrWhiteSpace(l))
            .Take(3);
        
        var excerpt = string.Join(" ", lines).Trim();
        
        if (excerpt.Length <= maxLength) return excerpt;
        
        return excerpt[..maxLength].TrimEnd() + "...";
    }
}

/// <summary>
/// Cached gallery response with expiration (T174).
/// </summary>
internal sealed record CachedGalleryResponse(BrowseGalleryResponse Response, DateTimeOffset ExpiresAt)
{
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}

/// <summary>
/// Complete user history including sessions and their artifacts.
/// </summary>
public sealed class UserHistoryDto
{
    /// <summary>
    /// All sessions for this user.
    /// </summary>
    public required List<SessionHistoryItemDto> Sessions { get; init; }

    /// <summary>
    /// Total number of sessions.
    /// </summary>
    public int TotalSessions => Sessions.Count;

    /// <summary>
    /// Total number of artifacts across all sessions.
    /// </summary>
    public int TotalArtifacts => Sessions.Sum(s => s.Artifacts.Count);

    /// <summary>
    /// Total number of published artifacts.
    /// </summary>
    public int TotalPublished => Sessions.Sum(s => s.Artifacts.Count(a => a.IsPublished));

    /// <summary>
    /// Total number of imports of user's published artifacts.
    /// </summary>
    public int TotalImports => Sessions.Sum(s => s.Artifacts.Sum(a => a.ImportCount));
}

/// <summary>
/// A session in the user's history with its artifacts.
/// </summary>
public sealed record SessionHistoryItemDto
{
    public required Guid SessionId { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required SessionPhase CurrentPhase { get; init; }
    public required SessionStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? IdeaTitle { get; init; }
    public required List<SessionArtifactDto> Artifacts { get; init; }
}

/// <summary>
/// An artifact within a session history item.
/// </summary>
public sealed record SessionArtifactDto
{
    public required Guid ArtifactId { get; init; }
    public required string Title { get; init; }
    public required string ArtifactType { get; init; }
    public required bool IsPublished { get; init; }
    public int ImportCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// DTO for user session history (legacy format).
/// </summary>
public sealed record UserSessionHistoryDto
{
    public required Guid SessionId { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required SessionPhase CurrentPhase { get; init; }
    public required SessionStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public required int ArtifactCount { get; init; }
    public required int PublishedCount { get; init; }
}
