using System.Collections.Concurrent;
using TcpSocketSystem.Core.Domain.Modelos;
using TcpSocketSystem.Core.Domain.Puertos;

namespace TcpSocketSystem.Core.Infrastructure.Repositories;

/// <summary>
/// Repositorio de sesiones concurrentes en memoria con thread-safety (ConcurrentDictionary).
/// </summary>
public sealed class RepositorioSesionesMemoria : IRepositorioSesiones
{
    private readonly ConcurrentDictionary<string, EstadoSesion> _sesiones = new();

    public void RegistrarSesion(EstadoSesion sesion)
    {
        _sesiones[sesion.SesionId] = sesion;
    }

    public void ActualizarSesion(EstadoSesion sesion)
    {
        _sesiones[sesion.SesionId] = sesion;
    }

    public void FinalizarSesion(string sesionId)
    {
        _sesiones.TryRemove(sesionId, out _);
    }

    public EstadoSesion? ObtenerPorId(string sesionId)
    {
        _sesiones.TryGetValue(sesionId, out var sesion);
        return sesion;
    }

    public IReadOnlyCollection<EstadoSesion> ObtenerSesionesActivas()
    {
        return _sesiones.Values.Where(s => s.EstaActiva).ToList();
    }

    public int ObtenerTotalSesionesActivas()
    {
        return _sesiones.Count;
    }
}
