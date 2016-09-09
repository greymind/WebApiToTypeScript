param(
    [Parameter(Mandatory = $true)]
    [string] $NuspecVersion
)

function Activate() {
    $confirmation = Read-Host -Prompt "Are you sure you want to push v$NuspecVersion (y/n)"
    if (!($confirmation.ToLower() -eq "y"))
    {
        return;
    }

    nuget pack .\Watts.nuspec
    nuget push ".\WebApiToTypeScript.$NuspecVersion.nupkg" -Source https://www.nuget.org/api/v2/package
    
    Remove-Item *.nupkg -Force
}

Activate