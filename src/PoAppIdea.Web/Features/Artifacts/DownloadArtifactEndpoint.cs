using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Endpoint for downloading an artifact as a file.
/// GET /api/artifacts/{artifactId}/download
/// </summary>
public static class DownloadArtifactEndpoint
{
    public static void MapDownloadArtifactEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/artifacts/{artifactId:guid}/download", HandleAsync)
            .WithName("DownloadArtifact")
            .WithTags("Artifacts")
            .RequireAuthorization()
            .WithSummary("Download artifact as file")
            .WithDescription("Downloads the artifact content as a file (Markdown for PRD/Tech, JSON for Visual Pack).")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid artifactId,
        [FromServices] ArtifactService artifactService,
        CancellationToken cancellationToken)
    {
        var result = await artifactService.GetArtifactForDownloadAsync(artifactId, cancellationToken);

        if (result is null)
        {
            return Results.NotFound(new { error = $"Artifact {artifactId} not found" });
        }

        var (content, fileName, contentType) = result.Value;
        var bytes = Encoding.UTF8.GetBytes(content);

        return Results.File(bytes, contentType, fileName);
    }
}
