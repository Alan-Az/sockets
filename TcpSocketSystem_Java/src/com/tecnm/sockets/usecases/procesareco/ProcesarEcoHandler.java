package com.tecnm.sockets.usecases.procesareco;

import com.tecnm.sockets.domain.modelos.MensajeEco;
import com.tecnm.sockets.domain.puertos.IServicioLogging;

/**
 * Implementación del Caso de Uso para procesar mensajes de eco en Java.
 */
public final class ProcesarEcoHandler implements IProcesarEcoUseCase {

    private final IServicioLogging logger;
    private final String nombreServidor;

    public ProcesarEcoHandler(IServicioLogging logger, String nombreServidor) {
        if (logger == null) {
            throw new IllegalArgumentException("El logger no puede ser nulo.");
        }
        this.logger = logger;
        this.nombreServidor = (nombreServidor != null) ? nombreServidor : "Servidor-Java21";
    }

    @Override
    public MensajeEco ejecutar(String contenidoRaw, String emisor) {
        MensajeEco mensajeCliente = new MensajeEco(contenidoRaw, emisor);

        logger.info(String.format("Procesando mensaje [%s] de %s: \"%s\"",
                mensajeCliente.getId(),
                mensajeCliente.getEmisor(),
                mensajeCliente.getContenido()), "ProcesarEcoHandler");

        return MensajeEco.crearRespuestaEco(mensajeCliente, nombreServidor);
    }
}
