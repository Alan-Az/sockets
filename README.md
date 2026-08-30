# 🌐 Práctica de Laboratorio: Sockets TCP/IP (TecNM)
### Materia: Programación en Ambiente Cliente/Servidor (.NET 10 y Java)

Este repositorio contiene la implementación limpia y directa de la práctica de Sockets TCP/IP, organizada exactamente según la especificación del documento oficial de laboratorio en `/Cliente-Servidor-CSharp` y `/Cliente-Servidor-Java`.

---

## 📁 Estructura del Repositorio

```text
/
├── 🔷 Cliente-Servidor-CSharp/
│   ├── Servidor/
│   │   ├── Program.cs          <-- Servidor TCP Asíncrono (.NET 10)
│   │   └── Servidor.csproj
│   └── Cliente/
│       ├── Program.cs          <-- Cliente TCP (.NET 10)
│       └── Cliente.csproj
│
├── ☕ Cliente-Servidor-Java/
│   ├── ServidorJava.java       <-- Servidor TCP Multihilo (Java)
│   └── ClienteJava.java        <-- Cliente TCP (Java)
│
├── 🚀 Scripts de Ejecución Rápida
│   ├── ejecutar_servidor_csharp.ps1 / .sh
│   ├── ejecutar_cliente_csharp.ps1 / .sh
│   ├── ejecutar_servidor_java.ps1 / .sh
│   └── ejecutar_cliente_java.ps1 / .sh
│
├── 📚 Docs/
│   ├── Manual_Ejecucion_y_Pruebas.md
│   └── Guia_Captura_Wireshark_Handshake.md
│
└── .gitignore
```

---

## 🚀 Cómo Ejecutar el Proyecto

### Opción A: Mediante Scripts en Windows (PowerShell)

Abre **dos terminales de PowerShell**:

1. **Terminal 1 (Servidor):**
   ```powershell
   pwsh ./ejecutar_servidor_csharp.ps1 -Puerto 5000
   # O si prefieres Java:
   pwsh ./ejecutar_servidor_java.ps1 -Puerto 5000
   ```

2. **Terminal 2 (Cliente):**
   ```powershell
   pwsh ./ejecutar_cliente_csharp.ps1 -HostDestino 127.0.0.1 -Puerto 5000
   # O si prefieres Java:
   pwsh ./ejecutar_cliente_java.ps1 -HostDestino 127.0.0.1 -Puerto 5000
   ```

---

### Opción B: Mediante Scripts en Linux / macOS (Bash)

```bash
# Dar permisos la primera vez:
chmod +x *.sh

# Terminal 1 (Servidor C# o Java):
./ejecutar_servidor_csharp.sh 5000
# ./ejecutar_servidor_java.sh 5000

# Terminal 2 (Cliente C# o Java):
./ejecutar_cliente_csharp.sh 127.0.0.1 5000
# ./ejecutar_cliente_java.sh 127.0.0.1 5000
```

---

### Opción C: Comandos Directos (Sin Scripts)

#### En C# (.NET 10):
```bash
# Servidor:
dotnet run --project Cliente-Servidor-CSharp/Servidor/Servidor.csproj -- 5000

# Cliente:
dotnet run --project Cliente-Servidor-CSharp/Cliente/Cliente.csproj -- 127.0.0.1 5000
```

#### En Java:
```bash
# Servidor:
javac -encoding UTF-8 -d Cliente-Servidor-Java/bin Cliente-Servidor-Java/ServidorJava.java
java -cp Cliente-Servidor-Java/bin ServidorJava 5000

# Cliente:
javac -encoding UTF-8 -d Cliente-Servidor-Java/bin Cliente-Servidor-Java/ClienteJava.java
java -cp Cliente-Servidor-Java/bin ClienteJava 127.0.0.1 5000
```

---

## 🧪 Validación de los 3 Ejercicios del PDF

1. **Flujo Básico Eco Unidireccional / Bidireccional:**
   - Inicia el servidor y el cliente.
   - Escribe mensajes en la consola del cliente y observa cómo el servidor recibe el texto y retorna `ECO: <mensaje>`.
   - Escribe `QUIT` para desconectar limpiamente.

2. **Servidor Concurrente Multicliente:**
   - Deja el servidor encendido.
   - Abre 2 o más ventanas de cliente simultáneamente.
   - Envía mensajes desde ambos clientes al mismo tiempo; el servidor atenderá a ambos sin bloquear el puerto de escucha.

3. **Prueba de Interoperabilidad Heterogénea:**
   - **Prueba 1:** Servidor C# (`dotnet run`) <--> Cliente Java (`java ClienteJava`).
   - **Prueba 2:** Servidor Java (`java ServidorJava`) <--> Cliente C# (`dotnet run`).
   - Ambos se comunican íntegramente gracias al estándar de transporte TCP y codificación UTF-8.

---

## 🦈 Captura en Wireshark (Three-Way Handshake)

1. En Wireshark selecciona la interfaz **`Adapter for loopback traffic capture`**.
2. Aplica el filtro: `tcp.port == 5000`.
3. Inicia el servidor, conecta el cliente, envía un mensaje y escribe `QUIT`.
4. Observa en la captura:
   - **SYN** (Cliente -> Servidor)
   - **SYN, ACK** (Servidor -> Cliente)
   - **ACK** (Cliente -> Servidor)
   - **PSH, ACK** (Transferencia de datos del eco)
   - **FIN, ACK** (Cierre ordenado)
