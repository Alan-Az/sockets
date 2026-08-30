#!/usr/bin/env bash
set -e

PUERTO=${1:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
PROJECT_PATH="$SCRIPT_DIR/Cliente-Servidor-CSharp/Servidor/Servidor.csproj"

echo "==================================================="
echo "  Iniciando Servidor TCP en .NET 10 (Puerto $PUERTO)"
echo "==================================================="

dotnet restore "$PROJECT_PATH" --verbosity quiet
dotnet run --project "$PROJECT_PATH" -- "$PUERTO"
