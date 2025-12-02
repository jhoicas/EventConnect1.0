using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class CategoriaController : BaseController
{
    private readonly CategoriaRepository _repository;
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(IConfiguration configuration, ILogger<CategoriaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new CategoriaRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // Allow anonymous access to view all categories
            var categorias = await _repository.GetAllAsync();
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null)
                return NotFound(new { message = "Categoría no encontrada" });

            // Categories are global, no permission check needed
            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Categoria categoria)
    {
        try
        {
            // Only SuperAdmin can create global categories
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return BadRequest(new { message = "El nombre de la categoría es requerido" });
            }

            // Set defaults for optional fields
            if (string.IsNullOrWhiteSpace(categoria.Icono))
            {
                categoria.Icono = "sparkles"; // Default icon
            }

            if (string.IsNullOrWhiteSpace(categoria.Color))
            {
                categoria.Color = "#3B82F6"; // Default blue color
            }

            categoria.Fecha_Creacion = DateTime.Now;
            categoria.Activo = true;

            var id = await _repository.AddAsync(categoria);
            categoria.Id = id;

            _logger.LogInformation("Categoría global creada: {Id} - {Nombre} por SuperAdmin {UserId}", id, categoria.Nombre, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria categoria)
    {
        try
        {
            // Only SuperAdmin can modify global categories
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Categoría no encontrada" });

            // Preserve ID and creation date
            categoria.Id = id;
            categoria.Fecha_Creacion = existing.Fecha_Creacion;
            
            var success = await _repository.UpdateAsync(categoria);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar la categoría" });

            _logger.LogInformation("Categoría global actualizada: {Id} - {Nombre} por SuperAdmin {UserId}", id, categoria.Nombre, GetCurrentUserId());
            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Only SuperAdmin can delete global categories
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Categoría no encontrada" });

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar la categoría" });

            _logger.LogInformation("Categoría global eliminada: {Id} - {Nombre} por SuperAdmin {UserId}", id, existing.Nombre, GetCurrentUserId());
            return Ok(new { message = "Categoría eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
