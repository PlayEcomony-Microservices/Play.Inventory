# PlayEconomy.Play.Inventory

Microservice to facilitate inventory functionality to Play Economy.

## Create and Publish Play.Inventory.Contracts package to GitHub

```powershell
$version="1.0.2"
$owner="PlayEcomony-Microservices"
$gh_pat="[PAT HERE]"

dotnet pack src\Play.Inventory.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Inventory -o ..\packages

dotnet nuget push ..\packages\Play.Inventory.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build docker image

```powershell
$env:GH_OWNER="PlayEcomony-Microservices"
$env:GH_PAT="[PAT HERE]"
$acrName="playeconomybkm"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$acrName.azurecr.io/play.inventory:$version" . 
```

## Run the docker image

```powershell
$cosmosDbConnStr="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"
docker run -it --rm -p 5004:5004 --name inventory -e MongoDbSettings__ConnectionString=$cosmosDbConnStr -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" play.inventory:$version
```

## Publish docker image to Azure Container Registry

```powershell
az acr login --name $acrName
docker push "$acrName.azurecr.io/play.inventory:$version"
```
