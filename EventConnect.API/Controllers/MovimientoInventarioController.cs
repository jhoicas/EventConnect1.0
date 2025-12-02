using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

[Authorize]
public class MovimientoInventarioController : BaseController
{
    private readonly MovimientoInventarioRepository _repository;
    private readonly ILogger<MovimientoInventarioController> _logger;

    public MovimientoInventarioController(IConfiguration configuration, ILogger<MovimientoInventarioController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new MovimientoInventarioRepository(connectionString);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var movimientos = await _repository.GetByEmpresaIdAsync(empresaId!.Value, fechaInicio, fechaFin);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos de inventario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var movimiento = await _repository.GetByIdAsync(id);
            if (movimiento == null)
                return NotFound(new { message = "Movimiento no encontrado" });

            if (!IsSuperAdmin() && movimiento.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(movimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimiento {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("producto/{productoId}")]
    public async Task<IActionResult> GetByProducto(int productoId)
    {
        try
        {
            var movimientos = await _repository.GetByProductoIdAsync(productoId);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos por producto {ProductoId}", productoId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("activo/{activoId}")]
    public async Task<IActionResult> GetByActivo(int activoId)
    {
        try
        {
            var movimientos = await _repository.GetByActivoIdAsync(activoId);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos por activo {ActivoId}", activoId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("tipo/{tipo}")]
    public async Task<IActionResult> GetByTipo(string tipo)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            var movimientos = await _repository.GetByTipoMovimientoAsync(empresaId!.Value, tipo);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos por tipo {Tipo}", tipo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MovimientoInventario movimiento)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            movimiento.Empresa_Id = empresaId ?? movimiento.Empresa_Id;
            movimiento.Usuario_Id = GetCurrentUserId();
            movimiento.Fecha_Movimiento = DateTime.Now;

            var id = await _repository.AddAsync(movimiento);
            movimiento.Id = id;

            _logger.LogInformation("Movimiento inventario creado: {Id} tipo {Tipo} por usuario {UserId}", 
                id, movimiento.Tipo_Movimiento, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, movimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear movimiento de inventario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
