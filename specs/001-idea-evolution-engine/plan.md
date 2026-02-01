# Implementation Plan: PoAppIdea - The Self-Evolving Ideation Engine

**Branch**: `001-idea-evolution-engine` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-idea-evolution-engine/spec.md`

## Summary

PoAppIdea is a self-evolving ideation platform that transforms vague concepts into professional-grade project roadmaps through a 7-phase directed evolution process. Users configure scope (App Type + Complexity 1-5), then rapidly swipe through 50 AI-generated ideas with real-time learning between batches. The top ideas undergo mutation (Crossover/Repurposing), feature expansion, and optional synthesis before deep refinement through 20 PM/Architect questions. The system generates visual mockups via Azure OpenAI DALL-E and produces final artifacts (PRD, Technical Deep-Dive, Visual Asset Pack). A Product Personality profile learns user preferences across sessions, and a public gallery enables community discovery.

**Technical Approach**: Blazor Server with SignalR for real-time swipe capture, Azure AI Foundry + Semantic Kernel for LLM orchestration, Azure Table Storage for session/personality data partitioned by UserId, Azure Blob Storage for visual assets, and Polly for resilience.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: Blazor Server, SignalR, Semantic Kernel, Radzen Blazor, Polly, Azure.Data.Tables, Azure.Storage.Blobs, Azure.AI.OpenAI  
**Storage**: Azure Table Storage (sessions, personalities) + Azure Blob Storage (visual assets)  
**Testing**: xUnit (Unit), xUnit + Testcontainers (Integration), Playwright (E2E - TypeScript)  
**Target Platform**: Azure App Service (Windows/Linux), browsers (Chromium, Mobile)  
**Project Type**: Web application (unified Blazor Web App - Server SSR + Interactive)  
**Performance Goals**: <200ms swipe response, <5s idea batch generation, <30s visual generation  
**Constraints**: 500 concurrent users, 99.5% uptime, SignalR MaximumReceiveMessageSize increased for large artifact outputs  
**Scale/Scope**: 500 concurrent users, indefinite data retention, ~50 screens across 7 phases

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Unified Identity | ✅ PASS | Namespace: `PoAppIdea.*`, Resource Group: `rg-PoAppIdea-prod` |
| II. Zero-Waste Codebase | ✅ PASS | `.copilotignore` will be created, VSA prevents dead code |
| III. Strict Compiler Safety | ✅ PASS | `Directory.Build.props` with TreatWarningsAsErrors + Nullable |
| IV. Observability & Health | ✅ PASS | `/health` endpoint, OpenTelemetry → Application Insights in PoShared |
| V. Secrets Management | ✅ PASS | user-secrets (local), Key Vault in PoShared (production) |
| VI. Three-Tier Testing | ✅ PASS | Unit (xUnit), Integration (Testcontainers), E2E (Playwright) |
| VII. Design Patterns | ✅ PASS | VSA, GoF (Strategy for mutations, Chain of Responsibility for phases) |
| Technology Stack | ✅ PASS | .NET 10, Blazor Web App, OpenAPI, Azure App Service |
| Azure Deployment | ✅ PASS | Subscription Punkouter26, PoShared for shared services |
| Development Standards | ✅ PASS | Ports 5000/5001, .http files, CPM via Directory.Packages.props |

**Gate Result**: ✅ ALL GATES PASS - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/001-idea-evolution-engine/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI specs)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── PoAppIdea.sln                    # Solution file
├── Directory.Build.props            # TreatWarningsAsErrors, Nullable
├── Directory.Packages.props         # Central Package Management
├── .copilotignore                   # AI context exclusions
│
├── PoAppIdea.Web/                   # Blazor Web App (Server + WASM)
│   ├── Features/                    # Vertical Slice Architecture
│   │   ├── Session/                 # Phase 0: Scope configuration
│   │   │   ├── StartSessionEndpoint.cs
│   │   │   ├── StartSessionRequest.cs
│   │   │   └── SessionService.cs
│   │   ├── Spark/                   # Phase 1: Idea discovery + swipe
│   │   │   ├── GenerateIdeasEndpoint.cs
│   │   │   ├── RecordSwipeEndpoint.cs
│   │   │   ├── SwipeHub.cs          # SignalR for real-time swipes
│   │   │   └── SparkService.cs
│   │   ├── Mutation/                # Phase 2: Directed evolution
│   │   │   ├── MutateIdeasEndpoint.cs
│   │   │   └── MutationService.cs
│   │   ├── FeatureExpansion/        # Phase 3: Service integrations
│   │   │   ├── ExpandFeaturesEndpoint.cs
│   │   │   └── FeatureExpansionService.cs
│   │   ├── Synthesis/               # Phase 4: Cohesive merge
│   │   │   ├── SynthesizeEndpoint.cs
│   │   │   └── SynthesisService.cs
│   │   ├── Refinement/              # Phases 4-5: PM/Architect questions
│   │   │   ├── GetQuestionsEndpoint.cs
│   │   │   ├── SubmitAnswersEndpoint.cs
│   │   │   └── RefinementService.cs
│   │   ├── Visual/                  # Phase 6: DALL-E mockups
│   │   │   ├── GenerateVisualsEndpoint.cs
│   │   │   └── VisualService.cs
│   │   ├── Artifacts/               # Phase 7: Final deliverables
│   │   │   ├── GenerateArtifactsEndpoint.cs
│   │   │   └── ArtifactService.cs
│   │   ├── Personality/             # Cross-session learning
│   │   │   ├── GetPersonalityEndpoint.cs
│   │   │   └── PersonalityService.cs
│   │   ├── Gallery/                 # Public gallery
│   │   │   ├── BrowseGalleryEndpoint.cs
│   │   │   ├── PublishArtifactEndpoint.cs
│   │   │   └── GalleryService.cs
│   │   └── Health/                  # /health endpoint
│   │       └── HealthEndpoint.cs
│   ├── Components/                  # Blazor UI components
│   │   ├── Layout/
│   │   ├── Shared/
│   │   └── Pages/
│   ├── Hubs/                        # SignalR hubs
│   │   └── SwipeHub.cs
│   ├── Infrastructure/              # Cross-cutting concerns
│   │   ├── AI/                      # Semantic Kernel orchestration
│   │   │   ├── IdeaGenerator.cs
│   │   │   ├── MutationEngine.cs
│   │   │   └── SynthesisEngine.cs
│   │   ├── Storage/                 # Azure Table/Blob abstractions
│   │   │   ├── TableStorageClient.cs
│   │   │   └── BlobStorageClient.cs
│   │   ├── Resilience/              # Polly policies
│   │   │   └── ResiliencePolicies.cs
│   │   └── Telemetry/               # OpenTelemetry setup
│   │       └── TelemetryConfiguration.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── PoAppIdea.Core/                  # Domain logic (no dependencies)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Session.cs
│   │   ├── Idea.cs
│   │   ├── Swipe.cs
│   │   ├── Mutation.cs
│   │   ├── FeatureVariation.cs
│   │   ├── Synthesis.cs
│   │   ├── RefinementAnswer.cs
│   │   ├── VisualAsset.cs
│   │   ├── Artifact.cs
│   │   └── ProductPersonality.cs
│   ├── Enums/
│   │   ├── AppType.cs
│   │   ├── SwipeDirection.cs
│   │   ├── MutationType.cs
│   │   ├── ArtifactType.cs
│   │   └── SessionPhase.cs
│   └── Interfaces/
│       ├── ISessionRepository.cs
│       ├── IPersonalityRepository.cs
│       └── IArtifactRepository.cs
│
└── PoAppIdea.Shared/                # Shared DTOs, constants
    ├── Contracts/                   # OpenAPI-aligned DTOs
    └── Constants/

tests/
├── PoAppIdea.Tests.Unit/            # Pure logic tests
│   ├── Core/
│   └── Services/
├── PoAppIdea.Tests.Integration/     # API + Storage tests
│   ├── Features/
│   └── Infrastructure/
└── PoAppIdea.Tests.E2E/             # Playwright (TypeScript)
    ├── playwright.config.ts
    ├── tests/
    │   ├── spark-flow.spec.ts
    │   ├── mutation-flow.spec.ts
    │   └── artifact-generation.spec.ts
    └── package.json
```

