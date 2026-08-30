import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

/**
 * Cliente TCP en Java basado en la Página 5 del PDF de la Práctica TecNM.
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

        System.out.println("[Java Cliente] Conectando a " + host + ":" + port + "...");

        try (Socket socket = new Socket(host, port);
             PrintWriter out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);
             BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
             Scanner scanner = new Scanner(System.in)) {

            System.out.println("[Java Cliente] Conexión establecida exitosamente.");
            System.out.println("[Java Cliente] Escribe mensajes para enviar al servidor (Escribe 'QUIT' para salir):");

            while (socket.isConnected() && !socket.isClosed()) {
                System.out.print("> ");
                String input = scanner.nextLine();
                if (input == null || input.trim().isEmpty()) continue;

                out.println(input);

                String response = in.readLine();
                if (response == null) {
                    System.out.println("[Java Cliente] El servidor ha cerrado la conexión.");
                    break;
                }

                System.out.println("[Java Cliente] Respuesta: " + response);

                if (input.trim().equalsIgnoreCase("QUIT") || input.trim().equalsIgnoreCase("SALIR")) {
                    System.out.println("[Java Cliente] Desconectado.");
                    break;
                }
            }
        } catch (IOException e) {
            System.err.println("[Java Cliente Error] No se pudo conectar al servidor en " + host + ":" + port + " (" + e.getMessage() + ")");
        }
    }
}
