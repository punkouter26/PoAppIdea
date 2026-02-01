# Research: PoAppIdea - The Self-Evolving Ideation Engine

**Branch**: `001-idea-evolution-engine` | **Date**: 2026-01-29

## Purpose

This document consolidates research findings to resolve technical unknowns identified during planning. Each decision is documented with rationale and alternatives considered.

---

## 1. Blazor Server + SignalR for Real-Time Swipe Capture

**Question**: How to achieve <200ms swipe response with real-time learning between batches?

**Decision**: Blazor Server with SignalR hub dedicated to swipe interactions

**Rationale**:
- Blazor Server maintains persistent SignalR connection by default
- Server-side state eliminates round-trip for swipe recording
- Real-time batch adaptation requires server-side inference anyway
- Radzen Blazor provides production-ready swipe-compatible components

**Alternatives Considered**:
- **Blazor WASM + REST API**: Rejected due to latency overhead for each swipe (100-300ms per request vs <50ms SignalR)
- **Pure JavaScript + REST**: Rejected due to loss of C# type safety and Semantic Kernel integration complexity

**Implementation Notes**:
- Create dedicated `SwipeHub` for swipe events
- Use `IHubContext<SwipeHub>` for server-initiated batch updates
- Configure `MaximumReceiveMessageSize` to 1MB for large artifact payloads

---

## 2. Semantic Kernel for LLM Orchestration

**Question**: How to orchestrate multi-step LLM workflows (idea generation → mutation → synthesis)?

**Decision**: Microsoft Semantic Kernel SDK with Azure AI Foundry

**Rationale**:
- Native .NET integration, first-class C# support
- Built-in prompt templating with variable injection
- Supports Chain-of-Thought reasoning for synthesis phase
- Azure AI Foundry provides managed model access (GPT-4o)

**Alternatives Considered**:
- **LangChain.NET**: Rejected due to less mature .NET ecosystem compared to Semantic Kernel
- **Direct Azure OpenAI SDK**: Rejected due to manual orchestration overhead for multi-step chains

**Implementation Notes**:
- Temperature 0.5 for initial idea generation (balance creativity/feasibility)
- Temperature 0.7 for mutations (higher creativity)
- Temperature 0.3 for synthesis (coherent merging)
- Extract "DNA keywords" from high-speed likes for prompt injection

---

## 3. Azure Table Storage Partitioning Strategy

**Question**: How to achieve O(1) lookup for user sessions and personalities?

**Decision**: Dual-table design with UserId as PartitionKey

**Rationale**:
- Table Storage provides predictable performance at scale
- PartitionKey = UserId ensures all user data co-located
- RowKey varies by table: SessionId, IdeaId, or TraitId
- No hot partition risk since UserId distributes load

**Schema Design**:

| Table | PartitionKey | RowKey | Data |
|-------|--------------|--------|------|
| Sessions | UserId | SessionId | Session state JSON |
| Swipes | UserId | `{SessionId}_{IdeaId}` | Direction, timestamp, duration |
| Personalities | UserId | "profile" | Product Personality JSON |
| Ideas | SessionId | IdeaId | Idea content, scores |
| Artifacts | UserId | `{SessionId}_{ArtifactType}` | Artifact content/reference |

**Alternatives Considered**:
- **Cosmos DB**: Rejected due to cost for simple key-value patterns; Table Storage sufficient
- **SQL Server**: Rejected due to schema flexibility needs and NoSQL fit for JSON documents

---

## 4. Azure Blob Storage for Visual Assets

**Question**: How to store and serve DALL-E generated images for gallery?

**Decision**: Azure Blob Storage with public read access and human-readable slugs

**Rationale**:
- Cost-effective for binary content
- CDN integration for global distribution
- Public read simplifies gallery sharing
- Human-readable naming: `{session-slug}/{artifact-type}-{timestamp}.png`

**Container Structure**:
```
poappidea-visuals/
├── public/                    # Gallery-published assets
│   └── {user-slug}/
│       └── {session-slug}/
│           └── mockup-001.png
└── private/                   # User's unpublished assets
    └── {user-id}/
        └── {session-id}/
            └── mockup-001.png
```

**Alternatives Considered**:
- **Azure Files**: Rejected due to higher cost and SMB overhead
- **Inline Base64 in Table Storage**: Rejected due to entity size limits (1MB)

---

## 5. Polly Resilience Strategy

**Question**: How to handle Azure OpenAI rate limits and transient failures?

**Decision**: Polly with exponential backoff, circuit breaker, and bulkhead

**Rationale**:
- Exponential backoff (2s, 4s, 8s) prevents thundering herd
- Circuit breaker prevents cascade failures during outages
- Bulkhead isolates visual generation from idea generation

