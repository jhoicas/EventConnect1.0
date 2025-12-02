# Guía de Ejecución - EventConnect

## Requisitos Previos

- .NET 9.0 SDK
- MySQL 8.0+
- Node.js 18+ (para el frontend)
- Git

## Configuración de Base de Datos

### 1. Crear la base de datos

```bash
mysql -u root -p
```

```sql
CREATE DATABASE db_eventconnect CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE db_eventconnect;
```

### 2. Ejecutar el schema completo

```bash
mysql -u root -p db_eventconnect < database/schema_completo.sql
```

### 3. Verificar tablas creadas

```sql
SHOW TABLES;
SELECT * FROM Empresa;
SELECT * FROM Usuario;
```

**Usuarios de prueba creados:**
- SuperAdmin: `superadmin` / `SuperAdmin123$`
- Admin Empresa 1: `admin_empresa` / `Admin123$`

## Configuración del Backend

### 1. Configurar cadena de conexión

Editar `EventConnect.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=127.0.0.1;Port=3306;Database=db_eventconnect;User=root;Password=TU_PASSWORD;AllowPublicKeyRetrieval=true;SslMode=none;"
  }
}
```

### 2. Restaurar paquetes NuGet

```powershell
cd EventConnect.API
dotnet restore
```

### 3. Compilar el proyecto

```powershell
dotnet build
```

### 4. Ejecutar el backend

```powershell
dotnet run
```

El API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger`

## Endpoints Principales

### Autenticación

**POST /api/auth/login**
```json
{
  "username": "superadmin",
  "password": "SuperAdmin123$"
}
```

**POST /api/auth/register**
```json
{
  "username": "nuevo_usuario",
  "email": "usuario@ejemplo.com",
  "password": "Password123$",
  "nombre": "Usuario Nuevo",
  "apellido": "Apellido",
  "empresaId": 1,
  "rolId": 2
}
```

### Categorías (requiere JWT)

**GET /api/categoria** - Listar todas las categorías
**GET /api/categoria/{id}** - Obtener categoría por ID
**POST /api/categoria** - Crear categoría
**PUT /api/categoria/{id}** - Actualizar categoría
**DELETE /api/categoria/{id}** - Eliminar categoría

### Productos (requiere JWT)

**GET /api/producto** - Listar todos los productos
**GET /api/producto/{id}** - Obtener producto por ID
**GET /api/producto/stock-bajo** - Productos con stock bajo
**POST /api/producto** - Crear producto
**PUT /api/producto/{id}** - Actualizar producto
**DELETE /api/producto/{id}** - Eliminar producto

### Clientes (requiere JWT)

**GET /api/cliente** - Listar todos los clientes
**GET /api/cliente/{id}** - Obtener cliente por ID
**POST /api/cliente** - Crear cliente
**PUT /api/cliente/{id}** - Actualizar cliente
**DELETE /api/cliente/{id}** - Eliminar cliente

### Reservas (requiere JWT)

**GET /api/reserva** - Listar todas las reservas
**GET /api/reserva/{id}** - Obtener reserva por ID
**GET /api/reserva/estado/{estado}** - Filtrar por estado
**POST /api/reserva** - Crear reserva
**PUT /api/reserva/{id}** - Actualizar reserva
**DELETE /api/reserva/{id}** - Eliminar reserva

## Uso de Swagger UI

1. Navegar a `http://localhost:5000/swagger`
2. Hacer clic en **POST /api/auth/login**
3. Hacer clic en "Try it out"
4. Ingresar credenciales:
   ```json
   {
     "username": "superadmin",
     "password": "SuperAdmin123$"
   }
   ```
5. Copiar el token de la respuesta
6. Hacer clic en el botón **Authorize** (arriba a la derecha)
7. Ingresar: `Bearer TU_TOKEN_AQUI`
8. Ahora puedes probar todos los endpoints protegidos

## Multi-Tenancy

El sistema implementa multi-tenancy a nivel de empresa:

- Cada usuario pertenece a una **Empresa**
- Los datos se filtran automáticamente por `Empresa_Id`
- El **SuperAdmin** (NivelAcceso = 0) puede ver todos los datos
- Los **Admin-Proveedor** (NivelAcceso = 1) solo ven datos de su empresa

## Niveles de Acceso

| Nivel | Rol | Permisos |
|-------|-----|----------|
| 0 | SuperAdmin | Acceso total a todas las empresas |
| 1 | Admin-Proveedor | Gestión completa de su empresa |
| 2 | Operador | Operaciones básicas |
| 3 | Usuario-Lectura | Solo lectura |

## Seguridad

- **JWT Bearer Tokens**: Autenticación basada en tokens
- **BCrypt**: Hashing de contraseñas con factor 12
- **Rate Limiting**: 100 requests por minuto
- **CORS**: Configurado para localhost:3000 y localhost:5173
- **SQL Injection Protection**: Queries parametrizadas con Dapper
- **Lockout Policy**: 5 intentos fallidos = bloqueo de 30 minutos

## Logging

Los logs se encuentran en:
- Consola durante desarrollo
- Archivos en producción (configurar en `appsettings.Production.json`)

## Troubleshooting

### Error: Connection refused

Verificar que MySQL esté corriendo:
```powershell
Get-Service -Name MySQL*
```

### Error: Unable to connect to database

Verificar credenciales en `appsettings.json`

### Error: JWT Secret not configured

Asegurar que `JwtSettings:Secret` esté en `appsettings.json`

### Error: 401 Unauthorized

1. Verificar que el token no haya expirado (60 minutos)
2. Usar el endpoint `/api/auth/refresh` para renovar
3. Incluir el header: `Authorization: Bearer TOKEN`

## Próximos Pasos

1. **Frontend**: Implementar cliente Next.js o React
2. **Módulo SIGI**: Agregar gestión avanzada de inventario
3. **Suscripciones**: Implementar período de prueba de 3 días
4. **Notificaciones**: Emails y push notifications
5. **Reportes**: Dashboard con analytics y exportación
6. **Logs de Auditoría**: Vista completa de trazabilidad

## Estructura de Archivos Creados

```
EventConnect/
 EventConnect.sln
 database/
    schema_completo.sql (30 tablas + views + procedures)
 EventConnect.API/
    Program.cs (configuración JWT, CORS, Swagger, DI)
    appsettings.json (configuración completa)
    Controllers/
        BaseController.cs (helpers de claims)
        AuthController.cs (login, register, refresh)
        CategoriaController.cs (CRUD completo)
        ProductoController.cs (CRUD + stock bajo)
        ClienteController.cs (CRUD completo)
        ReservaController.cs (CRUD + filtros)
 EventConnect.Domain/
    Entities/ (9 entidades con [Table] attributes)
    DTOs/ (AuthDTOs con LoginRequest, AuthResponse, etc.)
    Repositories/ (interfaces)
 EventConnect.Infrastructure/
     Repositories/ (6 repositories con Dapper)
     Services/ (AuthService con JWT + BCrypt)
```

## Comandos Útiles

```powershell
# Compilar solución completa
dotnet build

# Ejecutar con hot reload
dotnet watch run

# Ejecutar en producción
dotnet run --configuration Release

# Ver logs en tiempo real
dotnet run | Select-String "Error|Warning"

# Limpiar build
dotnet clean
```
