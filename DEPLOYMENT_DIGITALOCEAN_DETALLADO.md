# 🚀 Guía Detallada de Despliegue en DigitalOcean - EventConnect (Frontend + Backend)

## ✅ Respuesta Rápida

**Sí, es posible tener frontend y backend en la misma app de DigitalOcean.** App Platform soporta múltiples servicios (components) en una sola aplicación.

## 📋 Problema: "No components detected"

Este error ocurre porque DigitalOcean no puede detectar automáticamente la estructura del proyecto. La solución es usar un archivo de configuración `.do/app.yaml`.

## 🔧 Solución: Archivo de Configuración

He creado el archivo `.do/app.yaml` en la raíz del proyecto que define ambos componentes (backend y frontend).

### Ubicación del archivo
```
EventConnect1.0/
├── .do/
│   └── app.yaml          ← Archivo de configuración
├── EventConnect.API/     ← Backend
├── frontend/
│   └── apps/
│       └── host/         ← Frontend
└── ...
```

## 📝 Pasos para Desplegar

### Paso 1: Actualizar `.do/app.yaml`

Antes de conectar el repositorio, actualiza estos valores en `.do/app.yaml`:

```yaml
github:
  repo: tu-usuario/EventConnect  # ← Cambia por tu usuario/repo
  branch: main                   # ← Cambia si usas otro branch
```

### Paso 2: Crear App en DigitalOcean

1. Ve a **App Platform** → **Create App**
2. Selecciona **GitHub** o **GitLab**
3. Conecta tu repositorio
4. Selecciona el branch (`main` o `master`)
5. **DigitalOcean detectará automáticamente el archivo `.do/app.yaml`**

### Paso 3: Verificar Componentes Detectados

Deberías ver **2 componentes**:
- ✅ **api** (Backend .NET)
- ✅ **frontend** (Frontend Next.js)

### Paso 4: Configurar Variables de Entorno

En **Settings** → **App-Level Environment Variables**, agrega:

```bash
# JWT Secret (genera uno seguro)
JWT_SECRET=<GENERA_UN_SECRET_SEGURO_MÍNIMO_32_CARACTERES>

# URL del frontend (ajusta según tu dominio)
FRONTEND_URL=https://tu-app-xxxxx.ondigitalocean.app
```

