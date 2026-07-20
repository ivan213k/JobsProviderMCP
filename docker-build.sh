#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir"

image_name="jobsproviderapi"

resolve_semantic_version() {
    if ! command -v dotnet-gitversion >/dev/null 2>&1; then
        echo "dotnet-gitversion not found, installing GitVersion.Tool..." >&2
        if ! dotnet tool install --global GitVersion.Tool >&2; then
            echo "Could not install GitVersion.Tool, falling back to 'dev'." >&2
            echo "dev"
            return
        fi
        export PATH="$PATH:$HOME/.dotnet/tools"
    fi

    dotnet-gitversion /config GitVersion.yaml /showvariable SemVer 2>/dev/null || echo "dev"
}

SemanticVersion="$(resolve_semantic_version)"
SemanticVersion="${SemanticVersion:-dev}"

echo "Building '$image_name' with SemanticVersion=$SemanticVersion"

docker build \
    --build-arg SEMANTIC_VERSION="$SemanticVersion" \
    -t "$image_name:$SemanticVersion" \
    -t "$image_name:latest" \
    .

echo "Built $image_name:$SemanticVersion (also tagged latest)"
