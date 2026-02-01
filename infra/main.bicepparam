// PoAppIdea Infrastructure Parameters - Production
// Use with: az deployment group create -g PoAppIdea -f main.bicep -p main.parameters.json

using 'main.bicep'

param environment = 'prod'
param location = 'westus2'
param appName = 'poappidea'
param appServicePlanId = '/subscriptions/bbb8dfbe-9169-432f-9b7a-fbf861b51037/resourceGroups/PoShared/providers/Microsoft.Web/serverfarms/asp-poshared'
param keyVaultName = 'kv-poshared'
param appInsightsConnectionString = '' // Retrieved from Azure at deploy time
