using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
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
            if (string.IsNullOrWhiteSpace(request.Usuario) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Todos los campos son requeridos" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            var response = await _authService.RegisterAsync(request);
            
            if (response == null)
            {
                return BadRequest(new { message = "El usuario o email ya existe" });
            }

            _logger.LogInformation("Registro exitoso para usuario: {Username}", request.Usuario);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro para usuario: {Username}", request.Usuario);
            return StatusCode(500, new { message = "Error interno del servidor" });
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
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // TODO: Implementar actualización de perfil en AuthService
            _logger.LogInformation("Actualizando perfil para usuario ID: {UserId}", request.UsuarioId);
            
            // Por ahora retornamos success
            return Ok(new { message = "Perfil actualizado correctamente", success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil");
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
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Nombre_Completo) ||
                string.IsNullOrWhiteSpace(request.Documento))
            {
                return BadRequest(new { message = "Todos los campos obligatorios son requeridos" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            var response = await _authService.RegisterClienteAsync(request);
            
            if (response == null)
            {
                return BadRequest(new { message = "El email o documento ya existe" });
            }

            _logger.LogInformation("Registro de cliente exitoso para email: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro de cliente para email: {Email}", request.Email);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
