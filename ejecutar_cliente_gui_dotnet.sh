#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
PROJECT_PATH="$SCRIPT_DIR/Cliente-Servidor-CSharp/ClienteGUI/ClienteGUI.csproj"

echo "==================================================="
echo "  Iniciando Interfaz Gráfica Cliente C# (.NET 10)"
echo "==================================================="

# Restaurar paquetes de NuGet automáticamente (descarga Eto.Forms y dependencias GTK/WPF)
dotnet restore "$PROJECT_PATH" --verbosity quiet

# Ejecutar la aplicación GUI multiplataforma
dotnet run --project "$PROJECT_PATH"
