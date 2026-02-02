# Architecture Overview

Technical architecture documentation for PoAppIdea.

---

## System Architecture

### High-Level View

```
┌─────────────────────────────────────────────────────────────────┐
│                        USERS                                     │
│                    (Browsers / PWA)                              │
└─────────────────────────┬───────────────────────────────────────┘
                          │ HTTPS
┌─────────────────────────▼───────────────────────────────────────┐
│                   AZURE APP SERVICE                              │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                  PoAppIdea.Web                              │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │ │
│  │  │   Blazor     │  │  Minimal     │  │  SignalR         │  │ │
│  │  │   Server     │  │  APIs        │  │  (Real-time)     │  │ │
│  │  └──────────────┘  └──────────────┘  └──────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │              Feature Modules (VSA)                    │  │ │
│  │  │  Session | Spark | Mutation | Features | Synthesis   │  │ │
│  │  │  Refinement | Visual | Artifacts | Gallery | Auth    │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │              Infrastructure Layer                     │  │ │
│  │  │  Storage | AI | Auth | Telemetry | Health | Rate     │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────┬───────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
┌───────▼───────┐ ┌───────▼───────┐ ┌───────▼───────┐
│ AZURE OPENAI  │ │ AZURE STORAGE │ │ APP INSIGHTS  │
│ ┌───────────┐ │ │ ┌───────────┐ │ │ ┌───────────┐ │
│ │  GPT-4o   │ │ │ │  Tables   │ │ │ │  Traces   │ │
│ │  DALL-E 3 │ │ │ │  Blobs    │ │ │ │  Metrics  │ │
│ └───────────┘ │ │ └───────────┘ │ │ │  Logs     │ │
└───────────────┘ └───────────────┘ │ └───────────┘ │
                                    └───────────────┘
```

---

## Technology Stack

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| **Runtime** | .NET | 10.0 | Application framework |
| **Frontend** | Blazor Server | 10.0 | Interactive UI with SSR |
| **Components** | Radzen.Blazor | 5.4.0 | UI component library |
| **API** | Minimal APIs | 10.0 | RESTful endpoints |
| **AI** | Semantic Kernel | 1.32.0 | Azure OpenAI integration |
| **Storage** | Azure.Data.Tables | 12.10.0 | NoSQL entity storage |
| **Blobs** | Azure.Storage.Blobs | 12.23.0 | Artifact storage |
| **Telemetry** | OpenTelemetry | 1.12.0 | Distributed tracing |
| **Auth** | ASP.NET Identity | 10.0 | OAuth authentication |

---

## Project Structure

```
PoAppIdea/
├── src/
│   ├── PoAppIdea.Web/           # Main web application
│   │   ├── Components/          # Blazor components
│   │   │   ├── Pages/           # 14 page components
│   │   │   ├── Layout/          # MainLayout, NavMenu
│   │   │   └── Shared/          # Reusable components
│   │   ├── Features/            # 11 feature modules (VSA)
│   │   │   ├── Session/         # Session lifecycle
│   │   │   ├── Spark/           # Idea generation
│   │   │   ├── Mutation/        # Idea evolution
│   │   │   ├── FeatureExpansion/# Feature variations
│   │   │   ├── Synthesis/       # Idea merging
│   │   │   ├── Refinement/      # Q&A phases
│   │   │   ├── Visual/          # Mockup generation
│   │   │   ├── Artifacts/       # Document generation
│   │   │   ├── Gallery/         # Public browsing
│   │   │   ├── Personality/     # Product identity
│   │   │   └── Auth/            # Authentication
│   │   └── Infrastructure/      # Cross-cutting concerns
│   │       ├── AI/              # Azure OpenAI clients
│   │       ├── Storage/         # Table/Blob clients
│   │       ├── Auth/            # OAuth handlers
│   │       ├── Telemetry/       # OpenTelemetry setup
│   │       └── Health/          # Health checks
│   ├── PoAppIdea.Core/          # Domain layer
│   │   ├── Entities/            # 11 domain entities
│   │   ├── Enums/               # 9 enumeration types
│   │   └── Interfaces/          # Repository contracts
│   └── PoAppIdea.Shared/        # Shared contracts
│       └── Contracts/           # DTOs
└── tests/
    ├── PoAppIdea.UnitTests/     # Unit tests (xUnit)
    ├── PoAppIdea.IntegrationTests/  # API tests
    └── PoAppIdea.E2E/           # Playwright E2E tests
```

