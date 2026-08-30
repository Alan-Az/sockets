import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Servidor TCP Multiusuario en Java con soporte de Difusión (Broadcast).
 */
public class ServidorJava {

    private static final ConcurrentHashMap<String, PrintWriter> clientesConectados = new ConcurrentHashMap<>();

    public static void main(String[] args) throws IOException {
        int port = 5000;
        if (args.length > 0) {
            try {
                port = Integer.parseInt(args[0]);
            } catch (NumberFormatException ignored) {}
        }

        try (ServerSocket serverSocket = new ServerSocket(port)) {
            System.out.println("[Servidor Java] Servidor de Chat Multiusuario escuchando en puerto " + port + "...");

            while (true) {
                Socket clientSocket = serverSocket.accept();
                new Thread(() -> handleClient(clientSocket)).start();
            }
        }
    }

    private static void handleClient(Socket socket) {
        String endpoint = socket.getRemoteSocketAddress() != null
                ? socket.getRemoteSocketAddress().toString()
                : "Desconocido";
        String clienteId = socket.toString();

        try (Socket sock = socket;
             BufferedReader in = new BufferedReader(new InputStreamReader(sock.getInputStream(), StandardCharsets.UTF_8));
             PrintWriter out = new PrintWriter(new OutputStreamWriter(sock.getOutputStream(), StandardCharsets.UTF_8), true)) {

            clientesConectados.put(clienteId, out);
            System.out.println("[Java Servidor] Cliente conectado desde " + endpoint + " (Usuarios en línea: " + clientesConectados.size() + ")");

            String inputLine;
            while ((inputLine = in.readLine()) != null) {
                String mensaje = inputLine.trim();
                if (mensaje.isEmpty()) continue;

                System.out.println("[Mensaje] " + endpoint + ": " + mensaje);

                if (mensaje.equalsIgnoreCase("QUIT") || mensaje.equalsIgnoreCase("SALIR")) {
                    break;
                }

                // Difusión a todos los usuarios conectados (Broadcast)
                broadcast(mensaje);
            }
        } catch (IOException ignored) {
        } finally {
            clientesConectados.remove(clienteId);
            System.out.println("[Java Servidor] Conexión finalizada con " + endpoint + " (Usuarios en línea: " + clientesConectados.size() + ")");
        }
    }

    private static void broadcast(String mensaje) {
        for (PrintWriter out : clientesConectados.values()) {
            try {
                out.println(mensaje);
            } catch (Exception ignored) {}
        }
    }
}
