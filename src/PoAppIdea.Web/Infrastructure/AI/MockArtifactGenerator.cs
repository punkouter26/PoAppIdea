using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using System.Text;
using System.Text.Json;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Mock implementation of IArtifactGenerator for testing without AI costs.
/// Pattern: Null Object Pattern - Provides deterministic mock data for testing.
/// 
/// Enable by setting environment variable: MOCK_AI=true
/// Or in appsettings: "MockAI": true
/// </summary>
public sealed class MockArtifactGenerator : IArtifactGenerator
{
    private readonly ILogger<MockArtifactGenerator> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MockArtifactGenerator(ILogger<MockArtifactGenerator> logger)
    {
        _logger = logger;
    }

    public Task<ArtifactContent> GeneratePrdAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating PRD for session {SessionId}", context.SessionId);

        var content = GenerateMockPrd(context);
        var title = $"PRD: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        return Task.FromResult(new ArtifactContent
        {
            Type = ArtifactType.PRD,
            Title = title,
            Content = content,
            Slug = slug
        });
    }

    public Task<ArtifactContent> GenerateTechnicalDeepDiveAsync(
        ArtifactContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating Technical Deep-Dive for session {SessionId}", context.SessionId);

        var content = GenerateMockTechnicalDoc(context);
        var title = $"Technical Deep-Dive: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        return Task.FromResult(new ArtifactContent
        {
            Type = ArtifactType.TechnicalDeepDive,
            Title = title,
            Content = content,
            Slug = slug
        });
    }

    public Task<ArtifactContent> GenerateVisualAssetPackAsync(
        ArtifactContext context,
        IReadOnlyList<VisualAsset> selectedAssets,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating Visual Asset Pack for session {SessionId}", context.SessionId);

        var manifest = new
        {
            ProductTitle = context.ProductTitle,
            GeneratedAt = DateTimeOffset.UtcNow,
            TotalAssets = selectedAssets.Count,
            SelectedAsset = selectedAssets.FirstOrDefault(a => a.IsSelected),
            Assets = selectedAssets.Select(a => new
            {
                a.Id,
                a.BlobUrl,
                a.ThumbnailUrl,
                a.Prompt,
                a.StyleAttributes,
                a.IsSelected
            })
        };

        var content = JsonSerializer.Serialize(manifest, JsonOptions);
        var title = $"Visual Assets: {context.ProductTitle}";
        var slug = GenerateSlug(title, context.SessionId);

        return Task.FromResult(new ArtifactContent
        {
            Type = ArtifactType.VisualAssetPack,
            Title = title,
            Content = content,
            Slug = slug
        });
    }

    private static string GenerateMockPrd(ArtifactContext context)
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
        sb.AppendLine("## Executive Summary");
        sb.AppendLine();
        sb.AppendLine($"{context.ProductTitle} is an innovative {context.AppType} application designed to revolutionize how users interact with their daily tasks. This PRD outlines the comprehensive requirements for building a world-class product.");
        sb.AppendLine();
        sb.AppendLine(context.ProductDescription);
        sb.AppendLine();
        sb.AppendLine("## Problem Statement");
        sb.AppendLine();
        sb.AppendLine("Users currently face significant challenges in managing their workflows efficiently. Existing solutions are fragmented, complex, and fail to provide the seamless experience that modern users demand. This product addresses these pain points head-on.");
        sb.AppendLine();
        sb.AppendLine("## Target Users");
        sb.AppendLine();
        sb.AppendLine("| Persona | Description | Key Needs |");
        sb.AppendLine("|---------|-------------|-----------|");
        sb.AppendLine("| Power User | Tech-savvy individuals who demand efficiency | Speed, automation, integrations |");
        sb.AppendLine("| Casual User | Everyday users seeking simplicity | Easy onboarding, intuitive UI |");
        sb.AppendLine("| Team Lead | Managers coordinating team activities | Collaboration, reporting, oversight |");
        sb.AppendLine();
        sb.AppendLine("## Product Goals & Success Metrics");
        sb.AppendLine();
        sb.AppendLine("- **Goal 1**: Achieve 10,000 active users within 6 months");
        sb.AppendLine("- **Goal 2**: Maintain 40% monthly retention rate");
        sb.AppendLine("- **Goal 3**: Achieve NPS score of 50+");
        sb.AppendLine("- **Goal 4**: < 2 second average page load time");
        sb.AppendLine();
        sb.AppendLine("## Feature Requirements");
        sb.AppendLine();
        sb.AppendLine("### Must Have (P0)");
        sb.AppendLine("- User authentication and authorization");
        sb.AppendLine("- Core functionality as per product description");
        sb.AppendLine("- Mobile-responsive design");
        sb.AppendLine();
        sb.AppendLine("### Should Have (P1)");
        sb.AppendLine("- Social sharing capabilities");
        sb.AppendLine("- Export/import functionality");
        sb.AppendLine("- Basic analytics dashboard");
        sb.AppendLine();
        sb.AppendLine("### Could Have (P2)");
        sb.AppendLine("- AI-powered recommendations");
        sb.AppendLine("- Third-party integrations");
        sb.AppendLine("- Custom themes");
        sb.AppendLine();
        sb.AppendLine("## User Stories");
        sb.AppendLine();
        sb.AppendLine("1. As a user, I want to sign up with my email so that I can access the platform");
        sb.AppendLine("2. As a user, I want to create new items quickly so that I can stay productive");
        sb.AppendLine("3. As a user, I want to search my content so that I can find what I need fast");
        sb.AppendLine("4. As a user, I want to receive notifications so that I stay informed");
        sb.AppendLine("5. As a team lead, I want to see team activity so that I can track progress");
        sb.AppendLine();
        sb.AppendLine("## Technical Requirements");
        sb.AppendLine();
        sb.AppendLine("- Cloud-native architecture");
        sb.AppendLine("- 99.9% uptime SLA");
        sb.AppendLine("- GDPR/CCPA compliance");
        sb.AppendLine("- SOC 2 Type II certification path");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*[MOCK DATA] This PRD was generated for testing purposes.*");

        return sb.ToString();
    }

    private static string GenerateMockTechnicalDoc(ArtifactContext context)
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
        sb.AppendLine("## Architecture Overview");
        sb.AppendLine();
        sb.AppendLine("The system follows a modern cloud-native architecture with the following key components:");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                      Client Layer                            │");
        sb.AppendLine("│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐           │");
        sb.AppendLine("│  │  Web App    │  │ Mobile App  │  │   API       │           │");
        sb.AppendLine("│  │  (Blazor)   │  │  (MAUI)     │  │ Consumers   │           │");
        sb.AppendLine("│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘           │");
        sb.AppendLine("└─────────┴─────────────────┴───────────────┴──────────────────┘");
        sb.AppendLine("                            │");
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                    API Gateway / BFF                        │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine("                            │");
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                    Service Layer                             │");
        sb.AppendLine("│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │");
        sb.AppendLine("│  │  Core    │  │  User    │  │  AI      │  │ Storage  │     │");
        sb.AppendLine("│  │ Service  │  │ Service  │  │ Service  │  │ Service  │     │");
        sb.AppendLine("│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Technology Stack");
        sb.AppendLine();
        sb.AppendLine("| Layer | Technology | Justification |");
        sb.AppendLine("|-------|------------|---------------|");
        sb.AppendLine("| Frontend | Blazor WebAssembly | .NET ecosystem, C# end-to-end |");
        sb.AppendLine("| Backend | ASP.NET Core 10 | Performance, reliability, Azure integration |");
        sb.AppendLine("| Database | Azure Cosmos DB | Global distribution, flexible schema |");
        sb.AppendLine("| Caching | Redis | Sub-millisecond latency, pub/sub |");
        sb.AppendLine("| AI | Azure OpenAI | GPT-4 for intelligent features |");
        sb.AppendLine("| Storage | Azure Blob Storage | Cost-effective, durable |");
        sb.AppendLine("| Auth | Azure AD B2C | Enterprise-grade identity |");
        sb.AppendLine();
        sb.AppendLine("## API Design");
        sb.AppendLine();
        sb.AppendLine("### Core Endpoints");
        sb.AppendLine();
        sb.AppendLine("```http");
        sb.AppendLine("GET    /api/v1/items          # List all items");
        sb.AppendLine("POST   /api/v1/items          # Create item");
        sb.AppendLine("GET    /api/v1/items/{id}     # Get item by ID");
        sb.AppendLine("PUT    /api/v1/items/{id}     # Update item");
        sb.AppendLine("DELETE /api/v1/items/{id}     # Delete item");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Security Considerations");
        sb.AppendLine();
        sb.AppendLine("- OAuth 2.0 / OpenID Connect for authentication");
        sb.AppendLine("- Role-based access control (RBAC)");
        sb.AppendLine("- All data encrypted at rest and in transit");
        sb.AppendLine("- Regular security audits and penetration testing");
        sb.AppendLine("- OWASP Top 10 mitigation strategies");
        sb.AppendLine();
        sb.AppendLine("## Deployment Strategy");
        sb.AppendLine();
        sb.AppendLine("- Azure Container Apps for auto-scaling");
        sb.AppendLine("- GitHub Actions for CI/CD");
        sb.AppendLine("- Infrastructure as Code with Bicep");
        sb.AppendLine("- Blue-green deployments for zero downtime");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*[MOCK DATA] This technical document was generated for testing purposes.*");

        return sb.ToString();
    }

    private static string GenerateSlug(string title, Guid sessionId)
    {
        var baseSlug = title
            .ToLowerInvariant()
            .Replace(":", "")
            .Replace(" ", "-")
            .Replace("--", "-");

        var maxLength = 50;
        if (baseSlug.Length > maxLength)
        {
            baseSlug = baseSlug[..maxLength];
        }

        var suffix = sessionId.ToString()[..8];
        return $"{baseSlug}-{suffix}";
    }
}
