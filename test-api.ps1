# Script para testear la API en Digital Ocean
$API_URL = "https://eventconnect-api-8oih6.ondigitalocean.app"

Write-Host "=== EventConnect - Test API en Digital Ocean ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Check
Write-Host "Test 1: Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/health" -Method GET -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Health Check OK: $($response.Content)" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Health Check FAILED: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  La API no está respondiendo. Verifica que el deployment haya terminado." -ForegroundColor Yellow
}
Write-Host ""

# Test 2: Swagger
Write-Host "Test 2: Swagger UI..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/swagger/index.html" -Method GET -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Swagger está disponible en: $API_URL" -ForegroundColor Green
        Write-Host "  Abre en tu navegador: $API_URL" -ForegroundColor Gray
    }
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✗ Swagger NO disponible (404)" -ForegroundColor Red
        Write-Host "  Verifica que EnableSwagger=true esté configurado en las variables de entorno" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Error al acceder a Swagger: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 3: CORS Preflight (OPTIONS)
Write-Host "Test 3: CORS Preflight (OPTIONS)..." -ForegroundColor Yellow
try {
    $headers = @{
        'Origin' = 'https://eventconnect-qihii.ondigitalocean.app'
        'Access-Control-Request-Method' = 'POST'
        'Access-Control-Request-Headers' = 'content-type'
    }
    $response = Invoke-WebRequest -Uri "$API_URL/api/Auth/login" -Method OPTIONS -Headers $headers -UseBasicParsing
    
    $corsHeader = $response.Headers['Access-Control-Allow-Origin']
    if ($corsHeader) {
        Write-Host "✓ CORS configurado correctamente" -ForegroundColor Green
        Write-Host "  Access-Control-Allow-Origin: $corsHeader" -ForegroundColor Gray
    } else {
        Write-Host "✗ CORS no configurado" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ CORS Preflight FAILED: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Login Endpoint (sin credenciales válidas)
Write-Host "Test 4: Login Endpoint..." -ForegroundColor Yellow
try {
    $body = @{
        Username = "superadmin"
        Password = "SuperAdmin123`$"
    } | ConvertTo-Json

    $headers = @{
        'Content-Type' = 'application/json'
        'Origin' = 'https://eventconnect-qihii.ondigitalocean.app'
    }

    $response = Invoke-WebRequest -Uri "$API_URL/api/Auth/login" -Method POST -Body $body -Headers $headers -UseBasicParsing
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Login exitoso!" -ForegroundColor Green
        $data = $response.Content | ConvertFrom-Json
        Write-Host "  Token recibido (primeros 20 chars): $($data.token.Substring(0, 20))..." -ForegroundColor Gray
    }
} catch {
    $statusCode = $_.Exception.Response.StatusCode.Value__
    
    if ($statusCode -eq 401) {
        Write-Host "⚠ Endpoint funciona pero credenciales inválidas (401)" -ForegroundColor Yellow
        Write-Host "  El usuario 'superadmin' no existe o la contraseña es incorrecta" -ForegroundColor Gray
    } elseif ($statusCode -eq 500) {
        Write-Host "✗ Error 500 - Internal Server Error" -ForegroundColor Red
        Write-Host "  Problema en el servidor. Revisa los logs de Digital Ocean:" -ForegroundColor Yellow
        Write-Host "  - Posible problema de conexión a base de datos" -ForegroundColor Gray
        Write-Host "  - Variables de entorno mal configuradas" -ForegroundColor Gray
        
        # Intentar leer el cuerpo de la respuesta
        try {
            $errorStream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorStream)
            $errorBody = $reader.ReadToEnd()
            Write-Host ""
            Write-Host "  Detalles del error:" -ForegroundColor Yellow
            Write-Host "  $errorBody" -ForegroundColor Gray
        } catch {}
    } else {
        Write-Host "✗ Error inesperado: $statusCode - $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Resumen
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si ves errores 500:" -ForegroundColor White
Write-Host "  1. Ve a Digital Ocean > Tu App > Runtime Logs" -ForegroundColor Gray
Write-Host "  2. Busca errores relacionados con:" -ForegroundColor Gray
Write-Host "     - Connection string not found" -ForegroundColor Yellow
Write-Host "     - Could not establish connection" -ForegroundColor Yellow
Write-Host "     - JWT Secret not configured" -ForegroundColor Yellow
Write-Host ""
Write-Host "Si Swagger no está disponible:" -ForegroundColor White
Write-Host "  - Verifica que EnableSwagger=true en Environment Variables" -ForegroundColor Gray
Write-Host "  - Asegúrate de que el deployment se completó" -ForegroundColor Gray
Write-Host ""
