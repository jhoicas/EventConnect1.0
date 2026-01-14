# 🚀 Guía de Despliegue en DigitalOcean - EventConnect

Esta guía te ayudará a desplegar EventConnect en DigitalOcean App Platform siguiendo las mejores prácticas de seguridad y arquitectura.

## 📋 Requisitos Previos

- ✅ Cuenta en DigitalOcean
- ✅ Repositorio en GitHub/GitLab
- ✅ Base de datos PostgreSQL configurada
- ✅ URI de conexión PostgreSQL lista

## 🏗️ Arquitectura de Despliegue

Se recomienda **desplegar backend y frontend en apps separadas** para:
- Escalado independiente
- Despliegues independientes
- Mejor gestión de recursos
- Separación de responsabilidades

---

## 🔧 App 1: Backend API (.NET)

### Paso 1: Crear App en DigitalOcean

1. Ve a **App Platform** → **Create App**
2. Conecta tu repositorio: `jhoicas/EventConnect1.0`
3. Branch: `main`
4. **Source Directory**: **DEJAR VACÍO** (raíz del repositorio)
5. DigitalOcean detectará automáticamente el `Dockerfile`

### Paso 2: Configurar Componente Backend

**Información Básica:**
- **Name**: `api`
- **Type**: Web Service
- **Source Directory**: (vacío - raíz)
- **Dockerfile Path**: `Dockerfile`

**Build Settings:**
- **Build Strategy**: Dockerfile (detectado automáticamente)
- No requiere Build Command ni Run Command (el Dockerfile los define)

**HTTP Settings:**
- **Public HTTP Port**: `8080`
- **HTTP Request Routes**: 
  - Path: `/api` (o `/` si quieres que sea la raíz)
  - Component: `api`

**Health Check:**
- **HTTP Path**: `/health`
- **Initial Delay**: `60` seconds
- **Period**: `10` seconds
- **Timeout**: `5` seconds

**Instance Size:**
- **Plan**: Basic ($12/mes) o Professional según necesidades
- **RAM**: 1 GB mínimo
- **vCPU**: 1 Shared vCPU mínimo

### Paso 3: Variables de Entorno

Agrega estas variables en **App-Level Environment Variables**:

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Base de Datos PostgreSQL
ConnectionStrings__EventConnectConnection=<TU_URI_POSTGRESQL_COMPLETA>

# JWT Settings
JwtSettings__Secret=<GENERA_UN_SECRET_SEGURO_MÍNIMO_32_CARACTERES>
JwtSettings__Issuer=EventConnect
JwtSettings__Audience=EventConnectClients
JwtSettings__TokenExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7

# CORS (actualizar después de crear el frontend)
AllowedCorsOrigins__0=<URL_DEL_FRONTEND>
```

**Generar JWT Secret seguro:**
```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

**Formato Connection String PostgreSQL:**
```
Host=host;Port=5432;Database=database;Username=user;Password=password;SslMode=Require
```

### Paso 4: Desplegar

1. Haz clic en **"Create Resources"**
2. DigitalOcean construirá la imagen Docker y desplegará
3. Anota la URL del backend (ej: `https://api-xxxxx.ondigitalocean.app`)

---

## 🎨 App 2: Frontend (Next.js) - Si aplica

Si tienes frontend, crea una segunda app:

### Paso 1: Crear App Frontend

1. Ve a **App Platform** → **Create App** (nueva app)
2. Conecta el mismo repositorio: `jhoicas/EventConnect1.0`
3. Branch: `main`
4. **Source Directory**: `frontend/apps/host`

### Paso 2: Configurar Componente Frontend

**Información Básica:**
- **Name**: `frontend`
- **Type**: Web Service
- **Source Directory**: `frontend/apps/host`
- **Environment**: Node.js

**Build Settings:**
- **Build Command**: 
  ```bash
  corepack enable && corepack prepare pnpm@latest --activate && pnpm install && pnpm build
  ```
