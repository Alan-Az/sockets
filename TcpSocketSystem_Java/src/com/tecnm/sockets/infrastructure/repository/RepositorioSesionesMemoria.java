package com.tecnm.sockets.infrastructure.repository;

import com.tecnm.sockets.domain.modelos.EstadoSesion;
import com.tecnm.sockets.domain.puertos.IRepositorioSesiones;
import java.util.Collection;
import java.util.concurrent.ConcurrentHashMap;
import java.util.stream.Collectors;

/**
 * Implementación en memoria y thread-safe del repositorio de sesiones en Java.
 */
public final class RepositorioSesionesMemoria implements IRepositorioSesiones {

    private final ConcurrentHashMap<String, EstadoSesion> sesiones = new ConcurrentHashMap<>();

    @Override
    public void registrarSesion(EstadoSesion sesion) {
        sesiones.put(sesion.getSesionId(), sesion);
    }

    @Override
    public void actualizarSesion(EstadoSesion sesion) {
        sesiones.put(sesion.getSesionId(), sesion);
    }

    @Override
    public void finalizarSesion(String sesionId) {
        sesiones.remove(sesionId);
    }

    @Override
    public EstadoSesion obtenerPorId(String sesionId) {
        return sesiones.get(sesionId);
    }

    @Override
    public Collection<EstadoSesion> obtenerSesionesActivas() {
        return sesiones.values().stream()
                .filter(EstadoSesion::estaActiva)
                .collect(Collectors.toList());
    }

    @Override
    public int obtenerTotalSesionesActivas() {
        return sesiones.size();
    }
}
