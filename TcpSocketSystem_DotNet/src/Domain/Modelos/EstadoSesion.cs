namespace TcpSocketSystem.Core.Domain.Modelos;

/// <summary>
/// Representa el estado de una conexión o sesión TCP activa.
/// Cumple con los requerimientos de auditoría y trazabilidad.
/// </summary>
public sealed class EstadoSesion
{
    public string SesionId { get; }
    public string DireccionRemota { get; }
    public int PuertoRemoto { get; }
    public DateTime ConectadoEnUtc { get; }
    public DateTime? DesconectadoEnUtc { get; private set; }
    public long MensajesProcesados { get; private set; }
    public bool EstaActiva => DesconectadoEnUtc == null;

    public EstadoSesion(string sesionId, string direccionRemota, int puertoRemoto)
    {
        SesionId = sesionId;
        DireccionRemota = direccionRemota;
        PuertoRemoto = puertoRemoto;
        ConectadoEnUtc = DateTime.UtcNow;
        MensajesProcesados = 0;
    }

    public void IncrementarMensajes()
    {
        MensajesProcesados++;
    }

    public void CerrarSesion()
    {
        if (EstaActiva)
        {
            DesconectadoEnUtc = DateTime.UtcNow;
        }
    }
}
