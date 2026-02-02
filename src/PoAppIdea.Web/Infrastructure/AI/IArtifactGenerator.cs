using PoAppIdea.Core.Entities;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Interface for artifact generation services.
/// Pattern: Strategy Pattern - Allows swapping between real AI and mock implementations.
/// </summary>
public interface IArtifactGenerator
{
    /// <summary>
    /// Generates a Product Requirements Document (PRD) artifact.
    /// </summary>
    Task<ArtifactContent> GeneratePrdAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Technical Deep-Dive document artifact.
    /// </summary>
    Task<ArtifactContent> GenerateTechnicalDeepDiveAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Visual Asset Pack manifest artifact.
    /// </summary>
    Task<ArtifactContent> GenerateVisualAssetPackAsync(
        ArtifactContext context,
        IReadOnlyList<VisualAsset> selectedAssets,
        CancellationToken cancellationToken = default);
}
