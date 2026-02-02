# PoAppIdea

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://blazor.net/)
[![Azure](https://img.shields.io/badge/Azure-Hosted-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> **The Self-Evolving Ideation Engine** — Transform vague app concepts into professional product specifications through AI-powered swiping, synthesis, and artifact generation.

```mermaid
flowchart LR
    A[💡 Spark] --> B[🧬 Evolve] --> C[🎯 Features] --> D[📤 Submit] --> E[💬 Refine] --> F[📄 Artifacts]
```

---

## ✨ What is PoAppIdea?

PoAppIdea is an innovative brainstorming platform that helps creators evolve their app ideas through:

| Phase | What Happens | Output |
|-------|-------------|--------|
| ⚡ **Spark** | Tinder-style swiping on 20 AI-generated ideas | Top 3 favorites |
| 🧬 **Evolve** | AI mutates & combines your top ideas | 9 evolved concepts |
| 🎯 **Features** | Detailed feature variations with MoSCoW priorities | 50 feature sets |
| 📤 **Submit** | Select 1-10 ideas for synthesis | Unified concept |
| 💬 **Refine** | Answer PM & Architect questions | Refined specification |
| 📄 **Artifacts** | Generate PRD, Tech Doc, Visual Pack | Complete spec package |

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    PoAppIdea Platform                         │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐   │
│  │   Blazor    │  │  Minimal    │  │    Infrastructure   │   │
│  │   Server    │  │   APIs      │  │  (AI, Storage, Auth)│   │
│  └─────────────┘  └─────────────┘  └─────────────────────┘   │
├──────────────────────────────────────────────────────────────┤
│  PoAppIdea.Core (Entities) │ PoAppIdea.Shared (DTOs)         │
└──────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
   ┌──────────┐        ┌──────────┐        ┌──────────┐
   │  Azure   │        │  Azure   │        │   App    │
   │  OpenAI  │        │ Storage  │        │ Insights │
   └──────────┘        └──────────┘        └──────────┘
```

### Tech Stack

| Component | Technology |
|-----------|------------|
| **Frontend** | Blazor Server + Radzen 5.4 |
| **Backend** | .NET 10, Minimal APIs |
| **AI** | Azure OpenAI (GPT-4o, DALL-E 3) |
| **Storage** | Azure Table + Blob Storage |
| **Auth** | OAuth (Google, GitHub, Microsoft) |
| **Telemetry** | OpenTelemetry → Application Insights |

---

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (Azure Storage emulator)
- Azure OpenAI resource (or use mock mode)

### Setup

```bash
# Clone
git clone https://github.com/your-org/PoAppIdea.git
cd PoAppIdea

# Configure secrets
cd src/PoAppIdea.Web
dotnet user-secrets init
dotnet user-secrets set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR-API-KEY"

# Start Azurite
azurite --silent --location ./azurite

# Run
dotnet run
```

Open https://localhost:5001

### Mock Mode (No AI Costs)

Set `UseMockAI: true` in `appsettings.Development.json` to use mock AI services.

---

## 📁 Project Structure

```
PoAppIdea/
├── src/
│   ├── PoAppIdea.Web/           # Main Blazor application
│   │   ├── Components/          # Pages (14) and shared components
│   │   ├── Features/            # 11 VSA feature modules
│   │   └── Infrastructure/      # AI, Storage, Auth, Telemetry
│   ├── PoAppIdea.Core/          # Domain entities (11) and interfaces
│   └── PoAppIdea.Shared/        # DTOs and contracts
├── tests/
│   ├── PoAppIdea.UnitTests/     # xUnit unit tests
│   ├── PoAppIdea.IntegrationTests/  # API integration tests
│   └── PoAppIdea.E2E/           # Playwright E2E tests
├── docs/                        # Documentation
│   ├── mermaid/                 # Architecture diagrams
│   ├── product/                 # PRD, Features, Architecture
│   ├── kql/                     # Observability queries
│   └── mapping/                 # Config mappings
└── infra/                       # Bicep IaC
```

---

## 📡 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/sessions` | POST | Create session |
| `/api/sessions/{id}/ideas` | POST | Generate ideas |
| `/api/sessions/{id}/swipes` | POST | Record swipe |
| `/api/sessions/{id}/mutations` | POST | Generate mutations |
| `/api/sessions/{id}/features` | POST | Expand features |
| `/api/sessions/{id}/synthesis` | POST | Synthesize ideas |
| `/api/sessions/{id}/refinement` | POST | Submit answers |
| `/api/sessions/{id}/artifacts` | POST | Generate artifacts |
| `/health` | GET | Health check |

**API Documentation**: `/scalar/v1` (development)

---

## 🧪 Testing

```bash
# Unit tests
cd tests/PoAppIdea.UnitTests
dotnet test

# Integration tests
cd tests/PoAppIdea.IntegrationTests
dotnet test

# E2E tests (Playwright)
cd tests/PoAppIdea.E2E
npm install && npx playwright install chromium
npm test
```

---

## 🚢 Deployment

### Azure Resources

| Resource | Purpose |
|----------|---------|
| App Service | Web hosting |
| Storage Account | Tables + Blobs |
| Azure OpenAI | GPT-4o, DALL-E 3 |
| Key Vault | Production secrets |
| Application Insights | Monitoring |

### Deploy with Bicep

```bash
az deployment group create \
  --resource-group PoAppIdea \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [PRD](docs/product/PRD.md) | Product requirements |
| [Architecture](docs/product/Architecture.md) | Technical architecture |
| [Features](docs/product/Features.md) | Feature reference |
| [Walkthrough](docs/product/Walkthrough.md) | User guide |
| [API Examples](docs/api/extended-examples.http) | HTTP file examples |
| [KQL Queries](docs/kql/observability-queries.md) | Monitoring queries |
| [Key Vault Mapping](docs/mapping/keyvault-mapping.md) | Secret configuration |

### Diagrams

| Diagram | Location |
|---------|----------|
| C4 Context | [docs/mermaid/c4-context.md](docs/mermaid/c4-context.md) |
| C4 Container | [docs/mermaid/c4-container.md](docs/mermaid/c4-container.md) |
| C4 Component | [docs/mermaid/c4-component.md](docs/mermaid/c4-component.md) |
| User Journey | [docs/mermaid/user-journeys/full-journey.md](docs/mermaid/user-journeys/full-journey.md) |
| Data Workflows | [docs/mermaid/data-workflow/](docs/mermaid/data-workflow/) |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards

- Follow [SOLID principles](https://en.wikipedia.org/wiki/SOLID)
- Use Vertical Slice Architecture
- Ensure `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Add tests for new features

See [CONTRIBUTING.md](src/CONTRIBUTING.md) for details.

---

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.

---

<p align="center">
  Made with ❤️ by the PoAppIdea Team
</p>
