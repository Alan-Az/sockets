using TcpSocketSystem.Core.Domain.Modelos;

namespace TcpSocketSystem.Core.Domain.Puertos;

/// <summary>
/// Contrato para la persistencia en memoria y consulta de sesiones de sockets activas.
/// </summary>
public interface IRepositorioSesiones
{
    void RegistrarSesion(EstadoSesion sesion);
    void ActualizarSesion(EstadoSesion sesion);
    void FinalizarSesion(string sesionId);
    EstadoSesion? ObtenerPorId(string sesionId);
    IReadOnlyCollection<EstadoSesion> ObtenerSesionesActivas();
    int ObtenerTotalSesionesActivas();
}
