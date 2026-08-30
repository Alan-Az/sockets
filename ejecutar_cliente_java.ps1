param(
    [string]$HostDestino = "127.0.0.1",
    [int]$Puerto = 5000
)

$baseDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($baseDir)) {
    $baseDir = Get-Location
}

$jdkPath = "C:\Program Files\Eclipse Adoptium\jdk-21.0.11.10-hotspot\bin"

if (Test-Path "$jdkPath\java.exe") {
    $javaBin = "$jdkPath\java.exe"
    $javacBin = "$jdkPath\javac.exe"
} else {
    $javaBin = "java"
    $javacBin = "javac"
}

$binDir = Join-Path $baseDir "TcpSocketSystem_Java\bin"
$srcDir = Join-Path $baseDir "TcpSocketSystem_Java\src"

# Compilar si aún no existen los binarios
if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
    Write-Host "Compilando módulo Java por primera vez..." -ForegroundColor Yellow
    $javaSources = (Get-ChildItem -Recurse -Filter *.java $srcDir).FullName
    & $javacBin -encoding UTF-8 -d $binDir $javaSources
}

Write-Host "===================================================" -ForegroundColor Green
Write-Host "  Iniciando Cliente TCP Java 21 (${HostDestino}:${Puerto})" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green

& $javaBin -cp "$binDir" com.tecnm.sockets.app.ClienteApp $HostDestino $Puerto
