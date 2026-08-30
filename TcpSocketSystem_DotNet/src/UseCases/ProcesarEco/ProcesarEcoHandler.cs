using TcpSocketSystem.Core.Domain.Modelos;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.UseCases.ProcesarEco;

/// <summary>
/// Implementación del Caso de Uso para procesar el eco de red.
/// </summary>
public sealed class ProcesarEcoHandler : IProcesarEcoUseCase
{
    private readonly IServicioLogging _logger;
    private readonly string _nombreServidor;

    public ProcesarEcoHandler(IServicioLogging logger, string nombreServidor = "Servidor-DotNet10")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nombreServidor = nombreServidor;
    }

    public Task<MensajeEco> EjecutarAsync(string contenidoRaw, string emisor, CancellationToken cancellationToken = default)
    {
        // Validar y sanitizar a través de la entidad de dominio
        var mensajeCliente = new MensajeEco(contenidoRaw, emisor);

        _logger.Info($"Procesando mensaje [{mensajeCliente.Id}] de {mensajeCliente.Emisor}: \"{mensajeCliente.Contenido}\"", nameof(ProcesarEcoHandler));

        // Regla de negocio del eco: Reenviar el contenido agregando marca de respuesta
        var respuestaEco = MensajeEco.CrearRespuestaEco(mensajeCliente, _nombreServidor);

        return Task.FromResult(respuestaEco);
    }
}
