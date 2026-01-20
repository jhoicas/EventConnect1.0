using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuarioController : BaseController
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(
        IUsuarioRepository usuarioRepository, 
        ILogger<UsuarioController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los usuarios (filtrado por empresa para seguridad multi-tenant)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // SuperAdmin puede ver todos los usuarios (pasa null)
            // Admin-Proveedor y otros usuarios solo ven usuarios de su empresa
            int? empresaId = null;
            if (!IsSuperAdmin())
            {
                empresaId = GetCurrentEmpresaId();
                if (empresaId == null)
                {
                    return BadRequest(new { message = "Empresa no válida" });
                }
            }

            var usuarios = await _usuarioRepository.GetAllWithDetailsAsync(empresaId);
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, new { message = "Error al obtener usuarios" });
        }
    }

    /// <summary>
    /// Obtener conteo de usuarios empresa pendientes de activación
    /// </summary>
    [HttpGet("pendientes/count")]
    public async Task<IActionResult> GetPendingUsersCount()
    {
        try
        {
            var count = await _usuarioRepository.GetPendingUsersCountAsync();
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conteo de usuarios pendientes");
            return StatusCode(500, new { message = "Error al obtener conteo" });
        }
    }

    /// <summary>
    /// Obtener usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Validar multi-tenant: solo SuperAdmin puede ver usuarios de otras empresas
            if (!IsSuperAdmin() && usuario.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id}", id);
            return StatusCode(500, new { message = "Error al obtener usuario" });
        }
    }

    /// <summary>
    /// Actualizar estado del usuario
    /// </summary>
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoRequest request)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            usuario.Estado = request.Estado;
            usuario.Fecha_Actualizacion = DateTime.Now;

            await _usuarioRepository.UpdateAsync(usuario);
            
            _logger.LogInformation("Estado del usuario {Id} actualizado a {Estado}", id, request.Estado);
            return Ok(new { message = "Estado actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado del usuario {Id}", id);
            return StatusCode(500, new { message = "Error al actualizar estado" });
        }
    }

    /// <summary>
    /// Eliminar usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usuario no encontrado" });

            await _usuarioRepository.DeleteAsync(id);
            
            _logger.LogInformation("Usuario {Id} eliminado", id);
            return Ok(new { message = "Usuario eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
            return StatusCode(500, new { message = "Error al eliminar usuario" });
        }
    }

    /// <summary>
    /// Actualizar perfil de usuario
    /// </summary>
    [HttpPut("{id}/perfil")]
    public async Task<IActionResult> UpdatePerfil(int id, [FromBody] UpdatePerfilRequest request)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar perfil del usuario {Id}. Avatar_URL: {AvatarUrl}", id, request.Avatar_URL);
            
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario {Id} no encontrado", id);
                return NotFound(new { message = "Usuario no encontrado" });
            }

            _logger.LogInformation("Usuario encontrado: {NombreCompleto}", usuario.Nombre_Completo);

            // Usar el método específico que solo actualiza los campos enviados
            var updated = await _usuarioRepository.UpdatePerfilAsync(
                id, 
                request.Nombre_Completo, 
                request.Telefono, 
                request.Avatar_URL
            );

            if (!updated)
            {
                _logger.LogWarning("No se pudo actualizar el perfil del usuario {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar el perfil" });
            }

            // Obtener el usuario actualizado
            var usuarioActualizado = await _usuarioRepository.GetByIdAsync(id);
            
            _logger.LogInformation("Perfil del usuario {Id} actualizado exitosamente", id);
            return Ok(new { message = "Perfil actualizado exitosamente", usuario = usuarioActualizado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil del usuario {Id}: {ErrorMessage}", id, ex.Message);
            return StatusCode(500, new { message = "Error al actualizar perfil", error = ex.Message });
        }
    }

    /// <summary>
    /// Cambiar contraseña de usuario
    /// </summary>
    [HttpPut("{id}/cambiar-password")]
    public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Intentando cambiar contraseña del usuario {Id}", id);
            
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario {Id} no encontrado", id);
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Verificar la contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, usuario.Password_Hash))
            {
                _logger.LogWarning("Contraseña actual incorrecta para usuario {Id}", id);
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }

            // Validar la nueva contraseña
            if (request.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "La nueva contraseña debe tener al menos 8 caracteres" });
            }

            // Hash de la nueva contraseña
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            
            // Actualizar la contraseña
            var updated = await _usuarioRepository.UpdatePasswordAsync(id, newPasswordHash);
            
            if (!updated)
            {
                _logger.LogWarning("No se pudo actualizar la contraseña del usuario {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la contraseña" });
            }

            _logger.LogInformation("Contraseña cambiada exitosamente para usuario {Id}", id);
            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña del usuario {Id}", id);
            return StatusCode(500, new { message = "Error al cambiar la contraseña" });
        }
    }
}

public class UpdateEstadoRequest
{
    public string Estado { get; set; } = string.Empty;
}

public class UpdatePerfilRequest
{
    public string? Nombre_Completo { get; set; }
    public string? Telefono { get; set; }
    public string? Avatar_URL { get; set; }
}

public class CambiarPasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
