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
Console.WriteLine($"[Servidor C#] Escuchando en el puerto {port}...");

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    _ = Task.Run(() => HandleClientAsync(client));
}

async Task HandleClientAsync(TcpClient client)
{
    var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Desconocido";
    Console.WriteLine($"[C# Servidor] Cliente conectado desde {endpoint}");

    using (client)
    await using (var stream = client.GetStream())
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Desconexión del cliente

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"[C# Servidor] Mensaje recibido de {endpoint}: {message}");

                if (message.Equals("QUIT", StringComparison.OrdinalIgnoreCase) ||
                    message.Equals("SALIR", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] quitResp = Encoding.UTF8.GetBytes("ADIOS\n");
                    await stream.WriteAsync(quitResp, 0, quitResp.Length);
                    break;
                }

                byte[] response = Encoding.UTF8.GetBytes($"ECO: {message}\n");
                await stream.WriteAsync(response, 0, response.Length);
            }
        }
        catch (IOException)
        {
            // Desconexión intempestiva del cliente
        }
        finally
        {
            Console.WriteLine($"[C# Servidor] Conexión finalizada con {endpoint}");
        }
    }
}
