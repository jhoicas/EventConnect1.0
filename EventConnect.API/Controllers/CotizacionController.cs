using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;

namespace EventConnect.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CotizacionController : BaseController
{
    private readonly ReservaRepository _reservaRepo;
    private readonly ClienteRepository _clienteRepo;
    private readonly DetalleReservaRepository _detalleReservaRepository;

    public CotizacionController(
        ReservaRepository reservaRepo,
        ClienteRepository clienteRepo,
        DetalleReservaRepository detalleReservaRepository)
    {
        _reservaRepo = reservaRepo;
        _clienteRepo = clienteRepo;
        _detalleReservaRepository = detalleReservaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? incluirVencidas)
    {
        try
        {
            // SuperAdmin puede ver todas las cotizaciones (pasa null)
            // Admin-Proveedor y otros usuarios solo ven cotizaciones de su empresa
            int? empresaId = null;
            if (!IsSuperAdmin())
            {
                empresaId = GetCurrentEmpresaId();
                if (empresaId == null)
                {
                    return BadRequest(new { message = "Empresa no válida" });
                }
            }

            // GetAllCotizacionesAsync ahora acepta empresaId para filtro multi-tenant
            var cotizaciones = await _reservaRepo.GetAllCotizacionesAsync(empresaId, incluirVencidas);
            return Ok(cotizaciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cotizaciones", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var cotizacion = await _reservaRepo.GetCotizacionByIdAsync(id);
            if (cotizacion == null)
                return NotFound(new { message = "Cotización no encontrada" });

            // Verificar permisos multi-tenant
            if (!IsSuperAdmin() && cotizacion.Empresa_Id != GetCurrentEmpresaId())
                return Forbid();

            return Ok(cotizacion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cotización", error = ex.Message });
        }
    }

    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId() ?? 0;
            if (!IsSuperAdmin() && empresaId == 0)
                return BadRequest(new { message = "No se pudo determinar la empresa" });

            var stats = await _reservaRepo.GetEstadisticasCotizacionesAsync(empresaId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCotizacionRequest request)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId() ?? 0;
            if (!IsSuperAdmin() && empresaId == 0)
                return BadRequest(new { message = "No se pudo determinar la empresa" });

            // Verificar que el cliente existe y pertenece a la empresa
            var cliente = await _clienteRepo.GetByIdAsync(request.Cliente_Id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            if (!IsSuperAdmin() && cliente.Empresa_Id != empresaId)
                return Forbid();

            // Generar código único de cotización
            var codigoCotizacion = $"COT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            var cotizacion = new Reserva
            {
                Cliente_Id = request.Cliente_Id,
                Codigo_Reserva = codigoCotizacion,
                Estado = "Solicitado",
                Fecha_Evento = request.Fecha_Evento,
                Fecha_Entrega = request.Fecha_Entrega,
                Fecha_Devolucion_Programada = request.Fecha_Devolucion_Programada,
                Direccion_Entrega = request.Direccion_Entrega,
                Ciudad_Entrega = request.Ciudad_Entrega,
                Contacto_En_Sitio = request.Contacto_En_Sitio,
                Telefono_Contacto = request.Telefono_Contacto,
                Subtotal = request.Subtotal,
                Descuento = request.Descuento,
                Total = request.Total,
                Fianza = request.Fianza,
                Fianza_Devuelta = false,
                Metodo_Pago = "Efectivo",
                Estado_Pago = "Pendiente",
                Observaciones = request.Observaciones,
                Creado_Por_Id = GetCurrentUserId(),
                Fecha_Creacion = DateTime.Now,
                Fecha_Vencimiento_Cotizacion = DateTime.Now.AddDays(request.Dias_Validez_Cotizacion),
                Fecha_Actualizacion = DateTime.Now
            };

            var id = await _reservaRepo.AddAsync(cotizacion);
            cotizacion.Id = id;

            // MultiVendedor: La empresa se define en DetalleReserva cuando se agrega items
            // Se puede opcionalmente crear un detalle base aquí si es necesario

            return CreatedAtAction(nameof(GetById), new { id }, cotizacion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear cotización", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCotizacionRequest request)
    {
        try
        {
            var cotizacion = await _reservaRepo.GetByIdAsync(id);
            if (cotizacion == null)
                return NotFound(new { message = "Cotización no encontrada" });

            // Verificar que es una cotización (Estado Solicitado con fecha de vencimiento)
            if (cotizacion.Estado != "Solicitado" || cotizacion.Fecha_Vencimiento_Cotizacion == null)
                return BadRequest(new { message = "Esta reserva no es una cotización" });

            // MultiVendedor: Verificar permisos a través de detalles
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(id)).ToList();
                if (!detalles.Any(d => d.Empresa_Id == empresaId))
                    return Forbid();
            }

            // Actualizar campos
            cotizacion.Fecha_Evento = request.Fecha_Evento;
            cotizacion.Fecha_Entrega = request.Fecha_Entrega;
            cotizacion.Fecha_Devolucion_Programada = request.Fecha_Devolucion_Programada;
            cotizacion.Direccion_Entrega = request.Direccion_Entrega;
            cotizacion.Ciudad_Entrega = request.Ciudad_Entrega;
            cotizacion.Contacto_En_Sitio = request.Contacto_En_Sitio;
            cotizacion.Telefono_Contacto = request.Telefono_Contacto;
            cotizacion.Subtotal = request.Subtotal;
            cotizacion.Descuento = request.Descuento;
            cotizacion.Total = request.Total;
            cotizacion.Fianza = request.Fianza;
            cotizacion.Observaciones = request.Observaciones;
            cotizacion.Fecha_Actualizacion = DateTime.Now;

            await _reservaRepo.UpdateAsync(cotizacion);

            return Ok(new { message = "Cotización actualizada correctamente", cotizacion });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar cotización", error = ex.Message });
        }
    }

    [HttpPost("convertir")]
    public async Task<IActionResult> ConvertirAReserva([FromBody] ConvertirCotizacionRequest request)
    {
        try
        {
            var cotizacion = await _reservaRepo.GetByIdAsync(request.Cotizacion_Id);
            if (cotizacion == null)
                return NotFound(new { message = "Cotización no encontrada" });

            // Verificar que es una cotización
            if (cotizacion.Estado != "Solicitado" || cotizacion.Fecha_Vencimiento_Cotizacion == null)
                return BadRequest(new { message = "Esta reserva no es una cotización" });

            // MultiVendedor: Verificar permisos a través de detalles
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(request.Cotizacion_Id)).ToList();
                if (!detalles.Any(d => d.Empresa_Id == empresaId))
                    return Forbid();
            }

            // Verificar que no esté vencida
            if (cotizacion.Fecha_Vencimiento_Cotizacion < DateTime.Now)
                return BadRequest(new { message = "La cotización está vencida. Extienda su vencimiento antes de convertirla." });

            // Convertir a reserva aprobada
            cotizacion.Estado = "Aprobado";
            cotizacion.Metodo_Pago = request.Metodo_Pago;
            cotizacion.Aprobado_Por_Id = GetCurrentUserId();
            cotizacion.Fecha_Aprobacion = DateTime.Now;
            cotizacion.Fecha_Actualizacion = DateTime.Now;

            if (!string.IsNullOrEmpty(request.Observaciones_Adicionales))
            {
                cotizacion.Observaciones = string.IsNullOrEmpty(cotizacion.Observaciones)
                    ? request.Observaciones_Adicionales
                    : $"{cotizacion.Observaciones}\n\n[Conversión] {request.Observaciones_Adicionales}";
            }

            await _reservaRepo.UpdateAsync(cotizacion);

            return Ok(new { message = "Cotización convertida a reserva exitosamente", reserva = cotizacion });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al convertir cotización", error = ex.Message });
        }
    }

