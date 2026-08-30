import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;

/**
 * Servidor TCP Multihilo en Java basado en la Página 4 del PDF de la Práctica TecNM.
 */
public class ServidorJava {

    public static void main(String[] args) throws IOException {
        int port = 5000;
        if (args.length > 0) {
            try {
                port = Integer.parseInt(args[0]);
            } catch (NumberFormatException ignored) {}
        }

        try (ServerSocket serverSocket = new ServerSocket(port)) {
            System.out.println("[Servidor Java] Escuchando en puerto " + port + "...");

            while (true) {
                Socket clientSocket = serverSocket.accept();

                // Concurrencia multihilo para atender clientes simultáneos
                Thread hilo = new Thread(() -> handleClient(clientSocket));
                hilo.start();
            }
        }
    }

    private static void handleClient(Socket socket) {
        String endpoint = socket.getRemoteSocketAddress() != null
                ? socket.getRemoteSocketAddress().toString()
                : "Desconocido";

        System.out.println("[Java Servidor] Cliente conectado desde " + endpoint);

        try (Socket sock = socket;
             BufferedReader in = new BufferedReader(new InputStreamReader(sock.getInputStream(), StandardCharsets.UTF_8));
             PrintWriter out = new PrintWriter(new OutputStreamWriter(sock.getOutputStream(), StandardCharsets.UTF_8), true)) {

            String inputLine;
            while ((inputLine = in.readLine()) != null) {
                String mensaje = inputLine.trim();
                System.out.println("[Java Servidor] Recibido de " + endpoint + ": " + mensaje);

                if (mensaje.equalsIgnoreCase("QUIT") || mensaje.equalsIgnoreCase("SALIR")) {
                    out.println("ADIOS");
                    break;
                }

                out.println("ECO DESDE JAVA: " + mensaje);
            }
        } catch (IOException e) {
            // Manejo de desconexión del cliente
        } finally {
            System.out.println("[Java Servidor] Conexión finalizada con " + endpoint);
        }
    }
}
