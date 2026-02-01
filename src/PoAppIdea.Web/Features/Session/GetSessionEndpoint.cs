using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;
using System.Security.Claims;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Endpoint for getting session details.
/// GET /api/sessions/{id}
/// </summary>
public static class GetSessionEndpoint
{
    public static void MapGetSessionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{id:guid}", HandleAsync)
            .WithName("GetSession")
            .WithTags("Session")
            .WithSummary("Get session details")
            .WithDescription("Retrieves detailed information about a specific session")
            .RequireAuthorization()
            .Produces<SessionDto>(StatusCodes.Status200OK)
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

        var session = await sessionService.GetSessionAsync(id, cancellationToken);
        if (session == null)
        {
            return Results.NotFound(new { error = $"Session {id} not found" });
        }

        // Verify ownership
        if (session.UserId != userId)
        {
            return Results.Forbid();
        }

        var dto = new SessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            AppType = session.AppType,
            ComplexityLevel = session.ComplexityLevel,
            CurrentPhase = session.CurrentPhase,
            Status = session.Status,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            TopIdeaIds = session.TopIdeaIds,
            SelectedIdeaIds = session.SelectedIdeaIds,
            SynthesisId = session.SynthesisId
        };

        return Results.Ok(dto);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
