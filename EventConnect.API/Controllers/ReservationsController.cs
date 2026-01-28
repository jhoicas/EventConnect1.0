using EventConnect.Domain.DTOs;
using EventConnect.Application.Services;
using EventConnect.Application.Services.Implementation;
using EventConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventConnect.API.Controllers;

/// <summary>
/// Controlador para gestión de reservas
/// </summary>
[Authorize]
public class ReservationsController : BaseController
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IConfiguration configuration, ILogger<ReservationsController> logger)
    {
        var connectionString = configuration.GetConnectionString("EventConnectConnection")
            ?? throw new InvalidOperationException("Connection string not found");

        // Instanciar repositorios
        var reservaRepository = new ReservaRepository(connectionString);
        var empresaRepository = new EmpresaRepository(connectionString);
        var clienteRepository = new ClienteRepository(connectionString);
        var usuarioRepository = new UsuarioRepository(connectionString);

        // Crear logger factory para el servicio
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var serviceLogger = loggerFactory.CreateLogger<ReservationService>();

        // Instanciar servicio
        _reservationService = new ReservationService(
            reservaRepository,
            empresaRepository,
            clienteRepository,
            usuarioRepository,
            serviceLogger);

        _logger = logger;
    }

    /// <summary>
    /// Obtiene las reservas del usuario actual (como cliente)
    /// GET /api/reservations/mine
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyReservations()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var reservations = await _reservationService.GetMyReservationsAsync(userId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las reservas del usuario");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las reservas de la empresa del usuario actual
    /// GET /api/reservations
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado = null)
    {
        try
        {
            int? empresaId = null;

            // SuperAdmin puede ver todas las reservas o filtrar por empresa
            if (!IsSuperAdmin())
            {
                empresaId = GetCurrentEmpresaId();
                if (empresaId == null)
                {
                    return BadRequest(new { message = "Empresa no válida para el usuario" });
                }
            }
            else
            {
                // Si es SuperAdmin, puede pasar empresaId como query parameter
                if (Request.Query.ContainsKey("empresaId") && int.TryParse(Request.Query["empresaId"], out var empId))
                {
                    empresaId = empId;
                }
            }

            if (empresaId.HasValue)
            {
                var reservations = await _reservationService.GetReservationsByEmpresaAsync(empresaId.Value, estado);
                return Ok(reservations);
            }
            else
            {
                // SuperAdmin sin filtro de empresa específica
                // Nota: Podrías crear un método en el servicio para obtener TODAS las reservas
                return BadRequest(new { message = "Debe especificar un empresaId" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una reserva específica por ID
    /// GET /api/reservations/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            // Verificar permisos: solo SuperAdmin o usuarios de la empresa pueden ver la reserva
            if (!IsSuperAdmin() && reservation.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reserva {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva reserva
    /// POST /api/reservations
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Validar que el usuario tenga permiso para crear reservas en la empresa
            if (!IsSuperAdmin())
            {
                var empresaId = GetCurrentEmpresaId();
                if (empresaId == null || empresaId != request.Empresa_Id)
                {
                    return Forbid();
                }
            }

            var reservation = await _reservationService.CreateReservationAsync(request, userId);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = reservation.Id },
                reservation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear reserva");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de operación al crear reserva");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reserva");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de una reserva
    /// PUT /api/reservations/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Verificar permisos: obtener la reserva primero
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            if (!IsSuperAdmin() && reservation.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var resultado = await _reservationService.UpdateReservationStatusAsync(id, request, userId);
            
            if (resultado)
            {
                return Ok(new { message = "Estado de reserva actualizado correctamente" });
            }

            return BadRequest(new { message = "No se pudo actualizar el estado de la reserva" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar estado de reserva {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de reserva {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una reserva
    /// POST /api/reservations/{id}/cancel
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelReservationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            // Verificar permisos
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = "Reserva no encontrada" });
            }

            if (!IsSuperAdmin() && reservation.Empresa_Id != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var resultado = await _reservationService.CancelReservationAsync(
                id, 
                request.Razon ?? "No especificada", 
                userId);
            
            if (resultado)
            {
                return Ok(new { message = "Reserva cancelada correctamente" });
            }

            return BadRequest(new { message = "No se pudo cancelar la reserva" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reserva {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de reservas de una empresa
    /// GET /api/reservations/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var empresaId = GetCurrentEmpresaId();
            if (empresaId == null && !IsSuperAdmin())
            {
                return BadRequest(new { message = "Empresa no válida para el usuario" });
            }

            // Si es SuperAdmin puede solicitar stats de una empresa específica
            if (IsSuperAdmin() && Request.Query.ContainsKey("empresaId") && int.TryParse(Request.Query["empresaId"], out var empId))
            {
                empresaId = empId;
            }

            if (!empresaId.HasValue)
            {
                return BadRequest(new { message = "Debe especificar un empresaId" });
            }

            var stats = await _reservationService.GetReservationStatsAsync(empresaId.Value);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de reservas");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica disponibilidad para una fecha específica
    /// GET /api/reservations/check-availability
    /// </summary>
    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] int empresaId, [FromQuery] DateTime fechaEvento)
    {
        try
        {
            // Verificar permisos
            if (!IsSuperAdmin() && empresaId != GetCurrentEmpresaId())
            {
                return Forbid();
            }

            var disponible = await _reservationService.VerificarDisponibilidadAsync(empresaId, fechaEvento);
            
            return Ok(new 
            { 
                disponible,
                empresaId,
                fechaEvento,
                message = disponible ? "Fecha disponible" : "Fecha no disponible"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO para cancelar una reserva
/// </summary>
public class CancelReservationRequest
{
    public string? Razon { get; set; }
}
