package com.tecnm.sockets.domain.puertos;

import java.io.Closeable;
import java.io.IOException;

/**
 * Puerto que desacopla la conexión física del socket del dominio de la aplicación.
 */
public interface ISocketConexion extends Closeable, AutoCloseable {

    String getId();

    String getDireccionRemota();

    int getPuertoRemoto();

    boolean estaConectado();

    String leerLinea() throws IOException;

    void enviarLinea(String mensaje) throws IOException;

    @Override
    void close();
}
