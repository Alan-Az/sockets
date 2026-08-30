package com.tecnm.sockets.domain.modelos;

import java.time.Instant;
import java.util.concurrent.atomic.AtomicLong;

/**
 * Representa el estado de una conexión activa en el servidor Java.
 */
public final class EstadoSesion {

    private final String sesionId;
    private final String direccionRemota;
    private final int puertoRemoto;
    private final Instant conectadoEnUtc;
    private volatile Instant desconectadoEnUtc;
    private final AtomicLong mensajesProcesados;

    public EstadoSesion(String sesionId, String direccionRemota, int puertoRemoto) {
        this.sesionId = sesionId;
        this.direccionRemota = direccionRemota;
        this.puertoRemoto = puertoRemoto;
        this.conectadoEnUtc = Instant.now();
        this.desconectadoEnUtc = null;
        this.mensajesProcesados = new AtomicLong(0);
    }

    public String getSesionId() {
        return sesionId;
    }

    public String getDireccionRemota() {
        return direccionRemota;
    }

    public int getPuertoRemoto() {
        return puertoRemoto;
    }

    public Instant getConectadoEnUtc() {
        return conectadoEnUtc;
    }

    public Instant getDesconectadoEnUtc() {
        return desconectadoEnUtc;
    }

    public long getMensajesProcesados() {
        return mensajesProcesados.get();
    }

    public boolean estaActiva() {
        return desconectadoEnUtc == null;
    }

    public void incrementarMensajes() {
        mensajesProcesados.incrementAndGet();
    }

    public void cerrarSesion() {
        if (estaActiva()) {
            this.desconectadoEnUtc = Instant.now();
        }
    }
}
