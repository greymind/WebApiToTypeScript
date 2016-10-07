param(
    [Parameter(Mandatory = $true)]
    [string] $NuspecVersion
)

function Activate() {
    $confirmation = Read-Host -Prompt "Are you sure you want to push v$NuspecVersion (y/n)"
    if (!($confirmation.ToLower() -eq "y"))
    {
        return
    }

    $msbuildPath = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
    $solutionPath = (Resolve-Path ".\src\Watts\Watts.csproj")
    & $msbuildPath /t:Build /m /nologo /verbosity:quiet /p:Configuration=Release $solutionPath

    nuget pack .\Watts.nuspec
    nuget push ".\WebApiToTypeScript.$NuspecVersion.nupkg" -Source https://www.nuget.org/api/v2/package
    
    Remove-Item *.nupkg -Force
}

Activate