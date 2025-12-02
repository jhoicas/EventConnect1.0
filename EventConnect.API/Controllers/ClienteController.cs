using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class ClienteController : BaseController
{
    private readonly ClienteRepository _repository;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(IConfiguration configuration, ILogger<ClienteController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new ClienteRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // SuperAdmin puede ver todos los clientes con información de empresa
            if (IsSuperAdmin())
            {
                var clientesConEmpresa = await _repository.GetAllWithEmpresaAsync();
                return Ok(clientesConEmpresa);
            }

            // Usuarios normales solo ven clientes de su empresa
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null)
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var clientes = await _repository.GetByEmpresaIdAsync(empresaId.Value);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            if (!IsSuperAdmin() && cliente.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Cliente cliente)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            cliente.Empresa_Id = empresaId ?? cliente.Empresa_Id;
            cliente.Fecha_Registro = DateTime.Now;
            cliente.Fecha_Actualizacion = DateTime.Now;

            var id = await _repository.AddAsync(cliente);
            cliente.Id = id;

            _logger.LogInformation("Cliente creado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Cliente cliente)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Cliente no encontrado" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            cliente.Id = id;
            cliente.Empresa_Id = existing.Empresa_Id;
            cliente.Fecha_Actualizacion = DateTime.Now;
            
            var success = await _repository.UpdateAsync(cliente);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar el cliente" });

            _logger.LogInformation("Cliente actualizado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente {Id}", id);
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
                return NotFound(new { message = "Cliente no encontrado" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar el cliente" });

            _logger.LogInformation("Cliente eliminado: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Cliente eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
