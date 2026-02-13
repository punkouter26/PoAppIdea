using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;
using PoAppIdea.Shared.Contracts;
using PoAppIdea.Web.Infrastructure.AI;

using SessionEntity = PoAppIdea.Core.Entities.Session;
using SynthesisEntity = PoAppIdea.Core.Entities.Synthesis;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Service for managing idea submission and synthesis.
/// Implements single vs multi-select handling per FR-013, FR-014.
/// Uses Strategy pattern for single/multi selection workflows.
/// </summary>
public sealed class SynthesisService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IFeatureVariationRepository _featureVariationRepository;
    private readonly IMutationRepository _mutationRepository;
    private readonly ISynthesisRepository _synthesisRepository;
    private readonly SynthesisEngine _synthesisEngine;
    private readonly ILogger<SynthesisService> _logger;

    public SynthesisService(
        ISessionRepository sessionRepository,
        IFeatureVariationRepository featureVariationRepository,
        IMutationRepository mutationRepository,
        ISynthesisRepository synthesisRepository,
        SynthesisEngine synthesisEngine,
        ILogger<SynthesisService> logger)
    {
        _sessionRepository = sessionRepository;
        _featureVariationRepository = featureVariationRepository;
        _mutationRepository = mutationRepository;
        _synthesisRepository = synthesisRepository;
        _synthesisEngine = synthesisEngine;
        _logger = logger;
    }

    /// <summary>
    /// Submits selected ideas for synthesis.
    /// If 1 idea selected: bypass synthesis, proceed to refinement.
    /// If 2-10 ideas selected: synthesize into cohesive concept.
    /// </summary>
    public async Task<SubmitSelectionsResponse> SubmitSelectionsAsync(
        Guid sessionId,
        SubmitSelectionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot submit selections for a completed session");
        }

        // Validate selection count
        if (request.SelectedIdeaIds.Count == 0)
        {
            throw new InvalidOperationException("At least one idea must be selected");
        }

        if (request.SelectedIdeaIds.Count > AppConstants.MaxSelectableIdeas)
        {
            throw new InvalidOperationException(
                $"Cannot select more than {AppConstants.MaxSelectableIdeas} ideas");
        }

        // Update session with selected IDs
        session.SelectedIdeaIds = request.SelectedIdeaIds.ToList();
        session.CurrentPhase = SessionPhase.Phase4_ProductRefinement;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation(
            "Session {SessionId}: {Count} ideas submitted for synthesis",
            sessionId,
            request.SelectedIdeaIds.Count);

        // Single selection - bypass synthesis
        if (request.SelectedIdeaIds.Count == 1)
        {
            return new SubmitSelectionsResponse
            {
                SessionId = sessionId,
                SubmittedCount = 1,
                SynthesisPerformed = false,
                Synthesis = null,
                Message = "Single idea selected. Proceeding directly to PM refinement questions."
            };
        }

        // Multiple selections - perform synthesis
        var synthesis = await PerformSynthesisAsync(session, request.SelectedIdeaIds, cancellationToken);
        var synthesisDto = MapToDto(synthesis);

        return new SubmitSelectionsResponse
        {
            SessionId = sessionId,
            SubmittedCount = request.SelectedIdeaIds.Count,
            SynthesisPerformed = true,
            Synthesis = synthesisDto,
            Message = $"Successfully synthesized {request.SelectedIdeaIds.Count} ideas into a cohesive concept."
        };
    }

    /// <summary>
    /// Performs synthesis on multiple selected ideas.
    /// Edge Case T172: If synthesis fails, present individual paths instead.
    /// </summary>
    private async Task<SynthesisEntity> PerformSynthesisAsync(
        SessionEntity session,
        List<Guid> selectedIdeaIds,
        CancellationToken cancellationToken)
    {
        // Get source ideas from feature variations and mutations
        var sourceIdeas = await GetSourceIdeasAsync(session.Id, selectedIdeaIds, cancellationToken);

        if (sourceIdeas.Count < 2)
        {
            throw new InvalidOperationException(
                "Could not retrieve enough source ideas for synthesis. " +
                "Ensure at least 2 valid feature variations or mutations are selected.");
        }

        SynthesisResult synthesisResult;
        bool synthesisSucceeded = true;

        try
        {
            // Attempt AI-powered synthesis
            synthesisResult = await _synthesisEngine.SynthesizeIdeasAsync(sourceIdeas, cancellationToken);

            // Validate synthesis result has meaningful content
            if (string.IsNullOrWhiteSpace(synthesisResult.MergedTitle) ||
                string.IsNullOrWhiteSpace(synthesisResult.MergedDescription))
            {
                synthesisSucceeded = false;
            }
        }
        catch (Exception ex)
        {
            // Edge Case T172: Synthesis failed, fall back to individual paths
            _logger.LogWarning(
                ex,
                "Session {SessionId}: AI synthesis failed. Falling back to individual paths presentation.",
                session.Id);
            synthesisSucceeded = false;
            synthesisResult = null!;
        }

        if (!synthesisSucceeded)
        {
            // Edge Case T172: Create a "multiple paths" synthesis instead
            synthesisResult = CreateIndividualPathsSynthesis(sourceIdeas);

            _logger.LogInformation(
                "Session {SessionId}: Created individual paths synthesis with {Count} alternatives",
                session.Id,
                sourceIdeas.Count);
        }

        // Create synthesis entity
        var synthesis = new SynthesisEntity
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            SourceIdeaIds = selectedIdeaIds,
            MergedTitle = synthesisResult.MergedTitle,
            MergedDescription = synthesisResult.MergedDescription,
            ThematicBridge = synthesisResult.ThematicBridge,
            RetainedElements = synthesisResult.RetainedElements,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Save synthesis
        await _synthesisRepository.CreateAsync(synthesis, cancellationToken);

        // Update session with synthesis reference
        session.SynthesisId = synthesis.Id;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation(
            "Session {SessionId}: Synthesis created with title '{Title}'",
            session.Id,
            synthesis.MergedTitle);

        return synthesis;
    }

    /// <summary>
    /// Creates a fallback synthesis presenting individual paths when AI synthesis fails.
    /// This gives users actionable alternatives rather than an error.
    /// Pattern: Graceful Degradation - provide value even when primary function fails.
    /// </summary>
    private static SynthesisResult CreateIndividualPathsSynthesis(List<IdeaSource> sourceIdeas)
    {
        var topIdea = sourceIdeas.First();
        var alternativeIdeas = sourceIdeas.Skip(1).ToList();

        // Create a synthesis that presents each idea as a viable path
        var description = new System.Text.StringBuilder();
        description.AppendLine("We identified multiple promising directions from your selections:");
        description.AppendLine();

        description.AppendLine($"**Primary Path: {topIdea.Title}**");
        description.AppendLine(topIdea.Description);
        description.AppendLine();

        for (int i = 0; i < alternativeIdeas.Count; i++)
        {
            var alt = alternativeIdeas[i];
            description.AppendLine($"**Alternative {i + 1}: {alt.Title}**");
            description.AppendLine(alt.Description);
            description.AppendLine();
        }

        description.AppendLine("Consider these as independent paths to explore, or select fewer items for a more focused synthesis.");

        // Build retained elements from each path
        var retainedElements = new Dictionary<Guid, List<string>>();
        foreach (var idea in sourceIdeas)
        {
            retainedElements[idea.Id] = idea.KeyFeatures?.ToList() ?? [];
        }

        return new SynthesisResult
        {
            MergedTitle = $"{topIdea.Title} (with {alternativeIdeas.Count} alternatives)",
            MergedDescription = description.ToString(),
            ThematicBridge = "Multiple viable paths identified - consider exploring each direction",
            RetainedElements = retainedElements
        };
    }

    /// <summary>
    /// Gets the source ideas (feature variations or mutations) for synthesis.
    /// </summary>
    private async Task<List<IdeaSource>> GetSourceIdeasAsync(
        Guid sessionId,
        List<Guid> selectedIdeaIds,
        CancellationToken cancellationToken)
    {
        var sources = new List<IdeaSource>();

        // Try to find each ID in feature variations first, then mutations
        foreach (var ideaId in selectedIdeaIds)
        {
            // Check feature variations
            var variation = await _featureVariationRepository.GetByIdAsync(ideaId, cancellationToken);
            if (variation is not null)
            {
                sources.Add(new IdeaSource
                {
                    Id = variation.Id,
                    Title = variation.VariationTheme,
                    Description = $"A {variation.VariationTheme} approach with {variation.Features.Count} features " +
                                 $"and integrations: {string.Join(", ", variation.ServiceIntegrations)}",
                    KeyFeatures = variation.Features.Select(f => f.Name).ToList()
                });
                continue;
            }

            // Check mutations
            var mutation = await _mutationRepository.GetByIdAsync(ideaId, cancellationToken);
            if (mutation is not null)
            {
                sources.Add(new IdeaSource
                {
                    Id = mutation.Id,
                    Title = mutation.Title,
                    Description = mutation.Description,
                    KeyFeatures = new List<string> { mutation.MutationRationale }
                });
            }
        }

        return sources;
    }

    /// <summary>
    /// Triggers synthesis for a session (re-synthesize endpoint).
    /// </summary>
    public async Task<SynthesisDto> SynthesizeAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.SelectedIdeaIds.Count < 2)
        {
            throw new InvalidOperationException(
                "Synthesis requires at least 2 selected ideas. Current selection: " +
                session.SelectedIdeaIds.Count);
        }

        // Delete existing synthesis if any
        var existingSynthesis = await _synthesisRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (existingSynthesis is not null)
        {
            await _synthesisRepository.DeleteAsync(existingSynthesis.Id, cancellationToken);
        }

        // Perform new synthesis
        var synthesis = await PerformSynthesisAsync(session, session.SelectedIdeaIds, cancellationToken);
        return MapToDto(synthesis);
    }

    /// <summary>
    /// Gets the synthesis for a session.
    /// </summary>
    public async Task<SynthesisDto?> GetSynthesisAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var synthesis = await _synthesisRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return synthesis is null ? null : MapToDto(synthesis);
    }

    /// <summary>
    /// Gets the list of selectable items (top feature variations) for submission.
    /// </summary>
    public async Task<List<SelectableIdeaDto>> GetSelectableIdeasAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        // Get top feature variations
        var topVariations = await _featureVariationRepository.GetTopByScoreAsync(
            sessionId, 
            AppConstants.TopIdeasAfterPhase2, 
            cancellationToken);

        return topVariations.Select(v => new SelectableIdeaDto
        {
            Id = v.Id,
            Title = v.VariationTheme,
            Description = BuildFinalAppSummary(v),
            Score = v.Score,
            IsSelected = session.SelectedIdeaIds.Contains(v.Id)
        }).ToList();
    }

    private static string BuildFinalAppSummary(FeatureVariation v)
    {
        var mustCount = v.Features.Count(f => f.Priority == FeaturePriority.Must);
        var shouldCount = v.Features.Count(f => f.Priority == FeaturePriority.Should);
        var couldCount = v.Features.Count(f => f.Priority == FeaturePriority.Could);
        var integrationList = v.ServiceIntegrations.Take(3).ToList();
        var topFeatureNames = v.Features.Take(3).Select(f => f.Name).ToList();

        var integrationSummary = integrationList.Count > 0
            ? string.Join(", ", integrationList)
            : "no external integrations";

        var featureSummary = topFeatureNames.Count > 0
            ? string.Join(", ", topFeatureNames)
            : "core feature set";

        return $"{v.Features.Count} features (M:{mustCount}, S:{shouldCount}, C:{couldCount}). Key capabilities: {featureSummary}. Integrations: {integrationSummary}.";
    }

    private static SynthesisDto MapToDto(SynthesisEntity synthesis)
    {
        return new SynthesisDto
        {
            Id = synthesis.Id,
            SessionId = synthesis.SessionId,
            SourceIdeaIds = synthesis.SourceIdeaIds,
            MergedTitle = synthesis.MergedTitle,
            MergedDescription = synthesis.MergedDescription,
            ThematicBridge = synthesis.ThematicBridge,
            RetainedElements = synthesis.RetainedElements
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly()),
            CreatedAt = synthesis.CreatedAt
        };
    }
}

/// <summary>
/// DTO for selectable ideas in the submission UI.
/// </summary>
public sealed record SelectableIdeaDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required float Score { get; init; }
    public required bool IsSelected { get; init; }
}
