# EventConnect - Guía de Implementación del Backend

## ✅ Lo que ya está hecho:

### 1. Base de Datos MySQL (100% Completada)
- ✅ 30 tablas creadas con relaciones completas
- ✅ Vistas para reportes (productos stock bajo, activos mantenimiento, rentabilidad)
- ✅ Procedimientos almacenados (crear reserva, depreciación, clasificación ABC)
- ✅ Triggers de auditoría automática
- ✅ Índices optimizados para rendimiento
- ✅ Datos iniciales (roles, planes, empresa demo, superadmin)

### 2. Estructura del Proyecto .NET (Completa)
- ✅ Solución EventConnect.sln creada
- ✅ 4 proyectos con Clean Architecture:
  - EventConnect.API (Web API)
  - EventConnect.Domain (Entidades y contratos)
  - EventConnect.Application (Lógica de negocio)
  - EventConnect.Infrastructure (Acceso a datos y servicios)
- ✅ Referencias entre proyectos configuradas
- ✅ Paquetes NuGet instalados:
  - MySqlConnector 2.5.0
  - Dapper 2.1.66
  - BCrypt.Net-Next 4.0.3
  - Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0
  - Swashbuckle.AspNetCore 10.0.1
  - System.IdentityModel.Tokens.Jwt 8.15.0

## 📋 Pasos para completar la implementación:

### Paso 1: Crear Base de Datos
```powershell
# En MySQL Workbench o CLI:
mysql -u root -p
source C:/Users/yoiner.castillo/source/repos/EventConnect/database/schema_completo.sql
```

### Paso 2: Configurar appsettings.json

Editar `EventConnect.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=127.0.0.1;Port=3306;Database=db_eventconnect;User=root;Password=TU_PASSWORD;AllowPublicKeyRetrieval=true;SslMode=none;"
  },
  "JwtSettings": {
    "Secret": "EventConnect_SuperSecureKey_ForJWTTokenGeneration_MinLength32Chars_2025!",
    "Issuer": "EventConnectAPI",
    "Audience": "EventConnectClients",
    "TokenExpirationMinutes": 60
  },
  "AllowedCorsOrigins": [
    "http://localhost:3000"
  ]
}
```

### Paso 3: Crear Entidades del Dominio

En `EventConnect.Domain/Entities/`, crear las clases principales:

**Empresa.cs:**
```csharp
namespace EventConnect.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Razon_Social { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string Pais { get; set; } = "Colombia";
    public string? Logo_URL { get; set; }
    public string Estado { get; set; } = "Activa";
    public DateTime Fecha_Registro { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}
```

**Usuario.cs:**
```csharp
namespace EventConnect.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public int? Empresa_Id { get; set; }
    public int Rol_Id { get; set; }
    public string Usuario1 { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password_Hash { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Avatar_URL { get; set; }
    public string Estado { get; set; } = "Activo";
    public int Intentos_Fallidos { get; set; }
    public DateTime? Ultimo_Acceso { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
    public bool Requiere_Cambio_Password { get; set; }
    public bool TwoFA_Activo { get; set; }
    
    // Navegación
    public Empresa? Empresa { get; set; }
    public Rol? Rol { get; set; }
}
```

Crear también: Cliente, Producto, Categoria, Reserva, Activo, etc.

### Paso 4: Crear Repositorios (Infrastructure)

**IRepositoryBase.cs** en `EventConnect.Domain/Repositories/`:
```csharp
namespace EventConnect.Domain.Repositories;

public interface IRepositoryBase<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

**RepositoryBase.cs** en `EventConnect.Infrastructure/Repositories/`:
```csharp
using Dapper;
using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace EventConnect.Infrastructure.Repositories;

public class RepositoryBase<T> where T : class
{
    protected readonly string _connectionString;
    protected readonly string _tableName;

    public RepositoryBase(string connectionString)
    {
        _connectionString = connectionString;
        _tableName = GetTableName();
    }

