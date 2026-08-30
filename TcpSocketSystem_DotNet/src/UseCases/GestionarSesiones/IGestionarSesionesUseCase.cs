using TcpSocketSystem.Core.Domain.Modelos;

namespace TcpSocketSystem.Core.UseCases.GestionarSesiones;

/// <summary>
/// Caso de Uso: Gestionar el Ciclo de Vida y Concurrencia de Sesiones TCP.
/// </summary>
public interface IGestionarSesionesUseCase
{
    EstadoSesion RegistrarNuevaConexion(string sesionId, string direccionIp, int puerto);
    void RegistrarActividadMensaje(string sesionId);
    void RegistrarDesconexion(string sesionId, string motivo = "Desconexión normal");
    int ObtenerConexionesActivas();
}
