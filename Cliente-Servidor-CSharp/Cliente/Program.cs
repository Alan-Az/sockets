using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int p) ? p : 5000;
string usuario = args.Length > 2 ? args[2] : "Usuario-CSharp";

Console.WriteLine($"[C# Cliente] Conectando a {host}:{port} como '{usuario}'...");

try
{
    using var client = new TcpClient();
    await client.ConnectAsync(host, port);
    await using var stream = client.GetStream();
    using var cts = new CancellationTokenSource();

    Console.WriteLine("[C# Cliente] Conexión establecida exitosamente.");
    Console.WriteLine("[C# Cliente] Escribe mensajes para enviar (Escribe 'QUIT' para salir):");

    // Hilo de lectura en segundo plano para recibir mensajes de otros usuarios
    _ = Task.Run(async () =>
    {
        byte[] buffer = new byte[2048];
        try
        {
            while (!cts.Token.IsCancellationRequested && client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0) break;

                string recibido = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (!string.IsNullOrEmpty(recibido))
                {
                    Console.WriteLine($"\n{recibido}\n> ");
                }
            }
        }
        catch { }
    });

    while (true)
    {
        Console.Write("> ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        if (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("SALIR", StringComparison.OrdinalIgnoreCase))
        {
            byte[] quitBytes = Encoding.UTF8.GetBytes("QUIT\n");
            await stream.WriteAsync(quitBytes);
            cts.Cancel();
            Console.WriteLine("[C# Cliente] Desconectado.");
            break;
        }

        string mensajeConUsuario = $"[{usuario}]: {input}";
        byte[] data = Encoding.UTF8.GetBytes(mensajeConUsuario + "\n");
        await stream.WriteAsync(data, 0, data.Length);
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
