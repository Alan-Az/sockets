namespace TcpSocketSystem.Core.Domain.Puertos;

/// <summary>
/// Abstracción de un canal de comunicación Socket conectado.
/// Desacopla la lógica de aplicación del framework de red concreto.
/// </summary>
public interface ISocketConexion : IAsyncDisposable, IDisposable
{
    string Id { get; }
    string DireccionRemota { get; }
    int PuertoRemoto { get; }
    bool EstaConectado { get; }

    Task<string?> LeerLineaAsync(CancellationToken cancellationToken = default);
    Task EnviarLineaAsync(string mensaje, CancellationToken cancellationToken = default);
    Task CerrarAsync();
}
