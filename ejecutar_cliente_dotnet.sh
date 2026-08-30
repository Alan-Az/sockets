#!/usr/bin/env bash
set -e

HOST=${1:-"127.0.0.1"}
PUERTO=${2:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"

echo "==================================================="
echo "  Iniciando Cliente TCP en .NET 10 ($HOST:$PUERTO)"
echo "==================================================="

dotnet run --project "$SCRIPT_DIR/TcpSocketSystem_DotNet/apps/ClienteApp/ClienteApp.csproj" -- "$HOST" "$PUERTO"
