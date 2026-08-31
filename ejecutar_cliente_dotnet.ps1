param(
    [string]$HostDestino = "127.0.0.1",
    [int]$Puerto = 5000,
    [string]$Usuario = "Usuario-CSharp"
)

$baseDir = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($baseDir)) { $baseDir = Get-Location }

$projectPath = Join-Path $baseDir "Cliente-Servidor-CSharp\Cliente\Cliente.csproj"

Write-Host "===================================================" -ForegroundColor Green
Write-Host "  Iniciando Cliente Consola C# (${HostDestino}:${Puerto})" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green

dotnet run --project "$projectPath" -- $HostDestino $Puerto $Usuario