**Structure Decision**: Web application using Vertical Slice Architecture. Single deployable Blazor Web App (`PoAppIdea.Web`) with separated `PoAppIdea.Core` for domain logic and `PoAppIdea.Shared` for DTOs. This enables clean dependency inversion while keeping deployment simple for Azure App Service.

## Complexity Tracking

> No constitution violations requiring justification. Design follows all principles.

## Constitution Re-Check (Post-Design)

*GATE: Verify design decisions still comply after Phase 1 completion.*

| Principle | Status | Post-Design Evidence |
|-----------|--------|---------------------|
| I. Unified Identity | ✅ PASS | All namespaces: `PoAppIdea.Web`, `PoAppIdea.Core`, `PoAppIdea.Shared` |
| II. Zero-Waste Codebase | ✅ PASS | VSA eliminates orphan layers; feature folders self-contained |
| III. Strict Compiler Safety | ✅ PASS | `Directory.Build.props` defined in project structure |
| IV. Observability & Health | ✅ PASS | `/health` endpoint in `Features/Health/`, OpenTelemetry in `Infrastructure/Telemetry/` |
| V. Secrets Management | ✅ PASS | user-secrets documented in quickstart.md, Key Vault in deployment |
| VI. Three-Tier Testing | ✅ PASS | Three test projects defined: Unit, Integration, E2E |
| VII. Design Patterns | ✅ PASS | Strategy (MutationEngine), Repository (Storage clients), documented in research.md |
| Technology Stack | ✅ PASS | .NET 10, Blazor, OpenAPI via contracts/openapi.yaml |
| Azure Deployment | ✅ PASS | Subscription and PoShared documented in quickstart.md |
| Development Standards | ✅ PASS | Ports 5000/5001, .http files documented, CPM via Directory.Packages.props |

**Post-Design Gate Result**: ✅ ALL GATES PASS - Ready for Phase 2 (/speckit.tasks)
