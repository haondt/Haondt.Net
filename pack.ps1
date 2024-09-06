# Define the list of projects in the Visual Studio solution
$projects = @(
    ".\Haondt.Core\Haondt.Core.csproj",
    ".\Haondt.Persistence\Haondt.Persistence.csproj",
    ".\Haondt.Identity\Haondt.Identity.csproj",
    ".\Haondt.Web\Haondt.Web.csproj",
    ".\Haondt.Web.BulmaCSS\Haondt.Web.BulmaCSS.csproj",
    ".\Haondt.Web.Components\Haondt.Web.Components.csproj"
    ".\Haondt.Web.Core\Haondt.Web.Core.csproj"
)

$versionSuffix = "alpha-" + (Get-Date -Format "yyyyMMddHHmmss")

# Define the local NuGet directory where the packages will be copied
$localNugetDirectory = "D:\Documents\Projects\local.nuget"

# Loop through each project and pack it
foreach ($project in $projects) {
    Write-Host "Packing $project in Debug mode with version suffix $versionSuffix..."

    # Pack the project in Debug mode and append "alpha" to the version
    & dotnet pack $project -c Debug --version-suffix $versionSuffix --output "$localNugetDirectory" --no-build

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully packed $project" -ForegroundColor Green
    } else {
        Write-Host "Failed to pack $project" -ForegroundColor Red
    }
}

Write-Host "All projects have been packed and copied to $localNugetDirectory."
