#!/usr/bin/env bash
set -e

PUERTO=${1:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
JAVA_DIR="$SCRIPT_DIR/Cliente-Servidor-Java"
BIN_DIR="$JAVA_DIR/bin"

mkdir -p "$BIN_DIR"

echo "==================================================="
echo "  Compilando e Iniciando Servidor Java (Puerto $PUERTO)"
echo "==================================================="

javac -encoding UTF-8 -d "$BIN_DIR" "$JAVA_DIR/ServidorJava.java"
java -cp "$BIN_DIR" ServidorJava "$PUERTO"
