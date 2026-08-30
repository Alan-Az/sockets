param(
    [int]$Puerto = 5000
)

$baseDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($baseDir)) { $baseDir = Get-Location }

$projectPath = Join-Path $baseDir "Cliente-Servidor-CSharp\Servidor\Servidor.csproj"

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  Iniciando Servidor TCP en .NET 10 (Puerto $Puerto)" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan

dotnet run --project "$projectPath" -- $Puerto
