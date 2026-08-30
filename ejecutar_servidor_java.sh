#!/usr/bin/env bash
set -e

PUERTO=${1:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
BIN_DIR="$SCRIPT_DIR/TcpSocketSystem_Java/bin"

mkdir -p "$BIN_DIR"

echo "==================================================="
echo "  Compilando modulo Java..."
echo "==================================================="
find "$SCRIPT_DIR/TcpSocketSystem_Java/src" -name "*.java" > "$SCRIPT_DIR/sources.txt"
javac -encoding UTF-8 -d "$BIN_DIR" @"$SCRIPT_DIR/sources.txt"
rm -f "$SCRIPT_DIR/sources.txt"

echo "==================================================="
echo "  Iniciando Servidor TCP Java 21 (Puerto $PUERTO)"
echo "==================================================="
java -cp "$BIN_DIR" com.tecnm.sockets.app.ServidorApp "$PUERTO"
