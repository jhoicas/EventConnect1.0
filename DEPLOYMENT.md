#  Guía de Despliegue - EventConnect

Esta guía describe cómo desplegar EventConnect en diferentes entornos.

##  Índice

- [Despliegue en Desarrollo](#-despliegue-en-desarrollo)
- [Despliegue en Producción](#-despliegue-en-producción)
- [Docker](#-docker)
- [Configuración de Nginx](#-configuración-de-nginx)

##  Despliegue en Desarrollo

### Requisitos Previos
- .NET 9.0 SDK
- MySQL 8.0+
- Git

### Pasos

1. **Clonar el repositorio**
```bash
git clone https://github.com/jhoicas/EventConnect.git
cd EventConnect
```

2. **Configurar base de datos**
```bash
# Iniciar MySQL
mysql -u root -p

# Ejecutar scripts en orden
mysql -u root -p < database/EJECUTAR_PRIMERO.sql
mysql -u root -p < database/RENOMBRAR_TABLAS.sql
mysql -u root -p < database/crear_tablas_faltantes.sql
mysql -u root -p < database/schema_completo.sql
```

3. **Configurar appsettings.Development.json**
```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=localhost;Port=3306;Database=db_eventconnect;User=root;Password=tu_password;"
  },
  "Jwt": {
    "Key": "clave-secreta-desarrollo-al-menos-32-caracteres",
    "Issuer": "EventConnectAPI",
    "Audience": "EventConnectClients",
    "ExpiryMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

4. **Restaurar y compilar**
```bash
dotnet restore
dotnet build
```

5. **Ejecutar**
```bash
cd EventConnect.API
dotnet run
```

La API estará disponible en `http://localhost:5555`

##  Despliegue en Producción

### Opción 1: Servidor Linux (Ubuntu/Debian)

#### 1. Preparar el servidor

```bash
# Actualizar paquetes
sudo apt update && sudo apt upgrade -y

# Instalar .NET Runtime
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0 --runtime aspnetcore

# Instalar MySQL
sudo apt install mysql-server -y
sudo mysql_secure_installation
```

#### 2. Configurar MySQL

```bash
sudo mysql

CREATE DATABASE db_eventconnect CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'eventconnect'@'localhost' IDENTIFIED BY 'password_seguro';
GRANT ALL PRIVILEGES ON db_eventconnect.* TO 'eventconnect'@'localhost';
FLUSH PRIVILEGES;
EXIT;

# Importar schema
mysql -u eventconnect -p db_eventconnect < schema_completo.sql
```

#### 3. Publicar la aplicación

```bash
# En tu máquina local
dotnet publish -c Release -o ./publish

# Copiar al servidor
scp -r ./publish usuario@servidor:/var/www/eventconnect/
```

#### 4. Configurar systemd service

```bash
sudo nano /etc/systemd/system/eventconnect.service
```

```ini
[Unit]
Description=EventConnect API
After=network.target

[Service]
WorkingDirectory=/var/www/eventconnect
ExecStart=/usr/bin/dotnet /var/www/eventconnect/EventConnect.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=eventconnect
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Habilitar e iniciar servicio
sudo systemctl enable eventconnect.service
sudo systemctl start eventconnect.service
sudo systemctl status eventconnect.service
```

### Opción 2: Windows Server (IIS)

#### 1. Instalar requisitos

- .NET 9.0 Hosting Bundle
- IIS con ASP.NET Core Module
- MySQL Server

#### 2. Publicar aplicación

```powershell
dotnet publish -c Release -o C:\inetpub\EventConnect
```

#### 3. Configurar IIS

1. Abrir IIS Manager
2. Crear nuevo sitio web:
   - Nombre: EventConnect
   - Pool de aplicaciones: .NET CLR Version: No Managed Code
   - Ruta física: C:\inetpub\EventConnect
   - Binding: http, puerto 80 (o 443 para HTTPS)

#### 4. Configurar appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=localhost;Database=db_eventconnect;User=sa;Password=tu_password_seguro;"
  },
  "Jwt": {
    "Key": "clave-super-secreta-produccion-debe-ser-muy-larga-y-segura-123456",
    "Issuer": "EventConnectAPI",
    "Audience": "EventConnectClients",
    "ExpiryMinutes": 720
  },
  "AllowedHosts": "tudominio.com",
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

##  Docker

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EventConnect.API/EventConnect.API.csproj", "EventConnect.API/"]
COPY ["EventConnect.Application/EventConnect.Application.csproj", "EventConnect.Application/"]
COPY ["EventConnect.Infrastructure/EventConnect.Infrastructure.csproj", "EventConnect.Infrastructure/"]
COPY ["EventConnect.Domain/EventConnect.Domain.csproj", "EventConnect.Domain/"]

RUN dotnet restore "EventConnect.API/EventConnect.API.csproj"

COPY . .
WORKDIR "/src/EventConnect.API"
RUN dotnet build "EventConnect.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventConnect.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "EventConnect.API.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  eventconnect-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: eventconnect-api
    ports:
      - "5555:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__EventConnectConnection=Server=mysql-db;Port=3306;Database=db_eventconnect;User=root;Password=rootpassword;
    depends_on:
      - mysql-db
    networks:
      - eventconnect-network

  mysql-db:
    image: mysql:8.0
    container_name: eventconnect-mysql
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: db_eventconnect
    ports:
      - "3306:3306"
    volumes:
      - mysql-data:/var/lib/mysql
      - ./database:/docker-entrypoint-initdb.d
    networks:
      - eventconnect-network

volumes:
  mysql-data:

networks:
  eventconnect-network:
    driver: bridge
```

### Comandos Docker

```bash
# Construir y ejecutar
docker-compose up -d --build

# Ver logs
docker-compose logs -f eventconnect-api

# Detener
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v
```

##  Configuración de Nginx

### Configuración como Reverse Proxy

```nginx
server {
    listen 80;
    server_name tudominio.com www.tudominio.com;

    location / {
        proxy_pass http://localhost:5555;
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

### Con SSL (Let's Encrypt)

```bash
# Instalar Certbot
sudo apt install certbot python3-certbot-nginx -y

# Obtener certificado
sudo certbot --nginx -d tudominio.com -d www.tudominio.com

# Auto-renovación
sudo certbot renew --dry-run
```

```nginx
server {
    listen 80;
    server_name tudominio.com www.tudominio.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name tudominio.com www.tudominio.com;

    ssl_certificate /etc/letsencrypt/live/tudominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/tudominio.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://localhost:5555;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # CORS (si es necesario)
        add_header Access-Control-Allow-Origin *;
        add_header Access-Control-Allow-Methods 'GET, POST, PUT, DELETE, OPTIONS';
        add_header Access-Control-Allow-Headers 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization';
    }
}
```

##  Seguridad en Producción

### Checklist

- [ ] Cambiar JWT Key por una clave segura y larga
- [ ] Usar contraseñas seguras para base de datos
- [ ] Configurar HTTPS/SSL
- [ ] Habilitar CORS solo para dominios específicos
- [ ] Configurar firewall (solo puertos 80, 443)
- [ ] Deshabilitar Swagger en producción
- [ ] Configurar rate limiting
- [ ] Habilitar logging y monitoreo
- [ ] Backup automático de base de datos
- [ ] Actualizar conexión string sin credenciales hardcoded

### Configuración de appsettings.Production.json seguro

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "obtener-de-variables-de-entorno"
  },
  "Jwt": {
    "Key": "obtener-de-azure-key-vault-o-secretos",
    "Issuer": "EventConnectAPI",
    "Audience": "EventConnectClients",
    "ExpiryMinutes": 480
  },
  "AllowedHosts": "tudominio.com",
  "Swagger": {
    "Enabled": false
  }
}
```

##  Monitoreo

### Logs con Serilog (Opcional)

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

### Health Checks

Agregar en `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddMySql(connectionString);

app.MapHealthChecks("/health");
```

##  CI/CD

### GitHub Actions (.github/workflows/deploy.yml)

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --configuration Release --no-restore
      
    - name: Test
      run: dotnet test --no-restore --verbosity normal
      
    - name: Publish
      run: dotnet publish -c Release -o ./publish
      
    - name: Deploy to server
      uses: appleboy/scp-action@master
      with:
        host: ${{ secrets.SERVER_HOST }}
        username: ${{ secrets.SERVER_USERNAME }}
        key: ${{ secrets.SERVER_SSH_KEY }}
        source: "./publish/*"
        target: "/var/www/eventconnect"
```

---

 **Tip**: Siempre realiza pruebas en un entorno de staging antes de desplegar a producción.
