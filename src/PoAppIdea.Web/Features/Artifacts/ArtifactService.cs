using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Web.Infrastructure.AI;
using PoAppIdea.Web.Infrastructure.Storage;

using SessionEntity = PoAppIdea.Core.Entities.Session;
using SynthesisEntity = PoAppIdea.Core.Entities.Synthesis;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Service for managing artifact generation and retrieval.
/// Coordinates the generation of PRD, Technical Deep-Dive, and Visual Asset Pack.
/// </summary>
public sealed class ArtifactService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISynthesisRepository _synthesisRepository;
    private readonly IFeatureVariationRepository _featureVariationRepository;
    private readonly IRefinementAnswerRepository _refinementAnswerRepository;
    private readonly IVisualAssetRepository _visualAssetRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<ArtifactService> _logger;

    public ArtifactService(
        ISessionRepository sessionRepository,
        ISynthesisRepository synthesisRepository,
        IFeatureVariationRepository featureVariationRepository,
        IRefinementAnswerRepository refinementAnswerRepository,
        IVisualAssetRepository visualAssetRepository,
        IArtifactRepository artifactRepository,
        IArtifactGenerator artifactGenerator,
        ILogger<ArtifactService> logger)
    {
        _sessionRepository = sessionRepository;
        _synthesisRepository = synthesisRepository;
        _featureVariationRepository = featureVariationRepository;
        _refinementAnswerRepository = refinementAnswerRepository;
        _visualAssetRepository = visualAssetRepository;
        _artifactRepository = artifactRepository;
        _artifactGenerator = artifactGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current artifact generation status for a session.
    /// </summary>
    public async Task<GenerateArtifactsResponse> GetGenerationStatusAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        var artifacts = await _artifactRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var generatedCount = artifacts.Count;
        var expectedTypes = new[] { ArtifactType.PRD, ArtifactType.TechnicalDeepDive, ArtifactType.VisualAssetPack };

        var status = generatedCount >= 3
            ? ArtifactGenerationStatus.Completed
            : generatedCount > 0
                ? ArtifactGenerationStatus.InProgress
                : ArtifactGenerationStatus.Queued;

        return new GenerateArtifactsResponse
        {
            SessionId = sessionId,
            Status = status,
            ArtifactTypes = expectedTypes,
            GeneratedCount = generatedCount,
            EstimatedCompletionSeconds = status == ArtifactGenerationStatus.Completed
                ? null
                : (3 - generatedCount) * 30 // ~30 seconds per artifact
        };
    }

    /// <summary>
    /// Generates all artifacts for a session.
    /// </summary>
    public async Task<GenerateArtifactsResponse> GenerateArtifactsAsync(
        Guid sessionId,
        GenerateArtifactsRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        // Validate session is in the correct phase
        if (session.CurrentPhase != SessionPhase.Phase6_Visual && session.CurrentPhase != SessionPhase.Completed)
        {
            throw new InvalidOperationException($"Session must be in Phase6_Visual or Completed phase. Current phase: {session.CurrentPhase}");
        }

        // Check if artifacts already exist
        var existingArtifacts = await _artifactRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (existingArtifacts.Count >= 3)
        {
            _logger.LogInformation("Artifacts already exist for session {SessionId}", sessionId);
            return await GetGenerationStatusAsync(sessionId, cancellationToken);
        }

        _logger.LogInformation("Starting artifact generation for session {SessionId}", sessionId);

        // Build context from session data
        var context = await BuildArtifactContextAsync(session, request.TitleOverride, cancellationToken);

        // Generate artifacts
        var generatedArtifacts = new List<Artifact>();

        // 1. Generate PRD
        if (!existingArtifacts.Any(a => a.Type == ArtifactType.PRD))
        {
            var prdContent = await _artifactGenerator.GeneratePrdAsync(context, cancellationToken);
            var prdArtifact = await SaveArtifactAsync(session, prdContent, cancellationToken);
            generatedArtifacts.Add(prdArtifact);
        }

        // 2. Generate Technical Deep-Dive
        if (request.IncludeTechnicalDetails && !existingArtifacts.Any(a => a.Type == ArtifactType.TechnicalDeepDive))
        {
            var techContent = await _artifactGenerator.GenerateTechnicalDeepDiveAsync(context, cancellationToken);
            var techArtifact = await SaveArtifactAsync(session, techContent, cancellationToken);
            generatedArtifacts.Add(techArtifact);
        }

        // 3. Generate Visual Asset Pack
        if (request.IncludeVisualPack && !existingArtifacts.Any(a => a.Type == ArtifactType.VisualAssetPack))
        {
            var visualAssets = await _visualAssetRepository.GetBySessionIdAsync(sessionId, cancellationToken);
            var visualContent = await _artifactGenerator.GenerateVisualAssetPackAsync(context, visualAssets, cancellationToken);
            var visualArtifact = await SaveArtifactAsync(session, visualContent, cancellationToken);
            generatedArtifacts.Add(visualArtifact);
        }

        // Update session to Completed phase
        session.CurrentPhase = SessionPhase.Completed;
        session.CompletedAt = DateTimeOffset.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation(
            "Generated {Count} artifacts for session {SessionId}",
            generatedArtifacts.Count,
            sessionId);

        return await GetGenerationStatusAsync(sessionId, cancellationToken);
    }

    /// <summary>
    /// Gets all artifacts for a session.
    /// </summary>
    public async Task<IReadOnlyList<ArtifactResponseDto>> GetArtifactsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var artifacts = await _artifactRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return artifacts.Select(ToDto).ToList();
    }

    /// <summary>
    /// Gets a single artifact by ID.
    /// </summary>
    public async Task<ArtifactResponseDto?> GetArtifactByIdAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        return artifact is null ? null : ToDto(artifact);
    }

    /// <summary>
    /// Gets a single artifact by slug.
    /// </summary>
    public async Task<ArtifactResponseDto?> GetArtifactBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetBySlugAsync(slug, cancellationToken);
        return artifact is null ? null : ToDto(artifact);
    }

    /// <summary>
    /// Gets artifact content for download.
    /// </summary>
    public async Task<(string Content, string FileName, string ContentType)?> GetArtifactForDownloadAsync(
        Guid artifactId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository.GetByIdAsync(artifactId, cancellationToken);
        if (artifact is null)
        {
            return null;
        }

        var (extension, contentType) = artifact.Type switch
        {
            ArtifactType.PRD => ("md", "text/markdown"),
            ArtifactType.TechnicalDeepDive => ("md", "text/markdown"),
            ArtifactType.VisualAssetPack => ("json", "application/json"),
            _ => ("txt", "text/plain")
        };

        var fileName = $"{artifact.HumanReadableSlug}.{extension}";
        return (artifact.Content, fileName, contentType);
    }

    private async Task<ArtifactContext> BuildArtifactContextAsync(
        SessionEntity session,
        string? titleOverride,
        CancellationToken cancellationToken)
    {
        // Get synthesis if exists
        SynthesisEntity? synthesis = null;
        if (session.SynthesisId.HasValue)
        {
            synthesis = await _synthesisRepository.GetByIdAsync(session.SynthesisId.Value, cancellationToken);
        }

        // Get feature variations to extract features and integrations
        var featureVariations = await _featureVariationRepository.GetBySessionIdAsync(session.Id, cancellationToken);
        var topFeatures = featureVariations
            .OrderByDescending(fv => fv.Score)
            .FirstOrDefault();

        // Get refinement answers
        var allAnswers = await _refinementAnswerRepository.GetBySessionIdAsync(session.Id, cancellationToken);
        var pmAnswers = allAnswers.Where(a => a.Phase == RefinementPhase.Phase4_PM).ToList();
        var architectAnswers = allAnswers.Where(a => a.Phase == RefinementPhase.Phase5_Architect).ToList();

        // Get visual assets for style info
        var visualAssets = await _visualAssetRepository.GetBySessionIdAsync(session.Id, cancellationToken);
        var selectedVisual = visualAssets.FirstOrDefault(v => v.IsSelected);

        // Determine product title and description
        var productTitle = titleOverride
            ?? synthesis?.MergedTitle
            ?? "Untitled Product";

        var productDescription = synthesis?.MergedDescription
            ?? "Product description pending synthesis.";

        return new ArtifactContext
        {
            SessionId = session.Id,
            UserId = session.UserId,
            ProductTitle = productTitle,
            ProductDescription = productDescription,
            AppType = session.AppType,
            ComplexityLevel = session.ComplexityLevel,
            ThematicBridge = synthesis?.ThematicBridge,
            Features = topFeatures?.Features,
            ServiceIntegrations = topFeatures?.ServiceIntegrations,
            PmAnswers = pmAnswers,
            ArchitectAnswers = architectAnswers,
            VisualStyle = selectedVisual?.StyleAttributes
        };
    }

    private async Task<Artifact> SaveArtifactAsync(
        SessionEntity session,
        ArtifactContent content,
        CancellationToken cancellationToken)
    {
        var artifact = new Artifact
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            UserId = session.UserId,
            Type = content.Type,
            Title = content.Title,
            Content = content.Content,
            HumanReadableSlug = content.Slug,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await _artifactRepository.CreateAsync(artifact, cancellationToken);
    }

    private static ArtifactResponseDto ToDto(Artifact artifact) => new()
    {
        Id = artifact.Id,
        Type = artifact.Type,
        Title = artifact.Title,
        Content = artifact.Content,
        BlobUrl = artifact.BlobUrl,
        HumanReadableSlug = artifact.HumanReadableSlug,
        IsPublished = artifact.IsPublished,
        CreatedAt = artifact.CreatedAt,
        PublishedAt = artifact.PublishedAt
    };
}
