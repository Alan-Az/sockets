# Manual de Ejecución y Guía de Pruebas: Sockets TCP/IP (.NET 10 & Java 21)

Este documento describe paso a paso la compilación, puesta en marcha y validación de los cuatro ejercicios requeridos por la práctica de laboratorio **Sockets TCP/IP (TecNM)** bajo **Screaming Architecture**.

---

## 1. Requisitos Previos

- **.NET SDK**: 10.0+ (Comando: `dotnet --version`)
- **Java JDK**: JDK 21 LTS (Comando: `java --version` / Soporte de Virtual Threads)

---

## 2. Compilación de los Proyectos

### 2.1 Compilación de la Solución .NET 10 (C#)
Desde la raíz del proyecto (`c:\Dev\Rriojas\Nueva carpeta`):

```bash
dotnet build TcpSocketSystem_DotNet/TcpSocketSystem.slnx
```

### 2.2 Compilación del Proyecto Java 21
```powershell
& "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\javac.exe" -encoding UTF-8 -d TcpSocketSystem_Java/bin (Get-ChildItem -Recurse -Filter *.java TcpSocketSystem_Java/src).FullName
```

---

## 3. Guía de Ejecución de los 4 Ejercicios

### Ejercicio 1: Flujo Básico de Eco Unidireccional / Bidireccional

**Objetivo:** Establecer una conexión socket TCP limpia, transmitir un mensaje de texto plano UTF-8, recibir el eco procesado por el caso de uso y cerrar la sesión de forma ordenada.

#### Paso a Paso (.NET 10):
1. **Abrir Terminal 1 (Servidor C#):**
   ```bash
   dotnet run --project TcpSocketSystem_DotNet/apps/ServidorApp/ServidorApp.csproj -- 5000
   ```
2. **Abrir Terminal 2 (Cliente C#):**
   ```bash
   dotnet run --project TcpSocketSystem_DotNet/apps/ClienteApp/ClienteApp.csproj -- 127.0.0.1 5000
   ```
3. Seleccionar la opción `1` (Modo Eco Interactivo).
4. Escribir mensajes en la consola. Observar la respuesta devuelta por el servidor con su timestamp y formato seguro.
5. Escribir `QUIT` para cerrar la conexión con intercambio TCP `FIN-ACK`.

---

### Ejercicio 2: Servidor Concurrente Multicliente

**Objetivo:** Demostrar la atención simultánea y no bloqueante de múltiples clientes concurrentes.
- En **.NET 10**: Atendido mediante `Task.Run` asíncrono con `async/await`.
- En **Java 21**: Atendido mediante **Virtual Threads** nativos (`Thread.ofVirtual().start(...)`).

#### Paso a Paso:
1. Con el servidor activo (sea C# o Java en puerto 5000).
2. En la terminal del cliente (C# o Java), seleccionar la opción `2` (*Prueba Multicliente Simultáneo*).
3. El cliente disparará 5 hilos/tareas concurrentes en ráfagas simultáneas.
4. Observar en la consola del servidor cómo cada hilo recibe un `SesionId` único y se procesan las peticiones sin que ningún cliente bloquee a los demás.

---

### Ejercicio 3: Prueba Heterogénea (Interoperabilidad Cruzada)

**Objetivo:** Validar la independencia de plataforma y lenguaje demostrando que el protocolo TCP UTF-8 delimitado por saltos de línea (`\n`) permite interoperabilidad total.

#### Matriz de Interoperabilidad:

| Escenario | Servidor | Cliente | Comando Servidor | Comando Cliente |
| :--- | :--- | :--- | :--- | :--- |
| **A** | **.NET 10 (C#)** | **Java 21** | `dotnet run --project TcpSocketSystem_DotNet/apps/ServidorApp/ServidorApp.csproj -- 5000` | `& "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\java.exe" -cp TcpSocketSystem_Java/bin com.tecnm.sockets.app.ClienteApp 127.0.0.1 5000` |
| **B** | **Java 21 (VT)** | **.NET 10 (C#)** | `& "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\java.exe" -cp TcpSocketSystem_Java/bin com.tecnm.sockets.app.ServidorApp 5000` | `dotnet run --project TcpSocketSystem_DotNet/apps/ClienteApp/ClienteApp.csproj -- 127.0.0.1 5000` |

---

### Ejercicio 4: Manejo de Errores, Excepciones y Resiliencia

**Objetivo:** Verificar la estabilidad del sistema ante condiciones anómalas de red según los estándares de seguridad y hardening.

#### 1. Detección de Puerto Ocupado (`AddressAlreadyInUse` / `BindException`):
- Iniciar un servidor en el puerto 5000.
- En otra terminal, intentar iniciar un segundo servidor en el mismo puerto 5000.
- **Resultado esperado:** El segundo servidor captura la excepción de forma controlada, registra el error en los logs de auditoría y finaliza sin corromper el primer proceso.

#### 2. Timeout y Reintentos del Cliente:
- Ejecutar el cliente con destino a un puerto cerrado (ej. opción `3` del menú interactivo hacia el puerto `59999`).
- **Resultado esperado:** El cliente realiza reintentos exponenciales/pausados y expone un mensaje limpio al usuario sin volcar trazas crudas (*stack trace*).

#### 3. Desconexión Abrupta (Simulación de caída de enlace):
- Conectar un cliente interactivo al servidor.
- Cerrar la ventana de la terminal del cliente intempestivamente o presionar `Ctrl+C`.
- **Resultado esperado:** El servidor detecta la ruptura del stream (`IOException` / lectura de `null`), ejecuta el caso de uso `GestionarSesiones`, limpia la memoria de la sesión, registra la auditoría y continúa escuchando nuevas conexiones.

---

## 4. Estructura de Bitácoras y Auditoría (Audit Trail ISO 27001)

Todos los eventos de red generan registros estructurados en la carpeta `logs/`:
- `auditoria_sockets_YYYYMMDD.log` (.NET)
- `auditoria_sockets_java_YYYYMMDD.log` (Java)

Ejemplo de entrada de auditoría:
```text
[2026-08-30 22:05:14.120 UTC] [AUDIT] [SeguridadAuditoria] [AUDITORIA] Evento: CLIENTE_CONECTADO | Sujeto: 127.0.0.1:54321 | Detalle: Sesión ID: a1b2c3d4 iniciada exitosamente.
[2026-08-30 22:05:15.002 UTC] [INFO] [ProcesarEcoHandler] Procesando mensaje [e5f6g7h8] de Cliente-a1b2c3d4: "Hola Redes"
[2026-08-30 22:05:18.450 UTC] [AUDIT] [SeguridadAuditoria] [AUDITORIA] Evento: CLIENTE_DESCONECTADO | Sujeto: 127.0.0.1:54321 | Detalle: Sesión a1b2c3d4 cerrada. Motivo: Desconexión normal. Mensajes procesados: 1
```
