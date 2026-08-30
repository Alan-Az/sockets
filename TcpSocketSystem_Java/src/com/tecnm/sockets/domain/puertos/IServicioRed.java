package com.tecnm.sockets.domain.puertos;

import java.util.function.Consumer;

/**
 * Contrato para el servicio de red TCP en Java.
 */
public interface IServicioRed {

    boolean estaEscuchando();

    int getPuertoAsignado();

    void iniciarServidor(int puerto, Consumer<ISocketConexion> onClienteConectado) throws Exception;

    void detenerServidor();
}
