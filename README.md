# AzCleaner

[![license-badge]][license-link] [![build-badge]][build-link] [![coverage-badge]][build-link]

[![deploy-badge]][deploy-link] [![visualize-badge]][visualize-link]

## How does it workds

By Default, the tool will get triggered every 12 hours and search for any resources tagged with `expireOn` with a value in the past, and delete them. Once all the `expired` resources are deleted. It will search for empty Resource Group and delete them too.

## Development

```sh
az login
az ad sp create-for-rbac --sdk-auth
```

Save returned json to `azureauth.json` file on `src/AzCleaner.Func` folder.

## Deploy

```sh
az group create --name <resource-group-name> --location <location-name>
cd infra
az deployment group create -n <deployment-name> -g <resource-group-name> -f azuredeploy.json
```

[license-badge]: <https://img.shields.io/github/license/alexkhil/az-cleaner>
[license-link]: <https://github.com/alexkhil/az-cleaner/blob/main/LICENSE>

[build-badge]: <https://dev.azure.com/alexkhildev/az-cleaner/_apis/build/status/alexkhil.AzCleaner?branchName=main>
[build-link]: <https://dev.azure.com/alexkhildev/az-cleaner/_build/latest?definitionId=5&branchName=main>

[coverage-badge]: <https://img.shields.io/azure-devops/coverage/alexkhildev/az-cleaner/5/main>

[deploy-badge]: <https://aka.ms/deploytoazurebutton>
[deploy-link]: <https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Falexkhil%2Faz-cleaner%2Fmain%2Finfra%2Fazuredeploy.json>

[visualize-badge]: <https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/visualizebutton.svg?sanitize=true>
[visualize-link]: <http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Falexkhil%2Faz-cleaner%2Fmain%2Finfra%2Fazuredeploy.json>
