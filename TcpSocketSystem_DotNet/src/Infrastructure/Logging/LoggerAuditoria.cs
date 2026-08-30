using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Logging;

/// <summary>
/// Servicio de logging en consola para visualización en tiempo real sin persistencia en disco.
/// </summary>
public sealed class LoggerAuditoria : IServicioLogging
{
    private readonly object _lockObj = new();

    public LoggerAuditoria(string? carpetaLogs = null)
    {
        // No genera archivos en disco para mantener el repositorio limpio
    }

    public void Info(string mensaje, string? origen = null)
    {
        EscribirLog("INFO", mensaje, origen, ConsoleColor.Cyan);
    }

    public void Warning(string mensaje, string? origen = null)
    {
        EscribirLog("WARN", mensaje, origen, ConsoleColor.Yellow);
    }

    public void Error(string mensaje, Exception? excepcion = null, string? origen = null)
    {
        var detalle = excepcion != null ? $"{mensaje} | Excepción: {excepcion.GetType().Name} - {excepcion.Message}" : mensaje;
        EscribirLog("ERROR", detalle, origen, ConsoleColor.Red);
    }

    public void Auditoria(string evento, string ipOCliente, string detalles)
    {
        var entrada = $"[EVENTO] {evento} | Sujeto: {ipOCliente} | {detalles}";
        EscribirLog("AUDIT", entrada, "Seguridad", ConsoleColor.Green);
    }

    private void EscribirLog(string nivel, string mensaje, string? origen, ConsoleColor colorConsola)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var origenStr = string.IsNullOrWhiteSpace(origen) ? "General" : origen;
        var lineaCompleta = $"[{timestamp} UTC] [{nivel}] [{origenStr}] {mensaje}";

        lock (_lockObj)
        {
            var colorAnterior = Console.ForegroundColor;
            Console.ForegroundColor = colorConsola;
            Console.WriteLine(lineaCompleta);
            Console.ForegroundColor = colorAnterior;
        }
    }
}
