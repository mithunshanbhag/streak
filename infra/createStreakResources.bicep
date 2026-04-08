// common
targetScope = 'resourceGroup'

// parameters
////////////////////////////////////////////////////////////////////////////////

@allowed([
  'test'
])
@description('A unique environment name (max 6 characters, alphanumeric only).')
param envName string

@description('Commit SHA that triggered this deployment.')
param githubCommitSha string

@description('Github run that triggered this deployment.')
param githubWorkflowRunId string

@description('Github ref (branch, tag) that triggered this deployment.')
param githubRef string

param resourceLocation string = resourceGroup().location

param prefix string = 'streak'

// variables
////////////////////////////////////////////////////////////////////////////////

var minTlsVersionForApps = '1.2'
var minTlsVersionForStorage = 'TLS1_2'

var suffix = toLower(envName)

// key vault
// var keyVaultName = '${prefix}-kv-${suffix}'
// var cosmosDbConnectionStringSecretName = 'cosmosDbConnectionString'

// cosmos db
var cosmosAccountName = '${prefix}-cosmos-${suffix}'
var cosmosDbName = '${prefix}-db'

// log analytics & app insights
var logAnalyticsWSName = '${prefix}-law-${suffix}'
var appInsightsName = '${prefix}-ai-${suffix}'

// function app
var functionAppName = '${prefix}-api-ptl-${suffix}'
var functionAppStorageAccountName = '${prefix}ptl${suffix}'
var functionAppServicePlanName = '${prefix}-ptl-appsvcplan-${suffix}'

// tags
var resourceTags = {
  Product: prefix
  Environment: envName
  GithubCommitSha: githubCommitSha
  GithubWorkflowRunId: githubWorkflowRunId
  GithubRef: githubRef
}

// resources
////////////////////////////////////////////////////////////////////////////////

//
// Cosmos DB (for Cloud API, Metrics API)
//

resource resCosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: resourceLocation
  tags: resourceTags
  properties: {
    enableFreeTier: true
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: resourceLocation
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }

  resource resCosmosDb 'sqlDatabases@2023-04-15' = {
    tags: resourceTags
    location: resourceLocation
    name: cosmosDbName
    properties: {
      resource: {
        id: cosmosDbName
      }
    }
  }
}

//
// function app
//

// storage account
resource resFunctionAppStorageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: functionAppStorageAccountName
  location: resourceLocation
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: minTlsVersionForStorage
  }
}

// app service plan (consumption plan)
resource resFunctionAppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: functionAppServicePlanName
  location: resourceLocation
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

// function app
resource resFunctionApp 'Microsoft.Web/sites@2024-11-01' = {
  name: functionAppName
  location: resourceLocation
  tags: resourceTags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: resFunctionAppServicePlan.id
    siteConfig: {
      minTlsVersion: minTlsVersionForApps
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      functionAppScaleLimit: 1
      appSettings: [
        // app settings required by Azure functions
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${resFunctionAppStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${resFunctionAppStorageAccount.listKeys().keys[0].value}'
          // Commented out below lines because KV references don't work well with the "Azure/functions-action" github action. 
          // More details: https://github.com/Azure/functions-action/discussions/140
          // value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=${e2eTesterApiStorageAccountConnectionStringSecretName})'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: resAppInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: resAppInsights.properties.ConnectionString
        }
      ]
    }
  }
}


//
// log analytics & app insights
//

resource resLogAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWSName
  location: resourceLocation
  tags: resourceTags
  properties: {
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    sku: {
      name: 'PerGB2018' // pay as you go
    }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

// app insights
resource resAppInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  tags: resourceTags
  location: resourceLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: resLogAnalyticsWorkspace.id
  }
}
