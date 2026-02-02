# Configuration Mapping: appsettings.json → Azure Key Vault

Mapping of all configuration keys to their Key Vault secret names.

## Key Vault Details

| Property | Value |
|----------|-------|
| **Key Vault Name** | `kv-poshared` |
| **Resource Group** | `PoShared` |
| **Subscription** | `Punkouter26` (Bbb8dfbe-9169-432f-9b7a-fbf861b51037) |
| **Access Method** | Managed Identity |

---

## Secret Mapping Table

| appsettings.json Path | Key Vault Secret Name | Type | Required |
|----------------------|----------------------|------|----------|
| `AzureStorage:ConnectionString` | `AzureStorage--ConnectionString` | Connection String | ✅ Yes |
| `AzureOpenAI:Endpoint` | `AzureOpenAI--Endpoint` | URL | ✅ Yes |
| `AzureOpenAI:ApiKey` | `AzureOpenAI--ApiKey` | API Key | ✅ Yes |
| `AzureOpenAI:ChatDeployment` | `AzureOpenAI--ChatDeployment` | String | ✅ Yes |
| `AzureOpenAI:ImageDeployment` | `AzureOpenAI--ImageDeployment` | String | ✅ Yes |
| `ApplicationInsights:ConnectionString` | `ApplicationInsights--ConnectionString` | Connection String | ✅ Yes |
| `Authentication:Google:ClientId` | `Authentication--Google--ClientId` | OAuth ID | ⚠️ Optional |
| `Authentication:Google:ClientSecret` | `Authentication--Google--ClientSecret` | OAuth Secret | ⚠️ Optional |
| `Authentication:GitHub:ClientId` | `Authentication--GitHub--ClientId` | OAuth ID | ⚠️ Optional |
| `Authentication:GitHub:ClientSecret` | `Authentication--GitHub--ClientSecret` | OAuth Secret | ⚠️ Optional |
| `Authentication:Microsoft:ClientId` | `Authentication--Microsoft--ClientId` | OAuth ID | ⚠️ Optional |
| `Authentication:Microsoft:ClientSecret` | `Authentication--Microsoft--ClientSecret` | OAuth Secret | ⚠️ Optional |

---

## Configuration Hierarchy

```
Production Priority:
1. Azure Key Vault (Managed Identity) ← Highest priority
2. Environment Variables
3. appsettings.Production.json
4. appsettings.json ← Lowest priority

Development Priority:
1. User Secrets (dotnet user-secrets) ← Highest priority
2. Environment Variables
3. appsettings.Development.json
4. appsettings.json ← Lowest priority
```

---

## Key Vault Setup Commands

### Create Secrets (Azure CLI)

```bash
# Set variables
KV_NAME="kv-poshared"
RG="PoShared"

# Azure Storage
az keyvault secret set --vault-name $KV_NAME \
  --name "AzureStorage--ConnectionString" \
  --value "<your-connection-string>"

# Azure OpenAI
az keyvault secret set --vault-name $KV_NAME \
  --name "AzureOpenAI--Endpoint" \
  --value "https://<your-resource>.openai.azure.com/"

az keyvault secret set --vault-name $KV_NAME \
  --name "AzureOpenAI--ApiKey" \
  --value "<your-api-key>"

az keyvault secret set --vault-name $KV_NAME \
  --name "AzureOpenAI--ChatDeployment" \
  --value "gpt-4o-mini"

az keyvault secret set --vault-name $KV_NAME \
  --name "AzureOpenAI--ImageDeployment" \
  --value "dall-e-3"

# Application Insights
az keyvault secret set --vault-name $KV_NAME \
  --name "ApplicationInsights--ConnectionString" \
  --value "<your-app-insights-connection-string>"

# OAuth Providers (optional)
az keyvault secret set --vault-name $KV_NAME \
  --name "Authentication--Google--ClientId" \
  --value "<google-client-id>"

az keyvault secret set --vault-name $KV_NAME \
  --name "Authentication--Google--ClientSecret" \
  --value "<google-client-secret>"
```

### Grant App Service Access

```bash
# Get App Service identity
IDENTITY=$(az webapp identity show -g PoAppIdea -n poappidea-web --query principalId -o tsv)

# Grant Key Vault access
az keyvault set-policy --name $KV_NAME \
  --object-id $IDENTITY \
  --secret-permissions get list
```

---

## .NET Configuration Integration

Add to `Program.cs`:

```csharp
// Add Key Vault configuration provider
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

Add to `appsettings.Production.json`:

```json
{
  "KeyVault": {
    "Name": "kv-poshared"
  }
}
```

---

## Secret Rotation

| Secret Type | Rotation Period | Method |
|-------------|-----------------|--------|
| Storage Connection | 90 days | Regenerate keys in portal |
| OpenAI API Key | 90 days | Regenerate in Azure OpenAI |
| OAuth Secrets | 365 days | Regenerate in provider console |
| App Insights | Never | Connection strings don't expire |

## Monitoring

```kql
// Key Vault Access Audit
AzureDiagnostics
| where ResourceType == "VAULTS"
| where OperationName startswith "Secret"
| summarize count() by OperationName, CallerIPAddress, bin(TimeGenerated, 1h)
| order by TimeGenerated desc
```
