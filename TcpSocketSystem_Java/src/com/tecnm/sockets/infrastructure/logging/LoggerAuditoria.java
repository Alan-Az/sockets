package com.tecnm.sockets.infrastructure.logging;

import com.tecnm.sockets.domain.puertos.IServicioLogging;
import java.time.Instant;
import java.time.ZoneOffset;
import java.time.format.DateTimeFormatter;

/**
 * Servicio de Logging en consola para visualización en tiempo real sin persistencia en disco.
 */
public final class LoggerAuditoria implements IServicioLogging {

    private static final DateTimeFormatter FORMATTER_TS = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss.SSS").withZone(ZoneOffset.UTC);

    // Códigos de color ANSI para la terminal
    private static final String ANSI_RESET = "\u001B[0m";
    private static final String ANSI_CYAN = "\u001B[36m";
    private static final String ANSI_YELLOW = "\u001B[33m";
    private static final String ANSI_RED = "\u001B[31m";
    private static final String ANSI_GREEN = "\u001B[32m";

    private final Object lock = new Object();

    public LoggerAuditoria(String carpetaLogs) {
        // No genera archivos en disco para mantener el repositorio limpio
    }

    public LoggerAuditoria() {
        this("logs");
    }

    @Override
    public void info(String mensaje, String origen) {
        escribirLog("INFO", mensaje, origen, ANSI_CYAN);
    }

    @Override
    public void warning(String mensaje, String origen) {
        escribirLog("WARN", mensaje, origen, ANSI_YELLOW);
    }

    @Override
    public void error(String mensaje, Throwable excepcion, String origen) {
        String detalle = (excepcion != null)
                ? String.format("%s | Excepción: %s - %s", mensaje, excepcion.getClass().getSimpleName(), excepcion.getMessage())
                : mensaje;
        escribirLog("ERROR", detalle, origen, ANSI_RED);
    }

    @Override
    public void auditoria(String evento, String ipOCliente, String detalles) {
        String entrada = String.format("[EVENTO] %s | Sujeto: %s | %s", evento, ipOCliente, detalles);
        escribirLog("AUDIT", entrada, "Seguridad", ANSI_GREEN);
    }

    private void escribirLog(String nivel, String mensaje, String origen, String colorAnsi) {
        Instant ahora = Instant.now();
        String timestamp = FORMATTER_TS.format(ahora);
        String origenStr = (origen != null && !origen.trim().isEmpty()) ? origen : "General";
        String linea = String.format("[%s UTC] [%s] [%s] %s", timestamp, nivel, origenStr, mensaje);

        synchronized (lock) {
            System.out.println(colorAnsi + linea + ANSI_RESET);
        }
    }
}
