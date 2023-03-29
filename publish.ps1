$csproj = gc .\pamella.csproj
$versionText = $csproj | % {
    if ($_.Contains("PackageVersion"))
    {
        $_
    }
}

$version = ""
$flag = 0
for ($i = 0; $i -lt $versionText.Length; $i++)
{
    $char = $versionText[$i]

    if ($flag -eq 1)
    {
        if ($char -eq "<")
        {
            break
        }

        $version += $char
    }

    if ($char -eq ">")
    {
        $flag = 1
    }
}

$key = gc .\.env
dotnet pack -c Release
cp ".\bin\Release\pamella." + $version + ".nupkg" pamella.nupkg
dotnet nuget push pamella.nupkg --api-key $key --source https://api.nuget.org/v3/index.json
rm .\pamella.nupkg