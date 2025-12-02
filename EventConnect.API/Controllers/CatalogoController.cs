using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;

namespace EventConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CatalogoController : BaseController
{
    private readonly EstadoReservaRepository _estadoReservaRepo;
    private readonly EstadoActivoRepository _estadoActivoRepo;
    private readonly MetodoPagoRepository _metodoPagoRepo;
    private readonly TipoMantenimientoRepository _tipoMantenimientoRepo;

    public CatalogoController(
        EstadoReservaRepository estadoReservaRepo,
        EstadoActivoRepository estadoActivoRepo,
        MetodoPagoRepository metodoPagoRepo,
        TipoMantenimientoRepository tipoMantenimientoRepo)
    {
        _estadoReservaRepo = estadoReservaRepo;
        _estadoActivoRepo = estadoActivoRepo;
        _metodoPagoRepo = metodoPagoRepo;
        _tipoMantenimientoRepo = tipoMantenimientoRepo;
    }

    // ============================================
    // ESTADOS DE RESERVA
    // ============================================

    [HttpGet("estados-reserva")]
    public async Task<IActionResult> GetEstadosReserva([FromQuery] bool soloActivos = true)
    {
        try
        {
            var estados = soloActivos 
                ? await _estadoReservaRepo.GetActivosAsync()
                : await _estadoReservaRepo.GetAllAsync();
            return Ok(estados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estados de reserva", error = ex.Message });
        }
    }

    [HttpPost("estados-reserva")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateEstadoReserva([FromBody] CatalogoEstadoReserva request)
    {
        try
        {
            // Validar código único
            if (await _estadoReservaRepo.ExisteCodigoAsync(request.Codigo))
                return BadRequest(new { message = "Ya existe un estado con ese código" });

            request.Fecha_Creacion = DateTime.Now;
            request.Sistema = false; // Los creados por usuarios no son del sistema

            var id = await _estadoReservaRepo.AddAsync(request);
            request.Id = id;

            return CreatedAtAction(nameof(GetEstadosReserva), new { id }, request);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear estado", error = ex.Message });
        }
    }

    [HttpPut("estados-reserva/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateEstadoReserva(int id, [FromBody] CatalogoEstadoReserva request)
    {
        try
        {
            var existente = await _estadoReservaRepo.GetByIdAsync(id);
            if (existente == null)
                return NotFound(new { message = "Estado no encontrado" });

            if (existente.Sistema && request.Codigo != existente.Codigo)
                return BadRequest(new { message = "No se puede cambiar el código de estados del sistema" });

            // Validar código único (excepto el mismo registro)
            if (request.Codigo != existente.Codigo && await _estadoReservaRepo.ExisteCodigoAsync(request.Codigo, id))
                return BadRequest(new { message = "Ya existe un estado con ese código" });

            existente.Codigo = request.Codigo;
            existente.Nombre = request.Nombre;
            existente.Descripcion = request.Descripcion;
            existente.Color = request.Color;
            existente.Orden = request.Orden;
            existente.Activo = request.Activo;

            await _estadoReservaRepo.UpdateAsync(existente);
            return Ok(new { message = "Estado actualizado correctamente", estado = existente });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar estado", error = ex.Message });
        }
    }

    [HttpDelete("estados-reserva/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteEstadoReserva(int id)
    {
        try
        {
            var existente = await _estadoReservaRepo.GetByIdAsync(id);
            if (existente == null)
                return NotFound(new { message = "Estado no encontrado" });

            if (existente.Sistema)
                return BadRequest(new { message = "No se puede eliminar un estado del sistema. Desactívelo en su lugar." });

            // Soft delete (desactivar)
            await _estadoReservaRepo.DesactivarAsync(id);
            return Ok(new { message = "Estado desactivado correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar estado", error = ex.Message });
        }
    }

    // ============================================
    // ESTADOS DE ACTIVO
    // ============================================

    [HttpGet("estados-activo")]
    public async Task<IActionResult> GetEstadosActivo([FromQuery] bool soloActivos = true, [FromQuery] bool soloPermiteReserva = false)
    {
        try
        {
            IEnumerable<CatalogoEstadoActivo> estados;

            if (soloPermiteReserva)
                estados = await _estadoActivoRepo.GetPermiteReservaAsync();
            else if (soloActivos)
                estados = await _estadoActivoRepo.GetActivosAsync();
            else
                estados = await _estadoActivoRepo.GetAllAsync();

            return Ok(estados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estados de activo", error = ex.Message });
        }
    }

    [HttpPost("estados-activo")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateEstadoActivo([FromBody] CatalogoEstadoActivo request)
    {
        try
        {
            if (await _estadoActivoRepo.ExisteCodigoAsync(request.Codigo))
                return BadRequest(new { message = "Ya existe un estado con ese código" });

            request.Fecha_Creacion = DateTime.Now;
            request.Sistema = false;

            var id = await _estadoActivoRepo.AddAsync(request);
            request.Id = id;

            return CreatedAtAction(nameof(GetEstadosActivo), new { id }, request);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear estado", error = ex.Message });
        }
    }

    // ============================================
    // MÉTODOS DE PAGO
    // ============================================

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> GetMetodosPago([FromQuery] bool soloActivos = true)
    {
        try
        {
            var metodos = soloActivos 
                ? await _metodoPagoRepo.GetActivosAsync()
                : await _metodoPagoRepo.GetAllAsync();
            return Ok(metodos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener métodos de pago", error = ex.Message });
        }
    }

    [HttpPost("metodos-pago")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateMetodoPago([FromBody] CatalogoMetodoPago request)
    {
        try
        {
            if (await _metodoPagoRepo.ExisteCodigoAsync(request.Codigo))
                return BadRequest(new { message = "Ya existe un método de pago con ese código" });

            request.Fecha_Creacion = DateTime.Now;

            var id = await _metodoPagoRepo.AddAsync(request);
            request.Id = id;

            return CreatedAtAction(nameof(GetMetodosPago), new { id }, request);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear método de pago", error = ex.Message });
        }
    }

    [HttpPut("metodos-pago/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateMetodoPago(int id, [FromBody] CatalogoMetodoPago request)
    {
        try
        {
            var existente = await _metodoPagoRepo.GetByIdAsync(id);
            if (existente == null)
                return NotFound(new { message = "Método de pago no encontrado" });

            if (request.Codigo != existente.Codigo && await _metodoPagoRepo.ExisteCodigoAsync(request.Codigo, id))
                return BadRequest(new { message = "Ya existe un método de pago con ese código" });

            existente.Codigo = request.Codigo;
            existente.Nombre = request.Nombre;
            existente.Descripcion = request.Descripcion;
            existente.Requiere_Comprobante = request.Requiere_Comprobante;
            existente.Requiere_Referencia = request.Requiere_Referencia;
            existente.Activo = request.Activo;
            existente.Orden = request.Orden;

            await _metodoPagoRepo.UpdateAsync(existente);
            return Ok(new { message = "Método de pago actualizado correctamente", metodo = existente });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar método de pago", error = ex.Message });
        }
    }

    // ============================================
    // TIPOS DE MANTENIMIENTO
    // ============================================

    [HttpGet("tipos-mantenimiento")]
    public async Task<IActionResult> GetTiposMantenimiento([FromQuery] bool soloActivos = true, [FromQuery] bool soloPreventivos = false)
    {
        try
        {
            IEnumerable<CatalogoTipoMantenimiento> tipos;

            if (soloPreventivos)
                tipos = await _tipoMantenimientoRepo.GetPreventivosAsync();
            else if (soloActivos)
                tipos = await _tipoMantenimientoRepo.GetActivosAsync();
            else
                tipos = await _tipoMantenimientoRepo.GetAllAsync();

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener tipos de mantenimiento", error = ex.Message });
        }
    }

    [HttpPost("tipos-mantenimiento")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateTipoMantenimiento([FromBody] CatalogoTipoMantenimiento request)
    {
        try
        {
            if (await _tipoMantenimientoRepo.ExisteCodigoAsync(request.Codigo))
                return BadRequest(new { message = "Ya existe un tipo de mantenimiento con ese código" });

            request.Fecha_Creacion = DateTime.Now;

            var id = await _tipoMantenimientoRepo.AddAsync(request);
            request.Id = id;

            return CreatedAtAction(nameof(GetTiposMantenimiento), new { id }, request);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear tipo de mantenimiento", error = ex.Message });
        }
    }
}
