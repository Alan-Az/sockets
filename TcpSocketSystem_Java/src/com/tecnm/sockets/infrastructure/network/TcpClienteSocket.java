package com.tecnm.sockets.infrastructure.network;

import com.tecnm.sockets.domain.puertos.IServicioLogging;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.nio.charset.StandardCharsets;

/**
 * Cliente de Sockets TCP en Java con política de reintentos, timeouts configurables y cierre limpio.
 */
public final class TcpClienteSocket implements AutoCloseable {

    private final IServicioLogging logger;
    private Socket socket;
    private BufferedReader reader;
    private BufferedWriter writer;
    private volatile boolean cerrado;

    public TcpClienteSocket(IServicioLogging logger) {
        if (logger == null) {
            throw new IllegalArgumentException("El logger no puede ser nulo.");
        }
        this.logger = logger;
        this.cerrado = false;
    }

    public boolean conectarConReintentos(String host, int puerto, int maxReintentos, int delayMs) {
        for (int intento = 1; intento <= maxReintentos; intento++) {
            try {
                logger.info(String.format("Intentando conectar a %s:%d (Intento %d/%d)...", host, puerto, intento, maxReintentos), "TcpClienteSocket");

                this.socket = new Socket();
                this.socket.setTcpNoDelay(true);
                this.socket.setSoTimeout(10000); // 10s timeout de lectura
                this.socket.connect(new InetSocketAddress(host, puerto), 5000); // 5s timeout de conexión

                this.reader = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
                this.writer = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8));
                this.cerrado = false;

                logger.info(String.format("¡Conectado exitosamente al servidor %s:%d!", host, puerto), "TcpClienteSocket");
                return true;

            } catch (IOException ex) {
                logger.warning(String.format("Fallo en intento %d: %s", intento, ex.getMessage()), "TcpClienteSocket");
                desconectar();

                if (intento < maxReintentos) {
                    try {
                        Thread.sleep(delayMs);
                    } catch (InterruptedException ie) {
                        Thread.currentThread().interrupt();
                        break;
                    }
                }
            }
        }

        logger.error(String.format("No se pudo conectar a %s:%d tras %d intentos.", host, puerto, maxReintentos), null, "TcpClienteSocket");
        return false;
    }

    public boolean estaConectado() {
        return socket != null && socket.isConnected() && !socket.isClosed() && !cerrado;
    }

    public String enviarYRecibirEco(String mensaje) {
        if (!estaConectado()) {
            throw new IllegalStateException("El cliente no está conectado.");
        }

        try {
            logger.info(String.format("[ENVIANDO] -> \"%s\"", mensaje), "TcpClienteSocket");
            writer.write(mensaje);
            writer.write("\n");
            writer.flush();

            String respuesta = reader.readLine();
            logger.info(String.format("[RECIBIDO] <- \"%s\"", respuesta), "TcpClienteSocket");
            return respuesta;
        } catch (IOException ex) {
            logger.error("Error de comunicación durante el envío/recepción.", ex, "TcpClienteSocket");
            desconectar();
            return null;
        }
    }

    public void desconectar() {
        if (cerrado) return;
        cerrado = true;

        try { if (writer != null) writer.close(); } catch (Exception ignored) {}
        try { if (reader != null) reader.close(); } catch (Exception ignored) {}
        try { if (socket != null) socket.close(); } catch (Exception ignored) {}

        this.writer = null;
        this.reader = null;
        this.socket = null;
    }

    @Override
    public void close() {
        desconectar();
    }
}
