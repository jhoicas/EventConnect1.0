# Digital Ocean Deployment - Troubleshooting Guide

## Cambios Realizados

### 1. CORS Configuración Mejorada
- ✅ Cambiado de `.WithHeaders()` a `.AllowAnyHeader()` para permitir todos los headers necesarios
- Esto resuelve los errores de preflight OPTIONS

### 2. Swagger en Producción
- ✅ Swagger ahora puede habilitarse en producción mediante variable de entorno
- Por defecto está deshabilitado en producción (por seguridad)

### 3. Logging Mejorado
- ✅ Se agregó logging de inicio para diagnosticar problemas de conexión

## Variables de Entorno Requeridas en Digital Ocean

Asegúrate de configurar estas variables en tu App de Digital Ocean:

```bash
# Base de datos (REQUERIDO)
ConnectionStrings__EventConnectConnection="Host=your-db-host;Database=eventconnect;Username=your-user;Password=your-password;SSL Mode=Require"

# JWT Settings (REQUERIDO)
JwtSettings__Secret="tu-secreto-super-seguro-de-al-menos-32-caracteres"
JwtSettings__Issuer="EventConnect"
JwtSettings__Audience="EventConnectClients"

# Habilitar Swagger en producción (OPCIONAL - solo para debugging)
EnableSwagger=true

# Entorno
ASPNETCORE_ENVIRONMENT=Production
```

## Pasos para Corregir el Error 500

### Paso 1: Verificar Variables de Entorno
En Digital Ocean App Platform:
1. Ve a tu app → Settings → App-Level Environment Variables
2. Verifica que `ConnectionStrings__EventConnectConnection` esté configurado correctamente
3. Verifica que `JwtSettings__Secret` esté configurado

### Paso 2: Habilitar Swagger Temporalmente (para debugging)
Agrega esta variable de entorno:
```
EnableSwagger=true
```

### Paso 3: Ver Logs en Tiempo Real
```bash
# En Digital Ocean, ve a tu app → Runtime Logs
# Busca mensajes como:
# - "Connection String Configured: True/False"
# - "Error en login para usuario: superadmin"
```

### Paso 4: Verificar la Conexión a Base de Datos

El formato correcto para PostgreSQL en Digital Ocean debe ser:
```
Host=db-postgresql-xxxx.ondigitalocean.com;Port=25060;Database=eventconnect;Username=doadmin;Password=tu-password;SSL Mode=Require
```

### Paso 5: Rebuild y Deploy
Después de configurar las variables de entorno:
1. Commit los cambios del código
2. Push a tu repositorio
3. Digital Ocean automáticamente hará rebuild (o hazlo manual desde el dashboard)

## Verificar que CORS Funcione

Prueba esto en la consola del navegador desde tu frontend:
```javascript
fetch('https://eventconnect-api-8oih6.ondigitalocean.app/health')
  .then(r => r.text())
  .then(console.log)
  .catch(console.error)
```

Si funciona, CORS está correcto. Si no, verifica las variables de entorno.

## Comandos Útiles para Testing

### Probar Health Check
```bash
curl https://eventconnect-api-8oih6.ondigitalocean.app/health
```
Debe retornar "Healthy"

### Probar Login (cuando esté funcionando)
```bash
curl -X POST https://eventconnect-api-8oih6.ondigitalocean.app/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"Username":"superadmin","Password":"SuperAdmin123$"}'
```

### Ver Swagger (si está habilitado)
```
https://eventconnect-api-8oih6.ondigitalocean.app
```

## Errores Comunes y Soluciones

### Error: "Connection string not found"
- **Causa**: Variable de entorno mal configurada
- **Solución**: Usa el formato `ConnectionStrings__EventConnectConnection` (doble guion bajo)

### Error: "Could not establish connection to database"
- **Causa**: Database no accesible o SSL requerido
- **Solución**: Agrega `;SSL Mode=Require` al connection string

### Error: 401 Unauthorized en Login
- **Causa**: JWT Secret no configurado
- **Solución**: Configura `JwtSettings__Secret` con mínimo 32 caracteres

### CORS Error
- **Causa**: Frontend origin no permitido
- **Solución**: Verifica que `https://eventconnect-qihii.ondigitalocean.app` esté en AllowedCorsOrigins

## Checklist de Deployment

- [ ] ConnectionString configurado en variables de entorno
- [ ] JwtSettings__Secret configurado (mínimo 32 caracteres)
- [ ] Base de datos PostgreSQL creada y accesible
- [ ] Migraciones de base de datos ejecutadas
- [ ] Usuario superadmin creado en la base de datos
- [ ] CORS origins incluyen el dominio del frontend
- [ ] EnableSwagger=true (temporal para debugging)
- [ ] App rebuildeada después de cambios
- [ ] Health check responde correctamente
- [ ] Logs revisados para errores

## Próximos Pasos Después de Resolver el Error

1. **Quitar EnableSwagger en producción** por seguridad (dejarlo solo en desarrollo)
2. **Implementar rate limiting** para proteger el endpoint de login
3. **Configurar SSL/HTTPS** si no está ya configurado
4. **Monitoreo** de logs y performance
