using TcpSocketSystem.Core.Domain.Modelos;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.UseCases.GestionarSesiones;

/// <summary>
/// Implementación del caso de uso de gestión de sesiones concurrentes con auditoría.
/// </summary>
public sealed class GestionarSesionesHandler : IGestionarSesionesUseCase
{
    private readonly IRepositorioSesiones _repositorioSesiones;
    private readonly IServicioLogging _logger;

    public GestionarSesionesHandler(IRepositorioSesiones repositorioSesiones, IServicioLogging logger)
    {
        _repositorioSesiones = repositorioSesiones ?? throw new ArgumentNullException(nameof(repositorioSesiones));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public EstadoSesion RegistrarNuevaConexion(string sesionId, string direccionIp, int puerto)
    {
        var sesion = new EstadoSesion(sesionId, direccionIp, puerto);
        _repositorioSesiones.RegistrarSesion(sesion);

        _logger.Auditoria("CLIENTE_CONECTADO", $"{direccionIp}:{puerto}", $"Sesión ID: {sesionId} iniciada exitosamente.");
        _logger.Info($"[CONEXIÓN ABIERTA] Cliente {direccionIp}:{puerto} (ID: {sesionId}). Clientes activos: {_repositorioSesiones.ObtenerTotalSesionesActivas()}", nameof(GestionarSesionesHandler));

        return sesion;
    }

    public void RegistrarActividadMensaje(string sesionId)
    {
        var sesion = _repositorioSesiones.ObtenerPorId(sesionId);
        if (sesion != null && sesion.EstaActiva)
        {
            sesion.IncrementarMensajes();
            _repositorioSesiones.ActualizarSesion(sesion);
        }
    }

    public void RegistrarDesconexion(string sesionId, string motivo = "Desconexión normal")
    {
        var sesion = _repositorioSesiones.ObtenerPorId(sesionId);
        if (sesion != null)
        {
            sesion.CerrarSesion();
            _repositorioSesiones.FinalizarSesion(sesionId);

            _logger.Auditoria("CLIENTE_DESCONECTADO", $"{sesion.DireccionRemota}:{sesion.PuertoRemoto}", $"Sesión {sesionId} cerrada. Motivo: {motivo}. Mensajes procesados: {sesion.MensajesProcesados}");
            _logger.Info($"[CONEXIÓN CERRADA] Cliente {sesion.DireccionRemota}:{sesion.PuertoRemoto} (ID: {sesionId}). Clientes activos: {_repositorioSesiones.ObtenerTotalSesionesActivas()}", nameof(GestionarSesionesHandler));
        }
    }

    public int ObtenerConexionesActivas()
    {
        return _repositorioSesiones.ObtenerTotalSesionesActivas();
    }
}
