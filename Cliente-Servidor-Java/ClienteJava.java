import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

/**
 * Cliente TCP de Consola en Java con escucha de mensajes en tiempo real.
 */
public class ClienteJava {

    public static void main(String[] args) {
        String host = args.length > 0 ? args[0] : "127.0.0.1";
        int port = 5000;
        if (args.length > 1) {
            try {
                port = Integer.parseInt(args[1]);
            } catch (NumberFormatException ignored) {}
        }
        String usuario = args.length > 2 ? args[2] : "Usuario-Java";

        System.out.println("[Java Cliente] Conectando a " + host + ":" + port + " como '" + usuario + "'...");

        try (Socket socket = new Socket(host, port);
             PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);
             BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
             Scanner scanner = new Scanner(System.in)) {

            System.out.println("[Java Cliente] Conexión establecida exitosamente.");
            System.out.println("[Java Cliente] Escribe mensajes para enviar (Escribe 'QUIT' para salir):");

            // Hilo de escucha en segundo plano
            Thread listener = new Thread(() -> {
                try {
                    String linea;
                    while ((linea = in.readLine()) != null) {
                        System.out.println("\n" + linea.trim() + "\n> ");
                    }
                } catch (IOException ignored) {}
            });
            listener.setDaemon(true);
            listener.start();

            while (socket.isConnected() && !socket.isClosed()) {
                System.out.print("> ");
                String input = scanner.nextLine();
                if (input == null || input.trim().isEmpty()) continue;

                if (input.trim().equalsIgnoreCase("QUIT") || input.trim().equalsIgnoreCase("SALIR")) {
                    out.println("QUIT");
                    System.out.println("[Java Cliente] Desconectado.");
                    break;
                }

                out.println("[" + usuario + "]: " + input.trim());
            }
        } catch (IOException e) {
            System.err.println("[Java Cliente Error] No se pudo conectar al servidor en " + host + ":" + port + " (" + e.getMessage() + ")");
        }
    }
}
