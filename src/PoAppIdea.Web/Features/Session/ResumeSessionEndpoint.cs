using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Core.Enums;
using System.Security.Claims;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Endpoint for resuming an in-progress session.
/// GET /api/sessions/{id}/resume
/// </summary>
public static class ResumeSessionEndpoint
{
    public static void MapResumeSessionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{id:guid}/resume", HandleAsync)
            .WithName("ResumeSession")
            .WithTags("Session")
            .WithSummary("Resume an in-progress session")
            .WithDescription("Returns the state needed to resume an in-progress session (FR-024)")
            .RequireAuthorization()
            .Produces<ResumeSessionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] SessionService sessionService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var session = await sessionService.GetSessionForResumptionAsync(id, userId, cancellationToken);
        if (session == null)
        {
            return Results.NotFound(new { error = $"Session {id} not found" });
        }

        // Verify ownership
        if (session.UserId != userId)
        {
            return Results.Forbid();
        }

        // Can only resume in-progress sessions
        if (session.Status != SessionStatus.InProgress)
        {
            return Results.BadRequest(new { error = "Can only resume in-progress sessions" });
        }

        var response = new ResumeSessionResponse
        {
            SessionId = session.Id,
            AppType = session.AppType,
            ComplexityLevel = session.ComplexityLevel,
            CurrentPhase = session.CurrentPhase,
            TopIdeaIds = session.TopIdeaIds,
            SelectedIdeaIds = session.SelectedIdeaIds,
            CreatedAt = session.CreatedAt
        };

        return Results.Ok(response);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

/// <summary>
/// Response DTO for session resumption.
/// </summary>
public sealed record ResumeSessionResponse
{
    public required Guid SessionId { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required SessionPhase CurrentPhase { get; init; }
    public required IReadOnlyList<Guid> TopIdeaIds { get; init; }
    public required IReadOnlyList<Guid> SelectedIdeaIds { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
