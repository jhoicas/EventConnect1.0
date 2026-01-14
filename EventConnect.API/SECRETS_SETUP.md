# Configuración de Secrets - EventConnect API

## ⚠️ IMPORTANTE: Seguridad de Credenciales

**NUNCA** commitees contraseñas o secrets en `appsettings.json` al repositorio.

## 🔧 Configuración para Desarrollo

### Usar User Secrets (Recomendado)

```bash
# Inicializar User Secrets (solo una vez)
cd EventConnect.API
dotnet user-secrets init

# Configurar connection string
dotnet user-secrets set "ConnectionStrings:EventConnectConnection" "Server=127.0.0.1;Port=3306;Database=db_eventconnect;User=root;Password=TU_PASSWORD_AQUI;AllowPublicKeyRetrieval=true;SslMode=none;Pooling=true;MinimumPoolSize=0;MaximumPoolSize=100;ConnectionLifeTime=0;ConnectionTimeout=30;"

# Configurar JWT Secret
dotnet user-secrets set "JwtSettings:Secret" "EventConnect_SuperSecretKey_ForJWT_TokenGeneration_MustBe32CharactersOrMore_2024"
```

### Verificar Secrets Configurados

```bash
dotnet user-secrets list
```

## 🔒 Configuración para Producción

### Opción 1: Variables de Entorno

```bash
# En el servidor o contenedor
export ConnectionStrings__EventConnectConnection="Server=..."
export JwtSettings__Secret="..."
```

### Opción 2: Azure Key Vault (Recomendado para Azure)

```csharp
// En Program.cs (ya configurado)
builder.Configuration.AddAzureKeyVault(vaultUri, credential);
```

### Opción 3: appsettings.Production.json (No recomendado para secrets)

Solo para configuración no sensible. Los secrets deben usar Key Vault o variables de entorno.

## 📝 appsettings.json

Mantener solo valores de ejemplo o configuración no sensible:

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=127.0.0.1;Port=3306;Database=db_eventconnect;User=root;Password=***;..."
  },
  "JwtSettings": {
    "Secret": "***REEMPLAZAR_CON_SECRET_REAL***"
  }
}
```

## 🚨 Nota de Seguridad

- User Secrets solo funciona en **desarrollo local**
- Para producción, usar **Azure Key Vault** o **Variables de Entorno**
- Nunca exponer secrets en logs o respuestas de API
- Rotar secrets regularmente (cada 90 días recomendado)
