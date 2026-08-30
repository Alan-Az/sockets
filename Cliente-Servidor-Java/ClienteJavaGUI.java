import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.Font;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import javax.swing.BorderFactory;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextField;
import javax.swing.JTextPane;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;
import javax.swing.text.Style;
import javax.swing.text.StyleConstants;
import javax.swing.text.StyledDocument;

/**
 * Cliente TCP con Interfaz Gráfica (GUI) en Java Swing.
 * Muestra historial de mensajes con colores diferenciados por usuario.
 */
public class ClienteJavaGUI extends JFrame {

    private JTextField txtHost;
    private JTextField txtPuerto;
    private JTextField txtUsuario;
    private JButton btnConectar;
    private JLabel lblEstado;
    private JTextPane textPaneHistorial;
    private StyledDocument doc;
    private JTextField txtMensaje;
    private JButton btnEnviar;
    private JButton btnSalir;

    private Socket socket;
    private PrintWriter out;
    private BufferedReader in;
    private volatile boolean conectado;
    private Thread listenerThread;

    private static final DateTimeFormatter TIME_FMT = DateTimeFormatter.ofPattern("HH:mm:ss");

    public ClienteJavaGUI() {
        super("Cliente Sockets TCP/IP - Java Swing (TecNM)");
        configurarUI();
    }

    private void configurarUI() {
        try {
            UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        } catch (Exception ignored) {}

        setSize(700, 560);
        setMinimumSize(new Dimension(600, 450));
        setLocationRelativeTo(null);
        setDefaultCloseOperation(JFrame.DO_NOTHING_ON_CLOSE);
        setLayout(new BorderLayout(5, 5));

        // 1. Panel Superior: Conexión
        JPanel pnlConexion = new JPanel(new FlowLayout(FlowLayout.LEFT, 10, 10));
        pnlConexion.setBackground(Color.WHITE);
        pnlConexion.setBorder(BorderFactory.createMatteBorder(0, 0, 1, 0, new Color(220, 224, 230)));

        pnlConexion.add(new JLabel("Host / IP:"));
        txtHost = new JTextField("127.0.0.1", 9);
        pnlConexion.add(txtHost);

        pnlConexion.add(new JLabel("Puerto:"));
        txtPuerto = new JTextField("5000", 4);
        pnlConexion.add(txtPuerto);

        pnlConexion.add(new JLabel("Usuario:"));
        txtUsuario = new JTextField("Usuario-Java", 9);
        pnlConexion.add(txtUsuario);

        btnConectar = new JButton("Conectar");
        btnConectar.setBackground(new Color(37, 99, 235));
        btnConectar.setForeground(Color.WHITE);
        btnConectar.setFocusPainted(false);
        btnConectar.setCursor(new Cursor(Cursor.HAND_CURSOR));
        btnConectar.addActionListener(e -> toggleConexion());
        pnlConexion.add(btnConectar);

        lblEstado = new JLabel("🔴 Desconectado");
        lblEstado.setFont(new Font("SansSerif", Font.BOLD, 12));
        lblEstado.setForeground(Color.RED);
        pnlConexion.add(lblEstado);

        add(pnlConexion, BorderLayout.NORTH);

        // 2. Área Central: Historial de Chat con colores
        textPaneHistorial = new JTextPane();
        textPaneHistorial.setEditable(false);
        textPaneHistorial.setFont(new Font("Monospaced", Font.PLAIN, 13));
        textPaneHistorial.setBackground(new Color(250, 252, 255));
        doc = textPaneHistorial.getStyledDocument();

        JScrollPane scrollPane = new JScrollPane(textPaneHistorial);
        scrollPane.setBorder(BorderFactory.createEmptyBorder(5, 5, 5, 5));
        add(scrollPane, BorderLayout.CENTER);

        // 3. Panel Inferior: Envío de Mensajes
        JPanel pnlInferior = new JPanel(new BorderLayout(8, 8));
        pnlInferior.setBackground(Color.WHITE);
        pnlInferior.setBorder(BorderFactory.createEmptyBorder(10, 12, 10, 12));

        txtMensaje = new JTextField();
        txtMensaje.setFont(new Font("SansSerif", Font.PLAIN, 13));
        txtMensaje.addActionListener(e -> enviarMensaje());
        pnlInferior.add(txtMensaje, BorderLayout.CENTER);

        JPanel pnlBotones = new JPanel(new FlowLayout(FlowLayout.RIGHT, 6, 0));
        pnlBotones.setBackground(Color.WHITE);

        btnEnviar = new JButton("Enviar Eco");
        btnEnviar.setBackground(new Color(16, 185, 129));
        btnEnviar.setForeground(Color.WHITE);
        btnEnviar.setFocusPainted(false);
        btnEnviar.setEnabled(false);
        btnEnviar.setCursor(new Cursor(Cursor.HAND_CURSOR));
        btnEnviar.addActionListener(e -> enviarMensaje());
        pnlBotones.add(btnEnviar);

        btnSalir = new JButton("QUIT");
        btnSalir.setBackground(new Color(239, 68, 68));
        btnSalir.setForeground(Color.WHITE);
        btnSalir.setFocusPainted(false);
        btnSalir.setEnabled(false);
        btnSalir.setCursor(new Cursor(Cursor.HAND_CURSOR));
        btnSalir.addActionListener(e -> desconectar(true));
        pnlBotones.add(btnSalir);

        pnlInferior.add(pnlBotones, BorderLayout.EAST);
        add(pnlInferior, BorderLayout.SOUTH);

        agregarMensajeSistema("Bienvenido al Cliente TCP Java. Ingresa la IP/Puerto y presiona 'Conectar'.");

        addWindowListener(new WindowAdapter() {
            @Override
            public void windowClosing(WindowEvent e) {
                desconectar(true);
                System.exit(0);
            }
        });
    }

