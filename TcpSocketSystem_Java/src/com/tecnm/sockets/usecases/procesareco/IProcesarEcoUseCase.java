package com.tecnm.sockets.usecases.procesareco;

import com.tecnm.sockets.domain.modelos.MensajeEco;

/**
 * Caso de Uso: Procesamiento y sanitización de mensajes de eco.
 */
public interface IProcesarEcoUseCase {

    MensajeEco ejecutar(String contenidoRaw, String emisor);
}
