using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace EventConnect.Application.Services.Implementation;

public class ReservationService : IReservationService
{
    private readonly ReservaRepository _reservaRepository;
    private readonly EmpresaRepository _empresaRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        ReservaRepository reservaRepository,
        EmpresaRepository empresaRepository,
        ClienteRepository clienteRepository,
        UsuarioRepository usuarioRepository,
        ILogger<ReservationService> logger)
    {
        _reservaRepository = reservaRepository;
        _empresaRepository = empresaRepository;
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva reserva MULTIVENDEDOR
    /// Ahora acepta múltiples detalles de diferentes empresas
    /// </summary>
    public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, int createdById)
    {
        _logger.LogInformation($"Creando reserva multivendedor para cliente {request.Cliente_Id}");

        // Validar que el cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(request.Cliente_Id);
        if (cliente == null)
        {
            throw new ArgumentException("El cliente especificado no existe");
        }

        if (cliente.Estado != "Activo")
        {
            throw new ArgumentException("El cliente no está activo");
        }

        // Validar que hay detalles
        if (request.Detalles == null || !request.Detalles.Any())
        {
            throw new ArgumentException("Debe proporcionar al menos un detalle de reserva");
        }

        // Validar todas las empresas
        var empresasValidas = new List<int>();
        foreach (var detalle in request.Detalles)
        {
            var empresa = await _empresaRepository.GetByIdAsync(detalle.Empresa_Id);
            if (empresa == null || empresa.Estado != "Activa")
            {
                throw new ArgumentException($"Empresa {detalle.Empresa_Id} no existe o no está activa");
            }
            empresasValidas.Add(detalle.Empresa_Id);
        }

        // Generar código de reserva
        var codigoReserva = await _reservaRepository.GenerarCodigoReservaAsync();

        // Crear la entidad reserva (sin Empresa_Id)
        var reserva = new Reserva
        {
            Cliente_Id = request.Cliente_Id,
            Codigo_Reserva = codigoReserva,
            Estado = "Solicitado",
            Fecha_Evento = request.Fecha_Evento,
            Fecha_Entrega = request.Fecha_Entrega,
            Fecha_Devolucion_Programada = request.Fecha_Devolucion_Programada,
            Direccion_Entrega = request.Direccion_Entrega,
            Ciudad_Entrega = request.Ciudad_Entrega,
            Contacto_En_Sitio = request.Contacto_En_Sitio,
            Telefono_Contacto = request.Telefono_Contacto,
            Metodo_Pago = request.Metodo_Pago,
            Estado_Pago = "Pendiente",
            Observaciones = request.Observaciones,
            Creado_Por_Id = createdById,
            Fecha_Creacion = DateTime.UtcNow,
            Fecha_Actualizacion = DateTime.UtcNow,
            Fecha_Vencimiento_Cotizacion = request.Fecha_Vencimiento_Cotizacion,
            Subtotal = 0,
            Descuento = 0,
            Total = 0
        };

        var reservaId = await _reservaRepository.AddAsync(reserva);
        
        _logger.LogInformation($"Reserva base creada: {codigoReserva}, ID: {reservaId}");

        // Aquí se agregarían los detalles
        // (El controlador maneja la creación de detalles)

        var reservaCreada = await _reservaRepository.GetReservationByIdAsync(reservaId);
        if (reservaCreada == null)
        {
            throw new InvalidOperationException("Error al recuperar la reserva creada");
        }

        return reservaCreada;
    }

    /// <summary>
    /// Obtiene las reservas del usuario actual (como cliente)
    /// </summary>
    public async Task<IEnumerable<ReservationResponse>> GetMyReservationsAsync(int userId)
    {
        _logger.LogInformation($"Obteniendo reservas del usuario {userId}");

        var cliente = await _clienteRepository.GetByUsuarioIdAsync(userId);
        if (cliente == null)
        {
            _logger.LogWarning($"No se encontró un cliente asociado al usuario {userId}");
            return Enumerable.Empty<ReservationResponse>();
        }

        return await _reservaRepository.GetReservationsByClienteIdAsync(cliente.Id);
    }

    /// <summary>
    /// Obtiene las reservas de una empresa como proveedor
    /// Ahora filtra por Detalle_Reserva.Empresa_Id
    /// </summary>
    public async Task<IEnumerable<ReservationResponse>> GetReservationsByEmpresaAsync(int empresaId, string? estado = null)
    {
        _logger.LogInformation($"Obteniendo reservas de la empresa {empresaId}, estado: {estado ?? "todos"}");
        return await _reservaRepository.GetReservationsByEmpresaIdAsync(empresaId, estado);
    }

    /// <summary>
    /// Obtiene una reserva por su ID con información completa
    /// </summary>
    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        _logger.LogInformation($"Obteniendo reserva con ID {id}");
        return await _reservaRepository.GetReservationByIdAsync(id);
    }

    /// <summary>
    /// Actualiza el estado de una reserva
    /// </summary>
    public async Task<bool> UpdateReservationStatusAsync(int id, UpdateReservationStatusRequest request, int userId)
    {
        _logger.LogInformation($"Actualizando estado de reserva {id} a {request.Estado}");

        var reserva = await _reservaRepository.GetByIdAsync(id);
        if (reserva == null)
        {
            throw new ArgumentException("La reserva no existe");
        }

        // Validar transiciones de estado permitidas
        var estadosValidos = new[] { "Solicitado", "Confirmado", "Cancelado", "Completado", "En_Proceso" };
        if (!estadosValidos.Contains(request.Estado))
        {
            throw new ArgumentException($"Estado '{request.Estado}' no válido");
        }

        reserva.Estado = request.Estado;
        
        if (!string.IsNullOrEmpty(request.Observaciones))
        {
            reserva.Observaciones = request.Observaciones;
        }

        if (request.Estado == "Confirmado" && reserva.Aprobado_Por_Id == null)
        {
            reserva.Aprobado_Por_Id = userId;
            reserva.Fecha_Aprobacion = DateTime.UtcNow;
        }

        if (request.Estado == "Cancelado")
        {
            reserva.Cancelado_Por_Id = userId;
            reserva.Razon_Cancelacion = request.Razon_Cancelacion ?? "No especificada";
        }

        if (request.Estado == "Completado" && reserva.Fecha_Devolucion_Real == null)
        {
            reserva.Fecha_Devolucion_Real = DateTime.UtcNow;
        }

        reserva.Fecha_Actualizacion = DateTime.UtcNow;

        var resultado = await _reservaRepository.UpdateAsync(reserva);
        
        if (resultado)
        {
            _logger.LogInformation($"Reserva {id} actualizada a estado {request.Estado}");
        }

        return resultado;
    }

    /// <summary>
    /// Cancela una reserva
    /// </summary>
    public async Task<bool> CancelReservationAsync(int id, string razon, int userId)
    {
        var request = new UpdateReservationStatusRequest
        {
            Estado = "Cancelado",
            Razon_Cancelacion = razon
        };

        return await UpdateReservationStatusAsync(id, request, userId);
    }

    /// <summary>
    /// Obtiene estadísticas de reservas para una empresa como proveedor
    /// </summary>
    public async Task<ReservationStatsDTO> GetReservationStatsAsync(int empresaId)
    {
        _logger.LogInformation($"Obteniendo estadísticas de reservas para empresa {empresaId}");
        return await _reservaRepository.GetReservationStatsAsync(empresaId);
    }

    /// <summary>
    /// Verifica si hay disponibilidad en una fecha para una empresa específica
    /// </summary>
    public async Task<bool> VerificarDisponibilidadAsync(int empresaId, DateTime fechaEvento)
    {
        return await _reservaRepository.VerificarDisponibilidadAsync(empresaId, fechaEvento);
    }
}
