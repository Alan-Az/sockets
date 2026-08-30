namespace TcpSocketSystem.Core.Domain.Puertos;

/// <summary>
/// Puerto para el servicio de red que abstrae el inicio y parada del servidor de sockets TCP.
/// </summary>
public interface IServicioRed
{
    bool EstaEscuchando { get; }
    int PuertoAsignado { get; }

    Task IniciarServidorAsync(
        int puerto,
        Func<ISocketConexion, CancellationToken, Task> onClienteConectado,
        CancellationToken cancellationToken);

    Task DetenerServidorAsync();
}
