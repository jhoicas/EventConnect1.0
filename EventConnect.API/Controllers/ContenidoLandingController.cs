using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Route("api/[controller]")]
public class ContenidoLandingController : ControllerBase
{
    private readonly ContenidoLandingRepository _repository;
    private readonly ILogger<ContenidoLandingController> _logger;

    public ContenidoLandingController(IConfiguration configuration, ILogger<ContenidoLandingController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new ContenidoLandingRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var contenidos = await _repository.GetAllAsync();
            return Ok(contenidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contenidos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("activos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivos()
    {
        try
        {
            var contenidos = await _repository.GetActivosAsync();
            return Ok(contenidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contenidos activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("seccion/{seccion}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySeccion(string seccion)
    {
        try
        {
            var contenidos = await _repository.GetBySeccionAsync(seccion);
            return Ok(contenidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contenidos de sección {Seccion}", seccion);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var contenido = await _repository.GetByIdAsync(id);
            if (contenido == null)
                return NotFound(new { message = "Contenido no encontrado" });

            return Ok(contenido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contenido {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] ContenidoLanding contenido)
    {
        try
        {
            var id = await _repository.CreateAsync(contenido);
            contenido.Id = id;

            return CreatedAtAction(nameof(GetById), new { id }, contenido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contenido");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ContenidoLanding contenido)
    {
        try
        {
            contenido.Id = id;
            var success = await _repository.UpdateAsync(contenido);
            
            if (!success)
                return NotFound(new { message = "Contenido no encontrado" });

            return Ok(contenido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contenido {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _repository.DeleteAsync(id);
            
            if (!success)
                return NotFound(new { message = "Contenido no encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar contenido {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
