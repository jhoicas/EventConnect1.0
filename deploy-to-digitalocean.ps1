# Script para desplegar a Digital Ocean
# Ejecutar desde la raiz del proyecto EventConnect1.0

Write-Host "=== EventConnect - Deploy a Digital Ocean ===" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar cambios
Write-Host "Paso 1: Verificando cambios..." -ForegroundColor Yellow
git status

Write-Host ""
Write-Host "Deseas continuar con el commit y push? (s/n): " -NoNewline -ForegroundColor Green
$continue = Read-Host

if ($continue -ne 's' -and $continue -ne 'S') {
    Write-Host "Deploy cancelado." -ForegroundColor Red
    exit
}

# Paso 2: Commit y Push
Write-Host ""
Write-Host "Paso 2: Haciendo commit de cambios..." -ForegroundColor Yellow
git add .
git commit -m "Fix: CORS configuration and enable Swagger in production with env var"
git push

Write-Host ""
Write-Host "Cambios enviados al repositorio" -ForegroundColor Green
Write-Host ""

# Paso 3: Instrucciones para Digital Ocean
Write-Host "=== SIGUIENTE: Configurar Digital Ocean ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ve a Digital Ocean App Platform y:" -ForegroundColor White
Write-Host "  1. Abre tu app: eventconnect-api-8oih6" -ForegroundColor Gray
Write-Host "  2. Ve a Settings > App-Level Environment Variables" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Verifica/Agrega estas variables:" -ForegroundColor Yellow
Write-Host ""
Write-Host "     ConnectionStrings__EventConnectConnection" -ForegroundColor Cyan
Write-Host "       Valor: Host=TU-DB-HOST;Port=25060;Database=eventconnect;Username=doadmin;Password=TU-PASSWORD;SSL Mode=Require" -ForegroundColor Gray
Write-Host ""
Write-Host "     JwtSettings__Secret" -ForegroundColor Cyan
Write-Host "       Valor: [string de al menos 32 caracteres aleatorios]" -ForegroundColor Gray
Write-Host ""
Write-Host "     EnableSwagger" -ForegroundColor Cyan
Write-Host "       Valor: true" -ForegroundColor Gray
Write-Host ""
Write-Host "  4. Guarda los cambios" -ForegroundColor Gray
Write-Host "  5. Espera el rebuild automatico o hazlo manual en la pestana Deployments" -ForegroundColor Gray
Write-Host ""
Write-Host "Cuando termine el deployment, ejecuta: .\test-api.ps1" -ForegroundColor White
Write-Host ""
