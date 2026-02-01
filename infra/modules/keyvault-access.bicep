// Key Vault Access Policy Module
// Grants a principal access to secrets in the specified Key Vault

@description('The name of the Key Vault')
param keyVaultName string

@description('The principal ID to grant access to')
param principalId string

@description('The type of principal (User, Group, ServicePrincipal)')
param principalType string = 'ServicePrincipal'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource accessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}
