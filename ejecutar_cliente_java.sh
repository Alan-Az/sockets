#!/usr/bin/env bash
set -e

HOST=${1:-"127.0.0.1"}
PUERTO=${2:-5000}
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
BIN_DIR="$SCRIPT_DIR/TcpSocketSystem_Java/bin"

if [ ! -d "$BIN_DIR" ]; then
    echo "Compilando antes de ejecutar..."
    mkdir -p "$BIN_DIR"
    find "$SCRIPT_DIR/TcpSocketSystem_Java/src" -name "*.java" > "$SCRIPT_DIR/sources.txt"
    javac -encoding UTF-8 -d "$BIN_DIR" @"$SCRIPT_DIR/sources.txt"
    rm -f "$SCRIPT_DIR/sources.txt"
fi

echo "==================================================="
echo "  Iniciando Cliente TCP Java 21 ($HOST:$PUERTO)"
echo "==================================================="
java -cp "$BIN_DIR" com.tecnm.sockets.app.ClienteApp "$HOST" "$PUERTO"
