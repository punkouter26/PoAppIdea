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
public sealed class ArtifactGenerator : IArtifactGenerator
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
    /// Optimization 7: Capped max_tokens to control output length and cost.
    /// </summary>
    public async Task<ArtifactContent> GeneratePrdAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PRD for session {SessionId}", context.SessionId);

        var prompt = BuildPrdPrompt(context);
        
        var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        history.AddUserMessage(prompt);
        
        var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 2500,
                ["temperature"] = 0.5
            }
        };
        
        var response = await _chatService.GetChatMessageContentAsync(history, executionSettings, cancellationToken: cancellationToken);
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
    /// Optimization 7: Capped max_tokens to control output length and cost.
    /// </summary>
    public async Task<ArtifactContent> GenerateTechnicalDeepDiveAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Technical Deep-Dive for session {SessionId}", context.SessionId);

        var prompt = BuildTechnicalPrompt(context);
        
        var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        history.AddUserMessage(prompt);
        
        var executionSettings = new Microsoft.SemanticKernel.PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 3000,
                ["temperature"] = 0.5
            }
        };
        
        var response = await _chatService.GetChatMessageContentAsync(history, executionSettings, cancellationToken: cancellationToken);
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

    /// <summary>
    /// Optimization 4: Compact PRD prompt — summarizes features/answers as bullets instead of full JSON.
    /// </summary>
    private string BuildPrdPrompt(ArtifactContext context)
    {
        var featureList = context.Features is { Count: > 0 }
            ? string.Join("\n", context.Features.Select(f => $"- [{f.Priority}] {f.Name}: {f.Description}"))
            : "No features defined yet.";

        var pmAnswers = context.PmAnswers is { Count: > 0 }
            ? string.Join("\n", context.PmAnswers.Select(a => $"- {a.QuestionCategory}: {a.AnswerText}"))
            : "";

        return $$"""
            You are a senior product manager creating a Product Requirements Document (PRD).

            ## Product
            **{{context.ProductTitle}}** ({{context.AppType}}, Complexity {{context.ComplexityLevel}}/5)
            {{context.ProductDescription}}
            {{(context.ThematicBridge is not null ? $"Theme: {context.ThematicBridge}" : "")}}

            ## Features
            {{featureList}}

            {{(pmAnswers.Length > 0 ? $"## PM Insights\n{pmAnswers}" : "")}}

            Generate a concise PRD in Markdown with: Executive Summary, Problem Statement, Target Users, Goals & KPIs, Feature Requirements (MoSCoW), 10+ User Stories, Functional Requirements, Non-Functional Requirements, Dependencies, Timeline, Risks, Open Questions.
            Be specific and actionable for a dev team.
            """;
    }

    /// <summary>
    /// Optimization 4: Compact technical prompt — summarizes features/answers as bullets instead of full JSON.
    /// </summary>
    private string BuildTechnicalPrompt(ArtifactContext context)
    {
        var featureList = context.Features is { Count: > 0 }
            ? string.Join("\n", context.Features.Select(f => $"- {f.Name}: {f.Description}"))
            : "No features defined yet.";

        var archAnswers = context.ArchitectAnswers is { Count: > 0 }
            ? string.Join("\n", context.ArchitectAnswers.Select(a => $"- {a.QuestionCategory}: {a.AnswerText}"))
            : "";

        var integrations = context.ServiceIntegrations is { Count: > 0 }
            ? string.Join(", ", context.ServiceIntegrations)
            : "";

        return $$"""
            You are a senior software architect creating a Technical Deep-Dive document.

            ## Product
            **{{context.ProductTitle}}** ({{context.AppType}}, Complexity {{context.ComplexityLevel}}/5)
            {{context.ProductDescription}}

            ## Features
            {{featureList}}

            {{(integrations.Length > 0 ? $"## Integrations: {integrations}" : "")}}
            {{(archAnswers.Length > 0 ? $"## Architecture Insights\n{archAnswers}" : "")}}

            Generate a Technical Deep-Dive in Markdown with: Architecture Overview, Tech Stack, System Components, Data Model, API Design, Auth, Integration Architecture, Scalability, Performance, DevOps, Monitoring, Security, Tech Debt & Future.
            Include code examples. Be actionable for a dev team.
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
