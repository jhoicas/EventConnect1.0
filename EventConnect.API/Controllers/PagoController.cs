using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;

namespace EventConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PagoController : BaseController
{
    private readonly TransaccionPagoRepository _transaccionRepo;
    private readonly ReservaRepository _reservaRepo;

    public PagoController(
        TransaccionPagoRepository transaccionRepo,
        ReservaRepository reservaRepo)
    {
        _transaccionRepo = transaccionRepo;
        _reservaRepo = reservaRepo;
    }

    [HttpGet("reserva/{reservaId}")]
    public async Task<IActionResult> GetByReserva(int reservaId)
    {
        try
        {
            // Verificar que la reserva pertenece a la empresa del usuario
            var reserva = await _reservaRepo.GetByIdAsync(reservaId);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            var transacciones = await _transaccionRepo.GetByReservaIdAsync(reservaId);
            return Ok(transacciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener transacciones", error = ex.Message });
        }
    }

    [HttpGet("resumen/{reservaId}")]
    public async Task<IActionResult> GetResumenPagos(int reservaId)
    {
        try
        {
            // Verificar que la reserva pertenece a la empresa del usuario
            var reserva = await _reservaRepo.GetByIdAsync(reservaId);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            var resumen = await _transaccionRepo.GetResumenPagosAsync(reservaId);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener resumen de pagos", error = ex.Message });
        }
    }

    [HttpGet("empresa")]
    public async Task<IActionResult> GetByEmpresa([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
    {
        try
        {
            IEnumerable<TransaccionPagoDTO> transacciones;

            if (IsSuperAdmin())
            {
                transacciones = await _transaccionRepo.GetAllAsync();
            }
            else
            {
                var empresaId = GetCurrentEmpresaId() ?? 0;
                transacciones = await _transaccionRepo.GetByEmpresaIdAsync(empresaId, fechaInicio, fechaFin);
            }

            return Ok(transacciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener transacciones", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransaccionPagoRequest request)
    {
        try
        {
            // Verificar que la reserva existe y pertenece a la empresa
            var reserva = await _reservaRepo.GetByIdAsync(request.Reserva_Id);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            // Validar que no se pague más del total
            var totalPagado = await _transaccionRepo.GetTotalPagadoAsync(request.Reserva_Id);
            if (request.Tipo == "Pago" && (totalPagado + request.Monto) > reserva.Total)
            {
                return BadRequest(new { message = "El monto del pago excede el total de la reserva" });
            }

            var transaccion = new TransaccionPago
            {
                Reserva_Id = request.Reserva_Id,
                Monto = request.Monto,
                Tipo = request.Tipo,
                Metodo = request.Metodo,
                Referencia_Externa = request.Referencia_Externa,
                Comprobante_URL = request.Comprobante_URL,
                Fecha_Transaccion = DateTime.Now,
                Registrado_Por_Usuario_Id = GetCurrentUserId()
            };

            var id = await _transaccionRepo.AddAsync(transaccion);
            transaccion.Id = id;

            // Actualizar estado de pago de la reserva
            var nuevoTotalPagado = totalPagado + (request.Tipo == "Pago" ? request.Monto : 0);
            reserva.Estado_Pago = nuevoTotalPagado >= reserva.Total ? "Pagado" : "Pendiente";
            await _reservaRepo.UpdateAsync(reserva);

            return CreatedAtAction(nameof(GetByReserva), new { reservaId = request.Reserva_Id }, transaccion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear transacción", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var transaccion = await _transaccionRepo.GetByIdAsync(id);
            if (transaccion == null)
                return NotFound(new { message = "Transacción no encontrada" });

            // Verificar que la reserva pertenece a la empresa
            var reserva = await _reservaRepo.GetByIdAsync(transaccion.Reserva_Id);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            await _transaccionRepo.DeleteAsync(id);

            // Recalcular estado de pago
            var totalPagado = await _transaccionRepo.GetTotalPagadoAsync(transaccion.Reserva_Id);
            reserva.Estado_Pago = totalPagado >= reserva.Total ? "Pagado" : "Pendiente";
            await _reservaRepo.UpdateAsync(reserva);

            return Ok(new { message = "Transacción eliminada correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar transacción", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var transaccion = await _transaccionRepo.GetByIdAsync(id);
            if (transaccion == null)
                return NotFound(new { message = "Transacción no encontrada" });

            // Verificar que la reserva pertenece a la empresa
            var reserva = await _reservaRepo.GetByIdAsync(transaccion.Reserva_Id);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            return Ok(transaccion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener transacción", error = ex.Message });
        }
    }
}
