using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Generates final artifacts (PRD, Technical Deep-Dive, Visual Asset Pack) from session data.
/// Uses Chain of Responsibility pattern for artifact generation steps.
/// </summary>
public sealed class ArtifactGenerator
{
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<ArtifactGenerator> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ArtifactGenerator(
        IChatCompletionService chatService,
        ILogger<ArtifactGenerator> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a Product Requirements Document (PRD) artifact.
    /// </summary>
    public async Task<ArtifactContent> GeneratePrdAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PRD for session {SessionId}", context.SessionId);

        var prompt = BuildPrdPrompt(context);
        var response = await _chatService.GetChatMessageContentAsync(prompt, cancellationToken: cancellationToken);
        var content = response.Content ?? throw new InvalidOperationException("AI returned empty response for PRD");

        var title = $"PRD: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        _logger.LogDebug("PRD generated with {Length} characters", content.Length);

        return new ArtifactContent
        {
            Type = ArtifactType.PRD,
            Title = title,
            Content = FormatPrdContent(content, context),
            Slug = slug
        };
    }

    /// <summary>
    /// Generates a Technical Deep-Dive document artifact.
    /// </summary>
    public async Task<ArtifactContent> GenerateTechnicalDeepDiveAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Technical Deep-Dive for session {SessionId}", context.SessionId);

        var prompt = BuildTechnicalPrompt(context);
        var response = await _chatService.GetChatMessageContentAsync(prompt, cancellationToken: cancellationToken);
        var content = response.Content ?? throw new InvalidOperationException("AI returned empty response for Technical Deep-Dive");

        var title = $"Technical Deep-Dive: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        _logger.LogDebug("Technical Deep-Dive generated with {Length} characters", content.Length);

