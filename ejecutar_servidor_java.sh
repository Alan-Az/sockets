#!/usr/bin/env bash
set -e

PUERTO=${1:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
BIN_DIR="$SCRIPT_DIR/Cliente-Servidor-Java/bin"

mkdir -p "$BIN_DIR"
javac -encoding UTF-8 -d "$BIN_DIR" "$SCRIPT_DIR/Cliente-Servidor-Java/ServidorJava.java"

echo "==================================================="
echo "  Iniciando Servidor TCP en Java (Puerto $PUERTO)"
echo "==================================================="

java -cp "$BIN_DIR" ServidorJava "$PUERTO"
