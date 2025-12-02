using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class LoteController : BaseController
{
    private readonly LoteRepository _repository;
    private readonly ILogger<LoteController> _logger;

    public LoteController(IConfiguration configuration, ILogger<LoteController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new LoteRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var lotes = await _repository.GetAllAsync();
            return Ok(lotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lotes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var lote = await _repository.GetByIdAsync(id);
            if (lote == null)
                return NotFound(new { message = "Lote no encontrado" });

            return Ok(lote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lote {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("producto/{productoId}")]
    public async Task<IActionResult> GetByProducto(int productoId)
    {
        try
        {
            var lotes = await _repository.GetByProductoIdAsync(productoId);
            return Ok(lotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lotes por producto {ProductoId}", productoId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("vencidos")]
    public async Task<IActionResult> GetVencidos()
    {
        try
        {
            var lotes = await _repository.GetLotesVencidosAsync();
            return Ok(lotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lotes vencidos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("por-vencer/{dias}")]
    public async Task<IActionResult> GetPorVencer(int dias)
    {
        try
        {
            var lotes = await _repository.GetLotesPorVencerAsync(dias);
            return Ok(lotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lotes por vencer");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Lote lote)
    {
        try
        {
            lote.Fecha_Creacion = DateTime.Now;
            var id = await _repository.AddAsync(lote);
            lote.Id = id;

            _logger.LogInformation("Lote creado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, lote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear lote");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Lote lote)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Lote no encontrado" });

            lote.Id = id;
            var success = await _repository.UpdateAsync(lote);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar el lote" });

            _logger.LogInformation("Lote actualizado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(lote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar lote {Id}", id);
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
                return NotFound(new { message = "Lote no encontrado" });

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el lote" });

            _logger.LogInformation("Lote eliminado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Lote eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar lote {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
