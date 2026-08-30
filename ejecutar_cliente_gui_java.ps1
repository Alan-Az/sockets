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

Write-Host "===================================================" -ForegroundColor Green
Write-Host "  Compilando e Iniciando Interfaz Gráfica Java" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green

& $javacBin -encoding UTF-8 -d "$binDir" (Join-Path $javaDir "ClienteJavaGUI.java")
& $javaBin -cp "$binDir" ClienteJavaGUI
