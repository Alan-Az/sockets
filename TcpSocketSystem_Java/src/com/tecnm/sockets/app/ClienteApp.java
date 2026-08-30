package com.tecnm.sockets.app;

import com.tecnm.sockets.domain.puertos.IServicioLogging;
import com.tecnm.sockets.infrastructure.logging.LoggerAuditoria;
import com.tecnm.sockets.infrastructure.network.GestorHilos;
import com.tecnm.sockets.infrastructure.network.TcpClienteSocket;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

/**
 * Punto de entrada del Cliente TCP en Java 21 con menú interactivo y pruebas de concurrencia.
 */
public final class ClienteApp {

    public static void main(String[] args) {
        System.out.println("\u001B[32m===================================================================");
        System.out.println("  TECNM - LABORATORIO DE SOCKETS TCP/IP (SCREAMING ARCHITECTURE)   ");
        System.out.println("  Módulo de Cliente de Red TCP (Java 21)                          ");
        System.out.println("===================================================================\u001B[0m");

        String host = "127.0.0.1";
        int puerto = 5000;

        if (args.length >= 1) host = args[0];
        if (args.length >= 2) {
            try {
                puerto = Integer.parseInt(args[1]);
            } catch (NumberFormatException ignored) {}
        }

        IServicioLogging logger = new LoggerAuditoria("logs");
        Scanner scanner = new Scanner(System.in);

        while (true) {
            System.out.println();
            System.out.println("\u001B[33mDestino actual configurado: " + host + ":" + puerto);
            System.out.println("--------------------------------------------------");
            System.out.println("1. Iniciar Sesión Interactiva de Eco (Envío continuo)");
            System.out.println("2. Ejecutar Prueba Multicliente Simultáneo (5 clientes con Virtual Threads)");
            System.out.println("3. Prueba de Resiliencia / Timeout (Intento con servidor inexistente)");
            System.out.println("4. Cambiar Dirección IP / Puerto");
            System.out.println("5. Salir");
            System.out.print("Selecciona una opción [1-5]: \u001B[0m");

            String opcion = scanner.nextLine().trim();

            switch (opcion) {
                case "1":
                    ejecutarModoInteractivo(host, puerto, logger, scanner);
                    break;
                case "2":
                    ejecutarPruebaMulticliente(host, puerto, 5, logger);
                    break;
                case "3":
                    ejecutarPruebaTimeout(logger);
                    break;
                case "4":
                    System.out.print("Ingresa la nueva IP o Host [ej. 127.0.0.1]: ");
                    String nuevoHost = scanner.nextLine().trim();
                    if (!nuevoHost.isEmpty()) host = nuevoHost;

                    System.out.print("Ingresa el nuevo Puerto [ej. 5000]: ");
                    String nuevoPuertoStr = scanner.nextLine().trim();
                    try {
                        puerto = Integer.parseInt(nuevoPuertoStr);
                    } catch (NumberFormatException ignored) {}
                    break;
                case "5":
                    System.out.println("Saliendo del cliente Java...");
                    return;
                default:
                    System.out.println("Opción no válida.");
                    break;
            }
        }
    }

    private static void ejecutarModoInteractivo(String host, int puerto, IServicioLogging logger, Scanner scanner) {
        try (TcpClienteSocket cliente = new TcpClienteSocket(logger)) {
            boolean conectado = cliente.conectarConReintentos(host, puerto, 3, 1500);
            if (!conectado) {
                System.out.println("\u001B[31mNo se pudo conectar al servidor. Revisa si el servicio está activo.\u001B[0m");
                return;
            }

            System.out.println();
            System.out.println("\u001B[36m--- MODO ECO INTERACTIVO (JAVA) ACTIVADO ---");
            System.out.println("Escribe tu mensaje y presiona Enter.");
            System.out.println("Escribe 'QUIT' o 'SALIR' para desconectarte.");
            System.out.println("---------------------------------------------\u001B[0m");

            while (cliente.estaConectado()) {
                System.out.print("\n[Tu mensaje] > ");
                String mensaje = scanner.nextLine();
                if (mensaje == null || mensaje.trim().isEmpty()) continue;

                String respuesta = cliente.enviarYRecibirEco(mensaje);
                if (respuesta == null) {
                    System.out.println("El servidor cerró la conexión.");
                    break;
                }

                if (mensaje.trim().equalsIgnoreCase("QUIT") || mensaje.trim().equalsIgnoreCase("SALIR")) {
                    System.out.println("Sesión finalizada por el usuario.");
                    cliente.desconectar();
                    break;
                }
            }
        }
    }

    private static void ejecutarPruebaMulticliente(String host, int puerto, int totalClientes, IServicioLogging logger) {
        System.out.println("\u001B[35m\n[PRUEBA MULTICLIENTE VIRTUAL THREADS] Lanzando " + totalClientes + " clientes concurrentes contra " + host + ":" + puerto + "...\u001B[0m");

        List<Thread> hilos = new ArrayList<>();

        for (int i = 1; i <= totalClientes; i++) {
            final int clienteId = i;
            Thread vt = GestorHilos.iniciarHilo("vt-test-client-" + clienteId, () -> {
                try (TcpClienteSocket cliente = new TcpClienteSocket(logger)) {
                    boolean conectado = cliente.conectarConReintentos(host, puerto, 1, 1000);
                    if (conectado) {
                        for (int m = 1; m <= 3; m++) {
                            String mensaje = "Paquete_" + m + "_desde_ClienteJavaVT_" + clienteId;
                            cliente.enviarYRecibirEco(mensaje);
                            Thread.sleep(200);
                        }
                        cliente.enviarYRecibirEco("QUIT");
                    }
                } catch (Exception ex) {
                    logger.error("Error en cliente de prueba " + clienteId, ex, "PruebaMulticliente");
                }
            });
            hilos.add(vt);
        }

        for (Thread h : hilos) {
            try {
                h.join();
            } catch (InterruptedException ignored) {}
        }

        System.out.println("\u001B[32m\n[PRUEBA MULTICLIENTE FINALIZADA] Todas las ráfagas concurrentes fueron procesadas con éxito.\u001B[0m");
    }

    private static void ejecutarPruebaTimeout(IServicioLogging logger) {
        System.out.println("\u001B[33m\n[PRUEBA DE TIMEOUT Y RESILIENCIA] Intentando conectar a un puerto cerrado (59999)...\u001B[0m");
        try (TcpClienteSocket cliente = new TcpClienteSocket(logger)) {
            cliente.conectarConReintentos("127.0.0.1", 59999, 2, 500);
        }
    }
}
