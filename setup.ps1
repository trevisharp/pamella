function GetVersion($csprojurl)
{
    $csproj = gc $csprojurl
    $versionText = $csproj | ? { $_.Contains("PackageVersion") }

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

    return $version
}

function Publish()
{
    $key = gc .\.env

    cd Pamella
    
    $version = GetVersion(".\Pamella.csproj")

    dotnet pack Pamella.csproj -c Release
    $file = ".\bin\Release\Pamella." + $version + ".nupkg"
    cp $file Pamella.nupkg

    dotnet nuget push Pamella.nupkg --api-key $key --source https://api.nuget.org/v3/index.json
    rm .\Pamella.nupkg

    cd ..
}

function GetLocalSource()
{
    return Join-Path -Path $env:APPDATA -ChildPath "localnuget"
}

function CreateLocalIfNotExist()
{
    $localsTest = dotnet nuget list source | ? { $_.Contains("local") }
    $hasLocal = $localsTest.Length -gt 0
    if ($hasLocal) {
        return
    }

    $appDataPath = GetSource
    if (!(Test-Path -Path $appDataPath)) {
        ni -ItemType Directory -Path $appDataPath | Out-Null
    }

    dotnet nuget add source $appDataPath --name local
}

function LocalPublish()
{
    CreateLocalIfNotExist

    $key = gc .\.env
    
    cd Pamella

    $version = GetVersion(".\Pamella.csproj")

    dotnet pack Pamella.csproj -c Release
    $file = ".\bin\Release\Pamella." + $version + ".nupkg"
    cp $file Pamella.nupkg

    dotnet nuget push Pamella.nupkg --api-key $key --source local
    rm .\Pamella.nupkg

    cd ..
}