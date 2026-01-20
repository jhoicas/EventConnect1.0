using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class ActivoController : BaseController
{
    private readonly ActivoRepository _repository;
    private readonly ILogger<ActivoController> _logger;

    public ActivoController(IConfiguration configuration, ILogger<ActivoController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new ActivoRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                // Si no hay autenticación, retornar lista vacía
                return Ok(new List<Activo>());
            }

            IEnumerable<Activo> activos;
            if (IsSuperAdmin() && empresaId == null)
            {
                activos = await _repository.GetAllAsync();
            }
            else
            {
                activos = await _repository.GetByEmpresaIdAsync(empresaId!.Value);
            }

            return Ok(activos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var activo = await _repository.GetByIdAsync(id);
            if (activo == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && activo.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> GetByCodigo(string codigo)
    {
        try
        {
            var activo = await _repository.GetByCodigoActivoAsync(codigo);
            if (activo == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && activo.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activo por código {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("qr/{qrCode}")]
    public async Task<IActionResult> GetByQR(string qrCode)
    {
        try
        {
            var activo = await _repository.GetByQRCodeAsync(qrCode);
            if (activo == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && activo.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activo por QR {QR}", qrCode);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene la hoja de vida completa de un activo
    /// </summary>
    /// <param name="id">ID del activo</param>
    /// <returns>Hoja de vida con historial de movimientos, mantenimientos y reservas</returns>
    /// <response code="200">Hoja de vida encontrada</response>
    /// <response code="404">Activo no encontrado</response>
    /// <response code="403">No autorizado para ver este activo</response>
    [HttpGet("{id}/hoja-vida")]
    public async Task<IActionResult> GetHojaVida(int id)
    {
        try
        {
            // Validar que el activo existe y el usuario tiene acceso
            var activo = await _repository.GetByIdAsync(id);
            if (activo == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && activo.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            // Obtener hoja de vida completa
            var hojaVida = await _repository.GetHojaVidaAsync(id);
            if (hojaVida == null)
                return NotFound(new { message = "Hoja de vida no encontrada" });

            _logger.LogInformation("Hoja de vida consultada para activo {ActivoId} por usuario {UserId}", 
                id, GetCurrentUserId());

            return Ok(hojaVida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener hoja de vida del activo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("bodega/{bodegaId}")]
    public async Task<IActionResult> GetByBodega(int bodegaId)
    {
        try
        {
            var activos = await _repository.GetByBodegaIdAsync(bodegaId);
            return Ok(activos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos por bodega {BodegaId}", bodegaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("estado-disponibilidad/{estadoDisponibilidad}")]
    public async Task<IActionResult> GetByEstadoDisponibilidad(string estadoDisponibilidad)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var activos = await _repository.GetByEstadoDisponibilidadAsync(empresaId!.Value, estadoDisponibilidad);
            return Ok(activos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos por estado disponibilidad {EstadoDisponibilidad}", estadoDisponibilidad);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("depreciacion")]
    public async Task<IActionResult> GetActivosParaDepreciacion()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var activos = await _repository.GetActivosParaDepreciacionAsync(empresaId!.Value);
            return Ok(activos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos para depreciación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Activo activo)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            activo.Empresa_Id = empresaId ?? activo.Empresa_Id;
            activo.Fecha_Creacion = DateTime.Now;
            activo.Fecha_Actualizacion = DateTime.Now;

            var id = await _repository.AddAsync(activo);
            activo.Id = id;

            _logger.LogInformation("Activo creado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear activo");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Activo activo)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            activo.Id = id;
            activo.Empresa_Id = existing.Empresa_Id;
            activo.Fecha_Actualizacion = DateTime.Now;
            
            var success = await _repository.UpdateAsync(activo);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar el activo" });

            _logger.LogInformation("Activo actualizado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar activo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Activo no encontrado" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el activo" });

            _logger.LogInformation("Activo eliminado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Activo eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar activo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
