# Define the list of projects in the Visual Studio solution
$projects = @(
    ".\Haondt.Core\Haondt.Core.csproj",
    ".\Haondt.Persistence\Haondt.Persistence.csproj",
    ".\Haondt.Identity\Haondt.Identity.csproj",
    ".\Haondt.Web\Haondt.Web.csproj",
    ".\Haondt.Web.BulmaCSS\Haondt.Web.BulmaCSS.csproj",
    ".\Haondt.Web.Components\Haondt.Web.Components.csproj"
    ".\Haondt.Web.Core\Haondt.Web.Core.csproj"
    ".\Haondt.Persistence.MongoDb\Haondt.Persistence.MongoDb.csproj"
)

$versionSuffix = "alpha-" + (Get-Date -Format "yyyyMMddHHmmss")

# Define the local NuGet directory where the packages will be copied
$localNugetDirectory = "D:\Documents\Projects\local.nuget"

# Get the most recent Git tag and use it as the version
$gitTag = git describe --tags --abbrev=0
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to retrieve the latest Git tag." -ForegroundColor Red
    exit 1
}

$gitTag = $gitTag.Trim()
$baseVersion = $gitTag -replace '^v', ''

# Split the base version into its components
$versionParts = $baseVersion -split '\.'
if ($versionParts.Length -ne 3) {
    Write-Host "Error: Version format is incorrect." -ForegroundColor Red
    exit 1
}

# Bump the patch version (z)
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2] + 1  # Increment patch

# Create the new version string
$version = "$major.$minor.$patch-$versionSuffix"

Write-Host "Using nuget tag version: $version"

# Loop through each project and pack it
foreach ($project in $projects) {
    Write-Host "Packing $project in Debug mode with version $version..."

    # Pack the project in Debug mode and append "alpha" to the version
    & dotnet pack $project -c Debug --output "$localNugetDirectory" --no-build -p:Version=$version

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully packed $project" -ForegroundColor Green
    } else {
        Write-Host "Failed to pack $project" -ForegroundColor Red
    }
}

Write-Host "All projects have been packed and copied to $localNugetDirectory."
