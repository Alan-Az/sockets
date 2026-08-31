#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
JAVA_DIR="$SCRIPT_DIR/Cliente-Servidor-Java"
BIN_DIR="$JAVA_DIR/bin"

mkdir -p "$BIN_DIR"

echo "==================================================="
echo "  Compilando e Iniciando Interfaz Gráfica Java"
echo "==================================================="

javac -encoding UTF-8 -d "$BIN_DIR" "$JAVA_DIR/ClienteJavaGUI.java"
java -cp "$BIN_DIR" ClienteJavaGUI
