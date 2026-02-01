# PoAppIdea Infrastructure

This folder contains Bicep templates for Infrastructure as Code (IaC) deployment of PoAppIdea resources.

## Files

| File | Description |
|------|-------------|
| `main.bicep` | Main infrastructure template defining App Service, Storage Account, and Key Vault references |
| `main.bicepparam` | Production parameters file |
| `modules/keyvault-access.bicep` | Module for granting Key Vault access to managed identities |

## Prerequisites

1. Azure CLI installed and logged in
2. Access to the `PoAppIdea` and `PoShared` resource groups
3. The App Service Plan `asp-poshared` must exist in `PoShared`

## Deployment

### Deploy infrastructure

```bash
# Get the Application Insights connection string
$appInsightsConnStr = az monitor app-insights component show `
  --app poappideinsights8f9c9a4e `
  --resource-group PoShared `
  --query connectionString -o tsv

# Deploy the infrastructure
az deployment group create `
  --resource-group PoAppIdea `
  --template-file infra/main.bicep `
  --parameters infra/main.bicepparam `
  --parameters appInsightsConnectionString=$appInsightsConnStr
```

### What-if (preview changes)

```bash
az deployment group what-if `
  --resource-group PoAppIdea `
  --template-file infra/main.bicep `
  --parameters infra/main.bicepparam
```

## Resources Created

| Resource | Type | Purpose |
|----------|------|---------|
| `poappidea-web` | App Service | Blazor Server web application |
| `stpoappidea` | Storage Account | Azure Table Storage (entities) and Blob Storage (visual assets) |

## Shared Resources (PoShared)

The following resources are referenced from the `PoShared` resource group:

| Resource | Type | Purpose |
|----------|------|---------|
| `asp-poshared` | App Service Plan | Shared hosting plan |
| `kv-poshared` | Key Vault | Secrets (OAuth, API keys, connection strings) |
| `poappideinsights*` | Application Insights | Telemetry and monitoring |
| `PoShared-LogAnalytics` | Log Analytics | Centralized logging |

## Key Vault References

The App Service is configured with Key Vault references for secrets:

- `PoAppIdea--AzureStorage--ConnectionString`
- `PoAppIdea--AzureAI--Endpoint`
- `PoAppIdea--AzureAI--ApiKey`
- `PoAppIdea--AzureAI--DeploymentName`
- `PoAppIdea--GoogleOAuth--ClientId`
- `PoAppIdea--GoogleOAuth--ClientSecret`

The web app's managed identity is automatically granted `get` and `list` permissions on secrets.
