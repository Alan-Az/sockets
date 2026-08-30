using TcpSocketSystem.Core.Domain.Modelos;

namespace TcpSocketSystem.Core.UseCases.ProcesarEco;

/// <summary>
/// Caso de Uso: Procesar Eco de Mensajes TCP.
/// Recibe una solicitud, valida/sanitiza el mensaje y construye la respuesta adecuada.
/// </summary>
public interface IProcesarEcoUseCase
{
    Task<MensajeEco> EjecutarAsync(string contenidoRaw, string emisor, CancellationToken cancellationToken = default);
}
