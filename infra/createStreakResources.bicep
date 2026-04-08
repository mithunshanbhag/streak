// common
targetScope = 'resourceGroup'

// parameters
////////////////////////////////////////////////////////////////////////////////

@minLength(3)
@maxLength(6)
@description('A unique environment name (max 6 characters, alphanumeric only).')
param envName string

@description('Commit SHA that triggered this deployment.')
param githubCommitSha string

@description('Github run that triggered this deployment.')
param githubWorkflowRunId string

@description('Github ref (branch, tag) that triggered this deployment.')
param githubRef string

@description('Prefix used to compose Azure resource names.')
param prefix string = 'streak'

@description('The maximum scale-out instance count limit for the function app.')
@minValue(40)
@maxValue(1000)
param maximumInstanceCount int = 100

@description('The memory size of instances used by the function app.')
@allowed([
  2048
  4096
])
param instanceMemoryMB int = 2048

// variables
////////////////////////////////////////////////////////////////////////////////

var location = resourceGroup().location
var normalizedPrefix = toLower(prefix)
var normalizedEnvName = toLower(envName)
var resourceSuffix = toLower(take(uniqueString(subscription().subscriptionId, resourceGroup().id, normalizedEnvName), 6))

var resourceTags = {
  Product: normalizedPrefix
  Environment: normalizedEnvName
  GithubCommitSha: githubCommitSha
  GithubWorkflowRunId: githubWorkflowRunId
  GithubRef: githubRef
}

var cosmosAccountName = toLower(take('${normalizedPrefix}-cosmos-${normalizedEnvName}-${resourceSuffix}', 44))
var functionAppName = '${normalizedPrefix}-api-${normalizedEnvName}-${resourceSuffix}'
var functionPlanName = '${normalizedPrefix}-plan-${normalizedEnvName}-${resourceSuffix}'
var storagePrefixPart = take(normalizedPrefix, 8)
var storageEnvPart = take(normalizedEnvName, 6)
var storageAccountName = toLower(replace('${storagePrefixPart}func${storageEnvPart}${resourceSuffix}', '-', ''))
var keyVaultName = toLower(take('${normalizedPrefix}-kv-${normalizedEnvName}-${resourceSuffix}', 24))
var applicationInsightsName = '${normalizedPrefix}-ai-${normalizedEnvName}-${resourceSuffix}'
var logAnalyticsWorkspaceName = '${normalizedPrefix}-law-${normalizedEnvName}-${resourceSuffix}'
var functionStorageIdentityName = '${normalizedPrefix}-uai-${normalizedEnvName}-${resourceSuffix}'

var deploymentStorageContainerName = 'app-package'
var cosmosConnectionStringSecretName = 'CosmosDb--ConnectionString'

var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

// monitoring
////////////////////////////////////////////////////////////////////////////////

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: resourceTags
  properties: {
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  tags: resourceTags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// storage
////////////////////////////////////////////////////////////////////////////////

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    properties: {
      deleteRetentionPolicy: {}
    }

    resource deploymentContainer 'containers' = {
      name: deploymentStorageContainerName
      properties: {
        publicAccess: 'None'
      }
    }
  }
}

resource functionStorageIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: functionStorageIdentityName
  location: location
  tags: resourceTags
}

resource storageBlobDataOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, storageAccount.id, functionStorageIdentity.id, storageBlobDataOwnerRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    principalId: functionStorageIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageBlobDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, storageAccount.id, functionStorageIdentity.id, storageBlobDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRoleId)
    principalId: functionStorageIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageQueueDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, storageAccount.id, functionStorageIdentity.id, storageQueueDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataContributorRoleId)
    principalId: functionStorageIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, storageAccount.id, functionStorageIdentity.id, storageTableDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: functionStorageIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// data
////////////////////////////////////////////////////////////////////////////////

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  tags: resourceTags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
    locations: [
      {
        failoverPriority: 0
        isZoneRedundant: false
        locationName: location
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    minimalTlsVersion: 'Tls12'
    publicNetworkAccess: 'Enabled'
  }
}

// secrets
////////////////////////////////////////////////////////////////////////////////

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: resourceTags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enablePurgeProtection: false
    enableRbacAuthorization: false
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    publicNetworkAccess: 'Enabled'
    softDeleteRetentionInDays: 7
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

var cosmosPrimaryConnectionString = cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString

resource cosmosConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: cosmosConnectionStringSecretName
  properties: {
    value: cosmosPrimaryConnectionString
  }
}

// function app
////////////////////////////////////////////////////////////////////////////////

resource functionPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: functionPlanName
  location: location
  kind: 'functionapp'
  tags: resourceTags
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  tags: resourceTags
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${functionStorageIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
    }
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccount.properties.primaryEndpoints.blob}${deploymentStorageContainerName}'
          authentication: {
            type: 'UserAssignedIdentity'
            userAssignedIdentityResourceId: functionStorageIdentity.id
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10'
      }
    }
  }
}

resource keyVaultFunctionAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: functionApp.identity.principalId
        tenantId: subscription().tenantId
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

resource functionAppSettings 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: {
    AzureWebJobsStorage__accountName: storageAccount.name
    AzureWebJobsStorage__credential: 'managedidentity'
    AzureWebJobsStorage__clientId: functionStorageIdentity.properties.clientId
    APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsights.properties.ConnectionString
    CosmosDb__ConnectionString: '@Microsoft.KeyVault(SecretUri=${cosmosConnectionStringSecret.properties.secretUriWithVersion})'
    CosmosDb__AccountEndpoint: cosmosDbAccount.properties.documentEndpoint
  }
  dependsOn: [
    keyVaultFunctionAccessPolicy
  ]
}

// outputs
////////////////////////////////////////////////////////////////////////////////

output outputFunctionAppName string = functionApp.name
output outputFunctionAppHostName string = functionApp.properties.defaultHostName
output outputFunctionPlanName string = functionPlan.name
output outputStorageAccountName string = storageAccount.name
output outputCosmosDbAccountName string = cosmosDbAccount.name
output outputCosmosDbEndpoint string = cosmosDbAccount.properties.documentEndpoint
output outputKeyVaultName string = keyVault.name
output outputCosmosDbConnectionStringSecretUri string = cosmosConnectionStringSecret.properties.secretUriWithVersion
output outputApplicationInsightsName string = applicationInsights.name
