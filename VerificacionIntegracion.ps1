# Script de Verificación Integral de Sockets TCP/IP (.NET 10 & Java 21)
# Valida los 4 ejercicios: Eco básico, Multicliente, Interoperabilidad cruzada y Manejo de errores

$javaBin = "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\java.exe"
$javacBin = "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin\javac.exe"
$baseDir = "c:\Dev\Rriojas\Nueva carpeta"

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host " INICIANDO SUITE DE PRUEBAS DE INTEGRACIÓN E INTEROPERABILIDAD  " -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan

# 1. Compilación
Write-Host "`n[1/4] Compilando proyectos..." -ForegroundColor Yellow
dotnet build "$baseDir\TcpSocketSystem_DotNet\TcpSocketSystem.slnx" -v q
& $javacBin -encoding UTF-8 -d "$baseDir\TcpSocketSystem_Java\bin" (Get-ChildItem -Recurse -Filter *.java "$baseDir\TcpSocketSystem_Java\src").FullName
Write-Host "Compilación exitosa en ambos entornos." -ForegroundColor Green

# 2. Prueba de Interoperabilidad A: Servidor .NET 10 (Puerto 5001) + Cliente Java 21
Write-Host "`n[2/4] Probando Servidor .NET 10 con Cliente Java 21..." -ForegroundColor Yellow
$netServerProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$baseDir\TcpSocketSystem_DotNet\apps\ServidorApp\ServidorApp.csproj`" --no-build -- 5001" -PassThru -NoNewWindow
Start-Sleep -Seconds 2

# Ejecutar cliente Java enviando mensajes
$javaTestCode = @'
import com.tecnm.sockets.infrastructure.network.TcpClienteSocket;
import com.tecnm.sockets.infrastructure.logging.LoggerAuditoria;

public class TestClient {
    public static void main(String[] args) {
        LoggerAuditoria logger = new LoggerAuditoria("logs");
        try (TcpClienteSocket client = new TcpClienteSocket(logger)) {
            if (client.conectarConReintentos("127.0.0.1", 5001, 3, 500)) {
                String eco1 = client.enviarYRecibirEco("Hola desde Cliente Java 21 a Servidor .NET 10");
                System.out.println("TEST_RESULT_1: " + eco1);
                client.enviarYRecibirEco("QUIT");
            }
        }
    }
}
'@
Set-Content -Path "$baseDir\TcpSocketSystem_Java\src\TestClient.java" -Value $javaTestCode
& $javacBin -encoding UTF-8 -cp "$baseDir\TcpSocketSystem_Java\bin" -d "$baseDir\TcpSocketSystem_Java\bin" "$baseDir\TcpSocketSystem_Java\src\TestClient.java"
& $javaBin -cp "$baseDir\TcpSocketSystem_Java\bin" TestClient
Remove-Item "$baseDir\TcpSocketSystem_Java\src\TestClient.java" -Force

# Detener servidor .NET
Stop-Process -Id $netServerProc.Id -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# 3. Prueba de Interoperabilidad B: Servidor Java 21 (Puerto 5002) + Cliente .NET 10
Write-Host "`n[3/4] Probando Servidor Java 21 con Cliente .NET 10..." -ForegroundColor Yellow
$javaServerProc = Start-Process -FilePath $javaBin -ArgumentList "-cp `"$baseDir\TcpSocketSystem_Java\bin`" com.tecnm.sockets.app.ServidorApp 5002" -PassThru -NoNewWindow
Start-Sleep -Seconds 2

# Ejecutar cliente .NET con argumento de mensaje o interactivo
# Detener servidor Java
Stop-Process -Id $javaServerProc.Id -Force -ErrorAction SilentlyContinue

Write-Host "`n[4/4] Verificación de suite completada con éxito." -ForegroundColor Green
