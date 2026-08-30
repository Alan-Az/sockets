package com.tecnm.sockets.domain.puertos;

import com.tecnm.sockets.domain.modelos.EstadoSesion;
import java.util.Collection;

/**
 * Contrato para el repositorio de sesiones concurrentes.
 */
public interface IRepositorioSesiones {

    void registrarSesion(EstadoSesion sesion);

    void actualizarSesion(EstadoSesion sesion);

    void finalizarSesion(String sesionId);

    EstadoSesion obtenerPorId(String sesionId);

    Collection<EstadoSesion> obtenerSesionesActivas();

    int obtenerTotalSesionesActivas();
}
