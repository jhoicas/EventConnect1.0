using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class BodegaController : BaseController
{
    private readonly BodegaRepository _repository;
    private readonly ILogger<BodegaController> _logger;

    public BodegaController(IConfiguration configuration, ILogger<BodegaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new BodegaRepository(connectionString);
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
                return Ok(new List<Bodega>());
            }

            IEnumerable<Bodega> bodegas;
            if (IsSuperAdmin() && empresaId == null)
            {
                bodegas = await _repository.GetAllAsync();
            }
            else
            {
                bodegas = await _repository.GetByEmpresaIdAsync(empresaId!.Value);
            }

            return Ok(bodegas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bodegas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var bodega = await _repository.GetByIdAsync(id);
            if (bodega == null)
                return NotFound(new { message = "Bodega no encontrada" });

            if (!IsSuperAdmin() && bodega.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(bodega);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener bodega {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bodega bodega)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            bodega.Empresa_Id = empresaId ?? bodega.Empresa_Id;
            bodega.Fecha_Creacion = DateTime.Now;
            bodega.Fecha_Actualizacion = DateTime.Now;

            var id = await _repository.AddAsync(bodega);
            bodega.Id = id;

            _logger.LogInformation("Bodega creada: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, bodega);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear bodega");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bodega bodega)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Bodega no encontrada" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            bodega.Id = id;
            bodega.Empresa_Id = existing.Empresa_Id;
            bodega.Fecha_Actualizacion = DateTime.Now;
            
            var success = await _repository.UpdateAsync(bodega);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar la bodega" });

            _logger.LogInformation("Bodega actualizada: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(bodega);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar bodega {Id}", id);
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
                return NotFound(new { message = "Bodega no encontrada" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar la bodega" });

            _logger.LogInformation("Bodega eliminada: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Bodega eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar bodega {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
