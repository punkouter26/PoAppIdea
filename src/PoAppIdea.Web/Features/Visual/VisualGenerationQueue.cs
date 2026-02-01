using System.Collections.Concurrent;

namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Queue for offline visual generation requests (Edge Case T173).
/// Stores failed generation requests for later retry when the AI service is available.
/// Pattern: Queue Pattern - decouples request submission from processing.
/// </summary>
public sealed class VisualGenerationQueue
{
    /// <summary>
    /// Pending visual generation requests.
    /// </summary>
    public ConcurrentQueue<QueuedVisualRequest> PendingRequests { get; } = new();

    /// <summary>
    /// When the queue was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Maximum age for queued requests (24 hours).
    /// </summary>
    private const int MaxQueueAgeHours = 24;

    /// <summary>
    /// Whether the queue has expired and should be discarded.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow - CreatedAt > TimeSpan.FromHours(MaxQueueAgeHours);
}

/// <summary>
/// A queued visual generation request for later retry.
/// </summary>
public sealed record QueuedVisualRequest
{
    /// <summary>
    /// The app title for the visual prompt.
    /// </summary>
    public required string AppTitle { get; init; }

    /// <summary>
    /// The app description for context.
    /// </summary>
    public required string AppDescription { get; init; }

    /// <summary>
    /// The app type (Mobile, Web, Desktop, etc.).
    /// </summary>
    public required string AppType { get; init; }

    /// <summary>
    /// The style index for variation.
    /// </summary>
    public required int StyleIndex { get; init; }

    /// <summary>
    /// Optional style hint from the user.
    /// </summary>
    public string? StyleHint { get; init; }

    /// <summary>
    /// When the request was originally made.
    /// </summary>
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of retry attempts so far.
    /// </summary>
    public int RetryCount { get; init; }
}
