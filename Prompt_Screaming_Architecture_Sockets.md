# Prompt Maestro para Desarrollo de Práctica de Sockets TCP/IP (TecNM) bajo Screaming Architecture

Actúa como un Arquitecto de Software Senior y Desarrollador Full-Stack experto en .NET 10 (C#) y Java (LTS con Virtual Threads). Tu objetivo es guiar en la construcción de una solución de software profesional, robusta y limpia que cumpla al 100% con los requerimientos de la **Práctica de Laboratorio: Sockets TCP/IP del TecNM**, aplicando estrictamente el principio de **Screaming Architecture** (Arquitectura Grita).

---

## 1. Directrices de Arquitectura: Screaming Architecture (Arquitectura Grita)
La arquitectura debe "gritar" la intención del negocio/sistema en la estructura de directorios, desacoplando por completo los frameworks y mecanismos de transporte de las reglas de dominio. 

La organización del proyecto debe seguir esta estructura exacta por cada lenguaje (.NET 10 y Java):

```text
/TcpSocketSystem
│
├── /src
│   ├── /Domain                 <-- Reglas de negocio puras, entidades de mensajes, interfaces de puertos
│   │   ├── Modelos             <-- Estructuras de datos (MensajeEco, PaqueteDatos)
│   │   └── Puertos             <-- Interfaces de contratos (IServicioRed, IRepositorioSesiones)
│   │
│   ├── /UseCases               <-- Casos de uso / Interactor (Lógica de aplicación)
│   │   ├── ProcesarEco         <-- Lógica para manejar eco unidireccional/bidireccional
│   │   └── GestionarSesiones   <-- Lógica para concurrencia y multihilo
│   │
│   └── /Infrastructure         <-- Detalles técnicos (Frameworks, Sockets, I/O, Red)
│       ├── /Network            <-- Implementación de Sockets TCP (TcpListener, ServerSocket, Threads)
│       └── /Logging            <-- Manejo de bitácoras y diagnóstico
│
└── /Tests                      <-- Pruebas unitarias e integración
```

---

## 2. Requerimientos Técnicos a Implementar

### Módulo A: .NET 10 (C#)
- **Namespaces recomendados**: `System.Net.Sockets`, `System.Net`, `System.Text`, `System.Threading.Tasks`.
- **Concurrencia**: Uso de tareas asíncronas con `async/await` y `Task.Run` para la atención multihilo de clientes concurrentes sin bloqueo de puerto.
- **Interoperabilidad**: Capacidad de actuar como Servidor robusto para Clientes Java y viceversa.

### Módulo B: Java (Última versión LTS)
- **Paquetes**: `java.net`, `java.io`.
- **Concurrencia**: Uso nativo de **Virtual Threads** (`Thread.ofVirtual().start(...)`) para soportar alta concurrencia de clientes de forma ligera.
- **Interoperabilidad**: Compatible con los paquetes de red estándar y comunicación con C#.

---

## 3. Ejercicios Obligatorios del Programa
1. **Flujo Básico Eco Unidireccional/Bidireccional**: Conexión limpia, envío con confirmación y cierre ordenado de sockets.
2. **Servidor Concurrente Multicliente**: Gestión simultánea de 2 o más clientes sin bloqueo de cola.
3. **Prueba Heterogénea (Interoperabilidad)**: Servidor en C# atendiendo Cliente Java, y Servidor en Java atendiendo Cliente C#.
4. **Manejo de Errores y Excepciones**: Detección de desconexiones abruptas, reintentos, tiempos de espera (`Timeouts`) y puertos ocupados.

---

## 4. Estructura de la Respuesta Esperada

Por favor, genera el código fuente completo, modularizado y listo para compilar estructurado bajo **Screaming Architecture**, incluyendo:

1. **Estructura de Directorios detallada** para ambos lenguajes.
2. **Implementación de Clases y Casos de Uso** respetando los principios SOLID.
3. **Código del Servidor y Cliente** para C# y Java.
4. **Guía paso a paso para validación local, remota y captura con Wireshark** (identificando el *Three-Way Handshake*: SYN, SYN-ACK, ACK).