    private string GetTableName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        return tableAttr?.Name ?? type.Name;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = $"SELECT * FROM {_tableName}";
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<int> AddAsync(T entity)
    {
        using var connection = new MySqlConnection(_connectionString);
        var properties = GetProperties(entity, excludeKey: true);
        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}); SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        using var connection = new MySqlConnection(_connectionString);
        var properties = GetProperties(entity, excludeKey: true);
        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var sql = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    private IEnumerable<PropertyInfo> GetProperties(T entity, bool excludeKey = false)
    {
        var properties = typeof(T).GetProperties();
        if (excludeKey)
        {
            properties = properties.Where(p => p.Name != "Id").ToArray();
        }
        return properties;
    }
}
```

### Paso 5: Crear Servicios de Autenticación

**IAuthService.cs** en `EventConnect.Domain/Services/`:
```csharp
namespace EventConnect.Domain.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre_Completo { get; set; } = string.Empty;
    public int? Empresa_Id { get; set; }
    public string Rol { get; set; } = string.Empty;
}
```

**AuthService.cs** en `EventConnect.Infrastructure/Services/`:
```csharp
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using MySqlConnector;

namespace EventConnect.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public AuthService(string connectionString, IConfiguration configuration)
    {
        _connectionString = connectionString;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        using var connection = new MySqlConnection(_connectionString);
        
        var sql = @"
            SELECT u.*, r.Nombre as RolNombre 
            FROM Usuario u 
            INNER JOIN Rol r ON u.Rol_Id = r.Id 
            WHERE u.Usuario = @Username AND u.Estado = 'Activo'";
        
        var usuario = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = request.Username });
        
        if (usuario == null)
            return null;

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password_Hash))
        {
            // Incrementar intentos fallidos
            await connection.ExecuteAsync(
                "UPDATE Usuario SET Intentos_Fallidos = Intentos_Fallidos + 1 WHERE Id = @Id",
                new { Id = usuario.Id });
            return null;
        }

        // Resetear intentos fallidos y actualizar último acceso
        await connection.ExecuteAsync(
            "UPDATE Usuario SET Intentos_Fallidos = 0, Ultimo_Acceso = NOW() WHERE Id = @Id",
            new { Id = usuario.Id });

        // Generar token
        var token = GenerateJwtToken(usuario);
        var refreshToken = Guid.NewGuid().ToString();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Usuario = usuario.Usuario,
                Email = usuario.Email,
                Nombre_Completo = usuario.Nombre_Completo,
                Empresa_Id = usuario.Empresa_Id,
                Rol = usuario.RolNombre
            }
        };
    }

    private string GenerateJwtToken(dynamic usuario)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Usuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.RolNombre),
                new Claim("EmpresaId", usuario.Empresa_Id?.ToString() ?? "")
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        throw new NotImplementedException();
    }
}
```

### Paso 6: Configurar Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
var connectionString = builder.Configuration.GetConnectionString("EventConnectConnection");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventConnect API",
        Version = "v1",
        Description = "API para gestión de activos y alquileres"
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

// Registrar servicios personalizados
// builder.Services.AddScoped<IAuthService>(sp => new AuthService(connectionString!, builder.Configuration));

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventConnect API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Paso 7: Crear Controladores

**AuthController.cs** en `EventConnect.API/Controllers/`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (response == null)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var response = await _authService.RefreshTokenAsync(refreshToken);
        
        if (response == null)
            return Unauthorized(new { message = "Token inválido" });

        return Ok(response);
    }
}
```

## 🚀 Para ejecutar el proyecto:

```powershell
# Desde EventConnect.API
cd C:\Users\yoiner.castillo\source\repos\EventConnect\EventConnect.API
dotnet run
```

La API estará disponible en: **http://localhost:5000**  
Swagger UI: **http://localhost:5000/swagger**

## 📝 Credenciales de prueba:

**SuperAdmin:**
- Usuario: `superadmin`
- Password: `SuperAdmin123$`

**Admin Empresa:**
- Usuario: `admin_empresa`
- Password: `Admin123$`

## 🔄 Próximos pasos recomendados:

1. ✅ Implementar todos los repositorios (Producto, Categoria, Cliente, Reserva, etc.)
2. ✅ Crear controladores para cada módulo
3. ✅ Implementar middleware de manejo de excepciones global
4. ✅ Agregar validaciones con FluentValidation
5. ✅ Implementar sistema de logs de auditoría
6. ✅ Crear servicio para gestión de suscripciones
7. ✅ Implementar módulo S.I.G.I. completo
8. ✅ Agregar Rate Limiting
9. ✅ Implementar caché con Redis (opcional)
10. ✅ Crear tests unitarios e integración

## 📚 Documentación adicional:

- **README.md**: Documentación completa del proyecto
- **database/schema_completo.sql**: Script completo de base de datos
- **docs/**: Carpeta para documentación adicional

---

**EventConnect © 2025**
