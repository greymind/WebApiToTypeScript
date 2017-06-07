param()

function Activate() {
    $msbuildPath = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
    $msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    $solutionPath = (Resolve-Path ".\src\Watts\Watts.csproj")

    Write-Host "Building release $solutionPath..."
    & $msbuildPath /t:Build /m /nologo /verbosity:quiet /p:Configuration=Release $solutionPath

    $nuspecPath = (Resolve-Path ".\Watts.nuspec").Path
    [xml]$nuspecXml = Get-Content $nuspecPath
    $version = $nuspecXml.package.metadata.version

    $confirmation = Read-Host -Prompt "Are you sure you want to push v$version (y/n)"
    if (!($confirmation.ToLower() -eq "y"))
    {
        return
    }

    nuget pack .\Watts.nuspec
    nuget push ".\WebApiToTypeScript.$version.nupkg" -Source https://www.nuget.org/api/v2/package
    
    Remove-Item *.nupkg -Force
}

Activate