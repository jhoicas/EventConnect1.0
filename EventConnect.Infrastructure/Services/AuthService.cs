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
            var userId = usuario.id ?? usuario.Id;
            
            // Verificar que Id no sea null - con mejor logging
            if (userId == null)
            {
                // En desarrollo, mostrar todos los campos para debugging
                var fields = ((IDictionary<string, object>)usuario).Keys;
                var fieldsStr = string.Join(", ", fields);
                throw new InvalidOperationException($"El usuario '{request.Username}' no tiene un ID válido. Campos disponibles: {fieldsStr}");
            }

            // Verificar si está bloqueado
            var intentosFallidos = (usuario.intentos_fallidos ?? usuario.Intentos_Fallidos) ?? 0;
            if ((int)intentosFallidos >= 5)
            {
                return null; // Usuario bloqueado
            }

            // Verificar contraseña
            var passwordHash = (usuario.password_hash ?? usuario.Password_Hash)?.ToString() ?? "";
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

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                Usuario = new UsuarioDto
                {
                    Id = (int)(usuario.id ?? usuario.Id ?? 0),
                    Usuario = (usuario.usuario ?? usuario.Usuario)?.ToString() ?? "",
                    Email = (usuario.email ?? usuario.Email)?.ToString() ?? "",
                    Nombre_Completo = (usuario.nombre_completo ?? usuario.Nombre_Completo)?.ToString(),
                    Telefono = (usuario.telefono ?? usuario.Telefono)?.ToString(),
                    Avatar_URL = (usuario.avatar_url ?? usuario.Avatar_URL)?.ToString(),
                    Empresa_Id = (usuario.empresa_id ?? usuario.Empresa_Id) != null ? (int?)(usuario.empresa_id ?? usuario.Empresa_Id) : null,
                    Empresa_Nombre = (usuario.empresa_nombre ?? usuario.Empresa_Nombre)?.ToString(),
                    Rol_Id = (int)((usuario.rol_id ?? usuario.Rol_Id) ?? 0),
                    Rol = (usuario.rolnombre ?? usuario.RolNombre)?.ToString() ?? "",
                    Nivel_Acceso = (int)((usuario.nivel_acceso ?? usuario.Nivel_Acceso) ?? 0)
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

        // Crear nuevo usuario
        var usuario = new Usuario
        {
            Empresa_Id = request.Empresa_Id,
            Rol_Id = request.Rol_Id,
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
        
        var token = GenerateJwtToken(nuevoUsuario);
        var refreshToken = Guid.NewGuid().ToString();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Usuario = new UsuarioDto
            {
                Id = nuevoUsuario.Id,
                Usuario = nuevoUsuario.Usuario,
                Email = nuevoUsuario.Email,
                Nombre_Completo = nuevoUsuario.Nombre_Completo,
                Empresa_Id = nuevoUsuario.Empresa_Id,
                Empresa_Nombre = nuevoUsuario.Empresa_Nombre,
                Rol_Id = nuevoUsuario.Rol_Id,
                Rol = nuevoUsuario.RolNombre,
                Nivel_Acceso = nuevoUsuario.Nivel_Acceso
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
                        @Telefono, @Estado, 0, @FechaCreacion, @FechaActualizacion, false, false);
                SELECT LAST_INSERT_ID();";
            
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
                
                var token = GenerateJwtToken(nuevoUsuario);
                var refreshToken = Guid.NewGuid().ToString();

                return new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    Usuario = new UsuarioDto
                    {
                        Id = nuevoUsuario.Id,
                        Usuario = nuevoUsuario.Usuario,
                        Email = nuevoUsuario.Email,
                        Nombre_Completo = nuevoUsuario.Nombre_Completo,
                        Telefono = nuevoUsuario.Telefono,
                        Avatar_URL = nuevoUsuario.Avatar_URL,
                        Empresa_Id = nuevoUsuario.Empresa_Id,
                        Empresa_Nombre = nuevoUsuario.Empresa_Nombre,
                        Rol_Id = nuevoUsuario.Rol_Id,
                        Rol = nuevoUsuario.RolNombre,
                        Nivel_Acceso = nuevoUsuario.Nivel_Acceso
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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id?.ToString() ?? "0"),
                    new Claim(ClaimTypes.Name, usuario.Usuario?.ToString() ?? ""),
                    new Claim(ClaimTypes.Email, usuario.Email?.ToString() ?? ""),
                    new Claim(ClaimTypes.Role, usuario.RolNombre?.ToString() ?? ""),
                    new Claim("EmpresaId", usuario.Empresa_Id?.ToString() ?? ""),
                    new Claim("NivelAcceso", usuario.Nivel_Acceso?.ToString() ?? "0")
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
}
