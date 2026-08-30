using System.Net.Sockets;
using System.Text;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Network;

/// <summary>
/// Cliente TCP con políticas de reintento, timeout configurable, sanitización y cierre limpio.
/// </summary>
public sealed class TcpClienteSocket : IDisposable
{
    private readonly IServicioLogging _logger;
    private TcpClient? _cliente;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _disposed;

    public bool EstaConectado => _cliente?.Connected == true && !_disposed;

    public TcpClienteSocket(IServicioLogging logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ConectarConReintentosAsync(string host, int puerto, int maxReintentos = 3, int delayMs = 1500, CancellationToken cancellationToken = default)
    {
        for (int intento = 1; intento <= maxReintentos; intento++)
        {
            try
            {
                _logger.Info($"Intentando conectar a {host}:{puerto} (Intento {intento}/{maxReintentos})...", nameof(TcpClienteSocket));
                
                _cliente = new TcpClient
                {
                    NoDelay = true,
                    ReceiveTimeout = 10000,
                    SendTimeout = 10000
                };

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                await _cliente.ConnectAsync(host, puerto, cts.Token);
                
                _stream = _cliente.GetStream();
                _reader = new StreamReader(_stream, new UTF8Encoding(false), leaveOpen: false);
                _writer = new StreamWriter(_stream, new UTF8Encoding(false))
                {
                    AutoFlush = true,
                    NewLine = "\n"
                };

                _logger.Info($"¡Conectado exitosamente al servidor {host}:{puerto}!", nameof(TcpClienteSocket));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Fallo al conectar en el intento {intento}: {ex.Message}", nameof(TcpClienteSocket));
                Desconectar();

                if (intento < maxReintentos)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
        }

        _logger.Error($"No fue posible establecer conexión con {host}:{puerto} tras {maxReintentos} intentos.", origen: nameof(TcpClienteSocket));
        return false;
    }

    public async Task<string?> EnviarYRecibirEcoAsync(string mensaje, CancellationToken cancellationToken = default)
    {
        if (!EstaConectado || _writer == null || _reader == null)
        {
            throw new InvalidOperationException("El cliente no está conectado a ningún servidor.");
        }

        try
        {
            _logger.Info($"[ENVIANDO] -> \"{mensaje}\"", nameof(TcpClienteSocket));
            await _writer.WriteLineAsync(mensaje.AsMemory(), cancellationToken);

            var respuesta = await _reader.ReadLineAsync(cancellationToken);
            _logger.Info($"[RECIBIDO] <- \"{respuesta}\"", nameof(TcpClienteSocket));
            return respuesta;
        }
        catch (IOException ex)
        {
            _logger.Error("Conexión interrumpida durante la transmisión de datos.", ex, nameof(TcpClienteSocket));
            Desconectar();
            return null;
        }
    }

    public void Desconectar()
    {
        try { _writer?.Dispose(); } catch { }
        try { _reader?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _cliente?.Close(); } catch { }
        try { _cliente?.Dispose(); } catch { }

        _writer = null;
        _reader = null;
        _stream = null;
        _cliente = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Desconectar();
    }
}
