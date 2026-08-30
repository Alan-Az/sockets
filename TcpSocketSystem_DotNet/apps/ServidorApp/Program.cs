using TcpSocketSystem.Core.Domain.Puertos;
using TcpSocketSystem.Core.Infrastructure.Logging;
using TcpSocketSystem.Core.Infrastructure.Network;
using TcpSocketSystem.Core.Infrastructure.Repositories;
using TcpSocketSystem.Core.UseCases.GestionarSesiones;
using TcpSocketSystem.Core.UseCases.ProcesarEco;

Console.Title = "TecNM Sockets TCP/IP - Servidor Multihilo (.NET 10)";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("===================================================================");
Console.WriteLine("  TECNM - LABORATORIO DE SOCKETS TCP/IP (SCREAMING ARCHITECTURE)   ");
Console.WriteLine("  Módulo de Servidor Concurrente Multicliente (.NET 10 C#)        ");
Console.WriteLine("===================================================================");
Console.ResetColor();

// Configuración de puerto (vía argumentos de consola o por defecto 5000)
int puerto = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int puertoArg))
{
    puerto = puertoArg;
}

// 1. Inyección de Dependencias manual (Composición de capas de Screaming Architecture)
IServicioLogging logger = new LoggerAuditoria();
IRepositorioSesiones repositorioSesiones = new RepositorioSesionesMemoria();
IGestionarSesionesUseCase gestionarSesionesUseCase = new GestionarSesionesHandler(repositorioSesiones, logger);
IProcesarEcoUseCase procesarEcoUseCase = new ProcesarEcoHandler(logger, "Servidor-CSharp");
IServicioRed servicioRed = new TcpServidorSocket(logger);

using var cts = new CancellationTokenSource();

// Manejo de apagado ordenado (Graceful Shutdown)
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    logger.Warning("Señal de terminación (Ctrl+C) recibida. Apagando servidor...", "Main");
    cts.Cancel();
};

// 2. Definición del manejador de clientes conectados
async Task AtenderClienteAsync(ISocketConexion conexion, CancellationToken cancellationToken)
{
    var sesion = gestionarSesionesUseCase.RegistrarNuevaConexion(
        conexion.Id, 
        conexion.DireccionRemota, 
        conexion.PuertoRemoto);

    try
    {
        // Enviar banner de bienvenida con interoperabilidad
        await conexion.EnviarLineaAsync($"BIENVENIDO|Servidor=.NET 10|SesionId={sesion.SesionId}|Status=OK", cancellationToken);

        while (conexion.EstaConectado && !cancellationToken.IsCancellationRequested)
        {
            var lineaRecibida = await conexion.LeerLineaAsync(cancellationToken);
            
            // Si la línea es null, el cliente cerró el socket (FIN o RST)
            if (lineaRecibida == null)
            {
                gestionarSesionesUseCase.RegistrarDesconexion(sesion.SesionId, "Cierre detectado por fin de stream");
                break;
            }

            var textoLimpio = lineaRecibida.Trim();
            if (string.Equals(textoLimpio, "QUIT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(textoLimpio, "SALIR", StringComparison.OrdinalIgnoreCase))
            {
                await conexion.EnviarLineaAsync("ADIOS|SesionFinalizada", cancellationToken);
                gestionarSesionesUseCase.RegistrarDesconexion(sesion.SesionId, "Solicitud de desconexión del cliente (QUIT)");
                break;
            }

            // Ejecutar caso de uso de Procesar Eco
            gestionarSesionesUseCase.RegistrarActividadMensaje(sesion.SesionId);
            var respuestaEco = await procesarEcoUseCase.EjecutarAsync(textoLimpio, $"Cliente-{conexion.Id}", cancellationToken);

            // Responder trama
            await conexion.EnviarLineaAsync(respuestaEco.SerializarTrama(), cancellationToken);
        }
    }
    catch (Exception ex)
    {
        logger.Error($"Excepción durante la atención del cliente {conexion.DireccionRemota}", ex, "AtenderCliente");
        gestionarSesionesUseCase.RegistrarDesconexion(sesion.SesionId, $"Fallo de conexión: {ex.Message}");
    }
    finally
    {
        await conexion.CerrarAsync();
    }
}

try
{
    logger.Info($"Iniciando listener en el puerto TCP {puerto}...", "Main");
    await servicioRed.IniciarServidorAsync(puerto, AtenderClienteAsync, cts.Token);
}
catch (Exception ex)
{
    logger.Error($"No fue posible ejecutar el servidor: {ex.Message}", ex, "Main");
}
finally
{
    logger.Info("Proceso del Servidor finalizado.", "Main");
}