    [HttpPost("extender-vencimiento")]
    public async Task<IActionResult> ExtenderVencimiento([FromBody] ExtenderVencimientoCotizacionRequest request)
    {
        try
        {
            var cotizacion = await _reservaRepo.GetByIdAsync(request.Cotizacion_Id);
            if (cotizacion == null)
                return NotFound(new { message = "Cotización no encontrada" });

            // Verificar que es una cotización
            if (cotizacion.Estado != "Solicitado" || cotizacion.Fecha_Vencimiento_Cotizacion == null)
                return BadRequest(new { message = "Esta reserva no es una cotización" });

            // MultiVendedor: Verificar permisos a través de detalles
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(request.Cotizacion_Id)).ToList();
                if (!detalles.Any(d => d.Empresa_Id == empresaId))
                    return Forbid();
            }

            if (request.Dias_Extension <= 0 || request.Dias_Extension > 90)
                return BadRequest(new { message = "Los días de extensión deben estar entre 1 y 90" });

            var success = await _reservaRepo.ExtenderVencimientoCotizacionAsync(request.Cotizacion_Id, request.Dias_Extension);
            if (!success)
                return BadRequest(new { message = "No se pudo extender el vencimiento" });

            // Obtener cotización actualizada
            var cotizacionActualizada = await _reservaRepo.GetCotizacionByIdAsync(request.Cotizacion_Id);

            return Ok(new 
            { 
                message = $"Vencimiento extendido por {request.Dias_Extension} días", 
                cotizacion = cotizacionActualizada 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al extender vencimiento", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var cotizacion = await _reservaRepo.GetByIdAsync(id);
            if (cotizacion == null)
                return NotFound(new { message = "Cotización no encontrada" });

            // Verificar que es una cotización
            if (cotizacion.Estado != "Solicitado" || cotizacion.Fecha_Vencimiento_Cotizacion == null)
                return BadRequest(new { message = "Esta reserva no es una cotización" });

            // MultiVendedor: Verificar permisos a través de detalles
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                var detalles = (await _detalleReservaRepository.GetByReservaIdAsync(id)).ToList();
                if (!detalles.Any(d => d.Empresa_Id == empresaId))
                    return Forbid();
            }

            await _reservaRepo.DeleteAsync(id);

            return Ok(new { message = "Cotización eliminada correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar cotización", error = ex.Message });
        }
    }
}
