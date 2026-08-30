using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ClienteGUI;

public class FormCliente : Form
{
    private TextBox txtHost = null!;
    private TextBox txtPuerto = null!;
    private TextBox txtUsuario = null!;
    private Button btnConectar = null!;
    private Label lblEstado = null!;
    private RichTextBox rtbHistorial = null!;
    private TextBox txtMensaje = null!;
    private Button btnEnviar = null!;
    private Button btnSalir = null!;

    private TcpClient? _cliente;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private bool _conectado;

    // Paleta de colores para usuarios
    private readonly Color ColorServidor = Color.FromArgb(46, 117, 89);
    private readonly Color ColorSistema = Color.FromArgb(100, 100, 100);
    private readonly Color ColorPropio = Color.FromArgb(24, 119, 242);

    public FormCliente()
    {
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Cliente Sockets TCP/IP - .NET 10 (TecNM)";
        Size = new Size(680, 560);
        MinimumSize = new Size(580, 460);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        BackColor = Color.FromArgb(245, 247, 250);

        // Panel Superior: Conexión
        var pnlConexion = new Panel
        {
            Dock = DockStyle.Top,
            Height = 85,
            Padding = new Padding(15, 10, 15, 10),
            BackColor = Color.White
        };

        var lblHost = new Label { Text = "Host / IP:", AutoSize = true, Location = new Point(15, 15) };
        txtHost = new TextBox { Text = "127.0.0.1", Width = 110, Location = new Point(15, 38) };

        var lblPuerto = new Label { Text = "Puerto:", AutoSize = true, Location = new Point(135, 15) };
        txtPuerto = new TextBox { Text = "5000", Width = 65, Location = new Point(135, 38) };

        var lblUser = new Label { Text = "Tu Nombre / Usuario:", AutoSize = true, Location = new Point(210, 15) };
        txtUsuario = new TextBox { Text = "Usuario-CSharp", Width = 140, Location = new Point(210, 38) };

        btnConectar = new Button
        {
            Text = "Conectar",
            Location = new Point(365, 34),
            Width = 100,
            Height = 32,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnConectar.FlatAppearance.BorderSize = 0;
        btnConectar.Click += BtnConectar_Click;

        lblEstado = new Label
        {
            Text = "🔴 Desconectado",
            AutoSize = true,
            Location = new Point(480, 42),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.Crimson
        };

        pnlConexion.Controls.AddRange(new Control[] { lblHost, txtHost, lblPuerto, txtPuerto, lblUser, txtUsuario, btnConectar, lblEstado });

        // Panel Inferior: Entrada de Mensajes
        var pnlInferior = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            Padding = new Padding(15, 12, 15, 12),
            BackColor = Color.White
        };

        txtMensaje = new TextBox
        {
            Location = new Point(15, 16),
            Width = 440,
            Height = 30,
            Font = new Font("Segoe UI", 10F)
        };
        txtMensaje.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnEnviar_Click(s, e);
            }
        };

        btnEnviar = new Button
        {
            Text = "Enviar Eco",
            Location = new Point(465, 14),
            Width = 95,
            Height = 34,
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnEnviar.FlatAppearance.BorderSize = 0;
        btnEnviar.Click += BtnEnviar_Click;

        btnSalir = new Button
        {
            Text = "QUIT",
            Location = new Point(570, 14),
            Width = 75,
            Height = 34,
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnSalir.FlatAppearance.BorderSize = 0;
        btnSalir.Click += async (s, e) => await DesconectarAsync(enviarQuit: true);

        pnlInferior.Controls.AddRange(new Control[] { txtMensaje, btnEnviar, btnSalir });

        // Área Central: Historial de Mensajes con colores
        rtbHistorial = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = Color.FromArgb(250, 252, 255),
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10F),
            Padding = new Padding(10)
        };

        Controls.Add(rtbHistorial);
        Controls.Add(pnlInferior);
        Controls.Add(pnlConexion);

        AgregarMensajeSistema("Bienvenido al Cliente TCP. Ingresa la IP/Puerto y presiona 'Conectar'.");

        FormClosing += async (s, e) => await DesconectarAsync(enviarQuit: true);
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
            MessageBox.Show("Puerto inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            lblEstado.ForeColor = Color.FromArgb(16, 185, 129);
            btnConectar.Text = "Desconectar";
            btnConectar.BackColor = Color.FromArgb(107, 114, 128);
            btnEnviar.Enabled = true;
            btnSalir.Enabled = true;
            txtHost.Enabled = false;
            txtPuerto.Enabled = false;
            txtUsuario.Enabled = false;

            AgregarMensajeSistema("¡Conexión establecida exitosamente!");

            // Iniciar escucha en segundo plano
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
            txtMensaje.Clear();
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
                if (bytesLeidos == 0) break; // Desconexión remota

                string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesLeidos).Trim();
                
                Invoke(() =>
                {
                    AgregarMensajeServidor(respuesta);
                });
            }
        }
        catch
        {
            // Socket cerrado o cancelado
        }
        finally
        {
            if (_conectado)
            {
                BeginInvoke(async () =>
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

        if (IsHandleCreated)
        {
            Invoke(() =>
            {
                lblEstado.Text = "🔴 Desconectado";
                lblEstado.ForeColor = Color.Crimson;
                btnConectar.Text = "Conectar";
                btnConectar.BackColor = Color.FromArgb(37, 99, 235);
                btnEnviar.Enabled = false;
                btnSalir.Enabled = false;
                txtHost.Enabled = true;
                txtPuerto.Enabled = true;
                txtUsuario.Enabled = true;
            });
        }
    }

    private void AgregarMensajeUsuario(string usuario, string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        Color colorUser = GenerarColorUsuario(usuario);

        AppendTexto($"[{hora}] ", Color.Gray, FontStyle.Regular);
        AppendTexto($"[{usuario}]: ", colorUser, FontStyle.Bold);
        AppendTexto($"{mensaje}\n", Color.Black, FontStyle.Regular);
    }

    private void AgregarMensajeServidor(string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        AppendTexto($"[{hora}] ", Color.Gray, FontStyle.Regular);
        AppendTexto("[SERVIDOR]: ", ColorServidor, FontStyle.Bold);
        AppendTexto($"{mensaje}\n", Color.FromArgb(30, 41, 59), FontStyle.Italic);
    }

    private void AgregarMensajeSistema(string mensaje)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        AppendTexto($"[{hora}] [SISTEMA] {mensaje}\n", ColorSistema, FontStyle.Regular);
    }

    private void AppendTexto(string texto, Color color, FontStyle estilo)
    {
        rtbHistorial.SelectionStart = rtbHistorial.TextLength;
        rtbHistorial.SelectionLength = 0;
        rtbHistorial.SelectionColor = color;
        rtbHistorial.SelectionFont = new Font(rtbHistorial.Font, estilo);
        rtbHistorial.AppendText(texto);
        rtbHistorial.SelectionColor = rtbHistorial.ForeColor;
        rtbHistorial.ScrollToCaret();
    }

    private Color GenerarColorUsuario(string nombre)
    {
        if (nombre.Equals("Usuario-CSharp", StringComparison.OrdinalIgnoreCase)) return ColorPropio;
        
        // Genera un color consistente a partir del hash del nombre
        int hash = Math.Abs(nombre.GetHashCode());
        int r = (hash & 0xFF) % 180;
        int g = ((hash >> 8) & 0xFF) % 180;
        int b = ((hash >> 16) & 0xFF) % 200 + 40;
        return Color.FromArgb(r, g, b);
    }
}
