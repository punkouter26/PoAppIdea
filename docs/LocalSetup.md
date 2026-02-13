# PoAppIdea Local Setup Guide

> **Version:** 1.0  
> **Last Updated:** 2026-02-12

---

## 🚀 Quick Start (5 Minutes)

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0+ | Runtime |
| Docker Desktop | Latest | Azurite storage |
| VS Code / Rider | Latest | IDE |
| Git | Latest | Version control |

### Setup Steps

```bash
# 1. Clone repository
git clone https://github.com/punkouter26/PoAppIdea.git
cd PoAppIdea

# 2. Start Azurite (Docker)
docker compose -f docker-compose.azurite.yml up -d

# 3. Configure secrets
cd src/PoAppIdea.Web
dotnet user-secrets init
dotnet user-secrets set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "MockAI" "true"

# 4. Run the app
cd ..
dotnet run --project src/PoAppIdea.Web

# 5. Open browser
# https://localhost:5001
```

---

## 📋 Prerequisites Installation

### .NET 10 SDK

```bash
# Windows
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0

# macOS
brew install dotnet-sdk@10

# Linux
sudo apt-get install -y dotnet-sdk-10.0
```

### Docker Desktop

```bash
# Windows/macOS
# Download from: https://www.docker.com/products/docker-desktop

# Verify installation
docker --version
docker-compose --version
```

### Azure CLI (Optional)

```bash
# Windows
winget install Microsoft.AzureCLI

# macOS
brew install azure-cli

# Verify
az --version
```

---

## ⚙️ Configuration Options

### Option 1: Mock AI (Recommended for Dev)

No API costs, instant responses.

```bash
cd src/PoAppIdea.Web
dotnet user-secrets set "MockAI" "true"
```

### Option 2: Real Azure OpenAI

```bash
# Configure Azure OpenAI
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:ChatDeployment" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:ImageDeployment" "dall-e-3"

# Disable mock mode
dotnet user-secrets set "MockAI" "false"
```

---

## 🐳 Docker Compose

### Azurite Services

```yaml
# docker-compose.azurite.yml
version: '3.8'
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - "10000:10000"  # Blob
      - "10001:10001"  # Queue  
      - "10002:10002"  # Table
    volumes:
      - azurite-data:/data
    environment:
      - AZURITE_ACCOUNTS=devstoreaccount1

volumes:
  azurite-data:
```

### Start/Stop Commands

```bash
# Start Azurite
docker compose -f docker-compose.azurite.yml up -d

# Check status
docker ps | grep azurite

# View logs
docker compose -f docker-compose.azurite.yml logs -f

# Stop Azurite
docker compose -f docker-compose.azurite.yml down
```

---

## 🔐 Authentication Setup

### OAuth Credentials (Optional)

To enable real authentication:

```bash
# Google OAuth
dotnet user-secrets set "PoAppIdea:Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:Google:ClientSecret" "your-secret"

# Microsoft OAuth
dotnet user-secrets set "PoAppIdea:Authentication:Microsoft:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:Microsoft:ClientSecret" "your-secret"

# GitHub OAuth
dotnet user-secrets set "PoAppIdea:Authentication:GitHub:ClientId" "your-client-id"
dotnet user-secrets set "PoAppIdea:Authentication:GitHub:ClientSecret" "your-secret"
```

### Development Mode (No Auth)

To skip authentication entirely for testing:

In `appsettings.Development.json`:
```json
{
  "Authentication": {
    "SkipAuth": true
  }
}
```

---

## 🧪 Testing

### Run All Tests

```bash
# Unit tests
dotnet test tests/PoAppIdea.UnitTests

# Integration tests
dotnet test tests/PoAppIdea.IntegrationTests

# E2E tests
cd tests/PoAppIdea.E2E
npm install
npx playwright install chromium
npm test
```

### Run Specific Test Project

```bash
dotnet test tests/PoAppIdea.UnitTests --filter "FullyQualifiedName~SessionTests"
```

---

## 🔧 IDE Setup

### Visual Studio Code

```bash
# Install C# Dev Kit
code --install-extension ms-dotnettools.csdevkit

# Install recommended extensions
# See .vscode/extensions.json
```

### Launch Configuration

```json
// .vscode/launch.json
{
  "configurations": [
    {
      "name": "PoAppIdea.Web",
      "type": "dotnet",
      "request": "launch",
      "projectPath": "src/PoAppIdea.Web/PoAppIdea.Web.csproj",
      "launchBrowser": {
        "enabled": true,
        "openBrowser": "https://localhost:5001"
      }
    }
  ]
}
```

---

## 🐛 Troubleshooting

### Port Already in Use

```bash
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process
taskkill /PID <process-id> /F
```

### Azurite Connection Issues

```bash
# Restart Azurite
docker compose -f docker-compose.azurite.yml restart

# Clear data volume
docker compose -f docker-compose.azurite.yml down -v
docker compose -f docker-compose.azurite.yml up -d
```

### User Secrets Not Loading

```bash
# Re-initialize user secrets
cd src/PoAppIdea.Web
dotnet user-secrets clear
dotnet user-secrets init
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 📁 Project Structure

```
PoAppIdea/
├── src/
│   ├── PoAppIdea.Web/        # Main application
│   │   ├── Components/        # Blazor pages & components
│   │   ├── Features/         # Feature modules
│   │   └── Infrastructure/  # AI, Storage, Auth
│   ├── PoAppIdea.Core/      # Domain entities
│   └── PoAppIdea.Shared/    # DTOs
├── tests/
│   ├── PoAppIdea.UnitTests/
│   ├── PoAppIdea.IntegrationTests/
│   └── PoAppIdea.E2E/
├── docs/                     # Documentation
└── infra/                   # Infrastructure (Bicep)
```

---

## ✅ Verification Checklist

- [ ] .NET 10 SDK installed
- [ ] Docker Desktop running
- [ ] Azurite container started
- [ ] User secrets configured
- [ ] App runs on localhost:5001
- [ ] Can create a session
- [ ] Mock AI generates ideas
- [ ] Tests pass

---

## 📋 Simplified Setup

### 3-Command Setup

```bash
# 1. Clone
git clone https://github.com/punkouter26/PoAppIdea.git

# 2. Start storage
docker compose -f docker-compose.azurite.yml up -d

# 3. Run
cd src/PoAppIdea.Web
dotnet user-secrets set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "MockAI" "true"
cd ..
dotnet run --project src/PoAppIdea.Web
```

### Access
- **Web:** https://localhost:5001
- **API:** https://localhost:5001/api
- **Health:** https://localhost:5001/health
