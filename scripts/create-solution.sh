#!/usr/bin/env bash
set -euo pipefail

dotnet new sln -n ITAssetAccounting
find src tests -name "*.csproj" -print0 | xargs -0 -I{} dotnet sln ITAssetAccounting.sln add "{}"
