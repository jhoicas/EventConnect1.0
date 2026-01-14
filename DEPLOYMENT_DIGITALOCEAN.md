# 🚀 Guía de Despliegue en DigitalOcean - EventConnect Backend

Esta guía te ayudará a desplegar el backend de EventConnect en DigitalOcean usando App Platform.

## 📋 Requisitos Previos

- Cuenta en DigitalOcean
- Cuenta de GitHub/GitLab (para conectar el repositorio)
- Base de datos PostgreSQL (DigitalOcean Managed Database o externa)
- URI de conexión a PostgreSQL lista

## 🎯 Opciones de Despliegue

### Opción 1: App Platform (Recomendado - Más fácil)
Despliegue automático desde Git con actualizaciones continuas.

### Opción 2: Droplet (Más control)
VM completa donde instalas todo manualmente.

---

## 🌊 Opción 1: App Platform (Recomendado)

### Paso 1: Crear Base de Datos PostgreSQL

1. En DigitalOcean, ve a **Databases** → **Create Database Cluster**
2. Selecciona:
   - **Database Engine**: PostgreSQL
   - **Version**: PostgreSQL 15 o superior
   - **Plan**: Basic ($15/mes) o Standard según necesidades
   - **Datacenter Region**: Elige la más cercana
   - **Database Name**: `db_eventconnect` (o el nombre que prefieras)
3. Crea el cluster
4. Espera a que se cree (5-10 minutos)
5. Una vez creado, ve a **Connection Details** y copia la **Connection String**

### Paso 2: Configurar Variables de Entorno

Necesitarás las siguientes variables de entorno en App Platform:

```bash
# Base de datos
ConnectionStrings__EventConnectConnection=<TU_CONNECTION_STRING_POSTGRESQL>

# JWT Settings
JwtSettings__Secret=<TU_SECRET_JWT_MUY_SEGURO_MINIMO_32_CARACTERES>
JwtSettings__Issuer=EventConnect
JwtSettings__Audience=EventConnectClients
JwtSettings__TokenExpirationMinutes=60
JwtSettings__RefreshTokenExpirationDays=7

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# CORS (ajusta según tu dominio frontend)
AllowedOrigins__0=https://tu-dominio-frontend.com
AllowedOrigins__1=https://www.tu-dominio-frontend.com
```

**Generar Secret JWT seguro:**
```bash
# En PowerShell (Windows)
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)

# En Linux/Mac
openssl rand -base64 32
```

### Paso 3: Crear App en App Platform

1. En DigitalOcean, ve a **App Platform** → **Create App**
2. Conecta tu repositorio de GitHub/GitLab
3. Selecciona el repositorio y branch (ej: `main` o `master`)
4. DigitalOcean detectará automáticamente que es un proyecto .NET

### Paso 4: Configurar el Build

DigitalOcean debería detectar automáticamente la configuración, pero verifica:

- **Build Command**: 
  ```bash
  cd EventConnect.API && dotnet publish -c Release -o ./publish
  ```
- **Run Command**: 
  ```bash
  cd EventConnect.API && dotnet EventConnect.API.dll
  ```
- **Source Directory**: `EventConnect.API` (o la ruta donde está tu proyecto)

### Paso 5: Agregar Variables de Entorno

1. En la configuración de la App, ve a **Settings** → **App-Level Environment Variables**
2. Agrega todas las variables de entorno listadas en el Paso 2
3. **IMPORTANTE**: No agregues espacios antes/después del `=`

### Paso 6: Configurar Health Checks

1. En **Settings** → **Health Check**
2. **Path**: `/health`
3. **Initial Delay**: 60 seconds
4. **Period**: 10 seconds

### Paso 7: Conectar Base de Datos

1. En la configuración de la App, ve a **Components** → **Add Component** → **Database**
2. Selecciona la base de datos PostgreSQL que creaste
3. Esto automáticamente inyectará la variable `DATABASE_URL`

**Nota**: Si la variable es `DATABASE_URL` y tu código espera `ConnectionStrings__EventConnectConnection`, necesitarás mapearla:

```bash
ConnectionStrings__EventConnectConnection=${DATABASE_URL}
```

O ajustar el código en `Program.cs` para leer `DATABASE_URL` como fallback.

### Paso 8: Configurar Dominio (Opcional)

1. En **Settings** → **Domains**
2. Agrega tu dominio personalizado
3. Configura los registros DNS según las instrucciones

### Paso 9: Desplegar

1. Haz clic en **Save** y luego **Create Resources**
2. DigitalOcean comenzará a construir y desplegar tu aplicación
3. El proceso puede tomar 5-10 minutos
4. Una vez completado, tendrás una URL como: `https://tu-app-xxxxx.ondigitalocean.app`

---

## 🖥️ Opción 2: Droplet (Más Control)

### Paso 1: Crear Droplet

1. Ve a **Droplets** → **Create Droplet**
2. Selecciona:
   - **Image**: Ubuntu 22.04 LTS
   - **Plan**: Basic ($6/mes mínimo para empezar)
   - **Region**: Elige la más cercana
   - **Authentication**: SSH Keys (recomendado) o Password
3. Crea el Droplet

### Paso 2: Conectar al Droplet

```bash
ssh root@tu-droplet-ip
```

### Paso 3: Instalar .NET 9.0 SDK

```bash
# Actualizar sistema
apt update && apt upgrade -y

# Instalar dependencias
apt install -y wget apt-transport-https software-properties-common

# Agregar repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb

# Instalar .NET 9.0 SDK
apt update
apt install -y dotnet-sdk-9.0
apt install -y aspnetcore-runtime-9.0

# Verificar instalación
dotnet --version
```