---

## Data Architecture

### Entity Model

```
User (1) ──── (N) Session (1) ──── (N) Idea
                    │                    │
                    │                    └── (N) Swipe
                    │
                    ├── (N) Mutation
                    │         │
                    │         └── (N) FeatureVariation
                    │
                    ├── (1) Synthesis
                    │
                    ├── (N) RefinementAnswer
                    │
                    ├── (N) VisualAsset
                    │
                    ├── (N) Artifact
                    │
                    └── (1) ProductPersonality
```

### Storage Strategy

| Entity | Storage | Partition Key | Row Key |
|--------|---------|---------------|---------|
| Session | Table | UserId | SessionId |
| Idea | Table | SessionId | IdeaId |
| Swipe | Table | SessionId | SwipeId |
| Mutation | Table | SessionId | MutationId |
| FeatureVariation | Table | SessionId | VariationId |
| Synthesis | Table | SessionId | "synthesis" |
| RefinementAnswer | Table | SessionId | Phase_Index |
| VisualAsset | Table | SessionId | AssetId |
| Artifact | Table | SessionId | ArtifactType |
| Artifact Content | Blob | - | SessionId/filename |

---

## API Design

### Endpoint Patterns

```
POST   /api/sessions                    # Create session
GET    /api/sessions                    # List user sessions
GET    /api/sessions/{id}               # Get session
POST   /api/sessions/{id}/ideas         # Generate ideas
POST   /api/sessions/{id}/swipes        # Record swipe
GET    /api/sessions/{id}/ideas/top     # Get top ideas
POST   /api/sessions/{id}/mutations     # Generate mutations
POST   /api/sessions/{id}/features      # Expand features
POST   /api/sessions/{id}/synthesis     # Synthesize ideas
GET    /api/sessions/{id}/refinement    # Get questions
POST   /api/sessions/{id}/refinement    # Submit answers
POST   /api/sessions/{id}/visuals       # Generate visuals
POST   /api/sessions/{id}/artifacts     # Generate artifacts
GET    /api/gallery                     # Browse public ideas
```

### Authentication
- OAuth 2.0 with cookie-based sessions
- Protected routes require `[Authorize]`
- API endpoints require valid session cookie

---

## Security Architecture

### Authentication Flow

```
User → App → OAuth Provider → App → Cookie → Protected Resources
```

### Security Measures

| Layer | Protection |
|-------|------------|
| Transport | HTTPS (TLS 1.3) |
| Authentication | OAuth 2.0 (Google, GitHub, Microsoft) |
| Session | Encrypted cookies, SameSite=Strict |
| Secrets | Azure Key Vault (production) |
| Rate Limiting | 100 req/60s per IP |
| Input Validation | FluentValidation |
| Error Handling | Global exception middleware |

---

## Observability

### Telemetry Stack

```
Application → OpenTelemetry SDK → OTLP Exporter → Application Insights
```

### Collected Signals

| Signal | Examples |
|--------|----------|
| **Traces** | HTTP requests, AI calls, storage operations |
| **Metrics** | Request latency, AI token usage, error rates |
| **Logs** | Structured logs with correlation IDs |

### Health Checks

| Endpoint | Checks |
|----------|--------|
| `/health` | Azure Storage, Azure OpenAI, Key Vault |
| `/health/ready` | Application ready state |
| `/health/live` | Application liveness |

---

## Deployment

### Azure Resources

| Resource | SKU | Purpose |
|----------|-----|---------|
| App Service | B1 | Web hosting |
| Storage Account | Standard_LRS | Data persistence |
| Azure OpenAI | S0 | AI generation |
| Application Insights | - | Monitoring |
| Key Vault | Standard | Secrets |

### CI/CD Pipeline

```
Push → GitHub Actions → Build → Test → Deploy to Azure App Service
```
