using System.Text;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Logging;

/// <summary>
/// Servicio de logging y auditoría estructurado conforme a estándares de trazabilidad y seguridad (ISO 27001).
/// </summary>
public sealed class LoggerAuditoria : IServicioLogging
{
    private readonly string _rutaArchivoLogs;
    private readonly object _lockObj = new();

    public LoggerAuditoria(string? carpetaLogs = null)
    {
        var carpeta = carpetaLogs ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(carpeta);
        _rutaArchivoLogs = Path.Combine(carpeta, $"auditoria_sockets_{DateTime.UtcNow:yyyyMMdd}.log");
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
        // En cumplimiento con Clean Code y Seguridad: el mensaje expuesto es controlado y el stack trace se preserva únicamente en logs internos.
        var detalle = excepcion != null ? $"{mensaje} | Excepción: {excepcion.GetType().Name} - {excepcion.Message}" : mensaje;
        EscribirLog("ERROR", detalle, origen, ConsoleColor.Red);
    }

    public void Auditoria(string evento, string ipOCliente, string detalles)
    {
        var entrada = $"[AUDITORIA] Evento: {evento} | Sujeto: {ipOCliente} | Detalle: {detalles}";
        EscribirLog("AUDIT", entrada, "SeguridadAuditoria", ConsoleColor.Green);
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

            try
            {
                File.AppendAllText(_rutaArchivoLogs, lineaCompleta + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Manejo silencioso de error de escritura para no interrumpir el flujo principal
            }
        }
    }
}
