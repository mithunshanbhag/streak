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

// var minTlsVersionForApps = '1.2'
// var minTlsVersionForStorage = 'TLS1_2'

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

// function app (PTL)
// var functionAppName = '${prefix}-api-ptl-${suffix}'
// var functionAppStorageAccountName = '${prefix}ptl${suffix}'
// var functionAppServicePlanName = '${prefix}-ptl-appsvcplan-${suffix}'

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
