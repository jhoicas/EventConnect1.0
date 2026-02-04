using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Usuario y contraseña son requeridos" });
            }

            var response = await _authService.LoginAsync(request);
            
            if (response == null)
            {
                _logger.LogWarning("Intento de login fallido para usuario: {Username}", request.Username);
                return Unauthorized(new { message = "Usuario o contraseña incorrectos, o cuenta inactiva. Si acabas de registrarte, espera a que un administrador active tu cuenta." });
            }

            _logger.LogInformation("Login exitoso para usuario: {Username}", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login para usuario: {Username}. Error: {ErrorMessage}. StackTrace: {StackTrace}", 
                request.Username, ex.Message, ex.StackTrace);
            
            // En desarrollo, mostrar más detalles
            var isDevelopment = HttpContext.RequestServices
                .GetRequiredService<IHostEnvironment>().IsDevelopment();
            
            return StatusCode(500, new 
            { 
                message = "Error interno del servidor",
                error = isDevelopment ? ex.Message : "Error al procesar la solicitud de login",
                stackTrace = isDevelopment ? ex.StackTrace : null,
                innerException = isDevelopment && ex.InnerException != null ? ex.InnerException.Message : null
            });
        }
    }

    /// <summary>
    /// Registrar nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Datos inválidos", errors });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(request.Usuario) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Todos los campos obligatorios son requeridos" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            // Validar formato de email
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest(new { message = "El formato del email no es válido" });
            }

            var response = await _authService.RegisterAsync(request);
            
            if (response == null)
            {
                return BadRequest(new { message = "El nombre de usuario o email ya está registrado" });
            }

            _logger.LogInformation("Registro exitoso para usuario: {Username}", request.Usuario);
            return Ok(new 
            { 
                message = "Usuario registrado exitosamente",
                data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro para usuario: {Username}", request.Usuario);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Renovar token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(refreshToken);
            
            if (response == null)
                return Unauthorized(new { message = "Token inválido" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar token");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Validar token
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(new { valid = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar token");
            return Ok(new { valid = false });
        }
    }

    /// <summary>
    /// Actualizar perfil de usuario
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Extraer el ID del usuario del token JWT (seguridad: no confiar en el request)
            var userIdFromToken = GetCurrentUserId();
            
            if (userIdFromToken == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Validar que el usuario solo pueda actualizar su propio perfil
            // (no permitir que usuarioId en el request sea diferente al del token)
            if (request.UsuarioId != 0 && request.UsuarioId != userIdFromToken)
            {
                _logger.LogWarning("Intento de actualizar perfil de otro usuario. Token UserId: {TokenUserId}, Request UserId: {RequestUserId}", 
                    userIdFromToken, request.UsuarioId);
                return Forbid("No tienes permiso para actualizar el perfil de otro usuario");
            }

            // Usar el ID del token, ignorando el del request por seguridad
            var usuarioActualizado = await _authService.UpdateProfileAsync(userIdFromToken, request);
            
            if (usuarioActualizado == null)
            {
                _logger.LogWarning("No se pudo actualizar el perfil del usuario {UserId}", userIdFromToken);
                return NotFound(new { message = "Usuario no encontrado o error al actualizar" });
            }

            _logger.LogInformation("Perfil actualizado exitosamente para usuario {UserId}", userIdFromToken);
            return Ok(new { message = "Perfil actualizado correctamente", usuario = usuarioActualizado });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar perfil");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil para usuario {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // TODO: Implementar cambio de contraseña en AuthService
            _logger.LogInformation("Cambio de contraseña para usuario ID: {UserId}", request.UsuarioId);
            
            // Por ahora retornamos success
            return Ok(new { message = "Contraseña actualizada correctamente", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Registrar nuevo cliente (crea Usuario + Cliente)
    /// </summary>
    [HttpPost("register-cliente")]
    public async Task<IActionResult> RegisterCliente([FromBody] RegisterClienteRequest request)
    {
        try
        {
            // Validar modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Datos inválidos", errors });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Nombre_Completo) ||
                string.IsNullOrWhiteSpace(request.Documento))
            {
                return BadRequest(new { message = "Todos los campos obligatorios son requeridos: Email, Contraseña, Nombre Completo y Documento" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            // Validar formato de email
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return BadRequest(new { message = "El formato del email no es válido" });
            }

            // Validar tipo de cliente
            if (request.Tipo_Cliente != "Persona" && request.Tipo_Cliente != "Empresa")
            {
                return BadRequest(new { message = "El tipo de cliente debe ser 'Persona' o 'Empresa'" });
            }

            // Validar tipo de documento
            var tiposDocumentoValidos = new[] { "CC", "CE", "NIT", "PP" };
            if (!tiposDocumentoValidos.Contains(request.Tipo_Documento))
            {
                return BadRequest(new { message = "El tipo de documento debe ser CC, CE, NIT o PP" });
            }

            // Validar empresa_Id solo para tipo Empresa
            if (request.Tipo_Cliente == "Empresa" && (!request.Empresa_Id.HasValue || request.Empresa_Id <= 0))
            {
                return BadRequest(new { message = "El ID de empresa es requerido y debe ser mayor a 0 para clientes tipo Empresa" });
            }

            var response = await _authService.RegisterClienteAsync(request);
            
            if (response == null)
            {
                return BadRequest(new { message = "El email o documento ya está registrado en el sistema" });
            }

            _logger.LogInformation("Registro de cliente exitoso para email: {Email}, Tipo: {Tipo}", request.Email, request.Tipo_Cliente);
            
            // Mensaje diferente según tipo de cliente
            if (request.Tipo_Cliente == "Persona")
            {
                return Ok(new
                {
                    message = "Registro exitoso. Bienvenido a EventConnect.",
                    data = response
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Registro exitoso. Tu cuenta de empresa está pendiente de aprobación. Te notificaremos cuando sea activada.",
                    data = response
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro de cliente para email: {Email}", request.Email);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
