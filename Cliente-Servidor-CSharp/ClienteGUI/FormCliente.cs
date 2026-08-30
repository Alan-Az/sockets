using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;

namespace ClienteGUI;

public class FormCliente : Form
{
    private readonly TextBox txtHost;
    private readonly TextBox txtPuerto;
    private readonly TextBox txtUsuario;
    private readonly Button btnConectar;
    private readonly Label lblEstado;
    private readonly TextArea txtHistorial;
    private readonly TextBox txtMensaje;
    private readonly Button btnEnviar;
    private readonly Button btnSalir;

    private TcpClient? _cliente;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private bool _conectado;

    public FormCliente()
    {
        Title = "Cliente Sockets TCP/IP - .NET 10 (TecNM)";
        ClientSize = new Size(680, 520);
        MinimumSize = new Size(580, 440);

        // UI Controls
        txtHost = new TextBox { Text = "127.0.0.1", Width = 110 };
        txtPuerto = new TextBox { Text = "5000", Width = 65 };
        txtUsuario = new TextBox { Text = "Usuario-CSharp", Width = 130 };

        btnConectar = new Button { Text = "Conectar" };
        btnConectar.Click += BtnConectar_Click;

        lblEstado = new Label { Text = "🔴 Desconectado", TextColor = Colors.Red };

        btnEnviar = new Button { Text = "Enviar Eco", Enabled = false };
        btnEnviar.Click += BtnEnviar_Click;

        btnSalir = new Button { Text = "QUIT", Enabled = false };
        btnSalir.Click += async (s, e) => await DesconectarAsync(enviarQuit: true);

        txtMensaje = new TextBox { PlaceholderText = "Escribe tu mensaje..." };
        txtMensaje.KeyDown += (s, e) =>
        {
            if (e.Key == Keys.Enter)
            {
                BtnEnviar_Click(s, e);
            }
        };

        txtHistorial = new TextArea
        {
            ReadOnly = true,
            BackgroundColor = Colors.WhiteSmoke
        };

        // Layouts
        var pnlConexion = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Padding = new Padding(10),
            Items =
            {
                new Label { Text = "Host:" },
                txtHost,
                new Label { Text = "Puerto:" },
                txtPuerto,
                new Label { Text = "Usuario:" },
                txtUsuario,
                btnConectar,
                lblEstado
            }
        };

        var pnlInferior = new TableLayout
        {
            Padding = new Padding(10),
            Spacing = new Size(8, 8),
            Rows =
            {
                new TableRow(
                    new TableCell(txtMensaje, scaleWidth: true),
                    btnEnviar,
                    btnSalir
                )
            }
        };

        Content = new TableLayout
        {
            Padding = new Padding(5),
            Spacing = new Size(5, 5),
            Rows =
            {
                pnlConexion,
                new TableRow(txtHistorial) { ScaleHeight = true },
                pnlInferior
            }
        };

        AgregarMensajeSistema("Bienvenido al Cliente TCP .NET. Ingresa la IP/Puerto y presiona 'Conectar'.");

        Closed += async (s, e) => await DesconectarAsync(enviarQuit: true);
    }

    private async void BtnConectar_Click(object? sender, EventArgs e)
    {
        if (_conectado)
        {
            await DesconectarAsync(enviarQuit: true);
            return;
        }

        string host = txtHost.Text.Trim();
        if (!int.TryParse(txtPuerto.Text.Trim(), out int puerto))
        {
            MessageBox.Show(this, "Puerto inválido.", "Error", MessageBoxButtons.OK, MessageBoxType.Warning);
            return;
        }

        try
        {
            btnConectar.Enabled = false;
            AgregarMensajeSistema($"Conectando a {host}:{puerto}...");

            _cliente = new TcpClient();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _cliente.ConnectAsync(host, puerto, timeoutCts.Token);
            _stream = _cliente.GetStream();

            _conectado = true;
            _cts = new CancellationTokenSource();

            lblEstado.Text = "🟢 Conectado";
            lblEstado.TextColor = Colors.Green;
            btnConectar.Text = "Desconectar";
            btnEnviar.Enabled = true;
            btnSalir.Enabled = true;
            txtHost.Enabled = false;
            txtPuerto.Enabled = false;
            txtUsuario.Enabled = false;

            AgregarMensajeSistema("¡Conexión establecida exitosamente!");

            _ = Task.Run(() => EscucharServidorAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            AgregarMensajeSistema($"Error de conexión: {ex.Message}");
            await DesconectarAsync(enviarQuit: false);
        }
        finally
        {
            btnConectar.Enabled = true;
        }
    }

    private async void BtnEnviar_Click(object? sender, EventArgs e)
    {
        if (!_conectado || _stream == null) return;

        string texto = txtMensaje.Text.Trim();
        if (string.IsNullOrEmpty(texto)) return;

        string usuario = string.IsNullOrWhiteSpace(txtUsuario.Text) ? "Usuario" : txtUsuario.Text.Trim();
        string mensajeCompleto = $"[{usuario}]: {texto}";

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(mensajeCompleto + "\n");
            await _stream.WriteAsync(bytes);

            AgregarMensajeUsuario(usuario, texto);
            txtMensaje.Text = "";
            txtMensaje.Focus();
        }
        catch (Exception ex)
        {
            AgregarMensajeSistema($"Error al enviar mensaje: {ex.Message}");
            await DesconectarAsync(enviarQuit: false);
        }
    }

    private async Task EscucharServidorAsync(CancellationToken token)
    {
        byte[] buffer = new byte[2048];
        try
        {
            while (!token.IsCancellationRequested && _stream != null)
            {
                int bytesLeidos = await _stream.ReadAsync(buffer, token);
                if (bytesLeidos == 0) break;

                string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();
                
                Application.Instance.AsyncInvoke(() =>
                {
                    AgregarMensajeServidor(respuesta);
                });
            }
        }
        catch
        {
        }
        finally
        {
            if (_conectado)
            {
                Application.Instance.AsyncInvoke(async () =>
                {
                    AgregarMensajeSistema("El servidor ha cerrado la conexión.");
                    await DesconectarAsync(enviarQuit: false);
                });
            }
        }
    }

    private async Task DesconectarAsync(bool enviarQuit)
    {
        if (!_conectado && _cliente == null) return;

        _conectado = false;
        try
        {
            if (enviarQuit && _stream != null)
            {
                byte[] quitBytes = Encoding.UTF8.GetBytes("QUIT\n");
                await _stream.WriteAsync(quitBytes);
            }
        }
        catch { }

        _cts?.Cancel();
        _stream?.Dispose();
        _cliente?.Close();
        _cliente?.Dispose();

        _stream = null;
        _cliente = null;

        Application.Instance.AsyncInvoke(() =>
        {
            lblEstado.Text = "🔴 Desconectado";
            lblEstado.TextColor = Colors.Red;
            btnConectar.Text = "Conectar";
            btnEnviar.Enabled = false;
            btnSalir.Enabled = false;
            txtHost.Enabled = true;
            txtPuerto.Enabled = true;
            txtUsuario.Enabled = true;
        });
    }

    private void AgregarMensajeUsuario(string usuario, string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        AppendTexto($"[{hora}] [{usuario}]: {mensaje}\n");
    }

    private void AgregarMensajeServidor(string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        AppendTexto($"[{hora}] [SERVIDOR]: {mensaje}\n");
    }

    private void AgregarMensajeSistema(string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        AppendTexto($"[{hora}] [SISTEMA] {mensaje}\n");
    }

    private void AppendTexto(string texto)
    {
        txtHistorial.Append(texto, true);
    }
}
