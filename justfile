clean:
    dotnet clean

build:
    dotnet build

rebuild: clean build

local_nuget_dir := "$HOME/packages/nuget"

[script]
pack:
    #!/usr/bin/env bash
    set -euo pipefail

    projects=(
        "Haondt.Core/Haondt.Core.csproj"
        "Haondt.Identity/Haondt.Identity.csproj"
        "Haondt.Persistence/Haondt.Persistence.csproj"
        "Haondt.Persistence.MongoDb/Haondt.Persistence.MongoDb.csproj"
        "Haondt.Persistence.Postgresql/Haondt.Persistence.Postgresql.csproj"
        "Haondt.Persistence.Sqlite/Haondt.Persistence.Sqlite.csproj"
        "Haondt.Web/Haondt.Web.csproj"
        "Haondt.Web.BulmaCSS/Haondt.Web.BulmaCSS.csproj"
        "Haondt.Web.Core/Haondt.Web.Core.csproj"
        "Haondt.Json/Haondt.Json.csproj"
        "Haondt.Persistence.EntityFrameworkCore/Haondt.Persistence.EntityFrameworkCore.csproj"
    )

    git_tag=$(git describe --tags --abbrev=0 2>/dev/null) || {
        echo "Error: Failed to retrieve the latest Git tag."
        exit 1
    }

    base_version="${git_tag#v}"
    IFS='.' read -r major minor patch <<< "$base_version"

    if [[ -z "$major" || -z "$minor" || -z "$patch" ]]; then
        echo "Error: Version format is incorrect."
        exit 1
    fi

    patch=$((patch + 1))
    version_suffix="alpha-$(date +'%Y%m%d%H%M%S')"
    version="$major.$minor.$patch-$version_suffix"

    echo "Using nuget tag version: $version"

    for project in "${projects[@]}"; do
        echo "Packing $project in Debug mode with version $version..."
        if dotnet pack "$project" -c Debug --output "{{local_nuget_dir}}" --no-build -p:Version="$version"; then
            echo "Successfully packed $project"
        else
            echo "Failed to pack $project"
        fi
    done

    echo "All projects have been packed and copied to {{local_nuget_dir}}."
