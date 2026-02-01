# Quickstart: PoAppIdea - The Self-Evolving Ideation Engine

**Branch**: `001-idea-evolution-engine` | **Date**: 2026-01-29

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) (for Playwright E2E tests)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (local storage emulator)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Testcontainers)
- Visual Studio 2025 or VS Code with C# DevKit

## Quick Start (5 minutes)

### 1. Clone and Setup

```powershell
# Clone repository
git clone https://github.com/your-org/PoAppIdea.git
cd PoAppIdea
git checkout 001-idea-evolution-engine

# Restore dependencies
dotnet restore
```

### 2. Configure Secrets

```powershell
# Initialize user secrets for the web project
cd src/PoAppIdea.Web
dotnet user-secrets init

# Set required secrets
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:DalleDeploymentName" "dall-e-3"

# OAuth providers (app-prefixed for multi-app key vault)
dotnet user-secrets set "PoAppIdea:Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:Google:ClientSecret" "your-client-secret"
dotnet user-secrets set "PoAppIdea:Authentication:GitHub:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:GitHub:ClientSecret" "your-client-secret"
dotnet user-secrets set "PoAppIdea:Authentication:Microsoft:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:Microsoft:ClientSecret" "your-client-secret"

cd ../..
```

### 3. Start Azurite (Local Storage Emulator)

```powershell
# Option A: Using npm
npm install -g azurite
azurite --silent --location ./azurite-data --debug ./azurite-debug.log

# Option B: Using Docker
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

### 4. Run the Application

```powershell
cd src/PoAppIdea.Web
dotnet run
```

The app will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Health**: https://localhost:5001/health
- **OpenAPI**: https://localhost:5001/openapi/v1.json

### 5. Verify Installation

```powershell
# Check health endpoint
Invoke-RestMethod https://localhost:5001/health -SkipCertificateCheck | ConvertTo-Json
```

Expected output:
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "TableStorage", "status": "Healthy" },
    { "name": "BlobStorage", "status": "Healthy" },
    { "name": "AzureOpenAI", "status": "Healthy" }
  ]
}
```

---

## Project Structure

```
src/
├── PoAppIdea.sln                    # Solution file
├── PoAppIdea.Web/                   # Blazor Web App
├── PoAppIdea.Core/                  # Domain entities and interfaces
└── PoAppIdea.Shared/                # Shared DTOs

tests/
├── PoAppIdea.Tests.Unit/            # Unit tests
├── PoAppIdea.Tests.Integration/     # Integration tests
└── PoAppIdea.Tests.E2E/             # Playwright E2E tests
```

---

## Running Tests

### Unit Tests (Fast, No Dependencies)

```powershell
dotnet test tests/PoAppIdea.Tests.Unit --logger "console;verbosity=detailed"
```

### Integration Tests (Requires Docker)

```powershell
# Ensure Docker Desktop is running
dotnet test tests/PoAppIdea.Tests.Integration --logger "console;verbosity=detailed"
```

Testcontainers will automatically start Azurite containers.

### E2E Tests (Requires Running App)

```powershell
# Terminal 1: Start the app
cd src/PoAppIdea.Web
dotnet run

# Terminal 2: Run Playwright tests
cd tests/PoAppIdea.Tests.E2E
npm install
npx playwright install chromium
npx playwright test --headed  # Use --headed for visual debugging
```

### Run All Tests

```powershell
dotnet test PoAppIdea.sln
```

---

## Development Workflow

### Feature Development (VSA)

Each feature lives in its own folder under `Features/`:

```
Features/
├── Spark/
│   ├── GenerateIdeasEndpoint.cs    # Minimal API endpoint
│   ├── GenerateIdeasRequest.cs     # Request DTO
│   ├── GenerateIdeasResponse.cs    # Response DTO
│   ├── SparkService.cs             # Business logic
│   └── SparkServiceTests.cs        # Co-located unit tests (optional)
```

### Adding a New Endpoint

1. Create feature folder: `Features/NewFeature/`
2. Add endpoint class with `[EndpointSummary]` and `[EndpointDescription]`
3. Register in `Program.cs`: `app.MapNewFeatureEndpoints();`
4. Add `.http` file for testing: `Features/NewFeature/newfeature.http`

### .http File Example

