using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcpSocketSystem.Core.Domain.Modelos;
using TcpSocketSystem.Core.Domain.Puertos;
using TcpSocketSystem.Core.Infrastructure.Logging;
using TcpSocketSystem.Core.Infrastructure.Network;
using TcpSocketSystem.Core.Infrastructure.Repositories;
using TcpSocketSystem.Core.UseCases.GestionarSesiones;
using TcpSocketSystem.Core.UseCases.ProcesarEco;

namespace TcpSocketSystem.Tests;

[TestClass]
public class UnitAndIntegrationTests
{
    private IServicioLogging _logger = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerAuditoria(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_logs"));
    }

    [TestMethod]
    public void MensajeEco_SanitizaEntradaYPrevieneCaracteresMaliciosos()
    {
        // Arrange
        string entradaConCaracteresInseguros = "Mensaje<script>alert('xss')</script>\r\t";

        // Act
        var mensaje = new MensajeEco(entradaConCaracteresInseguros, "ClientePrueba");

        // Assert
        Assert.IsNotNull(mensaje);
        Assert.IsFalse(mensaje.Contenido.Contains("<"));
        Assert.IsFalse(mensaje.Contenido.Contains(">"));
        Assert.IsFalse(mensaje.Contenido.Contains("'"));
        Assert.AreEqual("Mensajescriptalert(xss)/script", mensaje.Contenido);
    }

    [TestMethod]
    public async Task ProcesarEcoUseCase_GeneraRespuestaAdecuada()
    {
        // Arrange
        IProcesarEcoUseCase useCase = new ProcesarEcoHandler(_logger, "Servidor-Test");

        // Act
        var respuesta = await useCase.EjecutarAsync("Hola Servidor", "Cliente-1");

        // Assert
        Assert.IsNotNull(respuesta);
        Assert.IsTrue(respuesta.EsRespuesta);
        Assert.AreEqual("Servidor-Test", respuesta.Emisor);
        Assert.AreEqual("Hola Servidor", respuesta.Contenido);
    }

    [TestMethod]
    public void GestionarSesionesUseCase_RegistraYAuditaConexionesConcurrencia()
    {
        // Arrange
        IRepositorioSesiones repo = new RepositorioSesionesMemoria();
        IGestionarSesionesUseCase useCase = new GestionarSesionesHandler(repo, _logger);

        // Act
        var sesion1 = useCase.RegistrarNuevaConexion("s1", "127.0.0.1", 50001);
        var sesion2 = useCase.RegistrarNuevaConexion("s2", "127.0.0.1", 50002);

        Assert.AreEqual(2, useCase.ObtenerConexionesActivas());

        useCase.RegistrarActividadMensaje("s1");
        Assert.AreEqual(1, repo.ObtenerPorId("s1")?.MensajesProcesados);

        useCase.RegistrarDesconexion("s1", "Prueba");
        Assert.AreEqual(1, useCase.ObtenerConexionesActivas());
    }

    [TestMethod]
    public async Task Integracion_ServidorYCliente_ComunicacionEcoYDesconexionLimpia()
    {
        // Arrange
        int puertoTest = 51234;
        var repo = new RepositorioSesionesMemoria();
        var sesionUseCase = new GestionarSesionesHandler(repo, _logger);
        var ecoUseCase = new ProcesarEcoHandler(_logger, "Servidor-IntegrationTest");
        var servidor = new TcpServidorSocket(_logger);

        using var cts = new CancellationTokenSource();

        var serverTask = servidor.IniciarServidorAsync(puertoTest, async (conexion, token) =>
        {
            var sesion = sesionUseCase.RegistrarNuevaConexion(conexion.Id, conexion.DireccionRemota, conexion.PuertoRemoto);
            while (conexion.EstaConectado && !token.IsCancellationRequested)
            {
                var linea = await conexion.LeerLineaAsync(token);
                if (linea == null || linea.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    sesionUseCase.RegistrarDesconexion(sesion.SesionId);
                    break;
                }
                var eco = await ecoUseCase.EjecutarAsync(linea, "TestClient", token);
                await conexion.EnviarLineaAsync(eco.SerializarTrama(), token);
            }
        }, cts.Token);

        // Dar tiempo a que el socket se enlace
        await Task.Delay(500);

        // Act: Conectar cliente
        using var cliente = new TcpClienteSocket(_logger);
        bool conectado = await cliente.ConectarConReintentosAsync("127.0.0.1", puertoTest, maxReintentos: 3);
        Assert.IsTrue(conectado, "El cliente debería conectarse satisfactoriamente.");

        var respuestaEco = await cliente.EnviarYRecibirEcoAsync("Prueba de Sockets TCP");
        Assert.IsNotNull(respuestaEco);
        Assert.IsTrue(respuestaEco.Contains("Prueba de Sockets TCP"));

        await cliente.EnviarYRecibirEcoAsync("QUIT");
        cliente.Desconectar();

        // Limpieza de servidor
        cts.Cancel();
        await servidor.DetenerServidorAsync();
    }

    [TestMethod]
    public async Task Integracion_Resiliencia_ConexionAPuertoInexistenteRetornaFalso()
    {
        // Arrange: Puerto cerrado
        using var cliente = new TcpClienteSocket(_logger);

        // Act
        bool resultado = await cliente.ConectarConReintentosAsync("127.0.0.1", 59998, maxReintentos: 1);

        // Assert
        Assert.IsFalse(resultado, "No debe conectar a un puerto cerrado.");
    }
}
