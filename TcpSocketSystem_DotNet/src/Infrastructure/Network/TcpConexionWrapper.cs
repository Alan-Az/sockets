using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Network;

/// <summary>
/// Envoltorio de Socket / TcpClient para desacoplar el protocolo y gestionar I/O asíncrono seguro.
/// </summary>
public sealed class TcpConexionWrapper : ISocketConexion
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private bool _disposed;

    public string Id { get; }
    public string DireccionRemota { get; }
    public int PuertoRemoto { get; }
    public bool EstaConectado => _tcpClient.Connected && !_disposed;

    public TcpConexionWrapper(TcpClient tcpClient)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        Id = Guid.NewGuid().ToString("N")[..8];

        if (tcpClient.Client.RemoteEndPoint is IPEndPoint endpoint)
        {
            DireccionRemota = endpoint.Address.ToString();
            PuertoRemoto = endpoint.Port;
        }
        else
        {
            DireccionRemota = "Desconocida";
            PuertoRemoto = 0;
        }

        _stream = tcpClient.GetStream();
        // UTF-8 sin BOM para garantizar compatibilidad universal con Java y otros clientes
        _reader = new StreamReader(_stream, new UTF8Encoding(false), leaveOpen: false);
        _writer = new StreamWriter(_stream, new UTF8Encoding(false))
        {
            AutoFlush = true,
            NewLine = "\n"
        };
    }

    public async Task<string?> LeerLineaAsync(CancellationToken cancellationToken = default)
    {
        if (!EstaConectado) return null;

        try
        {
            return await _reader.ReadLineAsync(cancellationToken);
        }
        catch (IOException)
        {
            // Ocurre en desconexiones abruptas (RST)
            return null;
        }
        catch (SocketException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task EnviarLineaAsync(string mensaje, CancellationToken cancellationToken = default)
    {
        if (!EstaConectado)
        {
            throw new InvalidOperationException("No se puede enviar datos a través de un socket cerrado.");
        }

        try
        {
            await _writer.WriteLineAsync(mensaje.AsMemory(), cancellationToken);
        }
        catch (IOException)
        {
            throw new SocketException((int)SocketError.ConnectionReset);
        }
    }

    public Task CerrarAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { _writer.Dispose(); } catch { }
        try { _reader.Dispose(); } catch { }
        try { _stream.Dispose(); } catch { }
        try { _tcpClient.Dispose(); } catch { }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try { await _writer.DisposeAsync(); } catch { }
        try { _reader.Dispose(); } catch { }
        try { await _stream.DisposeAsync(); } catch { }
        try { _tcpClient.Dispose(); } catch { }
    }
}