        return new ArtifactContent
        {
            Type = ArtifactType.TechnicalDeepDive,
            Title = title,
            Content = FormatTechnicalContent(content, context),
            Slug = slug
        };
    }

    /// <summary>
    /// Generates a Visual Asset Pack manifest artifact.
    /// </summary>
    public async Task<ArtifactContent> GenerateVisualAssetPackAsync(
        ArtifactContext context,
        IReadOnlyList<VisualAsset> selectedAssets,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Visual Asset Pack for session {SessionId}", context.SessionId);

        var manifest = BuildVisualManifest(context, selectedAssets);

        var title = $"Visual Assets: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        return new ArtifactContent
        {
            Type = ArtifactType.VisualAssetPack,
            Title = title,
            Content = manifest,
            Slug = slug
        };
    }

    private string BuildPrdPrompt(ArtifactContext context)
    {
        var contextJson = JsonSerializer.Serialize(new
        {
            context.ProductTitle,
            context.ProductDescription,
            context.AppType,
            context.ComplexityLevel,
            context.ThematicBridge,
            Features = context.Features?.Select(f => new { f.Name, f.Description, f.Priority }),
            ServiceIntegrations = context.ServiceIntegrations,
            PMAnswers = context.PmAnswers?.Select(a => new { a.QuestionCategory, a.QuestionText, a.AnswerText }),
            VisualDirection = context.VisualStyle
        }, _jsonOptions);

        return $$"""
            You are a senior product manager creating a comprehensive Product Requirements Document (PRD).

            ## Product Context
            {{contextJson}}

            ## Your Task
            Generate a complete PRD in Markdown format with the following sections:

            1. **Executive Summary** - 2-3 paragraphs summarizing the product vision
            2. **Problem Statement** - What problem does this solve?
            3. **Target Users** - User personas and their needs
            4. **Product Goals & Success Metrics** - SMART goals and KPIs
            5. **Feature Requirements** - Detailed feature list with priorities (Must/Should/Could/Won't)
            6. **User Stories** - At least 10 user stories in "As a... I want... So that..." format
            7. **Functional Requirements** - Detailed functional specs
            8. **Non-Functional Requirements** - Performance, security, scalability requirements
            9. **Dependencies & Integrations** - External services and APIs
            10. **Timeline & Milestones** - High-level project phases
            11. **Risks & Mitigations** - Potential risks and mitigation strategies
            12. **Open Questions** - Items needing further clarification

            Use proper Markdown formatting with headers, bullet points, and tables where appropriate.
            Be specific and actionable - this should be usable by a development team.
            """;
    }

    private string BuildTechnicalPrompt(ArtifactContext context)
    {
        var contextJson = JsonSerializer.Serialize(new
        {
            context.ProductTitle,
            context.ProductDescription,
            context.AppType,
            context.ComplexityLevel,
            Features = context.Features?.Select(f => new { f.Name, f.Description }),
            ServiceIntegrations = context.ServiceIntegrations,
            ArchitectAnswers = context.ArchitectAnswers?.Select(a => new { a.QuestionCategory, a.QuestionText, a.AnswerText })
        }, _jsonOptions);

        return $$"""
            You are a senior software architect creating a comprehensive Technical Deep-Dive document.

            ## Product Context
            {{contextJson}}

            ## Your Task
            Generate a complete Technical Deep-Dive document in Markdown format with the following sections:

            1. **Architecture Overview** - High-level system architecture with component diagram description
            2. **Technology Stack** - Recommended technologies with justifications
            3. **System Components** - Detailed breakdown of each major component
            4. **Data Model** - Entity relationship descriptions and key data structures
            5. **API Design** - Key API endpoints with request/response patterns
            6. **Authentication & Authorization** - Security approach and implementation
            7. **Integration Architecture** - How external services connect
            8. **Scalability Considerations** - Horizontal/vertical scaling strategies
            9. **Performance Optimization** - Caching, CDN, query optimization strategies
            10. **DevOps & Deployment** - CI/CD pipeline, infrastructure as code
            11. **Monitoring & Observability** - Logging, metrics, alerting approach
            12. **Security Considerations** - OWASP concerns and mitigations
            13. **Technical Debt & Future Considerations** - Known limitations and evolution path

            Use proper Markdown formatting with code blocks for examples.
            Include specific technology recommendations based on the app type and complexity.
            Be actionable - this should guide a development team's implementation.
            """;
    }

    private string BuildVisualManifest(ArtifactContext context, IReadOnlyList<VisualAsset> assets)
    {
        var manifest = new
        {
            ProductTitle = context.ProductTitle,
            GeneratedAt = DateTimeOffset.UtcNow,
            TotalAssets = assets.Count,
            SelectedAsset = assets.FirstOrDefault(a => a.IsSelected),
            Assets = assets.Select(a => new
            {
                a.Id,
                a.BlobUrl,
                a.ThumbnailUrl,
                a.Prompt,
                a.StyleAttributes,
                a.IsSelected
            })
        };

        return JsonSerializer.Serialize(manifest, _jsonOptions);
    }

    private static string FormatPrdContent(string aiContent, ArtifactContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Product Requirements Document: {context.ProductTitle}");
        sb.AppendLine();
        sb.AppendLine($"**Generated**: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm UTC}");
        sb.AppendLine($"**App Type**: {context.AppType}");
        sb.AppendLine($"**Complexity Level**: {context.ComplexityLevel}/5");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(aiContent);
        return sb.ToString();
    }

    private static string FormatTechnicalContent(string aiContent, ArtifactContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Technical Deep-Dive: {context.ProductTitle}");
        sb.AppendLine();
        sb.AppendLine($"**Generated**: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm UTC}");
        sb.AppendLine($"**Target Platform**: {context.AppType}");
        sb.AppendLine($"**Complexity Level**: {context.ComplexityLevel}/5");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(aiContent);
        return sb.ToString();
    }

    private static string GenerateSlug(string title, Guid sessionId)
    {
        var baseSlug = title
            .ToLowerInvariant()
            .Replace(":", "")
            .Replace(" ", "-")
            .Replace("--", "-");

        // Limit length and add uniqueness
        var maxLength = 50;
        if (baseSlug.Length > maxLength)
        {
            baseSlug = baseSlug[..maxLength];
        }

        // Add session ID suffix for uniqueness
        var suffix = sessionId.ToString()[..8];
        return $"{baseSlug}-{suffix}";
    }
}

/// <summary>
/// Context data for artifact generation.
/// </summary>
public sealed record ArtifactContext
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required string ProductTitle { get; init; }
    public required string ProductDescription { get; init; }
    public required AppType AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public string? ThematicBridge { get; init; }
    public IReadOnlyList<Feature>? Features { get; init; }
    public IReadOnlyList<string>? ServiceIntegrations { get; init; }
    public IReadOnlyList<RefinementAnswer>? PmAnswers { get; init; }
    public IReadOnlyList<RefinementAnswer>? ArchitectAnswers { get; init; }
    public StyleInfo? VisualStyle { get; init; }
}

/// <summary>
/// Generated artifact content.
/// </summary>
public sealed record ArtifactContent
{
    public required ArtifactType Type { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required string Slug { get; init; }
}
