param(
    [string]$HostDestino = "127.0.0.1",
    [int]$Puerto = 5000,
    [string]$Usuario = "Usuario-Java"
)

$baseDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($baseDir)) { $baseDir = Get-Location }

$jdkPath = "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin"
if (Test-Path "$jdkPath\java.exe") {
    $javaBin = "$jdkPath\java.exe"
    $javacBin = "$jdkPath\javac.exe"
} else {
    $javaBin = "java"
    $javacBin = "javac"
}

$javaDir = Join-Path $baseDir "Cliente-Servidor-Java"
$binDir = Join-Path $javaDir "bin"

if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
}

& $javacBin -encoding UTF-8 -d "$binDir" (Join-Path $javaDir "ClienteJava.java")

Write-Host "===================================================" -ForegroundColor Green
Write-Host "  Iniciando Cliente Consola Java (${HostDestino}:${Puerto})" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green

& $javaBin -cp "$binDir" ClienteJava $HostDestino $Puerto $Usuario