```http
### Generate Ideas Batch
POST https://localhost:5001/api/sessions/{{sessionId}}/ideas
Authorization: Bearer {{token}}
Content-Type: application/json

{}

### Record Swipe
POST https://localhost:5001/api/sessions/{{sessionId}}/swipes
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "ideaId": "{{ideaId}}",
  "direction": "Like",
  "durationMs": 500
}
```

---

## Configuration

### appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "TableNames": {
      "Users": "Users",
      "Sessions": "Sessions",
      "Ideas": "Ideas",
      "Swipes": "Swipes",
      "Personalities": "Personalities",
      "Artifacts": "Artifacts"
    },
    "ContainerNames": {
      "Visuals": "poappidea-visuals"
    }
  },
  "AzureOpenAI": {
    "Endpoint": "",
    "DeploymentName": "gpt-4o",
    "DalleDeploymentName": "dall-e-3"
  },
  "SignalR": {
    "MaximumReceiveMessageSize": 1048576
  },
  "ApplicationInsights": {
    "ConnectionString": ""
  }
}
```

### Environment-Specific Configuration

| Setting | Local | Production |
|---------|-------|------------|
| Storage | Azurite (UseDevelopmentStorage=true) | Azure Table/Blob |
| Secrets | dotnet user-secrets | Azure Key Vault |
| OpenAI | Same endpoint | Same endpoint |
| AppInsights | Optional | Required |

---

## Azure Deployment

### Resource Group Setup

```powershell
# Set subscription
az account set --subscription "bbb8dfbe-9169-432f-9b7a-fbf861b51037"

# Create resource group
az group create --name rg-PoAppIdea-prod --location eastus

# Create App Service plan
az appservice plan create \
  --name asp-PoAppIdea-prod \
  --resource-group rg-PoAppIdea-prod \
  --sku P1v3 \
  --is-linux

# Create Web App
az webapp create \
  --name poappidea \
  --resource-group rg-PoAppIdea-prod \
  --plan asp-PoAppIdea-prod \
  --runtime "DOTNETCORE:10.0"
```

### Shared Services (PoShared)

The following services are already configured in the `PoShared` resource group:
- **Application Insights**: For telemetry
- **Key Vault**: For production secrets

```powershell
# Link to Key Vault
az webapp config appsettings set \
  --name poappidea \
  --resource-group rg-PoAppIdea-prod \
  --settings "@Microsoft.KeyVault(VaultName=kv-poshared;SecretName=AzureOpenAI-ApiKey)"
```

### Enable Managed Identity

```powershell
# Enable system-assigned identity
az webapp identity assign \
  --name poappidea \
  --resource-group rg-PoAppIdea-prod

# Grant Key Vault access
az keyvault set-policy \
  --name kv-poshared \
  --object-id <identity-principal-id> \
  --secret-permissions get list
```

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| `Connection refused` on Azurite | Ensure Azurite is running: `azurite --silent` |
| OAuth redirect error | Check callback URLs match in provider console |
| SignalR connection drops | Check MaximumReceiveMessageSize in config |
| DALL-E rate limit | Polly will retry with exponential backoff |
| Tests fail on CI | Ensure Docker is available for Testcontainers |

### Logging

```powershell
# View structured logs
dotnet run | ConvertFrom-Json | Format-Table Timestamp, Level, Message
```

Browser console logs are enabled for all SignalR events and LLM calls.

### Health Check Details

```powershell
# Detailed health check
curl https://localhost:5001/health | jq
```

If unhealthy, check:
1. Azurite is running (TableStorage, BlobStorage)
2. Azure OpenAI API key is valid
3. Network connectivity to Azure services

---

## Next Steps

1. Review [data-model.md](data-model.md) for entity relationships
2. Review [research.md](research.md) for architectural decisions
3. Review [contracts/openapi.yaml](contracts/openapi.yaml) for API specification
4. Run `/speckit.tasks` to generate implementation tasks

---

## Useful Commands

```powershell
# Build all projects
dotnet build

# Run with watch (auto-reload)
dotnet watch run --project src/PoAppIdea.Web

# Generate OpenAPI spec
dotnet run --project src/PoAppIdea.Web -- --generate-openapi

# Format code
dotnet format

# Check for warnings (treated as errors)
dotnet build /warnaserror
```
