#!/usr/bin/env bash
set -euo pipefail

NAME="${1:?Usage: scripts/new-module.sh <ModuleName> (e.g. Address)}"
MODULE="ArasERP.Modules.${NAME}"
BASE="src/Modules/${NAME}"
SLNX="ArasERP.slnx"
HOST_CSPROJ="src/Host/ArasERP.Host/ArasERP.Host.csproj"
PROGRAM="src/Host/ArasERP.Host/Program.cs"
TEMPLATE="templates/araserp-module"

if [[ ! -f "$SLNX" || ! -f "$PROGRAM" ]]; then
    echo "Run this script from the repository root." >&2
    exit 1
fi

if [[ -d "$BASE" ]]; then
    echo "Module '$NAME' already exists at $BASE." >&2
    exit 1
fi

dotnet new install "$TEMPLATE" --force >/dev/null

dotnet new araserp-module -n "$NAME" -o "$BASE" >/dev/null

dotnet sln "$SLNX" add \
    "$BASE/${MODULE}/${MODULE}.csproj" \
    "$BASE/${MODULE}.Contracts/${MODULE}.Contracts.csproj" \
    "$BASE/${MODULE}.Api/${MODULE}.Api.csproj" \
    "$BASE/${MODULE}.Infrastructure/${MODULE}.Infrastructure.csproj" \
    --solution-folder "/src/Modules/${NAME}/" >/dev/null

sed -i '/<Folder Name="\/src\/" \/>/d' "$SLNX"

dotnet add "$HOST_CSPROJ" reference \
    "$BASE/${MODULE}/${MODULE}.csproj" \
    "$BASE/${MODULE}.Api/${MODULE}.Api.csproj" \
    "$BASE/${MODULE}.Infrastructure/${MODULE}.Infrastructure.csproj" >/dev/null

grep -q "using ArasERP.Modules.${NAME};" "$PROGRAM" ||
    sed -i "s|using Scalar.AspNetCore;|using Scalar.AspNetCore;\nusing ArasERP.Modules.${NAME};\nusing ArasERP.Modules.${NAME}.Api.Endpoints;\nusing ArasERP.Modules.${NAME}.Infrastructure;|" "$PROGRAM"

grep -q "builder.Services.Add${NAME}Module();" "$PROGRAM" ||
    sed -i "s|builder.Services.AddInventoryModule();|builder.Services.AddInventoryModule();\nbuilder.Services.Add${NAME}Module();\nbuilder.Services.Add${NAME}Infrastructure(\n    builder.Configuration.GetConnectionString(\"DefaultConnection\")!\n);|" "$PROGRAM"

grep -q "app.Map${NAME}Endpoints();" "$PROGRAM" ||
    sed -i "s|app.MapWarehouseEndpoints();|app.MapWarehouseEndpoints();\napp.Map${NAME}Endpoints();|" "$PROGRAM"

echo "Module '$NAME' scaffolded and wired into '$SLNX', '$HOST_CSPROJ' and '$PROGRAM'."
echo "Verify with: dotnet build '$SLNX'"