**Policy Configuration**:
```csharp
// Idea Generation Policy
.AddRetryPolicy(options => {
    options.MaxRetryAttempts = 3;
    options.Delay = TimeSpan.FromSeconds(2);
    options.BackoffType = DelayBackoffType.Exponential;
})
.AddCircuitBreakerPolicy(options => {
    options.FailureRatio = 0.5;
    options.SamplingDuration = TimeSpan.FromSeconds(30);
    options.MinimumThroughput = 10;
    options.BreakDuration = TimeSpan.FromSeconds(30);
})
```

**Alternatives Considered**:
- **Manual retry loops**: Rejected due to code duplication and lack of circuit breaker
- **Azure API Management retry**: Rejected due to additional infrastructure complexity

---

## 6. Authentication with OAuth Providers

**Question**: How to implement OAuth (Google, GitHub, Microsoft) + email fallback?

**Decision**: ASP.NET Core Identity with external OAuth providers

**Rationale**:
- Built-in support for Google, GitHub, Microsoft OAuth
- Cookie-based sessions for Blazor Server
- Optional email/password via Identity stores
- Extensible for future providers

**Implementation Notes**:
- Use `AddGoogle()`, `AddMicrosoftAccount()`, `AddGitHub()` extensions
- Store OAuth tokens in user-secrets (local) / Key Vault (production)
- Map external claims to local User entity

**Alternatives Considered**:
- **Azure AD B2C**: Rejected due to complexity for simple OAuth; can migrate later
- **Auth0/Okta**: Rejected to stay within Azure ecosystem

---

## 7. OpenAPI Documentation

**Question**: How to generate OpenAPI docs without Swashbuckle?

**Decision**: Microsoft.AspNetCore.OpenApi (built-in .NET 9+)

**Rationale**:
- Native support in .NET 9/10
- No external dependency (Swashbuckle deprecated path)
- Integrates with minimal APIs and endpoint metadata
- Scalar UI for API exploration

**Implementation Notes**:
- Use `[EndpointSummary]` and `[EndpointDescription]` attributes
- Generate OpenAPI spec at build time for contracts/
- Serve at `/openapi/v1.json`

---

## 8. Three-Tier Testing Strategy

**Question**: How to structure tests per constitution requirements?

**Decision**: Separate test projects with clear boundaries

| Tier | Project | Framework | Scope |
|------|---------|-----------|-------|
| Unit | PoAppIdea.Tests.Unit | xUnit | Pure logic, no I/O |
| Integration | PoAppIdea.Tests.Integration | xUnit + Testcontainers | API, Table/Blob Storage |
| E2E | PoAppIdea.Tests.E2E | Playwright (TypeScript) | Critical user paths |

**Testcontainers Usage**:
- Azurite container for Table/Blob Storage emulation
- No SQL database (Table Storage only)

**Playwright Scope** (Chromium + Mobile only):
1. Complete Phase 1 swipe flow
2. Phase 2 mutation rating
3. Final artifact download

---

## 9. Vertical Slice Architecture Organization

**Question**: How to organize feature folders for 7 phases?

**Decision**: One feature folder per phase + cross-cutting concerns

**Rationale**:
- Each phase is independently deployable slice
- Shared infrastructure in `/Infrastructure`
- Endpoints, DTOs, and Services co-located per feature

**Pattern Application**:
- **Strategy Pattern**: Mutation strategies (Crossover, Repurposing)
- **Chain of Responsibility**: Phase progression validation
- **Repository Pattern**: Storage abstraction (Table/Blob clients)
- **Factory Pattern**: Artifact generation (PRD, TechDoc, VisualPack)

---

## 10. SignalR Configuration for Large Payloads

**Question**: How to handle large artifact outputs via SignalR?

**Decision**: Increase MaximumReceiveMessageSize + chunked streaming

**Rationale**:
- PRD and Tech Deep-Dive can exceed default 32KB limit
- Chunked streaming for progressive artifact display
- Server-to-client streaming for long-running generation

**Configuration**:
```csharp
builder.Services.AddSignalR(options => {
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 20;
});
```

---

## Summary: All Unknowns Resolved

| Unknown | Resolution |
|---------|------------|
| Real-time swipe capture | Blazor Server + SignalR hub |
| LLM orchestration | Semantic Kernel + Azure AI Foundry |
| Session storage partitioning | Table Storage: UserId PartitionKey |
| Visual asset storage | Blob Storage with public/private containers |
| Resilience for AI APIs | Polly exponential backoff + circuit breaker |
| OAuth authentication | ASP.NET Core Identity + external providers |
| OpenAPI documentation | Microsoft.AspNetCore.OpenApi (built-in) |
| Testing infrastructure | xUnit + Testcontainers + Playwright |
| Feature organization | VSA with Strategy/Chain patterns |
| Large payload handling | SignalR MaximumReceiveMessageSize 1MB |
