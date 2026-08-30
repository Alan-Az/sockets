using System.Text.RegularExpressions;

namespace TcpSocketSystem.Core.Domain.Modelos;

/// <summary>
/// Representa el modelo inmutable de un mensaje que transita por los Sockets TCP.
/// Aplica reglas de validación y sanitización según los estándares de seguridad.
/// </summary>
public sealed record MensajeEco
{
    private const int MaxLongitud = 4096;
    private static readonly Regex RegexSanitizacion = new(@"[^\w\s.,!?:;\-_@#$/()\\[\]]", RegexOptions.Compiled);

    public string Id { get; init; }
    public string Contenido { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string Emisor { get; init; }
    public bool EsRespuesta { get; init; }

    public MensajeEco(string contenido, string emisor = "Cliente", bool esRespuesta = false, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(contenido))
        {
            throw new ArgumentException("El contenido del mensaje no puede estar vacío.", nameof(contenido));
        }

        if (contenido.Length > MaxLongitud)
        {
            contenido = contenido[..MaxLongitud];
        }

        // Sanitización para prevenir inyecciones o caracteres de control maliciosos
        Id = id ?? Guid.NewGuid().ToString("N")[..8];
        Contenido = RegexSanitizacion.Replace(contenido.Trim(), "");
        TimestampUtc = DateTime.UtcNow;
        Emisor = string.IsNullOrWhiteSpace(emisor) ? "Desconocido" : emisor.Trim();
        EsRespuesta = esRespuesta;
    }

    /// <summary>
    /// Serializa el mensaje a una trama limpia delimitada por fin de línea (\n)
    /// </summary>
    public string SerializarTrama()
    {
        return $"[{TimestampUtc:yyyy-MM-dd HH:mm:ss}] [{Emisor}] {(EsRespuesta ? "REPLY" : "MSG")}: {Contenido}";
    }

    public static MensajeEco CrearRespuestaEco(MensajeEco original, string servidorNombre = "Servidor-CSharp")
    {
        return new MensajeEco(
            contenido: original.Contenido,
            emisor: servidorNombre,
            esRespuesta: true,
            id: original.Id
        );
    }
}
