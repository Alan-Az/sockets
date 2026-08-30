param(
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

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  Compilando modulo Java..." -ForegroundColor Yellow

if (-not (Test-Path $binDir)) {
    New-Item -ItemType Directory -Path $binDir -Force | Out-Null
}

$javaSources = (Get-ChildItem -Recurse -Filter *.java $srcDir).FullName
& $javacBin -encoding UTF-8 -d $binDir $javaSources

Write-Host "  Iniciando Servidor TCP Java 21 (Puerto $Puerto)" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

& $javaBin -cp "$binDir" com.tecnm.sockets.app.ServidorApp $Puerto
