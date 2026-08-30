using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int p) ? p : 5000;

Console.WriteLine($"[C# Cliente] Conectando a {host}:{port}...");

try
{
    using var client = new TcpClient();
    await client.ConnectAsync(host, port);
    await using var stream = client.GetStream();

    Console.WriteLine("[C# Cliente] Conexión establecida exitosamente.");
    Console.WriteLine("[C# Cliente] Escribe mensajes para enviar al servidor (Escribe 'QUIT' para salir):");

    byte[] buffer = new byte[1024];

    while (true)
    {
        Console.Write("> ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        byte[] data = Encoding.UTF8.GetBytes(input + "\n");
        await stream.WriteAsync(data, 0, data.Length);

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead == 0)
        {
            Console.WriteLine("[C# Cliente] El servidor ha cerrado la conexión.");
            break;
        }

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        Console.WriteLine($"[C# Cliente] Respuesta: {response}");

        if (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("SALIR", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[C# Cliente] Desconectado.");
            break;
        }
    }
}
catch (SocketException ex)
{
    Console.WriteLine($"[C# Cliente Error] No se pudo conectar al servidor en {host}:{port}. ({ex.Message})");
}
catch (Exception ex)
{
    Console.WriteLine($"[C# Cliente Error] {ex.Message}");
}
