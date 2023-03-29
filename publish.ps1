$key = gc .\.env
dotnet pack -c Release
cp .\bin\Release\pamella.1.0.0.nupkg pamella.nupkg
dotnet nuget push pamella.nupkg --api-key $key --source https://api.nuget.org/v3/index.json
rm .\pamella.nupkg