- **Run Command**: 
  ```bash
  pnpm start
  ```

**HTTP Settings:**
- **Public HTTP Port**: `3000`
- **HTTP Request Routes**: 
  - Path: `/`
  - Component: `frontend`

**Health Check:**
- **HTTP Path**: `/`
- **Initial Delay**: `60` seconds

**Instance Size:**
- **Plan**: Basic ($12/mes)
- **RAM**: 1 GB mínimo

### Paso 3: Variables de Entorno Frontend

```bash
NODE_ENV=production
NEXT_PUBLIC_API_URL=<URL_DEL_BACKEND>
```

### Paso 4: Actualizar CORS en Backend

Después de crear el frontend, actualiza `AllowedCorsOrigins__0` en el backend con la URL del frontend.

---

## ✅ Verificación Post-Despliegue

### Backend

```bash
# Health check
curl https://api-xxxxx.ondigitalocean.app/health

# Debería responder: {"status":"Healthy"}
```

### Frontend

Abre en el navegador:
```
https://app-xxxxx.ondigitalocean.app
```

---

## 🔒 Seguridad

### Checklist de Seguridad

- [ ] ✅ JWT Secret configurado (mínimo 32 caracteres)
- [ ] ✅ Connection String no expuesta en código
- [ ] ✅ CORS configurado con dominios específicos
- [ ] ✅ HTTPS habilitado (automático en App Platform)
- [ ] ✅ Variables de entorno configuradas
- [ ] ✅ Health checks configurados
- [ ] ✅ Swagger deshabilitado en producción

### Variables Sensibles

**NUNCA** commitees estas variables:
- `ConnectionStrings__EventConnectConnection`
- `JwtSettings__Secret`

Siempre usa variables de entorno en DigitalOcean.

---

## 📊 Monitoreo

### Health Checks

El backend expone `/health` que verifica:
- Conexión a PostgreSQL
- Estado de la aplicación

### Logs

- **Backend**: Ve a **Runtime Logs** en DigitalOcean
- Los logs incluyen información estructurada del Global Exception Handler

---

## 🆘 Solución de Problemas

### Error: "Connection string not found"

**Solución**: Verifica que `ConnectionStrings__EventConnectConnection` esté configurada en variables de entorno.

### Error: "JWT Secret not configured"

**Solución**: Verifica que `JwtSettings__Secret` esté configurada.

### Error: CORS bloqueado

**Solución**: 
1. Verifica que `AllowedCorsOrigins__0` tenga la URL correcta del frontend
2. Verifica que el frontend esté usando la URL correcta del backend

### Error: Build falla

**Solución**:
1. Verifica que el Source Directory sea la raíz (vacío) para el backend
2. Verifica que el Dockerfile esté en la raíz
3. Revisa los logs de build en DigitalOcean

---

## 📝 Archivos de Configuración

### Dockerfile (Raíz)
- ✅ Optimizado para producción
- ✅ Multi-stage build
- ✅ Usuario no-root para seguridad
- ✅ Health check incluido

### .dockerignore (Raíz)
- ✅ Excluye archivos innecesarios del build
- ✅ Reduce tamaño de imagen

---

## 💰 Costos Estimados

- **Backend**: ~$12/mes (Basic, 1GB RAM)
- **Frontend**: ~$12/mes (Basic, 1GB RAM) - si aplica
- **Base de Datos**: Externa (PostgreSQL que ya tienes)
- **Total**: ~$24/mes (con frontend) o ~$12/mes (solo backend)

---

## 🎯 Próximos Pasos

1. ✅ Desplegar backend
2. ✅ Verificar health check
3. ✅ Configurar variables de entorno
4. ✅ Probar endpoints de la API
5. ⏳ Desplegar frontend (si aplica)
6. ⏳ Configurar dominio personalizado
7. ⏳ Configurar CI/CD para despliegues automáticos

---

¡Listo para desplegar! 🚀
