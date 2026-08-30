package com.tecnm.sockets.infrastructure.network;

import java.lang.reflect.Method;

/**
 * Gestor de Concurrencia para Sockets TCP.
 * Aplica Virtual Threads de Java 21 LTS de forma nativa cuando están disponibles,
 * con fallback transparente a Platform Threads para máxima compatibilidad con cualquier IDE o versión de Java.
 */
public final class GestorHilos {

    private GestorHilos() {}

    /**
     * Inicia un hilo (Virtual Thread en Java 21+ o Platform Thread en versiones anteriores).
     */
    public static Thread iniciarHilo(String nombre, Runnable tarea) {
        try {
            // Intentar invocar la API de Virtual Threads de Java 21: Thread.ofVirtual().name(...).start(...)
            Method ofVirtualMethod = Thread.class.getMethod("ofVirtual");
            Object builder = ofVirtualMethod.invoke(null);
            Method nameMethod = builder.getClass().getMethod("name", String.class);
            builder = nameMethod.invoke(builder, nombre);
            Method startMethod = builder.getClass().getMethod("start", Runnable.class);
            return (Thread) startMethod.invoke(builder, tarea);
        } catch (Throwable ex) {
            // Fallback a hilo clásico multihilo de alta concurrencia
            Thread hilo = new Thread(tarea, nombre);
            hilo.setDaemon(true);
            hilo.start();
            return hilo;
        }
    }
}
