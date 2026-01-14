# Configuración de PostgreSQL - EventConnect API

## 🔧 Configuración de la Cadena de Conexión

### URI de PostgreSQL proporcionada:
```
postgres://uecct3vhln2750:pa6b7d86f527f2bc8b418feadd03667970f779c77b3d79787ff9e3242b4417a6c@c7itisjfjj8ril.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com:5432/d21i4sul1k9fam
```

### Formato Connection String de Npgsql:
```
Host=c7itisjfjj8ril.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com;Port=5432;Database=d21i4sul1k9fam;Username=uecct3vhln2750;Password=pa6b7d86f527f2bc8b418feadd03667970f779c77b3d79787ff9e3242b4417a6c;SslMode=Require
```

## ⚠️ IMPORTANTE: Seguridad

**NUNCA** commitees la cadena de conexión completa con credenciales en `appsettings.json` al repositorio.

## 🔧 Configuración para Desarrollo

### Usar User Secrets (Recomendado)

```bash
# Inicializar User Secrets (solo una vez)
cd EventConnect.API
dotnet user-secrets init

# Configurar connection string de PostgreSQL
dotnet user-secrets set "ConnectionStrings:EventConnectConnection" "Host=c7itisjfjj8ril.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com;Port=5432;Database=d21i4sul1k9fam;Username=uecct3vhln2750;Password=pa6b7d86f527f2bc8b418feadd03667970f779c77b3d79787ff9e3242b4417a6c;SslMode=Require"

# Configurar JWT Secret (si no está configurado)
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
export ConnectionStrings__EventConnectConnection="Host=...;Port=5432;Database=...;Username=...;Password=...;SslMode=Require"
export JwtSettings__Secret="..."
```

### Opción 2: Azure Key Vault (Recomendado para Azure)

```csharp
// En Program.cs (ya configurado)
builder.Configuration.AddAzureKeyVault(vaultUri, credential);
```

## 📝 appsettings.json

El archivo `appsettings.json` debe mantener solo placeholders:

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "***CONFIGURAR_CON_USER_SECRETS_OR_ENV_VARS***"
  }
}
```

## 🚨 Nota de Seguridad

- User Secrets solo funciona en **desarrollo local**
- Para producción, usar **Azure Key Vault** o **Variables de Entorno**
- La cadena de conexión contiene credenciales sensibles - **NUNCA** commitees valores reales