### Paso 4: Configurar Nginx como Reverse Proxy

```bash
# Instalar Nginx
apt install -y nginx

# Crear configuración
nano /etc/nginx/sites-available/eventconnect
```

Contenido del archivo:

```nginx
server {
    listen 80;
    server_name tu-dominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Habilitar sitio
ln -s /etc/nginx/sites-available/eventconnect /etc/nginx/sites-enabled/
rm /etc/nginx/sites-enabled/default

# Verificar configuración
nginx -t

# Reiniciar Nginx
systemctl restart nginx
```

### Paso 5: Configurar SSL con Let's Encrypt

```bash
# Instalar Certbot
apt install -y certbot python3-certbot-nginx

# Obtener certificado SSL
certbot --nginx -d tu-dominio.com

# Certificado se renovará automáticamente
```

### Paso 6: Configurar Systemd Service

```bash
# Crear usuario para la aplicación
adduser --disabled-password --gecos "" eventconnect

# Crear directorio de la aplicación
mkdir -p /var/www/eventconnect
chown eventconnect:eventconnect /var/www/eventconnect

# Crear servicio
nano /etc/systemd/system/eventconnect.service
```

Contenido del archivo:

```ini
[Unit]
Description=EventConnect API
After=network.target

[Service]
Type=notify
User=eventconnect
WorkingDirectory=/var/www/eventconnect
ExecStart=/usr/bin/dotnet /var/www/eventconnect/EventConnect.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=eventconnect
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ConnectionStrings__EventConnectConnection=<TU_CONNECTION_STRING>

[Install]
WantedBy=multi-user.target
```

```bash
# Habilitar y iniciar servicio
systemctl daemon-reload
systemctl enable eventconnect
systemctl start eventconnect

# Ver logs
journalctl -u eventconnect -f
```

### Paso 7: Desplegar Aplicación

```bash
# Clonar repositorio (o usar CI/CD)
cd /var/www/eventconnect
git clone https://github.com/tu-usuario/EventConnect.git .

# Ir al directorio del proyecto
cd EventConnect.API

# Publicar aplicación
dotnet publish -c Release -o /var/www/eventconnect

# Configurar variables de entorno en el archivo .service o en appsettings.json

# Reiniciar servicio
systemctl restart eventconnect
```

---

## 📝 Configuración Adicional

### Health Check Endpoint

Tu aplicación ya tiene configurado el endpoint `/health`. Úsalo para verificar que todo funciona:

```bash
curl https://tu-dominio.com/health
```

### Variables de Entorno Importantes

Asegúrate de configurar correctamente:

1. **ConnectionStrings__EventConnectConnection**: URI de PostgreSQL
2. **JwtSettings__Secret**: Secret seguro (mínimo 32 caracteres)
3. **ASPNETCORE_ENVIRONMENT**: `Production`
4. **AllowedOrigins**: Dominios permitidos para CORS

### Migraciones de Base de Datos

Si necesitas ejecutar migraciones adicionales, puedes hacerlo manualmente o crear un job en App Platform.

### Logs

- **App Platform**: Ve a **Runtime Logs** en el dashboard
- **Droplet**: `journalctl -u eventconnect -f`

---

## 🔒 Seguridad

1. ✅ **Nunca** commitees secrets en el repositorio
2. ✅ Usa variables de entorno para toda la configuración sensible
3. ✅ Habilita HTTPS (automático en App Platform, Let's Encrypt en Droplet)
4. ✅ Configura CORS correctamente
5. ✅ Mantén .NET actualizado
6. ✅ Configura firewall (App Platform lo hace automáticamente)

---

## 📊 Monitoreo

DigitalOcean App Platform incluye monitoreo básico. Para monitoreo avanzado, considera:

- **Application Insights** (Azure)
- **Sentry** (Error tracking)
- **DataDog** o **New Relic** (APM)

---

## 🆘 Solución de Problemas

### La aplicación no inicia

1. Revisa los logs: `journalctl -u eventconnect -f` (Droplet) o Runtime Logs (App Platform)
2. Verifica que las variables de entorno estén configuradas
3. Verifica la conexión a la base de datos

### Error de conexión a base de datos

1. Verifica que el Connection String sea correcto
2. Verifica que el firewall de la base de datos permita conexiones desde tu App/Droplet
3. En DigitalOcean Managed Database, ve a **Trusted Sources** y agrega tu IP o App

### CORS errors

1. Verifica que `AllowedOrigins` incluya tu dominio frontend
2. Verifica que el frontend esté usando la URL correcta de la API

---

## 📚 Recursos

- [DigitalOcean App Platform Docs](https://docs.digitalocean.com/products/app-platform/)
- [.NET on DigitalOcean](https://docs.digitalocean.com/products/app-platform/how-to/manage-apps/#net)
- [PostgreSQL Managed Database](https://docs.digitalocean.com/products/databases/postgresql/)

---

## ✅ Checklist de Despliegue

- [ ] Base de datos PostgreSQL creada
- [ ] Connection String configurado
- [ ] JWT Secret generado y configurado
- [ ] Variables de entorno configuradas
- [ ] CORS configurado con dominios correctos
- [ ] Health check funcionando (`/health`)
- [ ] SSL/HTTPS habilitado
- [ ] Logs funcionando
- [ ] Aplicación accesible desde el frontend

---

¡Buena suerte con tu despliegue! 🚀
