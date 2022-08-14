param appName string
param enableApplicationInsights bool
param location string

var suffix = toLower('${appName}-${location}')
var storageAccountName_var = toLower('st${appName}${location}')
var applicationInsightsName_var = toLower('appi-${suffix}')
var functionAppName_var = toLower('func-${suffix}')
var hostingPlanName_var = toLower('plan-${suffix}')
var repoUrl = 'https://github.com/alexkhil/AzCleaner.git'

resource functionAppName 'Microsoft.Web/sites@2021-02-01' = {
  name: functionAppName_var
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    clientAffinityEnabled: false
    serverFarmId: hostingPlanName.id
    siteConfig: {
      use32BitWorkerProcess: false,
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AZURE_FUNCTIONS_ENVIRONMENT'
          value: 'Release'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: functionAppName_var
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: (enableApplicationInsights ? applicationInsightsName.properties.InstrumentationKey : null)
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName_var};AccountKey=${listKeys(storageAccountName.id, '2021-06-01').keys[0].value}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName_var};AccountKey=${listKeys(storageAccountName.id, '2021-06-01').keys[0].value}'
        }
      ]
    }
  }
}

resource functionAppName_web 'Microsoft.Web/sites/sourcecontrols@2021-02-01' = {
  parent: functionAppName
  name: 'web'
  properties: {
    repoUrl: repoUrl
    branch: 'main'
    isManualIntegration: true
  }
}

resource hostingPlanName 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: hostingPlanName_var
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource storageAccountName 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName_var
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource applicationInsightsName 'Microsoft.Insights/components@2020-02-02' = if (enableApplicationInsights) {
  name: applicationInsightsName_var
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
  }
}

output functionAppPrincipalId string = reference(functionAppName.id, '2021-02-01', 'Full').identity.principalId
