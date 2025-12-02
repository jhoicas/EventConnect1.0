using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ConfiguracionSistemaController : BaseController
{
    private readonly ConfiguracionSistemaRepository _repository;
    private readonly ILogger<ConfiguracionSistemaController> _logger;

    public ConfiguracionSistemaController(IConfiguration configuration, ILogger<ConfiguracionSistemaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new ConfiguracionSistemaRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            
            IEnumerable<ConfiguracionSistema> configuraciones;
            if (IsSuperAdmin())
            {
                configuraciones = await _repository.GetAllAsync();
            }
            else if (empresaId.HasValue)
            {
                configuraciones = await _repository.GetByEmpresaIdAsync(empresaId.Value);
            }
            else
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            return Ok(configuraciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuraciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("globales")]
    public async Task<IActionResult> GetGlobales()
    {
        try
        {
            var configuraciones = await _repository.GetGlobalesAsync();
            return Ok(configuraciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuraciones globales");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var configuracion = await _repository.GetByIdAsync(id);
            if (configuracion == null)
                return NotFound(new { message = "Configuración no encontrada" });

            // Verificar permisos
            if (!IsSuperAdmin() && !configuracion.Es_Global && configuracion.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("clave/{clave}")]
    public async Task<IActionResult> GetByClave(string clave)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            var configuracion = await _repository.GetByClaveAsync(clave, empresaId);
            
            if (configuracion == null)
                return NotFound(new { message = "Configuración no encontrada" });

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración por clave {Clave}", clave);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConfiguracionSistema configuracion)
    {
        try
        {
            // Solo SuperAdmin puede crear configuraciones globales
            if (configuracion.Es_Global && !IsSuperAdmin())
            {
                return Forbid();
            }

            // Si no es global, asignar empresa actual
            if (!configuracion.Es_Global)
            {
                var empresaId = GetCurrentEmpresaId();
                if (!empresaId.HasValue)
                {
                    return BadRequest(new { message = "Empresa no válida" });
                }
                configuracion.Empresa_Id = empresaId.Value;
            }

            var id = await _repository.CreateAsync(configuracion);
            configuracion.Id = id;

            return CreatedAtAction(nameof(GetById), new { id }, configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear configuración");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ConfiguracionSistema configuracion)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Configuración no encontrada" });

            // Verificar permisos
            if (!IsSuperAdmin())
            {
                if (existing.Es_Global)
                {
                    return Forbid();
                }
                if (existing.Empresa_Id != GetCurrentEmpresaId())
                {
                    return Forbid();
                }
            }

            configuracion.Id = id;
            
            // Preservar empresa y tipo global original si no es SuperAdmin
            if (!IsSuperAdmin())
            {
                configuracion.Empresa_Id = existing.Empresa_Id;
                configuracion.Es_Global = existing.Es_Global;
            }

            var success = await _repository.UpdateAsync(configuracion);
            if (!success)
                return NotFound(new { message = "Configuración no encontrada" });

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var configuracion = await _repository.GetByIdAsync(id);
            if (configuracion == null)
                return NotFound(new { message = "Configuración no encontrada" });

            // Verificar permisos
            if (!IsSuperAdmin())
            {
                if (configuracion.Es_Global)
                {
                    return Forbid();
                }
                if (configuracion.Empresa_Id != GetCurrentEmpresaId())
                {
                    return Forbid();
                }
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Configuración no encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar configuración {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
