# PoAppIdea

> A self-evolving ideation platform that transforms raw concepts into refined product visions through AI-powered tinder-style swiping, creative synthesis, and professional artifact generation.

## 🎯 Overview

PoAppIdea is an innovative brainstorming tool that helps users evolve their app ideas through:

- **Tinder-style Swiping**: Generate and rate app ideas with intuitive swipe gestures
- **AI Mutations**: Combine, invert, and enhance ideas using GPT-4o
- **Feature Expansion**: Break ideas into detailed feature variations
- **Creative Synthesis**: Merge the best elements into unified concepts
- **PM & Architect Refinement**: Professional Q&A to sharpen the vision
- **Visual Generation**: AI-generated mockups and style exploration
- **Artifact Generation**: Production-ready PRDs, technical deep-dives, and visual packs
- **Persistent Learning**: Your preferences improve suggestions over time
- **Community Gallery**: Discover and build upon ideas shared by others

## 🏗️ Architecture

```
PoAppIdea/
├── src/
│   ├── PoAppIdea.Web/           # Blazor Server Web App
│   │   ├── Components/          # Blazor pages and shared components
│   │   ├── Features/            # Vertical Slice feature modules
│   │   └── Infrastructure/      # AI, Auth, Storage, Telemetry
│   ├── PoAppIdea.Core/          # Domain entities and interfaces
│   └── PoAppIdea.Shared/        # DTOs and contracts
├── tests/
│   ├── PoAppIdea.UnitTests/     # xUnit unit tests
│   ├── PoAppIdea.IntegrationTests/  # Integration tests with Testcontainers
│   └── PoAppIdea.E2E/           # Playwright E2E tests
└── specs/                       # Feature specifications
```

### Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Blazor Server + Radzen Components |
| Backend | .NET 10, Minimal APIs |
| AI | Azure OpenAI (GPT-4o) via Semantic Kernel |
| Storage | Azure Table Storage + Blob Storage |
| Auth | OAuth (Google, Microsoft) |
| Telemetry | OpenTelemetry → Application Insights |
| Real-time | SignalR for live updates |

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) or Azurite
- Azure OpenAI resource (for AI features)
- Node.js 18+ (for E2E tests)

### Configuration

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd PoAppIdea
   ```

2. **Set up user secrets** (recommended for local development):
   ```bash
   cd src/PoAppIdea.Web
   dotnet user-secrets init
   dotnet user-secrets set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://<your-resource>.openai.azure.com/"
   dotnet user-secrets set "AzureOpenAI:ApiKey" "<your-api-key>"
   dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
   ```

3. **Start Azurite** (Azure Storage emulator):
   ```bash
   azurite --silent --location ./azurite --debug ./azurite/debug.log
   ```

4. **Run the application**:
   ```bash
   cd src/PoAppIdea.Web
   dotnet run
   ```

5. **Open in browser**:
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001

### Health Check

Verify all dependencies are configured:
```bash
curl https://localhost:5001/health
```

## 📡 API Endpoints

The API follows RESTful conventions. See [API.http](src/PoAppIdea.Web/API.http) for complete examples.

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/sessions` | POST | Start new session |
| `/api/sessions/{id}/ideas/generate` | POST | Generate ideas |
| `/api/sessions/{id}/swipes` | POST | Record swipe |
| `/api/sessions/{id}/mutations/generate` | POST | Trigger mutations |
| `/api/sessions/{id}/synthesis` | POST | Run synthesis |
| `/api/sessions/{id}/artifacts` | POST | Generate artifacts |
| `/api/gallery` | GET | Browse public gallery |
| `/health` | GET | Health check |

API documentation available at:
- OpenAPI spec: `/openapi/v1.json`
- Scalar UI: `/scalar/v1`

## 🧪 Testing

### Unit Tests
```bash
cd tests/PoAppIdea.UnitTests
dotnet test
```

### Integration Tests
```bash
cd tests/PoAppIdea.IntegrationTests
dotnet test
```

### E2E Tests (Playwright)
```bash
cd tests/PoAppIdea.E2E
npm install
npx playwright install chromium
npm test
```

## 🔧 Development

### Project Structure

Each feature follows **Vertical Slice Architecture**:
```
Features/
├── Session/
│   ├── StartSessionRequest.cs
│   ├── StartSessionResponse.cs
│   ├── SessionService.cs
│   └── StartSessionEndpoint.cs
├── Spark/
├── Mutation/
└── ...
```

### Key Patterns

- **SOLID Principles**: Single responsibility, dependency injection
- **Repository Pattern**: All storage behind `I*Repository` interfaces
- **Options Pattern**: Configuration via strongly-typed settings
- **Problem Details**: Consistent error responses

### Code Quality

- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- `<Nullable>enable</Nullable>`
- Central Package Management via `Directory.Packages.props`

## 🚢 Deployment

### Azure App Service

The application is designed for Azure App Service with:
- Managed Identity for Key Vault access
- Application Insights for monitoring
- Azure Blob Storage for artifacts
- Azure Table Storage for entities

### Environment Variables

| Variable | Description |
|----------|-------------|
| `AzureStorage__ConnectionString` | Azure Storage connection |
| `AzureOpenAI__Endpoint` | Azure OpenAI endpoint |
| `AzureOpenAI__DeploymentName` | GPT model deployment |
| `KeyVault__VaultUri` | Key Vault for secrets |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights |

## 📚 Documentation

- [Specification](specs/001-idea-evolution-engine/spec.md)
- [Technical Plan](specs/001-idea-evolution-engine/plan.md)
- [Data Model](specs/001-idea-evolution-engine/data-model.md)
- [OpenAPI Contract](specs/001-idea-evolution-engine/contracts/openapi.yaml)
- [Contributing Guide](CONTRIBUTING.md)

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.
