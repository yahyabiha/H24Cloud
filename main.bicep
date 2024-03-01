param location string = 'westus'

// Création d'un compte de stockage
resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'documentstorage${uniqueString(resourceGroup().id)}' // Assurez-vous que le nom est unique au niveau global
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

// Création d'un plan de service d'application
resource appServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: 'DocumentServicePlan${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Y1' // C'est un exemple de niveau tarifaire, choisissez en fonction de vos besoins
  }
}

// Création d'une application de fonction
resource functionApp 'Microsoft.Web/sites@2020-12-01' = {
  name: 'DocumentFunctionApp${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value};EndpointSuffix=core.windows.net'
        },
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        },
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet' // L'environnement d'exécution pour .NET
        }
      ]
    }
  }
}
