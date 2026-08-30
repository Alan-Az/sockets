package com.tecnm.sockets.infrastructure.network;

import com.tecnm.sockets.domain.puertos.IServicioLogging;
import com.tecnm.sockets.domain.puertos.IServicioRed;
import com.tecnm.sockets.domain.puertos.ISocketConexion;
import java.io.IOException;
import java.net.BindException;
import java.net.InetSocketAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.function.Consumer;

/**
 * Servidor TCP multihilo en Java 21 utilizando Virtual Threads (Project Loom)
 * para atender concurrencia masiva de sockets de forma ultra-ligera sin bloqueo de hilos de SO.
 */
public final class TcpServidorSocket implements IServicioRed {

    private final IServicioLogging logger;
    private ServerSocket serverSocket;
    private final AtomicBoolean estaEscuchando = new AtomicBoolean(false);
    private int puertoAsignado;

    public TcpServidorSocket(IServicioLogging logger) {
        if (logger == null) {
            throw new IllegalArgumentException("El servicio de logging no puede ser nulo.");
        }
        this.logger = logger;
    }

    @Override
    public boolean estaEscuchando() {
        return estaEscuchando.get();
    }

    @Override
    public int getPuertoAsignado() {
        return puertoAsignado;
    }

    @Override
    public void iniciarServidor(int puerto, Consumer<ISocketConexion> onClienteConectado) throws Exception {
        if (estaEscuchando.get()) {
            throw new IllegalStateException("El servidor ya está en ejecución.");
        }

        try {
            this.puertoAsignado = puerto;
            this.serverSocket = new ServerSocket();
            this.serverSocket.setReuseAddress(true);
            this.serverSocket.bind(new InetSocketAddress(puerto));
            this.estaEscuchando.set(true);

            logger.info(String.format("Servidor TCP Java (Virtual Threads) iniciado en 0.0.0.0:%d", puerto), "TcpServidorSocket");
            logger.auditoria("SERVIDOR_INICIADO", "0.0.0.0:" + puerto, "Listo para atender conexiones.");

            while (estaEscuchando.get() && !serverSocket.isClosed()) {
                try {
                    Socket socket = serverSocket.accept();
                    socket.setTcpNoDelay(true);
                    socket.setSoTimeout(30000); // 30 segundos timeout

                    TcpSocketConexion conexion = new TcpSocketConexion(socket);

                    // Concurrencia de alta eficiencia (Virtual Threads en Java 21 / Fallback multiplataforma)
                    GestorHilos.iniciarHilo("vt-cliente-" + conexion.getId(), () -> {
                        try (TcpSocketConexion conn = conexion) {
                            onClienteConectado.accept(conn);
                        } catch (Exception ex) {
                            logger.error(String.format("Error en la atención del cliente %s:%d",
                                    conexion.getDireccionRemota(), conexion.getPuertoRemoto()), ex, "TcpServidorSocket");
                        }
                    });

                } catch (IOException ex) {
                    if (!estaEscuchando.get() || serverSocket.isClosed()) {
                        break;
                    }
                    logger.error("Error al aceptar conexión entrante.", ex, "TcpServidorSocket");
                }
            }
        } catch (BindException ex) {
            logger.error(String.format("No se pudo iniciar el servidor. El puerto %d ya está en uso.", puerto), ex, "TcpServidorSocket");
            throw ex;
        } finally {
            detenerServidor();
        }
    }

    @Override
    public void detenerServidor() {
        if (!estaEscuchando.getAndSet(false)) {
            return;
        }

        try {
            if (serverSocket != null && !serverSocket.isClosed()) {
                serverSocket.close();
                logger.info("Servidor TCP Java detenido limpiamente.", "TcpServidorSocket");
                logger.auditoria("SERVIDOR_DETENIDO", "0.0.0.0:" + puertoAsignado, "Servidor cerró el listener.");
            }
        } catch (IOException ex) {
            logger.error("Error al cerrar el ServerSocket.", ex, "TcpServidorSocket");
        }
    }
}
