#!/usr/bin/env bash
set -e

PUERTO=${1:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"

echo "==================================================="
echo "  Iniciando Servidor TCP en .NET 10 (Puerto $PUERTO)"
echo "==================================================="

dotnet run --project "$SCRIPT_DIR/TcpSocketSystem_DotNet/apps/ServidorApp/ServidorApp.csproj" -- "$PUERTO"
