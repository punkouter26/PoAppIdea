// PoAppIdea Infrastructure as Code
// Deploys App Service, Storage Account, and configures Key Vault references
// Usage: az deployment group create --resource-group PoAppIdea --template-file main.bicep

@description('The environment name (e.g., prod, dev)')
param environment string = 'prod'

@description('The Azure region for resources')
param location string = resourceGroup().location

@description('The name prefix for resources')
param appName string = 'poappidea'

@description('The App Service Plan resource ID from PoShared')
param appServicePlanId string

@description('The Key Vault name in PoShared resource group')
param keyVaultName string = 'kv-poshared'

@description('The Application Insights connection string')
param appInsightsConnectionString string

// Storage Account for Table and Blob storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'st${appName}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
  tags: {
    environment: environment
    app: appName
  }
}

// Blob Services for visual assets
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// Blob container for visual assets
resource visualAssetsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: 'visual-assets'
  properties: {
    publicAccess: 'None'
  }
}

// Table Services for entity storage
resource tableServices 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// App Service for the Blazor web app
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${appName}-web'
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'AzureStorage__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--AzureStorage--ConnectionString)'
        }
        {
          name: 'AzureAI__Endpoint'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--AzureAI--Endpoint)'
        }
        {
          name: 'AzureAI__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--AzureAI--ApiKey)'
        }
        {
          name: 'AzureAI__DeploymentName'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--AzureAI--DeploymentName)'
        }
        {
          name: 'PoAppIdea__Authentication__Google__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--GoogleOAuth--ClientId)'
        }
        {
          name: 'PoAppIdea__Authentication__Google__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--GoogleOAuth--ClientSecret)'
        }
        {
          name: 'PoAppIdea__Authentication__Microsoft__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--MicrosoftOAuth--ClientId)'
        }
        {
          name: 'PoAppIdea__Authentication__Microsoft__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoAppIdea--MicrosoftOAuth--ClientSecret)'
        }
      ]
    }
  }
  tags: {
    environment: environment
    app: appName
  }
}

// Key Vault access policy for the web app's managed identity
module keyVaultAccess 'modules/keyvault-access.bicep' = {
  name: 'keyVaultAccess-${appName}'
  scope: resourceGroup('PoShared')
  params: {
    keyVaultName: keyVaultName
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output webAppName string = webApp.name
output webAppHostName string = webApp.properties.defaultHostName
output webAppPrincipalId string = webApp.identity.principalId
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
