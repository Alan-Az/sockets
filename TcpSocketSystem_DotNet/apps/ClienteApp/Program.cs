using TcpSocketSystem.Core.Domain.Puertos;
using TcpSocketSystem.Core.Infrastructure.Logging;
using TcpSocketSystem.Core.Infrastructure.Network;

Console.Title = "TecNM Sockets TCP/IP - Cliente (.NET 10)";

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("===================================================================");
Console.WriteLine("  TECNM - LABORATORIO DE SOCKETS TCP/IP (SCREAMING ARCHITECTURE)   ");
Console.WriteLine("  Módulo de Cliente de Red TCP (.NET 10 C#)                       ");
Console.WriteLine("===================================================================");
Console.ResetColor();

string host = "127.0.0.1";
int puerto = 5000;

if (args.Length >= 1) host = args[0];
if (args.Length >= 2 && int.TryParse(args[1], out int p)) puerto = p;

IServicioLogging logger = new LoggerAuditoria();

while (true)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Destino actual configurado: {host}:{puerto}");
    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine("1. Iniciar Sesión Interactiva de Eco (Envío continuo)");
    Console.WriteLine("2. Ejecutar Prueba Multicliente Simultáneo (5 clientes concurrentes)");
    Console.WriteLine("3. Prueba de Resiliencia / Timeout (Intento con servidor inexistente)");
    Console.WriteLine("4. Cambiar Dirección IP / Puerto");
    Console.WriteLine("5. Salir");
    Console.Write("Selecciona una opción [1-5]: ");
    Console.ResetColor();

    var opcion = Console.ReadLine()?.Trim();

    switch (opcion)
    {
        case "1":
            await EjecutarModoInteractivoAsync(host, puerto, logger);
            break;

        case "2":
            await EjecutarPruebaMulticlienteAsync(host, puerto, 5, logger);
            break;

        case "3":
            await EjecutarPruebaTimeoutAsync(logger);
            break;

        case "4":
            Console.Write("Ingresa la nueva IP o Host [ej. 127.0.0.1]: ");
            var nuevoHost = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(nuevoHost)) host = nuevoHost;

            Console.Write("Ingresa el nuevo Puerto [ej. 5000]: ");
            var nuevoPuertoStr = Console.ReadLine()?.Trim();
            if (int.TryParse(nuevoPuertoStr, out int np)) puerto = np;
            break;

        case "5":
            Console.WriteLine("Saliendo del cliente...");
            return;

        default:
            Console.WriteLine("Opción no válida.");
            break;
    }
}

static async Task EjecutarModoInteractivoAsync(string host, int puerto, IServicioLogging logger)
{
    using var cliente = new TcpClienteSocket(logger);
    var conectado = await cliente.ConectarConReintentosAsync(host, puerto, maxReintentos: 3);
    if (!conectado)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No se pudo conectar al servidor. Revisa si está encendido.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("--- MODO ECO INTERACTIVO ACTIVADO ---");
    Console.WriteLine("Escribe tu mensaje y presiona Enter para recibir el Eco.");
    Console.WriteLine("Escribe 'QUIT' o 'SALIR' para desconectarte ordenadamente.");
    Console.WriteLine("--------------------------------------");
    Console.ResetColor();

    while (cliente.EstaConectado)
    {
        Console.Write("\n[Tu mensaje] > ");
        var mensaje = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(mensaje)) continue;

        var respuesta = await cliente.EnviarYRecibirEcoAsync(mensaje);
        if (respuesta == null)
        {
            Console.WriteLine("El servidor ha cerrado la conexión.");
            break;
        }

        if (mensaje.Trim().Equals("QUIT", StringComparison.OrdinalIgnoreCase) ||
            mensaje.Trim().Equals("SALIR", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Sesión finalizada por el usuario.");
            cliente.Desconectar();
            break;
        }
    }
}

static async Task EjecutarPruebaMulticlienteAsync(string host, int puerto, int totalClientes, IServicioLogging logger)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"\n[PRUEBA MULTICLIENTE] Lanzando {totalClientes} clientes asíncronos concurrentes contra {host}:{puerto}...");
    Console.ResetColor();

    var tareas = new List<Task>();

    for (int i = 1; i <= totalClientes; i++)
    {
        int clienteId = i;
        tareas.Add(Task.Run(async () =>
        {
            using var cliente = new TcpClienteSocket(logger);
            bool conectado = await cliente.ConectarConReintentosAsync(host, puerto, maxReintentos: 1);
            if (conectado)
            {
                for (int m = 1; m <= 3; m++)
                {
                    string mensaje = $"Paquete_{m}_desde_ClienteDotNet_{clienteId}";
                    var eco = await cliente.EnviarYRecibirEcoAsync(mensaje);
                    await Task.Delay(200); // Pausa entre ráfagas
                }
                await cliente.EnviarYRecibirEcoAsync("QUIT");
                cliente.Desconectar();
            }
        }));
    }

    await Task.WhenAll(tareas);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[PRUEBA MULTICLIENTE FINALIZADA] Se enviaron todas las peticiones concurrentes con éxito.");
    Console.ResetColor();
}

static async Task EjecutarPruebaTimeoutAsync(IServicioLogging logger)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[PRUEBA DE TIMEOUT Y RESILIENCIA] Intentando conectar a un puerto inexistente (59999)...");
    Console.ResetColor();

    using var cliente = new TcpClienteSocket(logger);
    // Intenta conectar a un puerto cerrado deliberadamente para probar timeout y captura de errores
    await cliente.ConectarConReintentosAsync("127.0.0.1", 59999, maxReintentos: 2, delayMs: 500);
}
