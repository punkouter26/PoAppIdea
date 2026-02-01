using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Core.Enums;
using System.Security.Claims;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Endpoint for listing user's sessions.
/// GET /api/sessions
/// </summary>
public static class ListSessionsEndpoint
{
    public static void MapListSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions", HandleAsync)
            .WithName("ListSessions")
            .WithTags("Session")
            .WithSummary("List user's sessions")
            .WithDescription("Retrieves all sessions for the authenticated user, optionally filtered by status")
            .RequireAuthorization()
            .Produces<IReadOnlyList<SessionSummary>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        [FromServices] SessionService sessionService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetUserId(user);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var sessions = await sessionService.GetUserSessionsAsync(userId, status, cancellationToken);

        var summaries = sessions
            .Take(Math.Min(limit, 100))
            .Select(s => new SessionSummary
            {
                SessionId = s.Id,
                AppType = s.AppType,
                ComplexityLevel = s.ComplexityLevel,
                CurrentPhase = s.CurrentPhase,
                Status = s.Status,
                TopIdeaCount = s.TopIdeaIds.Count,
                CreatedAt = s.CreatedAt,
                CompletedAt = s.CompletedAt
            })
            .ToList();

        return Results.Ok(summaries);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

/// <summary>
/// Summary DTO for session list.
/// </summary>
public sealed record SessionSummary
{
    public required Guid SessionId { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required SessionPhase CurrentPhase { get; init; }
    public required SessionStatus Status { get; init; }
    public required int TopIdeaCount { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
