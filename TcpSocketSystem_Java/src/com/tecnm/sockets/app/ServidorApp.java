package com.tecnm.sockets.app;

import com.tecnm.sockets.domain.modelos.EstadoSesion;
import com.tecnm.sockets.domain.modelos.MensajeEco;
import com.tecnm.sockets.domain.puertos.IRepositorioSesiones;
import com.tecnm.sockets.domain.puertos.IServicioLogging;
import com.tecnm.sockets.domain.puertos.IServicioRed;
import com.tecnm.sockets.domain.puertos.ISocketConexion;
import com.tecnm.sockets.infrastructure.logging.LoggerAuditoria;
import com.tecnm.sockets.infrastructure.network.TcpServidorSocket;
import com.tecnm.sockets.infrastructure.repository.RepositorioSesionesMemoria;
import com.tecnm.sockets.usecases.gestionarsesiones.GestionarSesionesHandler;
import com.tecnm.sockets.usecases.gestionarsesiones.IGestionarSesionesUseCase;
import com.tecnm.sockets.usecases.procesareco.IProcesarEcoUseCase;
import com.tecnm.sockets.usecases.procesareco.ProcesarEcoHandler;

/**
 * Punto de entrada del Servidor TCP Multihilo en Java 21 (Virtual Threads).
 */
public final class ServidorApp {

    public static void main(String[] args) {
        System.out.println("\u001B[36m===================================================================");
        System.out.println("  TECNM - LABORATORIO DE SOCKETS TCP/IP (SCREAMING ARCHITECTURE)   ");
        System.out.println("  Módulo de Servidor Concurrente Java 21 (Virtual Threads)         ");
        System.out.println("===================================================================\u001B[0m");

        int puerto = 5000;
        if (args.length > 0) {
            try {
                puerto = Integer.parseInt(args[0]);
            } catch (NumberFormatException ignored) {}
        }

        // Composición de dependencias de Screaming Architecture
        IServicioLogging logger = new LoggerAuditoria("logs");
        IRepositorioSesiones repositorioSesiones = new RepositorioSesionesMemoria();
        IGestionarSesionesUseCase gestionarSesionesUseCase = new GestionarSesionesHandler(repositorioSesiones, logger);
        IProcesarEcoUseCase procesarEcoUseCase = new ProcesarEcoHandler(logger, "Servidor-Java21");
        IServicioRed servicioRed = new TcpServidorSocket(logger);

        // Hook para apagado ordenado (Shutdown Hook)
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            logger.warning("Cierre de proceso detectado. Deteniendo servidor...", "Main");
            servicioRed.detenerServidor();
        }));

        try {
            logger.info("Iniciando listener en el puerto TCP " + puerto + "...", "Main");
            servicioRed.iniciarServidor(puerto, (ISocketConexion conexion) -> {
                EstadoSesion sesion = gestionarSesionesUseCase.registrarNuevaConexion(
                        conexion.getId(),
                        conexion.getDireccionRemota(),
                        conexion.getPuertoRemoto());

                try {
                    // Enviar banner de bienvenida con interoperabilidad
                    conexion.enviarLinea("BIENVENIDO|Servidor=Java 21 Virtual Threads|SesionId=" + sesion.getSesionId() + "|Status=OK");

                    while (conexion.estaConectado()) {
                        String lineaRecibida = conexion.leerLinea();
                        if (lineaRecibida == null) {
                            gestionarSesionesUseCase.registrarDesconexion(sesion.getSesionId(), "Cierre por fin de stream del cliente");
                            break;
                        }

                        String textoLimpio = lineaRecibida.trim();
                        if (textoLimpio.equalsIgnoreCase("QUIT") || textoLimpio.equalsIgnoreCase("SALIR")) {
                            conexion.enviarLinea("ADIOS|SesionFinalizada");
                            gestionarSesionesUseCase.registrarDesconexion(sesion.getSesionId(), "Solicitud de desconexión (QUIT)");
                            break;
                        }

                        gestionarSesionesUseCase.registrarActividadMensaje(sesion.getSesionId());
                        MensajeEco respuestaEco = procesarEcoUseCase.ejecutar(textoLimpio, "Cliente-" + conexion.getId());

                        conexion.enviarLinea(respuestaEco.serializarTrama());
                    }
                } catch (Exception ex) {
                    logger.error("Excepción durante atención del cliente " + conexion.getDireccionRemota(), ex, "AtenderCliente");
                    gestionarSesionesUseCase.registrarDesconexion(sesion.getSesionId(), "Fallo: " + ex.getMessage());
                } finally {
                    conexion.close();
                }
            });
        } catch (Exception ex) {
            logger.error("Error crítico en el servidor: " + ex.getMessage(), ex, "Main");
        } finally {
            logger.info("Proceso del Servidor Java finalizado.", "Main");
        }
    }
}
