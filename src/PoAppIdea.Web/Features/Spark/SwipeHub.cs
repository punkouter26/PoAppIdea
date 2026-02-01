using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PoAppIdea.Core.Enums;
using PoAppIdea.Web.Infrastructure;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// SignalR hub for real-time swipe capture and updates.
/// Provides real-time communication for the swipe interface.
/// Note: AllowAnonymous is used because Blazor Server SignalR connections
/// don't automatically include auth cookies. Auth is validated per-method.
/// </summary>
[AllowAnonymous]
public sealed class SwipeHub : Hub
{
    private readonly SparkService _sparkService;
    private readonly ILogger<SwipeHub> _logger;

    public SwipeHub(SparkService sparkService, ILogger<SwipeHub> logger)
    {
        _sparkService = sparkService;
        _logger = logger;
    }

    /// <summary>
    /// Joins a session group for receiving real-time updates.
    /// </summary>
    public async Task JoinSession(Guid sessionId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            throw new HubException("User not authenticated");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroup(sessionId));
        _logger.LogDebug("User {UserId} joined session {SessionId}", userId, sessionId);
    }

    /// <summary>
    /// Leaves a session group.
    /// </summary>
    public async Task LeaveSession(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSessionGroup(sessionId));
        _logger.LogDebug("Connection {ConnectionId} left session {SessionId}", Context.ConnectionId, sessionId);
    }

    /// <summary>
    /// Records a swipe action in real-time.
    /// </summary>
    public async Task RecordSwipe(Guid sessionId, Guid ideaId, SwipeDirection direction, int durationMs)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            throw new HubException("User not authenticated");
        }

        try
        {
            var request = new RecordSwipeRequest
            {
                IdeaId = ideaId,
                Direction = direction,
                DurationMs = durationMs
            };

            var swipe = await _sparkService.RecordSwipeAsync(sessionId, userId.Value, request);

            // Notify the session group about the swipe
            await Clients.Group(GetSessionGroup(sessionId)).SendAsync("SwipeRecorded", new
            {
                IdeaId = ideaId,
                Direction = direction.ToString(),
                DurationMs = durationMs,
                SpeedCategory = swipe.SpeedCategory.ToString(),
                Timestamp = swipe.Timestamp
            });

            _logger.LogDebug(
                "Swipe recorded via SignalR: {Direction} on idea {IdeaId}, session {SessionId}",
                direction,
                ideaId,
                sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording swipe via SignalR");
            throw new HubException($"Failed to record swipe: {ex.Message}");
        }
    }

    /// <summary>
    /// Requests next batch of ideas for the session.
    /// </summary>
    public async Task RequestNextBatch(Guid sessionId, int batchSize = 10)
    {
        try
        {
            var request = new GenerateIdeasRequest { BatchSize = batchSize };
            var response = await _sparkService.GenerateIdeasAsync(sessionId, request);

            // Send ideas directly to the caller
            await Clients.Caller.SendAsync("NextBatchReady", response);

            _logger.LogDebug(
                "Batch {BatchNumber} with {Count} ideas sent via SignalR for session {SessionId}",
                response.BatchNumber,
                response.Ideas.Count,
                sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next batch via SignalR");
            throw new HubException($"Failed to generate ideas: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets top ideas for the session.
    /// </summary>
    public async Task RequestTopIdeas(Guid sessionId)
    {
        try
        {
            var topIdeas = await _sparkService.GetTopIdeasAsync(sessionId);
            await Clients.Caller.SendAsync("TopIdeasReady", topIdeas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top ideas via SignalR");
            throw new HubException($"Failed to get top ideas: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} connected to SwipeHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from SwipeHub with error", userId);
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from SwipeHub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private Guid? GetUserId()
    {
        // First try to get from claims (for direct HTTP auth)
        var userId = UserIdHelper.GetUserId(Context.User);
        if (userId.HasValue)
        {
            return userId;
        }
        
        // Fall back to query string (for Blazor Server where cookies don't propagate)
        var httpContext = Context.GetHttpContext();
        var queryUserId = httpContext?.Request.Query["userId"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryUserId))
        {
            _logger.LogDebug("Got userId from query string: {UserId}", queryUserId);
            return UserIdHelper.ParseOrCreateGuid(queryUserId);
        }

        return null;
    }

    private static string GetSessionGroup(Guid sessionId) => $"session_{sessionId}";
}
