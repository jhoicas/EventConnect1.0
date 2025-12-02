using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class EmpresaController : BaseController
{
    private readonly EmpresaRepository _repository;
    private readonly ILogger<EmpresaController> _logger;

    public EmpresaController(IConfiguration configuration, ILogger<EmpresaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new EmpresaRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var empresas = await _repository.GetActivasAsync();
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var empresa = await _repository.GetByIdAsync(id);
            if (empresa == null)
            {
                return NotFound(new { message = "Empresa no encontrada" });
            }
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Empresa empresa)
    {
        try
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var id = await _repository.AddAsync(empresa);
            empresa.Id = id;
            return CreatedAtAction(nameof(GetById), new { id }, empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Empresa empresa)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (!IsSuperAdmin() && empresaId != id)
            {
                return Forbid();
            }

            empresa.Id = id;
            var success = await _repository.UpdateAsync(empresa);
            if (!success)
            {
                return NotFound(new { message = "Empresa no encontrada" });
            }
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Empresa no encontrada" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
