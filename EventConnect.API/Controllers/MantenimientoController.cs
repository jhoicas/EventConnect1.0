using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class MantenimientoController : BaseController
{
    private readonly MantenimientoRepository _repository;
    private readonly ILogger<MantenimientoController> _logger;

    public MantenimientoController(IConfiguration configuration, ILogger<MantenimientoController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection")
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new MantenimientoRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // Para todos los usuarios, devolver listado completo
            var mantenimientos = await _repository.GetAllAsync();
            return Ok(mantenimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mantenimientos");
            // Devolver lista vacía en caso de error (tabla no existe, etc.)
            return Ok(new List<Mantenimiento>());
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var mantenimiento = await _repository.GetByIdAsync(id);
            if (mantenimiento == null)
                return NotFound(new { message = "Mantenimiento no encontrado" });

            return Ok(mantenimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mantenimiento {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("activo/{activoId}")]
    public async Task<IActionResult> GetByActivo(int activoId)
    {
        try
        {
            var mantenimientos = await _repository.GetByActivoIdAsync(activoId);
            return Ok(mantenimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mantenimientos por activo {ActivoId}", activoId);
            return Ok(new List<Mantenimiento>());
        }
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> GetPendientes()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null)
            {
                return Ok(new List<Mantenimiento>());
            }

            var mantenimientos = await _repository.GetPendientesAsync(empresaId.Value);
            return Ok(mantenimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mantenimientos pendientes");
            return Ok(new List<Mantenimiento>());
        }
    }

    [HttpGet("vencidos")]
    public async Task<IActionResult> GetVencidos()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null)
            {
                return Ok(new List<Mantenimiento>());
            }

            var mantenimientos = await _repository.GetVencidosAsync(empresaId.Value);
            return Ok(mantenimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mantenimientos vencidos");
            return Ok(new List<Mantenimiento>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Mantenimiento mantenimiento)
    {
        try
        {
            mantenimiento.Fecha_Creacion = DateTime.Now;
            var id = await _repository.AddAsync(mantenimiento);
            mantenimiento.Id = id;

            _logger.LogInformation("Mantenimiento creado: {Id} para activo {ActivoId} por usuario {UserId}",
                id, mantenimiento.Activo_Id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, mantenimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear mantenimiento");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Mantenimiento mantenimiento)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Mantenimiento no encontrado" });

            mantenimiento.Id = id;
            var success = await _repository.UpdateAsync(mantenimiento);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar el mantenimiento" });

            _logger.LogInformation("Mantenimiento actualizado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(mantenimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar mantenimiento {Id}", id);
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
                return NotFound(new { message = "Mantenimiento no encontrado" });

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el mantenimiento" });

            _logger.LogInformation("Mantenimiento eliminado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Mantenimiento eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar mantenimiento {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
