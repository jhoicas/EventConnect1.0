# 🔧 Solución: "No components detected" en DigitalOcean

## ⚠️ Problema

DigitalOcean no detecta automáticamente el archivo `.do/app.yaml` cuando conectas el repositorio desde la interfaz web. Este archivo solo funciona cuando usas `doctl` CLI o GitHub Actions.

## ✅ Solución: Configuración Manual (Recomendado)

Como estás usando la interfaz web, necesitas configurar los componentes manualmente. Sigue estos pasos:

### Paso 1: Conectar Repositorio

1. Ve a **App Platform** → **Create App**
2. Conecta tu repositorio de GitHub/GitLab
3. Selecciona el repositorio y branch
4. **Cuando veas "No components detected"**, haz clic en **"Edit"** o **"Add Component"**

### Paso 2: Configurar Backend (Componente 1)

1. Haz clic en **"Add Component"** → **"Service"**
2. Configura:

   **Información Básica:**
   - **Name**: `api`
   - **Source Directory**: `/` (raíz del repositorio)

   **Build Settings:**
   - **Build Command**: 
     ```bash
     cd EventConnect.API && dotnet restore && dotnet publish -c Release -o ./publish
     ```
   - **Run Command**: 
     ```bash
     cd EventConnect.API/publish && dotnet EventConnect.API.dll
     ```
   - **Environment**: Selecciona **.NET 9.0** (o la versión más cercana disponible)

   **HTTP Settings:**
   - **HTTP Port**: `8080`
   - **HTTP Request Routes**: 
     - Path: `/api`
     - Component: `api`

   **Health Check:**
   - **HTTP Path**: `/health`
   - **Initial Delay**: `60` seconds
   - **Period**: `10` seconds

   **Environment Variables** (agrega estas):
   ```bash
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ConnectionStrings__EventConnectConnection=<TU_URI_POSTGRESQL>
   JwtSettings__Secret=<GENERA_UN_SECRET_SEGURO>
   JwtSettings__Issuer=EventConnect
   JwtSettings__Audience=EventConnectClients
   JwtSettings__TokenExpirationMinutes=60
   JwtSettings__RefreshTokenExpirationDays=7
   AllowedCorsOrigins__0=https://tu-app-xxxxx.ondigitalocean.app
   ```

   **Instance Settings:**
   - **Instance Count**: `1`
   - **Instance Size**: `Basic XXS` ($5/mes)

### Paso 3: Configurar Frontend (Componente 2)

1. Haz clic en **"Add Component"** → **"Service"**
2. Configura:

   **Información Básica:**
   - **Name**: `frontend`
   - **Source Directory**: `frontend/apps/host`

   **Build Settings:**
   - **Build Command**: 
     ```bash
     corepack enable && corepack prepare pnpm@latest --activate && pnpm install && pnpm build
     ```
   - **Run Command**: 
     ```bash
     pnpm start
     ```
   - **Environment**: Selecciona **Node.js**

   **HTTP Settings:**
   - **HTTP Port**: `3000`
   - **HTTP Request Routes**: 
     - Path: `/`
     - Component: `frontend`

   **Health Check:**
   - **HTTP Path**: `/`
   - **Initial Delay**: `60` seconds
   - **Period**: `10` seconds

   **Environment Variables** (agrega estas):
   ```bash
   NODE_ENV=production
   NEXT_PUBLIC_API_URL=https://tu-app-xxxxx.ondigitalocean.app/api
   ```

   **Instance Settings:**
   - **Instance Count**: `1`
   - **Instance Size**: `Basic XXS` ($5/mes)

### Paso 4: Configurar Variables Globales

1. Ve a **Settings** → **App-Level Environment Variables**
2. Agrega estas variables (se compartirán entre componentes):

   ```bash
   JWT_SECRET=<GENERA_UN_SECRET_SEGURO_MÍNIMO_32_CARACTERES>
   FRONTEND_URL=https://tu-app-xxxxx.ondigitalocean.app
   ```

   **Generar JWT Secret:**
   ```powershell
   # PowerShell
   $bytes = New-Object byte[] 32
   [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
   [Convert]::ToBase64String($bytes)
   ```

   **Nota**: Después de crear la app, actualiza `FRONTEND_URL` con la URL real que te dé DigitalOcean.

### Paso 5: Conectar Base de Datos (Opcional)

Si usas Managed Database de DigitalOcean:

1. Ve a **Components** → **Add Component** → **Database**
2. Selecciona tu base de datos PostgreSQL
3. Esto inyectará automáticamente `DATABASE_URL`

Si usas base de datos externa (como la que ya tienes):

1. En las variables de entorno del componente `api`, agrega:
   ```bash
   ConnectionStrings__EventConnectConnection=postgres://usuario:password@host:5432/database
   ```

### Paso 6: Desplegar

1. Haz clic en **"Save"** y luego **"Create Resources"**
2. DigitalOcean comenzará a construir ambos componentes
3. El proceso puede tomar 10-15 minutos
4. Una vez completado, tendrás una URL como: `https://tu-app-xxxxx.ondigitalocean.app`

---

## 🛠️ Alternativa: Usar `doctl` CLI (Avanzado)

Si prefieres usar el archivo `app.yaml` directamente, puedes usar `doctl`:

### Instalar doctl

```bash
# Windows (con Chocolatey)
choco install doctl

# O descargar desde: https://github.com/digitalocean/doctl/releases
```

### Autenticarse

```bash
doctl auth init
```

### Crear App desde app.yaml

```bash
# Desde la raíz del repositorio
doctl apps create --spec .do/app.yaml
```

Esto creará la app directamente desde el archivo de configuración.

---

## 📝 Verificación Post-Despliegue

### Verificar Backend

```bash
curl https://tu-app-xxxxx.ondigitalocean.app/api/health
```

Debería responder: `{"status":"Healthy"}`

### Verificar Frontend

Abre en el navegador:
```
https://tu-app-xxxxx.ondigitalocean.app
```

---

## 🆘 Solución de Problemas

### Error: Build falla en Backend

**Verifica:**
1. Que el `source_dir` sea `/` (raíz)
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

---

## ✅ Checklist Final

- [ ] 2 componentes creados (api y frontend)
- [ ] Backend configurado con build y run commands correctos
- [ ] Frontend configurado con build y run commands correctos
- [ ] Variables de entorno configuradas
- [ ] Base de datos conectada (Managed o externa)
- [ ] Health checks configurados
- [ ] Rutas configuradas (`/api` para backend, `/` para frontend)
- [ ] Build exitoso para ambos componentes
- [ ] Backend accesible en `/api/health`
- [ ] Frontend accesible en `/`

---

¡Con estos pasos deberías poder desplegar exitosamente! 🚀