    private void toggleConexion() {
        if (conectado) {
            desconectar(true);
            return;
        }

        String host = txtHost.getText().trim();
        int puerto;
        try {
            puerto = Integer.parseInt(txtPuerto.getText().trim());
        } catch (NumberFormatException ex) {
            JOptionPane.showMessageDialog(this, "Puerto inválido.", "Error", JOptionPane.WARNING_MESSAGE);
            return;
        }

        new Thread(() -> {
            try {
                SwingUtilities.invokeLater(() -> {
                    btnConectar.setEnabled(false);
                    agregarMensajeSistema("Conectando a " + host + ":" + puerto + "...");
                });

                socket = new Socket(host, puerto);
                out = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);
                in = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
                conectado = true;

                SwingUtilities.invokeLater(() -> {
                    lblEstado.setText("🟢 Conectado");
                    lblEstado.setForeground(new Color(16, 185, 129));
                    btnConectar.setText("Desconectar");
                    btnConectar.setBackground(new Color(107, 114, 128));
                    btnConectar.setEnabled(true);
                    btnEnviar.setEnabled(true);
                    btnSalir.setEnabled(true);
                    txtHost.setEnabled(false);
                    txtPuerto.setEnabled(false);
                    txtUsuario.setEnabled(false);
                    agregarMensajeSistema("¡Conexión establecida exitosamente!");
                });

                // Iniciar hilo de escucha
                listenerThread = new Thread(this::escucharServidor);
                listenerThread.start();

            } catch (Exception ex) {
                SwingUtilities.invokeLater(() -> {
                    agregarMensajeSistema("Error de conexión: " + ex.getMessage());
                    desconectar(false);
                    btnConectar.setEnabled(true);
                });
            }
        }).start();
    }

    private void enviarMensaje() {
        if (!conectado || out == null) return;

        String texto = txtMensaje.getText().trim();
        if (texto.isEmpty()) return;

        String usuario = txtUsuario.getText().trim();
        if (usuario.isEmpty()) usuario = "Usuario";

        String mensajeCompleto = "[" + usuario + "]: " + texto;
        out.println(mensajeCompleto);

        agregarMensajeUsuario(usuario, texto);
        txtMensaje.setText("");
        txtMensaje.requestFocus();
    }

    private void escucharServidor() {
        try {
            String respuesta;
            while (conectado && in != null && (respuesta = in.readLine()) != null) {
                final String msg = respuesta.trim();
                SwingUtilities.invokeLater(() -> agregarMensajeServidor(msg));
            }
        } catch (Exception ignored) {
        } finally {
            if (conectado) {
                SwingUtilities.invokeLater(() -> {
                    agregarMensajeSistema("El servidor ha cerrado la conexión.");
                    desconectar(false);
                });
            }
        }
    }

    private void desconectar(boolean enviarQuit) {
        if (!conectado && socket == null) return;

        conectado = false;
        try {
            if (enviarQuit && out != null) {
                out.println("QUIT");
            }
        } catch (Exception ignored) {}

        try { if (in != null) in.close(); } catch (Exception ignored) {}
        try { if (out != null) out.close(); } catch (Exception ignored) {}
        try { if (socket != null) socket.close(); } catch (Exception ignored) {}

        in = null;
        out = null;
        socket = null;

        SwingUtilities.invokeLater(() -> {
            lblEstado.setText("🔴 Desconectado");
            lblEstado.setForeground(Color.RED);
            btnConectar.setText("Conectar");
            btnConectar.setBackground(new Color(37, 99, 235));
            btnConectar.setEnabled(true);
            btnEnviar.setEnabled(false);
            btnSalir.setEnabled(false);
            txtHost.setEnabled(true);
            txtPuerto.setEnabled(true);
            txtUsuario.setEnabled(true);
        });
    }

    private void agregarMensajeUsuario(String usuario, String mensaje) {
        String hora = LocalTime.now().format(TIME_FMT);
        Color colorUser = obtenerColorUsuario(usuario);

        appendTexto("[" + hora + "] ", Color.GRAY, false);
        appendTexto("[" + usuario + "]: ", colorUser, true);
        appendTexto(mensaje + "\n", Color.BLACK, false);
    }

    private void agregarMensajeServidor(String mensaje) {
        String hora = LocalTime.now().format(TIME_FMT);
        appendTexto("[" + hora + "] ", Color.GRAY, false);
        appendTexto("[SERVIDOR]: ", new Color(46, 117, 89), true);
        appendTexto(mensaje + "\n", new Color(30, 41, 59), false);
    }

    private void agregarMensajeSistema(String mensaje) {
        String hora = LocalTime.now().format(TIME_FMT);
        appendTexto("[" + hora + "] [SISTEMA] " + mensaje + "\n", new Color(100, 100, 100), false);
    }

    private void appendTexto(String texto, Color color, boolean negrita) {
        Style style = textPaneHistorial.addStyle("ColorStyle", null);
        StyleConstants.setForeground(style, color);
        StyleConstants.setBold(style, negrita);
        try {
            doc.insertString(doc.getLength(), texto, style);
            textPaneHistorial.setCaretPosition(doc.getLength());
        } catch (Exception ignored) {}
    }

    private Color obtenerColorUsuario(String usuario) {
        if ("Usuario-Java".equalsIgnoreCase(usuario)) {
            return new Color(168, 85, 247); // Morado
        }
        int hash = Math.abs(usuario.hashCode());
        int r = (hash & 0xFF) % 180;
        int g = ((hash >> 8) & 0xFF) % 180;
        int b = ((hash >> 16) & 0xFF) % 200 + 40;
        return new Color(r, g, b);
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            ClienteJavaGUI gui = new ClienteJavaGUI();
            gui.setVisible(true);
        });
    }
}
