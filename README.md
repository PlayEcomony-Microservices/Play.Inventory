# PlayEconomy.Play.Inventory
My second microservices in the PlayEcononmy project. All my inventory functionality is in this microservice

## Create and Publish Play.Inventory.Contracts package to Github
```powershell
$version=1.0.1
$owner="PlayEcomony-Microservices"
$gh_pat="[PAT HERE]"

dotnet pack src\Play.Inventory.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Inventory -o ..\packages

dotnet nuget push ..\packages\Play.Inventory.Contracts.$version.nupkg --api-key $gh_pat --source "github"

```
