$baseDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($baseDir)) { $baseDir = Get-Location }

$projectPath = Join-Path $baseDir "Cliente-Servidor-CSharp\ClienteGUI\ClienteGUI.csproj"

Write-Host "===================================================" -ForegroundColor Green
Write-Host "  Iniciando Interfaz Gráfica Cliente C# (.NET 10)" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green

dotnet run --project "$projectPath"
