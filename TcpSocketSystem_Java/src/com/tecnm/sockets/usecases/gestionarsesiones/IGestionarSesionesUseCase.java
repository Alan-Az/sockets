package com.tecnm.sockets.usecases.gestionarsesiones;

import com.tecnm.sockets.domain.modelos.EstadoSesion;

/**
 * Caso de Uso: Control de ciclo de vida de sesiones y concurrencia.
 */
public interface IGestionarSesionesUseCase {

    EstadoSesion registrarNuevaConexion(String sesionId, String direccionIp, int puerto);

    void registrarActividadMensaje(String sesionId);

    void registrarDesconexion(String sesionId, String motivo);

    int obtenerConexionesActivas();
}
