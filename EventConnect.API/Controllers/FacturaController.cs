using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controlador para gestión de facturas preparadas para DIAN (Colombia)
/// </summary>
[Authorize]
public class FacturaController : BaseController
{
    private readonly FacturaRepository _repository;
    private readonly ReservaRepository _reservaRepository;
    private readonly DetalleReservaRepository _detalleReservaRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly ProductoRepository? _productoRepository;
    private readonly ILogger<FacturaController> _logger;
    private readonly decimal TASA_IVA = 0.19m; // IVA 19% Colombia

    public FacturaController(
        IConfiguration configuration, 
        ILogger<FacturaController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _repository = new FacturaRepository(connectionString);
        _reservaRepository = new ReservaRepository(connectionString);
        _detalleReservaRepository = new DetalleReservaRepository(connectionString);
        _clienteRepository = new ClienteRepository(connectionString);
        _logger = logger;
        
        // ProductoRepository es opcional
        try
        {
            _productoRepository = new ProductoRepository(connectionString);
        }
        catch
        {
            _productoRepository = null;
        }
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

            IEnumerable<Factura> facturas;
            if (IsSuperAdmin() && empresaId == null)
            {
                facturas = await _repository.GetAllAsync();
            }
            else
            {
                facturas = await _repository.GetByEmpresaAsync(empresaId!.Value);
            }

            return Ok(facturas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener facturas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var factura = await _repository.GetByIdAsync(id);
            if (factura == null)
                return NotFound(new { message = "Factura no encontrada" });

            if (!IsSuperAdmin() && factura.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            // Obtener factura con detalles
            var facturaConDetalles = await _repository.GetWithDetailsAsync(id);
            return Ok(facturaConDetalles ?? factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener factura {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Genera una factura automáticamente desde una reserva confirmada
    /// </summary>
    /// <param name="reservaId">ID de la reserva confirmada</param>
    /// <returns>Factura creada en estado Borrador</returns>
    /// <response code="200">Factura generada exitosamente</response>
    /// <response code="400">La reserva no está confirmada o ya tiene factura</response>
    /// <response code="404">Reserva no encontrada</response>
    [HttpPost("generar-desde-reserva/{reservaId}")]
    public async Task<IActionResult> GenerarDesdeReserva(int reservaId)
    {
        try
        {
            // Validar que la reserva existe
            var reserva = await _reservaRepository.GetByIdAsync(reservaId);
            if (reserva == null)
                return NotFound(new { message = "Reserva no encontrada" });

            // Validar autorización multi-tenant
            if (!IsSuperAdmin() && reserva.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            // Validar que la reserva esté confirmada
            if (reserva.Estado != "Confirmado")
            {
                return BadRequest(new { message = $"La reserva debe estar en estado 'Confirmado'. Estado actual: {reserva.Estado}" });
            }

            // Validar que no exista ya una factura para esta reserva
            var facturasExistentes = await _repository.GetByEmpresaAsync(reserva.Empresa_Id);
            if (facturasExistentes.Any(f => f.Reserva_Id == reservaId))
            {
                return BadRequest(new { message = "Ya existe una factura para esta reserva" });
            }

            // Obtener detalles de la reserva
            var detallesReserva = (await _detalleReservaRepository.GetByReservaIdAsync(reservaId)).ToList();
            if (!detallesReserva.Any())
            {
                return BadRequest(new { message = "La reserva no tiene items para facturar" });
            }

            // Obtener datos del cliente para el snapshot
            var cliente = await _clienteRepository.GetByIdAsync(reserva.Cliente_Id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            // Crear snapshot JSON del cliente
            var clienteSnapshot = new
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Tipo_Documento = cliente.Tipo_Documento,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Ciudad = cliente.Ciudad,
                Tipo_Cliente = cliente.Tipo_Cliente,
                Fecha_Snapshot = DateTime.Now
            };
            var datosClienteJson = JsonSerializer.Serialize(clienteSnapshot);

            // Obtener siguiente consecutivo
            var prefijo = "FE"; // Factura Electrónica
            var consecutivo = await _repository.GetSiguienteConsecutivoAsync(reserva.Empresa_Id, prefijo);

            // Crear factura
            var factura = new Factura
            {
                Empresa_Id = reserva.Empresa_Id,
                Cliente_Id = reserva.Cliente_Id,
                Reserva_Id = reservaId,
                Prefijo = prefijo,
                Consecutivo = consecutivo,
                CUFE = null, // Se generará cuando se emita la factura electrónica
                Fecha_Emision = DateTime.Now,
                Fecha_Vencimiento = DateTime.Now.AddDays(30), // 30 días por defecto
                Subtotal = reserva.Subtotal,
                Impuestos = reserva.Subtotal * TASA_IVA,
                Total = reserva.Subtotal * (1 + TASA_IVA),
                Estado = "Borrador",
                Datos_Cliente_Snapshot = datosClienteJson,
                Observaciones = $"Factura generada desde reserva {reserva.Codigo_Reserva}",
                Creado_Por_Id = GetCurrentUserId(),
                Fecha_Creacion = DateTime.Now,
                Fecha_Actualizacion = DateTime.Now
            };

            // Crear detalles de factura desde detalles de reserva
            var detallesFactura = new List<DetalleFactura>();
            foreach (var detalleReserva in detallesReserva)
            {
                // Obtener nombre del producto si existe
                string nombreServicio = "Producto/Alquiler"; // Valor por defecto
                if (detalleReserva.Producto_Id.HasValue && _productoRepository != null)
                {
                    var producto = await _productoRepository.GetByIdAsync(detalleReserva.Producto_Id.Value);
                    if (producto != null)
                    {
                        nombreServicio = producto.Nombre;
                    }
                }

                var subtotal = detalleReserva.Subtotal;
                var impuesto = subtotal * TASA_IVA;
                var total = subtotal + impuesto;

                detallesFactura.Add(new DetalleFactura
                {
                    Producto_Id = detalleReserva.Producto_Id,
                    Servicio = nombreServicio,
                    Cantidad = detalleReserva.Cantidad,
                    Precio_Unitario = detalleReserva.Precio_Unitario,
                    Subtotal = subtotal,
                    Tasa_Impuesto = TASA_IVA,
                    Impuesto = impuesto,
                    Total = total,
                    Unidad_Medida = detalleReserva.Cantidad > 1 ? $"Días x {detalleReserva.Dias_Alquiler}" : "Día",
                    Observaciones = detalleReserva.Observaciones,
                    Fecha_Creacion = DateTime.Now
                });
            }

            // Recalcular totales basados en los detalles
            factura.Subtotal = detallesFactura.Sum(d => d.Subtotal);
            factura.Impuestos = detallesFactura.Sum(d => d.Impuesto);
            factura.Total = factura.Subtotal + factura.Impuestos;

            // Guardar factura con detalles en transacción
            var facturaId = await _repository.CreateWithDetailsAsync(factura, detallesFactura);
            factura.Id = facturaId;

            _logger.LogInformation(
                "Factura {FacturaId} generada desde reserva {ReservaId} por usuario {UserId}",
                facturaId, reservaId, GetCurrentUserId());

            // Retornar factura con detalles
            var facturaCompleta = await _repository.GetWithDetailsAsync(facturaId);
            return Ok(facturaCompleta ?? factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar factura desde reserva {ReservaId}", reservaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Factura factura)
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida" });
            }

            factura.Empresa_Id = empresaId ?? factura.Empresa_Id;
            factura.Fecha_Creacion = DateTime.Now;
            factura.Fecha_Actualizacion = DateTime.Now;
            factura.Creado_Por_Id = GetCurrentUserId();

            // Si no tiene consecutivo, obtenerlo automáticamente
            if (factura.Consecutivo == 0)
            {
                factura.Consecutivo = await _repository.GetSiguienteConsecutivoAsync(
                    factura.Empresa_Id, 
                    factura.Prefijo);
            }

            var id = await _repository.AddAsync(factura);
            factura.Id = id;

            _logger.LogInformation("Factura {Id} creada por usuario {UserId}", id, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id }, factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear factura");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
