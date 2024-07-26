cd ..
dotnet pack Riateu/Riateu.csproj -c Release
API_KEY=$1
VERSION=$2

dotnet nuget push ./artifacts/Riateu.$VERSION.nupkg --source https://api.nuget.org/v3/index.json --skip-duplicate --api-key $API_KEY