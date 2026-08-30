namespace TcpSocketSystem.Core.Domain.Puertos;

/// <summary>
/// Contrato para el subsistema de bitácoras, diagnóstico y auditoría (Audit Trail)
/// acorde a normativas de seguridad (ISO 27001).
/// </summary>
public interface IServicioLogging
{
    void Info(string mensaje, string? origen = null);
    void Warning(string mensaje, string? origen = null);
    void Error(string mensaje, Exception? excepcion = null, string? origen = null);
    void Auditoria(string evento, string ipOCliente, string detalles);
}
