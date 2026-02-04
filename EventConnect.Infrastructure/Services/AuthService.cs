using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Services;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Npgsql;

namespace EventConnect.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private readonly UsuarioRepository _usuarioRepository;

    public AuthService(string connectionString, IConfiguration configuration)
    {
        _connectionString = connectionString;
        _configuration = configuration;
        _usuarioRepository = new UsuarioRepository(connectionString);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
        
        var sql = @"
                SELECT 
                    u.Id,
                    u.Usuario,
                    u.Email,
                    u.Password_Hash,
                    u.Nombre_Completo,
                    u.Telefono,
                    u.Avatar_URL,
                    u.Empresa_Id,
                    u.Rol_Id,
                    u.Intentos_Fallidos,
                    u.Estado,
                    r.Nombre as RolNombre, 
                    r.Nivel_Acceso, 
                    e.Razon_Social as Empresa_Nombre
            FROM Usuario u 
            INNER JOIN Rol r ON u.Rol_Id = r.Id 
            LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
            WHERE u.Usuario = @Username AND u.Estado = 'Activo'";
        
        var usuario = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = request.Username });
        
        if (usuario == null)
            return null;

            // PostgreSQL devuelve nombres de columnas en minúsculas, intentar ambos
            var userId = GetValue(usuario, "id") ?? GetValue(usuario, "Id");
            
            // Verificar que Id no sea null - con mejor logging
            if (userId == null)
            {
                // En desarrollo, mostrar todos los campos para debugging
                try
                {
                    var dict = usuario as IDictionary<string, object>;
                    var fields = dict?.Keys ?? new string[0];
                    var fieldsStr = string.Join(", ", fields);
                    throw new InvalidOperationException($"El usuario '{request.Username}' no tiene un ID válido. Campos disponibles: {fieldsStr}");
                }
                catch
                {
                    throw new InvalidOperationException($"El usuario '{request.Username}' no tiene un ID válido.");
                }
            }

        // Verificar si está bloqueado
            var intentosFallidos = GetValue(usuario, "intentos_fallidos") ?? GetValue(usuario, "Intentos_Fallidos") ?? 0;
            if ((int)intentosFallidos >= 5)
        {
            return null; // Usuario bloqueado
        }

        // Verificar contraseña
            var passwordHash = GetValue(usuario, "password_hash")?.ToString() ?? GetValue(usuario, "Password_Hash")?.ToString() ?? "";
            if (string.IsNullOrEmpty(passwordHash) || !VerifyPassword(request.Password, passwordHash))
        {
                await _usuarioRepository.IncrementFailedAttemptsAsync((int)userId);
            return null;
        }

        // Resetear intentos fallidos y actualizar último acceso
            await _usuarioRepository.ResetFailedAttemptsAsync((int)userId);

        // Generar token
        var token = GenerateJwtToken(usuario);
        var refreshToken = Guid.NewGuid().ToString();

            // Normalizar rol
            var rolNombre = NormalizeRole(GetValue(usuario, "rolnombre")?.ToString() ?? GetValue(usuario, "RolNombre")?.ToString() ?? "");

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Usuario = new UsuarioDto
            {
                    Id = (int)(GetValue(usuario, "id") ?? GetValue(usuario, "Id") ?? 0),
                    Usuario = GetValue(usuario, "usuario")?.ToString() ?? GetValue(usuario, "Usuario")?.ToString() ?? "",
                    Email = GetValue(usuario, "email")?.ToString() ?? GetValue(usuario, "Email")?.ToString() ?? "",
                    Nombre_Completo = GetValue(usuario, "nombre_completo")?.ToString() ?? GetValue(usuario, "Nombre_Completo")?.ToString() ?? "",
                    Telefono = GetValue(usuario, "telefono")?.ToString() ?? GetValue(usuario, "Telefono")?.ToString(),
                    Avatar_URL = GetValue(usuario, "avatar_url")?.ToString() ?? GetValue(usuario, "Avatar_URL")?.ToString(),
                    Empresa_Id = GetValue(usuario, "empresa_id") as int? ?? GetValue(usuario, "Empresa_Id") as int?,
                    Empresa_Nombre = GetValue(usuario, "empresa_nombre")?.ToString() ?? GetValue(usuario, "Empresa_Nombre")?.ToString(),
                    Rol_Id = (int)(GetValue(usuario, "rol_id") ?? GetValue(usuario, "Rol_Id") ?? 0),
                    Rol = rolNombre,
                    Nivel_Acceso = (int)(GetValue(usuario, "nivel_acceso") ?? GetValue(usuario, "Nivel_Acceso") ?? 0)
            }
        };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error en LoginAsync: {ex.Message}. InnerException: {ex.InnerException?.Message}", ex);
        }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Verificar si el usuario ya existe
        var existingUser = await _usuarioRepository.GetByUsernameAsync(request.Usuario);
        if (existingUser != null)
            return null;

        var existingEmail = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            return null;

        // Asignar valores por defecto si son null
        var empresaId = request.Empresa_Id ?? 1; // Empresa por defecto: 1
        var rolId = request.Rol_Id ?? 3; // Rol por defecto: 3 (Cliente)

        // Crear nuevo usuario
        var usuario = new Usuario
        {
            Empresa_Id = empresaId,
            Rol_Id = rolId,
            Usuario1 = request.Usuario,
            Email = request.Email,
            Password_Hash = HashPassword(request.Password),
            Nombre_Completo = request.Nombre_Completo,
            Telefono = request.Telefono,
            Estado = "Activo",
            Intentos_Fallidos = 0,
            Fecha_Creacion = DateTime.Now,
            Fecha_Actualizacion = DateTime.Now,
            Requiere_Cambio_Password = false,
            TwoFA_Activo = false
        };

        var userId = await _usuarioRepository.AddAsync(usuario);
        usuario.Id = userId;

        // Generar token para login automático
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = @"
            SELECT u.*, r.Nombre as RolNombre, r.Nivel_Acceso, e.Razon_Social as Empresa_Nombre
            FROM Usuario u 
            INNER JOIN Rol r ON u.Rol_Id = r.Id 
            LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
            WHERE u.Id = @UserId";
        
        var nuevoUsuario = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { UserId = userId });
        
        if (nuevoUsuario == null)
        {
            throw new InvalidOperationException("Usuario registrado no encontrado después de la creación");
        }
        
        var token = GenerateJwtToken(nuevoUsuario);
        var refreshToken = Guid.NewGuid().ToString();

        // Normalizar rol
        var rolNombre = NormalizeRole(GetValue(nuevoUsuario, "rolnombre")?.ToString() ?? GetValue(nuevoUsuario, "RolNombre")?.ToString() ?? "");

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Usuario = new UsuarioDto
            {
                Id = (int)(GetValue(nuevoUsuario, "id") ?? GetValue(nuevoUsuario, "Id") ?? userId),
                Usuario = GetValue(nuevoUsuario, "usuario")?.ToString() ?? GetValue(nuevoUsuario, "Usuario")?.ToString() ?? "",
                Email = GetValue(nuevoUsuario, "email")?.ToString() ?? GetValue(nuevoUsuario, "Email")?.ToString() ?? "",
                Nombre_Completo = GetValue(nuevoUsuario, "nombre_completo")?.ToString() ?? GetValue(nuevoUsuario, "Nombre_Completo")?.ToString() ?? "",
                Telefono = GetValue(nuevoUsuario, "telefono")?.ToString() ?? GetValue(nuevoUsuario, "Telefono")?.ToString(),
                Avatar_URL = GetValue(nuevoUsuario, "avatar_url")?.ToString() ?? GetValue(nuevoUsuario, "Avatar_URL")?.ToString(),
                Empresa_Id = GetValue(nuevoUsuario, "empresa_id") as int? ?? GetValue(nuevoUsuario, "Empresa_Id") as int?,
                Empresa_Nombre = GetValue(nuevoUsuario, "empresa_nombre")?.ToString() ?? GetValue(nuevoUsuario, "Empresa_Nombre")?.ToString(),
                Rol_Id = (int)(GetValue(nuevoUsuario, "rol_id") ?? GetValue(nuevoUsuario, "Rol_Id") ?? 0),
                Rol = rolNombre,
                Nivel_Acceso = (int)(GetValue(nuevoUsuario, "nivel_acceso") ?? GetValue(nuevoUsuario, "Nivel_Acceso") ?? 0)
            }
        };
    }

    public async Task<AuthResponse?> RegisterClienteAsync(RegisterClienteRequest request)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(); // Abrir conexión antes de iniciar transacción
        using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            // 1. Verificar si el email ya existe
            var existingEmail = await connection.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT * FROM Usuario WHERE Email = @Email",
                new { request.Email },
                transaction);
            
            if (existingEmail != null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // 2. Verificar si el documento ya existe
            var existingDoc = await connection.QueryFirstOrDefaultAsync<Cliente>(
                "SELECT * FROM Cliente WHERE Documento = @Documento AND Empresa_Id = @EmpresaId",
                new { Documento = request.Documento, EmpresaId = request.Empresa_Id },
                transaction);
            
            if (existingDoc != null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // 3. Crear el usuario con Rol_Id = 4 (Cliente)
            // Estado: Activo para Personas, Inactivo para Empresas (requieren aprobación)
            var estadoInicial = request.Tipo_Cliente == "Persona" ? "Activo" : "Inactivo";
            
            var usuarioSql = @"
                INSERT INTO Usuario (Empresa_Id, Rol_Id, Usuario, Email, Password_Hash, Nombre_Completo, 
                                    Telefono, Estado, Intentos_Fallidos, Fecha_Creacion, Fecha_Actualizacion,
                                    Requiere_Cambio_Password, TwoFA_Activo)
                VALUES (@EmpresaId, 4, @Email, @Email, @PasswordHash, @NombreCompleto, 
                        @Telefono, @Estado, 0, @FechaCreacion, @FechaActualizacion, false, false)
                RETURNING id;";
            
            var usuarioId = await connection.ExecuteScalarAsync<int>(usuarioSql, new
            {
                EmpresaId = request.Empresa_Id,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                NombreCompleto = request.Nombre_Completo,
                Telefono = request.Telefono,
                Estado = estadoInicial,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            }, transaction);

            // 4. Crear el perfil de cliente vinculado al usuario
            var clienteSql = @"
                INSERT INTO Cliente (Empresa_Id, Usuario_Id, Tipo_Cliente, Nombre, Documento, Tipo_Documento,
                                    Email, Telefono, Direccion, Ciudad, Rating, Total_Alquileres, 
                                    Total_Danos_Reportados, Estado, Fecha_Registro, Fecha_Actualizacion)
                VALUES (@EmpresaId, @UsuarioId, @TipoCliente, @Nombre, @Documento, @TipoDocumento,
                        @Email, @Telefono, @Direccion, @Ciudad, 5.0, 0, 0, 'Activo', 
                        @FechaRegistro, @FechaActualizacion);";
            
            await connection.ExecuteAsync(clienteSql, new
            {
                EmpresaId = request.Empresa_Id,
                UsuarioId = usuarioId,
                TipoCliente = request.Tipo_Cliente,
                Nombre = request.Nombre_Completo,
                Documento = request.Documento,
                TipoDocumento = request.Tipo_Documento,
                Email = request.Email,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                Ciudad = request.Ciudad,
                FechaRegistro = DateTime.Now,
                FechaActualizacion = DateTime.Now
            }, transaction);

            await transaction.CommitAsync();

            // 5. Generar respuesta según tipo de cliente
            if (request.Tipo_Cliente == "Persona")
            {
                // Para personas: login automático
                var sql = @"
                    SELECT u.*, r.Nombre as RolNombre, r.Nivel_Acceso, e.Razon_Social as Empresa_Nombre
                    FROM Usuario u 
                    INNER JOIN Rol r ON u.Rol_Id = r.Id 
                    LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
                    WHERE u.Id = @UserId";
                
                var nuevoUsuario = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { UserId = usuarioId });
                
                if (nuevoUsuario == null)
                {
                    throw new InvalidOperationException("Usuario registrado no encontrado después de la creación");
                }
                
                var token = GenerateJwtToken(nuevoUsuario);
                var refreshToken = Guid.NewGuid().ToString();

                // Normalizar rol
                var rolNombre = NormalizeRole(GetValue(nuevoUsuario, "rolnombre")?.ToString() ?? GetValue(nuevoUsuario, "RolNombre")?.ToString() ?? "");

                return new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    Usuario = new UsuarioDto
                    {
                        Id = (int)(GetValue(nuevoUsuario, "id") ?? GetValue(nuevoUsuario, "Id") ?? usuarioId),
                        Usuario = GetValue(nuevoUsuario, "usuario")?.ToString() ?? GetValue(nuevoUsuario, "Usuario")?.ToString() ?? "",
                        Email = GetValue(nuevoUsuario, "email")?.ToString() ?? GetValue(nuevoUsuario, "Email")?.ToString() ?? "",
                        Nombre_Completo = GetValue(nuevoUsuario, "nombre_completo")?.ToString() ?? GetValue(nuevoUsuario, "Nombre_Completo")?.ToString() ?? "",
                        Telefono = GetValue(nuevoUsuario, "telefono")?.ToString() ?? GetValue(nuevoUsuario, "Telefono")?.ToString(),
                        Avatar_URL = GetValue(nuevoUsuario, "avatar_url")?.ToString() ?? GetValue(nuevoUsuario, "Avatar_URL")?.ToString(),
                        Empresa_Id = GetValue(nuevoUsuario, "empresa_id") as int? ?? GetValue(nuevoUsuario, "Empresa_Id") as int?,
                        Empresa_Nombre = GetValue(nuevoUsuario, "empresa_nombre")?.ToString() ?? GetValue(nuevoUsuario, "Empresa_Nombre")?.ToString(),
                        Rol_Id = (int)(GetValue(nuevoUsuario, "rol_id") ?? GetValue(nuevoUsuario, "Rol_Id") ?? 0),
                        Rol = rolNombre,
                        Nivel_Acceso = (int)(GetValue(nuevoUsuario, "nivel_acceso") ?? GetValue(nuevoUsuario, "Nivel_Acceso") ?? 0)
                    },
                    Message = "Registro exitoso. Bienvenido a EventConnect."
                };
            }
            else
            {
                // Para empresas: sin token, requiere aprobación
                return new AuthResponse
                {
                    Token = null,
                    RefreshToken = null,
                    Expiration = null,
                    Usuario = new UsuarioDto
                    {
                        Id = usuarioId,
                        Usuario = request.Email,
                        Email = request.Email,
                        Nombre_Completo = request.Nombre_Completo,
                        Empresa_Id = request.Empresa_Id,
                        Empresa_Nombre = null,
                        Rol_Id = 3,
                        Rol = "Cliente-Final",
                        Nivel_Acceso = 1
                    },
                    Message = "Registro exitoso. Tu cuenta será activada por un administrador."
                };
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implementar refresh tokens con almacenamiento en BD
        throw new NotImplementedException();
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateJwtToken(dynamic usuario)
    {
        try
    {
        var key = Encoding.ASCII.GetBytes(GetJwtSecret());

            // Extraer valores de forma segura para PostgreSQL (campos en minúsculas)
            var userId = GetValue(usuario, "id") ?? GetValue(usuario, "Id") ?? 0;
            var username = GetValue(usuario, "usuario")?.ToString() ?? GetValue(usuario, "Usuario")?.ToString() ?? "";
            var email = GetValue(usuario, "email")?.ToString() ?? GetValue(usuario, "Email")?.ToString() ?? "";
            var empresaId = GetValue(usuario, "empresa_id") ?? GetValue(usuario, "Empresa_Id");
            var nivelAcceso = GetValue(usuario, "nivel_acceso") ?? GetValue(usuario, "Nivel_Acceso") ?? 0;
            
            // Normalizar roles según los 4 roles principales del sistema
            var rolNombre = NormalizeRole(GetValue(usuario, "rolnombre")?.ToString() ?? GetValue(usuario, "RolNombre")?.ToString() ?? "");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, rolNombre),
                    new Claim("EmpresaId", empresaId?.ToString() ?? ""),
                    new Claim("NivelAcceso", nivelAcceso.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Issuer = GetJwtIssuer(),
            Audience = GetJwtAudience(),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al generar token JWT: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Normaliza los roles de la base de datos a los 4 roles principales del sistema
    /// </summary>
    private string NormalizeRole(string? rolNombre)
    {
        if (string.IsNullOrWhiteSpace(rolNombre))
            return "Cliente";

        return rolNombre.Trim() switch
        {
            "SuperAdmin" => "SuperAdmin",
            "Admin-Proveedor" => "Admin-Proveedor",
            "Operario-Logística" or "Operario" => "Operario",
            "Cliente-Final" or "Cliente" => "Cliente",
            _ => rolNombre // Mantener el nombre original si no coincide
        };
    }

    /// <summary>
    /// Extrae valores de objetos dynamic de forma segura (maneja campos en minúsculas de PostgreSQL)
    /// Dapper devuelve objetos dynamic que pueden accederse como diccionarios o propiedades
    /// </summary>
    private object? GetValue(dynamic obj, string key)
    {
        if (obj == null) return null;

        try
        {
            // Dapper devuelve objetos dynamic como IDictionary<string, object>
            if (obj is IDictionary<string, object> dict)
            {
                // Intentar con la clave exacta (minúsculas para PostgreSQL)
                if (dict.ContainsKey(key))
                    return dict[key];

                // Intentar con la clave en minúsculas
                var lowerKey = key.ToLowerInvariant();
                if (dict.ContainsKey(lowerKey))
                    return dict[lowerKey];

                // Intentar con la clave en PascalCase (primera letra mayúscula)
                var pascalKey = char.ToUpperInvariant(key[0]) + (key.Length > 1 ? key.Substring(1).ToLowerInvariant() : "");
                if (dict.ContainsKey(pascalKey))
                    return dict[pascalKey];

                return null;
            }

            // Si no es un diccionario, intentar acceso directo como propiedad
            var type = ((object)obj).GetType();
            var prop = type.GetProperty(key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return prop?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private string GetJwtSecret()
    {
        return _configuration["JwtSettings:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured");
    }

    private string GetJwtIssuer()
    {
        return _configuration["JwtSettings:Issuer"] ?? "EventConnectAPI";
    }

    private string GetJwtAudience()
    {
        return _configuration["JwtSettings:Audience"] ?? "EventConnectClients";
    }

    private int GetTokenExpirationMinutes()
    {
        return int.TryParse(_configuration["JwtSettings:TokenExpirationMinutes"], out var minutes) 
            ? minutes : 60;
    }

    public async Task<UsuarioDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            // Validar que el usuario exista
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
            {
                return null;
            }

            // Preparar los valores a actualizar (solo los campos que vienen en el request)
            var nombreCompleto = !string.IsNullOrWhiteSpace(request.Nombre_Completo) 
                ? request.Nombre_Completo 
                : null;
            var telefono = !string.IsNullOrWhiteSpace(request.Telefono) 
                ? request.Telefono 
                : null;
            var avatarUrl = request.Avatar_URL; // Puede ser null para eliminar el avatar

            // Actualizar el perfil usando el repositorio
            var updated = await _usuarioRepository.UpdatePerfilAsync(
                userId, 
                nombreCompleto, 
                telefono, 
                avatarUrl
            );

            if (!updated)
            {
                return null;
            }

            // Si se actualiza el email, hacerlo por separado (requiere validación adicional)
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != usuario.Email)
            {
                // Verificar que el email no esté en uso por otro usuario
                var existingEmail = await _usuarioRepository.GetByEmailAsync(request.Email);
                if (existingEmail != null && existingEmail.Id != userId)
                {
                    throw new InvalidOperationException("El email ya está en uso por otro usuario");
                }

                // Actualizar el email
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var updateEmailSql = "UPDATE Usuario SET Email = @Email, Fecha_Actualizacion = NOW() WHERE Id = @UserId";
                await connection.ExecuteAsync(updateEmailSql, new { Email = request.Email, UserId = userId });
            }

            // Obtener el usuario actualizado con toda la información
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            var sql = @"
                SELECT 
                    u.Id,
                    u.Usuario,
                    u.Email,
                    u.Nombre_Completo,
                    u.Telefono,
                    u.Avatar_URL,
                    u.Empresa_Id,
                    u.Rol_Id,
                    r.Nombre as RolNombre, 
                    r.Nivel_Acceso, 
                    e.Razon_Social as Empresa_Nombre
                FROM Usuario u 
                INNER JOIN Rol r ON u.Rol_Id = r.Id 
                LEFT JOIN Empresa e ON u.Empresa_Id = e.Id
                WHERE u.Id = @UserId";
            
            var usuarioActualizado = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { UserId = userId });
            
            if (usuarioActualizado == null)
            {
                return null;
            }

            // Normalizar rol
            var rolNombre = NormalizeRole(usuarioActualizado.RolNombre?.ToString() ?? usuarioActualizado.rolnombre?.ToString() ?? "");

            return new UsuarioDto
            {
                Id = GetValue(usuarioActualizado, "id") as int? ?? GetValue(usuarioActualizado, "Id") as int? ?? userId,
                Usuario = GetValue(usuarioActualizado, "usuario")?.ToString() ?? GetValue(usuarioActualizado, "Usuario")?.ToString() ?? "",
                Email = GetValue(usuarioActualizado, "email")?.ToString() ?? GetValue(usuarioActualizado, "Email")?.ToString() ?? "",
                Nombre_Completo = GetValue(usuarioActualizado, "nombre_completo")?.ToString() ?? GetValue(usuarioActualizado, "Nombre_Completo")?.ToString() ?? "",
                Telefono = GetValue(usuarioActualizado, "telefono")?.ToString() ?? GetValue(usuarioActualizado, "Telefono")?.ToString(),
                Avatar_URL = GetValue(usuarioActualizado, "avatar_url")?.ToString() ?? GetValue(usuarioActualizado, "Avatar_URL")?.ToString(),
                Empresa_Id = GetValue(usuarioActualizado, "empresa_id") as int? ?? GetValue(usuarioActualizado, "Empresa_Id") as int?,
                Empresa_Nombre = GetValue(usuarioActualizado, "empresa_nombre")?.ToString() ?? GetValue(usuarioActualizado, "Empresa_Nombre")?.ToString(),
                Rol_Id = GetValue(usuarioActualizado, "rol_id") as int? ?? GetValue(usuarioActualizado, "Rol_Id") as int? ?? 0,
                Rol = rolNombre,
                Nivel_Acceso = GetValue(usuarioActualizado, "nivel_acceso") as int? ?? GetValue(usuarioActualizado, "Nivel_Acceso") as int? ?? 0
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al actualizar perfil: {ex.Message}", ex);
        }
    }
}
