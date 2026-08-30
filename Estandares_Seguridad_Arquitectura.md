# Guía Definitiva de Estándares: Código Limpio, Seguridad y Arquitectura

Este documento establece los lineamientos obligatorios para garantizar que el desarrollo de software sea seguro, mantenible, escalable y auditable. Está diseñado para abarcar desde la estructuración del código hasta el despliegue en entornos de contenedores y la gestión de múltiples motores de bases de datos.

## 1. Arquitectura y Estructura del Proyecto
* **Separación de Responsabilidades (Clean Architecture / MVC):** El código debe dividirse en capas lógicas (presentación, lógica de negocio, acceso a datos). La interfaz de usuario (ej. Vue 3 o Razor Pages) nunca debe comunicarse directamente con la base de datos sin pasar por un controlador o API.
* **Modularidad y Microservicios:** Diseñar componentes de software que hagan una sola cosa y la hagan bien (Principio de Responsabilidad Única). Si el sistema escala, la arquitectura debe permitir separar módulos en servicios independientes orquestados.
* **Inyección de Dependencias:** Evitar instanciar clases directamente dentro de otras. Utilizar contenedores de inyección (comunes en ecosistemas como .NET o Node.js) para facilitar el testing y el desacoplamiento.
* **Control de Versiones Estricto (Git Flow):** Prohibido hacer commits directos a `main` o `master`. Todo cambio debe ir en una rama de característica (`feature/nombre-tarea`), ser revisado (Pull Request) y luego integrado.

## 2. Prácticas de Código Limpio (Clean Code)
* **Nomenclatura Significativa:** 
  * Clases y Modelos: `PascalCase` (ej. `UsuarioService`).
  * Variables y Métodos: `camelCase` (ej. `obtenerDatosUsuario()`) o `snake_case` (según el lenguaje, ej. Python).
  * No usar abreviaturas confusas (`usr`, `dt`, `x`). Las variables deben describir exactamente qué contienen.
* **KISS (Keep It Simple, Stupid) y DRY (Don't Repeat Yourself):** Evitar la duplicación de código. Si un bloque lógico o de conexión se usa más de dos veces, debe refactorizarse en una función, helper o microservicio independiente.
* **Manejo de Errores Silencioso hacia el Cliente:** Las excepciones deben ser capturadas (Try/Catch). El usuario final nunca debe ver un "Stack Trace" (trazas del sistema que revelan tecnologías o rutas del servidor). Se deben devolver mensajes genéricos de error (ej. "Error interno del servidor, contacte a soporte").

## 3. Seguridad y Prevención de Filtraciones (Hardening)
* **Gestión de Secretos:** ESTRICTAMENTE PROHIBIDO colocar contraseñas, cadenas de conexión o API Keys en el código fuente. Todo secreto debe inyectarse a través de variables de entorno (`.env`) o gestores de secretos, asegurando su compatibilidad en despliegues como Docker Compose.
* **Sanitización de Entradas (Input Validation):** Toda entrada del usuario (formularios, parámetros de URL, headers) debe ser validada en el backend antes de ser procesada para evitar Inyección SQL o ataques XSS.
* **Autenticación y Autorización:**
  * Uso de tokens seguros con tiempo de expiración (ej. JWT).
  * Control de acceso basado en roles (RBAC). Validar siempre que el usuario que solicita un recurso tiene los permisos necesarios para verlo o modificarlo (gestión de accesos).
* **Políticas CORS y Rate Limiting:** Limitar desde qué dominios se puede consumir la API (CORS) y restringir el número de peticiones por minuto/IP (Rate Limiting) para evitar ataques de fuerza bruta o denegación de servicio (DDoS).

## 4. Diseño y Gestión de Bases de Datos
* **Normalización y Optimización:** Diseñar esquemas relacionales utilizando hasta la 3ra Forma Normal (3NF) para evitar redundancia de datos. Evaluar constantemente las estructuras de índices (Clustered/Non-Clustered) para optimizar consultas de lectura rápida, ya sea en SQL Server, PostgreSQL, MySQL o MariaDB.
* **Abstracción del Motor (ORM vs Scripts Puros):** Dependiendo de la complejidad, utilizar ORMs (como Entity Framework o Prisma) para la seguridad base, o Procedimientos Almacenados parametrizados si se requiere extremo rendimiento, asegurando blindaje contra inyecciones SQL.
* **Control de Cambios en la BD (Migraciones):** Cualquier alteración a la estructura de la base de datos debe estar versionada a través de scripts de migración. No modificar la base de datos en producción directamente.
* **Borrado Lógico (Soft Delete):** En sistemas auditables, NUNCA usar comandos `DELETE` directos en tablas transaccionales. Emplear banderas de estado (ej. `activo = 0`, `deleted_at = timestamp`) y mover archivos obsoletos a carpetas de almacenamiento en frío (Cold Storage) con sufijos de fecha para control de versiones.

## 5. Auditoría, Logs y Cumplimiento (Compliance)
* **Trazabilidad de Datos (Audit Trails):** Toda tabla crítica debe tener campos obligatorios que respondan a: ¿Quién lo hizo? ¿Cuándo lo hizo? (`created_at`, `created_by`, `updated_at`, `updated_by`).
* **Bitácora de Eventos (Logs Centralizados):** Registrar todos los eventos de seguridad (inicios de sesión fallidos, escalada de privilegios, cambios en configuraciones del sistema) en archivos rotativos o sistemas externos. Esto es fundamental para cumplir con los checklists de normativas de seguridad de la información (ej. cláusulas de gestión de acceso de ISO 27001).
* **Integridad de los Datos:** Aplicar transacciones de base de datos (BEGIN TRAN / COMMIT / ROLLBACK) cuando se afecten múltiples tablas simultáneamente, asegurando los principios ACID para que no queden datos inconsistentes si falla un proceso a la mitad.

## 6. Documentación y Estandarización de Entornos
* **README Obligatorio:** Todo repositorio debe tener un `README.md` detallando las dependencias, la arquitectura, comandos para levantar el entorno (ej. `docker-compose up -d`) y las rutas principales.
* **Estandarización del Formato:** Uso obligatorio de herramientas de formateo de código (Prettier, EditorConfig, Black) y linters (ESLint, SonarQube) integradas en el pipeline para bloquear subidas de código que no cumplan el estándar (código "sucio").
