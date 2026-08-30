package com.tecnm.sockets.domain.modelos;

import java.time.Instant;
import java.time.ZoneOffset;
import java.time.format.DateTimeFormatter;
import java.util.UUID;
import java.util.regex.Pattern;

/**
 * Representa el modelo inmutable de un mensaje que transita por los Sockets TCP.
 * Aplica validaciones de seguridad, sanitización y límites de longitud.
 */
public final class MensajeEco {

    private static final int MAX_LONGITUD = 4096;
    private static final Pattern PATRON_SANITIZACION = Pattern.compile("[^\\w\\s.,!?:;\\-_@#$/()\\[\\]\\\\]");
    private static final DateTimeFormatter FORMATTER = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss").withZone(ZoneOffset.UTC);

    private final String id;
    private final String contenido;
    private final Instant timestampUtc;
    private final String emisor;
    private final boolean esRespuesta;

    public MensajeEco(String contenido, String emisor, boolean esRespuesta, String id) {
        if (contenido == null || contenido.trim().isEmpty()) {
            throw new IllegalArgumentException("El contenido del mensaje no puede estar vacío.");
        }

        String contenidoTratado = contenido.trim();
        if (contenidoTratado.length() > MAX_LONGITUD) {
            contenidoTratado = contenidoTratado.substring(0, MAX_LONGITUD);
        }

        this.id = (id != null && !id.trim().isEmpty()) ? id : UUID.randomUUID().toString().replace("-", "").substring(0, 8);
        this.contenido = PATRON_SANITIZACION.matcher(contenidoTratado).replaceAll("");
        this.timestampUtc = Instant.now();
        this.emisor = (emisor == null || emisor.trim().isEmpty()) ? "Desconocido" : emisor.trim();
        this.esRespuesta = esRespuesta;
    }

    public MensajeEco(String contenido, String emisor) {
        this(contenido, emisor, false, null);
    }

    public String getId() {
        return id;
    }

    public String getContenido() {
        return contenido;
    }

    public Instant getTimestampUtc() {
        return timestampUtc;
    }

    public String getEmisor() {
        return emisor;
    }

    public boolean isEsRespuesta() {
        return esRespuesta;
    }

    /**
     * Serializa el mensaje a la trama común para interoperabilidad.
     */
    public String serializarTrama() {
        String fechaStr = FORMATTER.format(timestampUtc);
        return String.format("[%s] [%s] %s: %s",
                fechaStr,
                emisor,
                esRespuesta ? "REPLY" : "MSG",
                contenido);
    }

    public static MensajeEco crearRespuestaEco(MensajeEco original, String nombreServidor) {
        return new MensajeEco(
                original.getContenido(),
                (nombreServidor != null) ? nombreServidor : "Servidor-Java21",
                true,
                original.getId()
        );
    }
}
