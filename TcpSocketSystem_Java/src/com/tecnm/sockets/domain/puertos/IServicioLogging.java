package com.tecnm.sockets.domain.puertos;

/**
 * Puerto de auditoría y diagnóstico para el sistema de Sockets Java.
 */
public interface IServicioLogging {

    void info(String mensaje, String origen);

    void warning(String mensaje, String origen);

    void error(String mensaje, Throwable excepcion, String origen);

    void auditoria(String evento, String ipOCliente, String detalles);
}