**Generar JWT Secret:**
```powershell
# PowerShell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

### Paso 5: Conectar Base de Datos PostgreSQL

**Si usas Managed Database de DigitalOcean:**

1. En **Components**, haz clic en **Add Component** → **Database**
2. Selecciona tu base de datos PostgreSQL
3. Esto inyectará automáticamente `${db.DATABASE_URL}`

**Si usas base de datos externa (como la que ya tienes):**

1. En **Settings** → **App-Level Environment Variables**
2. Agrega:
   ```bash
   ConnectionStrings__EventConnectConnection=postgres://usuario:password@host:5432/database
   ```
3. **IMPORTANTE**: Comenta o elimina la sección `databases:` en `.do/app.yaml`

### Paso 6: Configurar Health Checks

**Backend (api):**
- **Path**: `/health`
- **Initial Delay**: 60 seconds

**Frontend (frontend):**
- **Path**: `/`
- **Initial Delay**: 60 seconds

### Paso 7: Configurar Rutas

Las rutas ya están configuradas en `app.yaml`:
- **Backend**: `/api/*` → Componente `api`
- **Frontend**: `/*` → Componente `frontend`

### Paso 8: Desplegar

1. Haz clic en **Save** y luego **Create Resources**
2. DigitalOcean comenzará a construir ambos componentes
3. El proceso puede tomar 10-15 minutos
4. Una vez completado, tendrás una URL como: `https://tu-app-xxxxx.ondigitalocean.app`

## 🔍 Verificación Post-Despliegue

### Verificar Backend

```bash
# Health check
curl https://tu-app-xxxxx.ondigitalocean.app/api/health

# Debería responder: {"status":"Healthy"}
```

### Verificar Frontend

```bash
# Abre en el navegador
https://tu-app-xxxxx.ondigitalocean.app
```

## 🛠️ Configuración Manual (Si app.yaml no funciona)

Si DigitalOcean no detecta el archivo `app.yaml`, puedes configurar manualmente:

### Backend (Componente 1)

1. **Add Component** → **Service**
2. Configura:
   - **Name**: `api`
   - **Source Directory**: `/` (raíz del repo)
   - **Build Command**: 
     ```bash
     cd EventConnect.API && dotnet restore && dotnet publish -c Release -o ./publish
     ```
   - **Run Command**: 
     ```bash
     cd EventConnect.API/publish && dotnet EventConnect.API.dll
     ```
   - **HTTP Port**: `8080`
   - **Environment**: `.NET 9.0`

### Frontend (Componente 2)

1. **Add Component** → **Service**
2. Configura:
   - **Name**: `frontend`
   - **Source Directory**: `frontend/apps/host`
   - **Build Command**: 
     ```bash
     corepack enable && corepack prepare pnpm@latest --activate && pnpm install && pnpm build
     ```
   - **Run Command**: 
     ```bash
     pnpm start
     ```
   - **HTTP Port**: `3000`
   - **Environment**: `Node.js`

## ⚙️ Variables de Entorno por Componente

### Backend (api)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__EventConnectConnection=${db.DATABASE_URL}
JwtSettings__Secret=${JWT_SECRET}
JwtSettings__Issuer=EventConnect
JwtSettings__Audience=EventConnectClients
JwtSettings__TokenExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7
AllowedCorsOrigins__0=${FRONTEND_URL}
```

### Frontend (frontend)

```bash
NODE_ENV=production
NEXT_PUBLIC_API_URL=${api.PUBLIC_URL}
```

**Nota**: `${api.PUBLIC_URL}` se resuelve automáticamente a la URL del componente `api`.

## 🔄 Actualizar el Repositorio en app.yaml

Si tu repositorio tiene un nombre diferente o está en otra organización:

1. Edita `.do/app.yaml`
2. Cambia:
   ```yaml
   github:
     repo: tu-usuario/EventConnect  # ← Tu usuario/repo real
     branch: main                   # ← Tu branch
   ```
3. Haz commit y push
4. DigitalOcean detectará los cambios automáticamente

## 🆘 Solución de Problemas

### Error: "No components detected"

**Solución:**
1. Verifica que el archivo `.do/app.yaml` esté en la raíz del repositorio
2. Verifica que el repositorio esté conectado correctamente
3. Si persiste, configura manualmente (ver sección arriba)

### Error: Build falla en Backend

**Verifica:**
1. Que el `source_dir` sea correcto
2. Que el `build_command` compile correctamente
3. Revisa los logs de build en DigitalOcean

### Error: Build falla en Frontend

**Verifica:**
1. Que `pnpm` esté disponible (el comando `corepack enable` lo instala)
2. Que el `source_dir` apunte a `frontend/apps/host`
3. Revisa los logs de build

### Error: Frontend no puede conectar al Backend

**Verifica:**
1. Que `NEXT_PUBLIC_API_URL` esté configurado correctamente
2. Que el backend esté funcionando (`/api/health`)
3. Que CORS esté configurado correctamente en el backend

## 📊 Estructura Final

Después del despliegue, tendrás:

```
https://tu-app-xxxxx.ondigitalocean.app/
├── /                    → Frontend (Next.js)
├── /api/health          → Backend Health Check
├── /api/auth/login      → Backend API
├── /api/categoria       → Backend API
└── ...                  → Más endpoints del backend
```

## 💰 Costos Estimados

- **Backend (Basic XXS)**: ~$5/mes
- **Frontend (Basic XXS)**: ~$5/mes
- **Base de datos PostgreSQL (si usas Managed)**: ~$15/mes
- **Total**: ~$25/mes (con Managed DB) o ~$10/mes (con DB externa)

## ✅ Checklist Final

- [ ] Archivo `.do/app.yaml` creado y actualizado con tu repo
- [ ] Repositorio conectado en DigitalOcean
- [ ] 2 componentes detectados (api y frontend)
- [ ] Variables de entorno configuradas
- [ ] Base de datos conectada (Managed o externa)
- [ ] Health checks configurados
- [ ] Build exitoso para ambos componentes
- [ ] Backend accesible en `/api/health`
- [ ] Frontend accesible en `/`
- [ ] Frontend puede comunicarse con el backend

---

¡Listo para desplegar! 🚀
