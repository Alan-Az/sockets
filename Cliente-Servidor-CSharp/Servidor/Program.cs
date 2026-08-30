using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = IPAddress.Any;
int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int p))
{
    port = p;
}

var listener = new TcpListener(ipAddress, port);
listener.Start();
Console.WriteLine($"[Servidor C#] Servidor de Chat Multiusuario escuchando en el puerto {port}...");

// Almacena todas las conexiones activas para difusión masiva (Broadcast)
var clientesConectados = new ConcurrentDictionary<string, (TcpClient client, NetworkStream stream)>();

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    _ = Task.Run(() => HandleClientAsync(client));
}

async Task HandleClientAsync(TcpClient client)
{
    var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Desconocido";
    var clientId = Guid.NewGuid().ToString("N")[..8];
    var stream = client.GetStream();

    clientesConectados[clientId] = (client, stream);
    Console.WriteLine($"[C# Servidor] Cliente conectado desde {endpoint} (Usuarios en línea: {clientesConectados.Count})");

    byte[] buffer = new byte[2048];
    try
    {
        while (client.Connected)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // Desconexión

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            if (string.IsNullOrWhiteSpace(message)) continue;

            Console.WriteLine($"[Mensaje] {endpoint}: {message}");

            if (message.Equals("QUIT", StringComparison.OrdinalIgnoreCase) ||
                message.Equals("SALIR", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            // DIFUSIÓN A TODOS LOS USUARIOS CONECTADOS (BROADCAST)
            await BroadcastAsync(message);
        }
    }
    catch (IOException)
    {
        // Desconexión abrupta
    }
    finally
    {
        clientesConectados.TryRemove(clientId, out _);
        client.Dispose();
        Console.WriteLine($"[C# Servidor] Conexión finalizada con {endpoint} (Usuarios en línea: {clientesConectados.Count})");
    }
}

async Task BroadcastAsync(string mensaje)
{
    byte[] bytes = Encoding.UTF8.GetBytes(mensaje + "\n");

    foreach (var kvp in clientesConectados)
    {
        try
        {
            if (kvp.Value.client.Connected)
            {
                await kvp.Value.stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        catch
        {
            clientesConectados.TryRemove(kvp.Key, out _);
        }
    }
}
