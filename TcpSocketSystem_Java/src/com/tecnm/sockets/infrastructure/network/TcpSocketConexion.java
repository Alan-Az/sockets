package com.tecnm.sockets.infrastructure.network;

import com.tecnm.sockets.domain.puertos.ISocketConexion;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.UUID;

/**
 * Adaptador de Socket en Java que implementa el puerto ISocketConexion.
 * Gestiona el I/O UTF-8 con buffers eficientes.
 */
public final class TcpSocketConexion implements ISocketConexion {

    private final Socket socket;
    private final String id;
    private final String direccionRemota;
    private final int puertoRemoto;
    private final BufferedReader reader;
    private final BufferedWriter writer;
    private volatile boolean cerrado;

    public TcpSocketConexion(Socket socket) throws IOException {
        if (socket == null) {
            throw new IllegalArgumentException("El socket no puede ser nulo.");
        }
        this.socket = socket;
        this.id = UUID.randomUUID().toString().replace("-", "").substring(0, 8);

        if (socket.getRemoteSocketAddress() instanceof InetSocketAddress) {
            InetSocketAddress endpoint = (InetSocketAddress) socket.getRemoteSocketAddress();
            this.direccionRemota = endpoint.getAddress().getHostAddress();
            this.puertoRemoto = endpoint.getPort();
        } else {
            this.direccionRemota = "Desconocida";
            this.puertoRemoto = 0;
        }

        // UTF-8 estándar universal
        this.reader = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        this.writer = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8));
        this.cerrado = false;
    }

    @Override
    public String getId() {
        return id;
    }

    @Override
    public String getDireccionRemota() {
        return direccionRemota;
    }

    @Override
    public int getPuertoRemoto() {
        return puertoRemoto;
    }

    @Override
    public boolean estaConectado() {
        return socket.isConnected() && !socket.isClosed() && !cerrado;
    }

    @Override
    public String leerLinea() throws IOException {
        if (!estaConectado()) return null;
        try {
            return reader.readLine();
        } catch (IOException ex) {
            // Manejo de reset de conexión o timeout
            return null;
        }
    }

    @Override
    public void enviarLinea(String mensaje) throws IOException {
        if (!estaConectado()) {
            throw new IOException("No se puede enviar datos a través de un socket cerrado.");
        }
        writer.write(mensaje);
        writer.write("\n");
        writer.flush();
    }

    @Override
    public void close() {
        if (cerrado) return;
        cerrado = true;

        try { writer.close(); } catch (Exception ignored) {}
        try { reader.close(); } catch (Exception ignored) {}
        try { socket.close(); } catch (Exception ignored) {}
    }
}
