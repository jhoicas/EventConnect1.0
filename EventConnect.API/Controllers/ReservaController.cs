using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class ReservaController : BaseController
{
    private readonly ReservaRepository _repository;
    private readonly ILogger<ReservaController> _logger;

    public ReservaController(IConfiguration configuration, ILogger<ReservaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new ReservaRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            IEnumerable<Reserva> reservas;
            if (IsSuperAdmin() && empresaId == null)
            {
                reservas = await _repository.GetAllAsync();
            }
            else
            {
                reservas = await _repository.GetByEmpresaIdAsync(empresaId!.Value);
            }

            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var reserva = await _repository.GetByIdAsync(id);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(reserva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reserva {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("estado/{estado}")]
    public async Task<IActionResult> GetByEstado(string estado)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var reservas = await _repository.GetByEstadoAsync(empresaId!.Value, estado);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas por estado {Estado}", estado);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reserva reserva)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            reserva.Empresa_Id = empresaId ?? reserva.Empresa_Id;
            reserva.Creado_Por_Id = GetCurrentUserId();
            reserva.Fecha_Creacion = DateTime.Now;
            reserva.Fecha_Actualizacion = DateTime.Now;
            
            // Generar código único
            reserva.Codigo_Reserva = $"RES-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            var id = await _repository.AddAsync(reserva);
            reserva.Id = id;

            _logger.LogInformation("Reserva creada: {Id} con código {Codigo} por usuario {UserId}", 
                id, reserva.Codigo_Reserva, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, reserva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reserva");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Reserva reserva)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            reserva.Id = id;
            reserva.Empresa_Id = existing.Empresa_Id;
            reserva.Codigo_Reserva = existing.Codigo_Reserva;
            reserva.Fecha_Actualizacion = DateTime.Now;
            
            var success = await _repository.UpdateAsync(reserva);
            if (!success)
                return BadRequest(new { message = "No se pudo actualizar la reserva" });

            _logger.LogInformation("Reserva actualizada: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(reserva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar reserva {Id}", id);
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
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && existing.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return BadRequest(new { message = "No se pudo eliminar la reserva" });

            _logger.LogInformation("Reserva eliminada: {Id} por usuario {UserId}", id, GetCurrentUserId());
            return Ok(new { message = "Reserva eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar reserva {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
