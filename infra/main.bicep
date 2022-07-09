targetScope = 'subscription'

@description('Location for the resources.')
param location string = deployment().location

@description('Value uses as base-template to named the resources deployed in Azure.')
param appName string = 'AzCleaner'

@description('Indicates whether ApplicationInsights should be enabled and integrated.')
param enableApplicationInsights bool = true

var suffix = toLower('${appName}-${location}')
var resourceGroupName_var = toLower('rg-${suffix}')
var roleAssignmentName_var = guid(subscription().subscriptionId, resourceGroupName_var)
var deploymentName_var = 'AzCleaner.Template-${uniqueString(deployment().name)}'
var contributorRoleId = 'b24988ac-6180-42a0-ab88-20f7382dd24c'

resource resourceGroupName 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: resourceGroupName_var
  location: location
}

module deploymentName './modules/func.bicep' = {
  name: deploymentName_var
  scope: resourceGroup(resourceGroupName_var)
  params: {
    appName: appName
    enableApplicationInsights: enableApplicationInsights
    location: location
  }
  dependsOn: [
    resourceGroupName
  ]
}

resource roleAssignmentName 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: roleAssignmentName_var
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', contributorRoleId)
    principalId: deploymentName.outputs.functionAppPrincipalId
  }
}
