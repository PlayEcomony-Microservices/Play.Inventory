# PlayEconomy.Play.Inventory

My second microservices in the Play-Economy project. All my inventory functionality is in this microservice

## Create and Publish Play.Inventory.Contracts package to GitHub

```powershell
$version="1.0.1"
$owner="PlayEcomony-Microservices"
$gh_pat="[PAT HERE]"

dotnet pack src\Play.Inventory.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Inventory -o ..\packages

dotnet nuget push ..\packages\Play.Inventory.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build docker image

```powershell
$env:GH_OWNER="PlayEcomony-Microservices"
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.inventory:$version . 
```

## Run the docker image

```powershell
$adminPass="[password here]"
docker run -it --rm -p 5004:5004 --name inventory -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network playinfra_default play.inventory:$version
```
