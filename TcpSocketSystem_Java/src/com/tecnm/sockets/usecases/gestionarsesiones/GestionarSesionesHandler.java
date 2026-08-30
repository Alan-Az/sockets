package com.tecnm.sockets.usecases.gestionarsesiones;

import com.tecnm.sockets.domain.modelos.EstadoSesion;
import com.tecnm.sockets.domain.puertos.IRepositorioSesiones;
import com.tecnm.sockets.domain.puertos.IServicioLogging;

/**
 * Implementación de la gestión y auditoría de sesiones concurrentes en Java.
 */
public final class GestionarSesionesHandler implements IGestionarSesionesUseCase {

    private final IRepositorioSesiones repositorioSesiones;
    private final IServicioLogging logger;

    public GestionarSesionesHandler(IRepositorioSesiones repositorioSesiones, IServicioLogging logger) {
        if (repositorioSesiones == null || logger == null) {
            throw new IllegalArgumentException("Las dependencias no pueden ser nulas.");
        }
        this.repositorioSesiones = repositorioSesiones;
        this.logger = logger;
    }

    @Override
    public EstadoSesion registrarNuevaConexion(String sesionId, String direccionIp, int puerto) {
        EstadoSesion sesion = new EstadoSesion(sesionId, direccionIp, puerto);
        repositorioSesiones.registrarSesion(sesion);

        logger.auditoria("CLIENTE_CONECTADO", direccionIp + ":" + puerto, "Sesión ID: " + sesionId + " iniciada.");
        logger.info(String.format("[CONEXIÓN ABIERTA] Cliente %s:%d (ID: %s). Activos: %d",
                direccionIp, puerto, sesionId, repositorioSesiones.obtenerTotalSesionesActivas()), "GestionarSesionesHandler");

        return sesion;
    }

    @Override
    public void registrarActividadMensaje(String sesionId) {
        EstadoSesion sesion = repositorioSesiones.obtenerPorId(sesionId);
        if (sesion != null && sesion.estaActiva()) {
            sesion.incrementarMensajes();
            repositorioSesiones.actualizarSesion(sesion);
        }
    }

    @Override
    public void registrarDesconexion(String sesionId, String motivo) {
        EstadoSesion sesion = repositorioSesiones.obtenerPorId(sesionId);
        if (sesion != null) {
            sesion.cerrarSesion();
            repositorioSesiones.finalizarSesion(sesionId);

            String motivoFinal = (motivo != null) ? motivo : "Desconexión normal";
            logger.auditoria("CLIENTE_DESCONECTADO", sesion.getDireccionRemota() + ":" + sesion.getPuertoRemoto(),
                    "Sesión " + sesionId + " finalizada. Motivo: " + motivoFinal + ". Mensajes: " + sesion.getMensajesProcesados());
            logger.info(String.format("[CONEXIÓN CERRADA] Cliente %s:%d (ID: %s). Activos: %d",
                    sesion.getDireccionRemota(), sesion.getPuertoRemoto(), sesionId, repositorioSesiones.obtenerTotalSesionesActivas()), "GestionarSesionesHandler");
        }
    }

    @Override
    public int obtenerConexionesActivas() {
        return repositorioSesiones.obtenerTotalSesionesActivas();
    }
}
