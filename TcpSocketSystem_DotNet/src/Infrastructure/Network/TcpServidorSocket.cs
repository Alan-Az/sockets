using System.Net;
using System.Net.Sockets;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Network;

/// <summary>
/// Servidor TCP asíncrono y multihilo basado en TcpListener y Task.Run.
/// Gestiona concurrencia, captura puertos ocupados y previene bloqueos de cola.
/// </summary>
public sealed class TcpServidorSocket : IServicioRed
{
    private readonly IServicioLogging _logger;
    private TcpListener? _listener;
    private bool _estaEscuchando;
    private int _puertoAsignado;

    public bool EstaEscuchando => _estaEscuchando;
    public int PuertoAsignado => _puertoAsignado;

    public TcpServidorSocket(IServicioLogging logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task IniciarServidorAsync(
        int puerto,
        Func<ISocketConexion, CancellationToken, Task> onClienteConectado,
        CancellationToken cancellationToken)
    {
        if (_estaEscuchando)
        {
            throw new InvalidOperationException("El servidor de sockets ya se encuentra en ejecución.");
        }

        try
        {
            _puertoAsignado = puerto;
            _listener = new TcpListener(IPAddress.Any, puerto);
            _listener.Start();
            _estaEscuchando = true;

            _logger.Info($"Servidor TCP iniciado exitosamente y escuchando en 0.0.0.0:{puerto}", nameof(TcpServidorSocket));
            _logger.Auditoria("SERVIDOR_INICIADO", $"0.0.0.0:{puerto}", "Servidor listo para aceptar conexiones entrantes.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Aceptación asíncrona no bloqueante
                    var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                    
                    // Configuración de sockets para optimizar rendimiento y resiliencia
                    tcpClient.NoDelay = true; // Deshabilita algoritmo de Nagle para baja latencia
                    tcpClient.ReceiveTimeout = 30000; // 30 segundos timeout
                    tcpClient.SendTimeout = 10000;

                    var conexion = new TcpConexionWrapper(tcpClient);

                    // Despachar en hilo de trabajo independiente (Multicliente sin bloqueo)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await onClienteConectado(conexion, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error no controlado en la atención del cliente {conexion.DireccionRemota}:{conexion.PuertoRemoto}", ex, nameof(TcpServidorSocket));
                        }
                        finally
                        {
                            await conexion.DisposeAsync();
                        }
                    }, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException) when (!_estaEscuchando)
                {
                    // El listener se detuvo deliberadamente
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error al aceptar una nueva conexión entrante.", ex, nameof(TcpServidorSocket));
                }
            }
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            _logger.Error($"No se pudo iniciar el servidor. El puerto {puerto} ya se encuentra ocupado por otro proceso.", ex, nameof(TcpServidorSocket));
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Fallo crítico al iniciar el servidor en el puerto {puerto}.", ex, nameof(TcpServidorSocket));
            throw;
        }
        finally
        {
            await DetenerServidorAsync();
        }
    }

    public Task DetenerServidorAsync()
    {
        if (!_estaEscuchando) return Task.CompletedTask;

        _estaEscuchando = false;
        try
        {
            _listener?.Stop();
            _logger.Info("Servidor TCP detenido de forma ordenada.", nameof(TcpServidorSocket));
            _logger.Auditoria("SERVIDOR_DETENIDO", $"0.0.0.0:{_puertoAsignado}", "Servidor finalizó la escucha.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error al detener el listener de sockets.", ex, nameof(TcpServidorSocket));
        }

        return Task.CompletedTask;
    }
}
