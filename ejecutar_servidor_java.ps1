param(
    [int]$Puerto = 5000
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

& $javacBin -encoding UTF-8 -d "$binDir" (Join-Path $javaDir "ServidorJava.java")

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  Iniciando Servidor TCP en Java (Puerto $Puerto)" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

& $javaBin -cp "$binDir" ServidorJava $Puerto
