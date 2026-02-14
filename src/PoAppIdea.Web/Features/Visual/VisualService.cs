using System.Collections.Concurrent;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Web.Infrastructure.AI;
using PoAppIdea.Web.Infrastructure.Storage;

using SessionEntity = PoAppIdea.Core.Entities.Session;
using SynthesisEntity = PoAppIdea.Core.Entities.Synthesis;

namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Service for managing visual asset generation and selection.
/// Edge Case T173: Implements queue-based retry for handling DALL-E rate limits and offline scenarios.
/// Pattern: Queue Pattern - stores failed requests for later retry.
/// </summary>
public sealed class VisualService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISynthesisRepository _synthesisRepository;
    private readonly VisualAssetRepository _visualAssetRepository;
    private readonly IVisualGenerator _visualGenerator;
    private readonly ILogger<VisualService> _logger;

    /// <summary>
    /// Delay between DALL-E requests to avoid rate limiting (milliseconds).
    /// </summary>
    private const int RequestDelayMs = 2000;

    /// <summary>
    /// Maximum retry attempts for failed image generation.
    /// </summary>
    private const int MaxRetries = 3;

    /// <summary>
    /// Maximum age for queued generation requests before they expire (hours).
    /// </summary>
    private const int MaxQueueAgeHours = 24;

    /// <summary>
    /// In-memory queue for failed generation requests (Edge Case T173).
    /// In production, this would be backed by durable storage (e.g., Azure Queue Storage).
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, VisualGenerationQueue> _generationQueues = new();

    public VisualService(
        ISessionRepository sessionRepository,
        ISynthesisRepository synthesisRepository,
        IVisualAssetRepository visualAssetRepository,
        IVisualGenerator visualGenerator,
        ILogger<VisualService> logger)
    {
        _sessionRepository = sessionRepository;
        _synthesisRepository = synthesisRepository;
        // Cast to concrete type to access blob upload methods
        _visualAssetRepository = (VisualAssetRepository)visualAssetRepository;
        _visualGenerator = visualGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current visual generation status for a session.
    /// Includes queue status for offline retry (T173).
    /// </summary>
    public async Task<GenerateVisualsResponse> GetGenerationStatusAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        var count = await _visualAssetRepository.CountBySessionIdAsync(sessionId, cancellationToken);

        // Check if there's a pending queue (Edge Case T173)
        var hasQueuedItems = _generationQueues.TryGetValue(sessionId, out var queue) && 
                             queue.PendingRequests.Count > 0 &&
                             !queue.IsExpired;

        // Determine status based on count, phase, and queue
        VisualGenerationStatus status;
        if (session.CurrentPhase == SessionPhase.Phase6_Visual && count >= 10)
        {
            status = VisualGenerationStatus.Completed;
        }
        else if (hasQueuedItems)
        {
            status = VisualGenerationStatus.Queued;
        }
        else if (count > 0)
        {
            status = VisualGenerationStatus.InProgress;
        }
        else
        {
            status = VisualGenerationStatus.Queued;
        }

        return new GenerateVisualsResponse
        {
            SessionId = sessionId,
            Status = status,
            GeneratedCount = count,
            QueuedCount = hasQueuedItems ? queue!.PendingRequests.Count : 0,
            EstimatedCompletionSeconds = status == VisualGenerationStatus.Completed
                ? null
                : (10 - count) * (RequestDelayMs / 1000 + 5) // ~7 seconds per image
        };
    }

    /// <summary>
    /// Retries any queued visual generation requests for a session (Edge Case T173).
    /// Call this when the AI service is back online.
    /// </summary>
    public async Task<int> RetryQueuedGenerationsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_generationQueues.TryGetValue(sessionId, out var queue) || queue.PendingRequests.IsEmpty)
        {
            return 0;
        }

        if (queue.IsExpired)
        {
            _generationQueues.TryRemove(sessionId, out _);
            _logger.LogInformation("Expired queue removed for session {SessionId}", sessionId);
            return 0;
        }

        var retried = 0;
        var generatedAssets = new List<VisualAsset>();

        while (queue.PendingRequests.TryDequeue(out var request))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var asset = await GenerateSingleVisualWithRetryAsync(
                sessionId,
                request.AppTitle,
                request.AppDescription,
                request.AppType,
                request.StyleIndex,
                request.StyleHint,
                cancellationToken);

            if (asset is not null)
            {
                generatedAssets.Add(asset);
                retried++;
            }
            else
            {
                // Re-queue if still failing
                queue.PendingRequests.Enqueue(request);
                _logger.LogWarning(
                    "Re-queued visual generation for session {SessionId}, style {StyleIndex}",
                    sessionId, request.StyleIndex);
                break; // Stop retrying if service is still down
            }

            // Delay between requests
            if (!queue.PendingRequests.IsEmpty)
            {
                await Task.Delay(RequestDelayMs, cancellationToken);
            }
        }

        // Save successfully generated assets
        if (generatedAssets.Count > 0)
        {
            await _visualAssetRepository.CreateBatchAsync(generatedAssets, cancellationToken);
        }

        // Clean up empty queue
        if (queue.PendingRequests.IsEmpty)
        {
            _generationQueues.TryRemove(sessionId, out _);
        }

        _logger.LogInformation(
            "Retried {Count} queued visual generations for session {SessionId}",
            retried, sessionId);

        return retried;
    }

    /// <summary>
    /// Generates visual mockups for a session.
    /// Uses the synthesis or selected ideas to create context-appropriate visuals.
    /// </summary>
    public async Task<GenerateVisualsResponse> GenerateVisualsAsync(
        Guid sessionId,
        GenerateVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        // Verify session is in a valid phase for visual generation
        // Allow from Phase4 onward since user may navigate directly
        if (session.CurrentPhase < SessionPhase.Phase4_ProductRefinement)
        {
            throw new InvalidOperationException(
                $"Session is in phase {session.CurrentPhase}. Must complete refinement before generating visuals.");
        }

        // Advance session to visual phase if not already there
        if (session.CurrentPhase != SessionPhase.Phase6_Visual)
        {
            session.CurrentPhase = SessionPhase.Phase6_Visual;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        // Check if visuals already exist (Optimization 6: reduced from 10 to 4)
        var existingCount = await _visualAssetRepository.CountBySessionIdAsync(sessionId, cancellationToken);
        if (existingCount >= 4)
        {
            return new GenerateVisualsResponse
            {
                SessionId = sessionId,
                Status = VisualGenerationStatus.Completed,
                GeneratedCount = existingCount
            };
        }

        // Get app context from synthesis or session
        var (appTitle, appDescription) = await GetAppContextAsync(session, cancellationToken);
        var appType = session.AppType.ToString();
        var count = Math.Min(request.Count, 4); // Optimization 6: reduced from 10 to 4

        _logger.LogInformation(
            "Starting visual generation for session {SessionId}: {Title}, generating {Count} visuals",
            sessionId, appTitle, count);

        // Update session phase to Visual
        session.CurrentPhase = SessionPhase.Phase6_Visual;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Generate visuals (this runs synchronously for simplicity, but could be made async with background jobs)
        var generatedAssets = new List<VisualAsset>();
        var startIndex = existingCount;

        for (var i = startIndex; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var asset = await GenerateSingleVisualWithRetryAsync(
                sessionId,
                appTitle,
                appDescription,
                appType,
                i,
                request.StyleHint,
                cancellationToken);

            if (asset is not null)
            {
                generatedAssets.Add(asset);
            }

            // Delay between requests to avoid rate limiting
            if (i < count - 1)
            {
                await Task.Delay(RequestDelayMs, cancellationToken);
            }
        }

        // Save all generated assets
        if (generatedAssets.Count > 0)
        {
            await _visualAssetRepository.CreateBatchAsync(generatedAssets, cancellationToken);
        }

        var totalCount = await _visualAssetRepository.CountBySessionIdAsync(sessionId, cancellationToken);

        return new GenerateVisualsResponse
        {
            SessionId = sessionId,
            Status = totalCount >= 3 ? VisualGenerationStatus.Completed : VisualGenerationStatus.InProgress,
            GeneratedCount = totalCount
        };
    }

    /// <summary>
    /// Gets all visual assets for a session.
    /// </summary>
    public async Task<GetVisualsResponse> GetVisualsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        var assets = await _visualAssetRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var selected = assets.FirstOrDefault(a => a.IsSelected);

        return new GetVisualsResponse
        {
            SessionId = sessionId,
            Visuals = assets.Select(ToDto).ToList(),
            HasSelection = selected is not null,
            SelectedVisualId = selected?.Id
        };
    }

    /// <summary>
    /// Selects a visual as the chosen direction for the session.
    /// </summary>
    public async Task SelectVisualAsync(
        Guid sessionId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        var asset = await _visualAssetRepository.GetByIdAsync(sessionId, assetId, cancellationToken);
        if (asset is null)
        {
            throw new InvalidOperationException($"Visual asset {assetId} not found");
        }

        // Select this visual (deselects others)
        await _visualAssetRepository.SelectAsync(sessionId, assetId, cancellationToken);

        _logger.LogInformation(
            "Visual {AssetId} selected for session {SessionId}",
            assetId, sessionId);
    }

    private async Task<(string Title, string Description)> GetAppContextAsync(
        SessionEntity session,
        CancellationToken cancellationToken)
    {
        // Try to get synthesis first
        if (session.SynthesisId.HasValue)
        {
            var synthesis = await _synthesisRepository.GetBySessionIdAsync(
                session.Id,
                cancellationToken);

            if (synthesis is not null)
            {
                return (synthesis.MergedTitle, synthesis.MergedDescription);
            }
        }

        // Fallback to session description
        return ($"New {session.AppType} App", $"A {session.AppType.ToString().ToLowerInvariant()} application with complexity level {session.ComplexityLevel}");
    }

    private async Task<VisualAsset?> GenerateSingleVisualWithRetryAsync(
        Guid sessionId,
        string appTitle,
        string appDescription,
        string appType,
        int styleIndex,
        string? styleHint,
        CancellationToken cancellationToken)
    {
        var prompt = _visualGenerator.GeneratePrompt(appTitle, appDescription, appType, styleIndex, styleHint);
        var styleInfo = _visualGenerator.GetStyleInfo(styleIndex);

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var imageBytes = await _visualGenerator.GenerateImageAsync(prompt, cancellationToken);
                if (imageBytes is null || imageBytes.Length == 0)
                {
                    _logger.LogWarning("Empty image returned for style {StyleIndex}, attempt {Attempt}", styleIndex, attempt + 1);
                    continue;
                }

                var assetId = Guid.NewGuid();

                // Upload to blob storage
                var blobUrl = await _visualAssetRepository.UploadImageAsync(assetId, imageBytes, cancellationToken);
                var thumbnailUrl = await _visualAssetRepository.UploadThumbnailAsync(assetId, imageBytes, cancellationToken);

                return new VisualAsset
                {
                    Id = assetId,
                    SessionId = sessionId,
                    BlobUrl = blobUrl,
                    ThumbnailUrl = thumbnailUrl,
                    Prompt = prompt,
                    StyleAttributes = styleInfo,
                    IsSelected = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                _logger.LogWarning(ex, "Visual generation failed for style {StyleIndex}, attempt {Attempt}. Retrying...",
                    styleIndex, attempt + 1);

                // Exponential backoff
                await Task.Delay(1000 * (attempt + 1), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visual generation failed for style {StyleIndex} after {MaxRetries} attempts",
                    styleIndex, MaxRetries);
                break;
            }
        }

        return null;
    }

    private static VisualAssetDto ToDto(VisualAsset asset)
    {
        return new VisualAssetDto
        {
            Id = asset.Id,
            BlobUrl = asset.BlobUrl,
            ThumbnailUrl = asset.ThumbnailUrl,
            Prompt = asset.Prompt,
            StyleAttributes = new StyleInfoDto
            {
                ColorPalette = asset.StyleAttributes.ColorPalette,
                LayoutStyle = asset.StyleAttributes.LayoutStyle,
                Vibe = asset.StyleAttributes.Vibe
            },
            IsSelected = asset.IsSelected,
            CreatedAt = asset.CreatedAt
        };
    }
}
