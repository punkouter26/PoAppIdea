# PoAppIdea DevOps Documentation

> **Version:** 1.0  
> **Last Updated:** 2026-02-12

---

## 🏗️ Infrastructure Overview

### Azure Resources

| Resource | Type | Purpose |
|----------|------|---------|
| App Service (Linux) | Web App | Hosting |
| Storage Account | Storage | Tables + Blobs |
| Azure OpenAI | AI | GPT-4o, DALL-E 3 |
| Key Vault | Security | Secrets management |
| Application Insights | Monitoring | Telemetry |

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore
        run: dotnet restore
        
      - name: Build
        run: dotnet build --configuration Release
        
      - name: Test
        run: dotnet test --configuration Release
        
      - name: Publish
        run: dotnet publish src/PoAppIdea.Web -c Release -o ./publish

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Azure Login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v3
        with:
          app-name: poappidea-web
          package: ./publish
```

---

## 🔧 Environment Configuration

### Configuration Hierarchy

```
1. appsettings.json          # Base defaults
2. appsettings.{env}.json    # Environment overrides
3. Key Vault                 # Production secrets
4. User Secrets              # Local development
5. Environment Variables     # Runtime overrides
```

### Development Secrets

```bash
# Set up user secrets
cd src/PoAppIdea.Web
dotnet user-secrets init

# Azure Storage
dotnet user-secrets set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"

# Azure OpenAI (real AI)
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:ChatDeployment" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:ImageDeployment" "dall-e-3"

# Mock AI (no API costs)
dotnet user-secrets set "MockAI" "true"
```

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `AzureStorage__ConnectionString` | Storage connection string | Dev |
| `AzureStorage__TableServiceUri` | Table endpoint (MSI) | Prod |
| `AzureStorage__BlobServiceUri` | Blob endpoint (MSI) | Prod |
| `AzureOpenAI__Endpoint` | OpenAI endpoint | Yes |
| `AzureOpenAI__ApiKey` | OpenAI API key | Yes |
| `KeyVault__Uri` | Key Vault URI | Prod |
| `APPLICATIONINSIGHTS__CONNECTIONSTRING` | App Insights | Prod |
| `MOCK_AI` | Use mock AI | Optional |

---

## 🔐 Key Vault Secrets

### Production Secrets Mapping

| Secret Name | Description | Required |
|-------------|-------------|----------|
| `PoAppIdea--AzureAI--Endpoint` | Azure OpenAI endpoint | ✅ |
| `PoAppIdea--AzureAI--ApiKey` | Azure OpenAI API key | ✅ |
| `PoAppIdea--AzureAI--DeploymentName` | GPT-4o deployment name | ✅ |
| `PoAppIdea--GoogleOAuth--ClientId` | Google OAuth client ID | ✅ |
| `PoAppIdea--GoogleOAuth--ClientSecret` | Google OAuth secret | ✅ |
| `PoAppIdea--MicrosoftOAuth--ClientId` | Microsoft OAuth client ID | ✅ |
| `PoAppIdea--MicrosoftOAuth--ClientSecret` | Microsoft OAuth secret | ✅ |

### Bicep Deployment

```bash
# Deploy infrastructure
az deployment group create \
  --resource-group PoAppIdea \
  --template-file infra/main.bicep \
  --parameters @infra/main.bicepparam \
  --parameters environment=prod
```

---

## 🐳 Docker Support

### Local Development with Azurite

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

volumes:
  azurite-data:
```

```bash
# Start Azurite
docker compose -f docker-compose.azurite.yml up -d

# Verify connection
# Use: UseDevelopmentStorage=true
```

---

## 📊 Monitoring

### Application Insights

#### KQL Queries

**Failed Requests**
```kql
requests
| where success == false
| where timestamp > ago(1h)
| summarize count() by name, resultCode
| order by count_ desc
```

**Slow Requests**
```kql
requests
| where duration > 1000
| where timestamp > ago(1h)
| summarize avg(duration), max(duration) by name
| order by avg_duration desc
```

**AI Latency**
```kql
customEvents
| where name == "AIGeneration"
| summarize avgduration = avg(todouble(customDimensions.durationMs]))
  by customDimensions.model
```

---

## ✅ Acceptance Testing

### E2E Tests

```bash
# Install Playwright
cd tests/PoAppIdea.E2E
npm install
npx playwright install chromium

# Run tests
npm test

# Run specific test
npx playwright test home.spec.ts
```

### Integration Tests

```bash
cd tests/PoAppIdea.IntegrationTests
dotnet test
```

---

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] All tests pass
- [ ] Secrets configured in Key Vault
- [ ] Bicep parameters validated
- [ ] Health check endpoint verified

### Post-Deployment
- [ ] App Service running
- [ ] Health check returns 200
- [ ] Login flow works
- [ ] AI generation works
- [ ] Monitoring dashboards visible

---

## 📋 Simplified DevOps Guide

### Quick Deploy

```bash
# 1. Build
dotnet build --configuration Release

# 2. Test
dotnet test --configuration Release

# 3. Deploy
az webapp deployment source config-local-git \
  --name poappidea-web \
  --resource-group PoAppIdea

git remote add azure <deployment-url>
git push azure main
```

### Environment Setup

| Env | Storage | AI | Auth |
|-----|---------|-----|------|
| Dev | Azurite | Mock or Real | Local secrets |
| Prod | Azure | Real | Key Vault |

### Key Commands
```bash
# Build
dotnet build -c Release

# Test
dotnet test -c Release

# Deploy
az webapp up --name poappidea-web

# View logs
az webapp log tail --name poappidea-web
```
