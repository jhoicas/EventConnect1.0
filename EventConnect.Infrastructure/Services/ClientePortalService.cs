using System.Data;
using Dapper;
using Npgsql;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace EventConnect.Infrastructure.Services;

/// <summary>
/// Servicio para el Portal de Autogestión de Clientes
/// Permite a los clientes gestionar sus propias reservas y cotizaciones
/// </summary>
public class ClientePortalService : IClientePortalService
{
    private readonly string _connectionString;
    private readonly IAuditoriaService _auditoriaService;

    public ClientePortalService(IConfiguration configuration, IAuditoriaService auditoriaService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _auditoriaService = auditoriaService;
    }

    public async Task<List<MiReservaResponse>> ObtenerMisReservasAsync(int clienteId, string? estado = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClause = "r.cliente_id = @clienteId";
            var parameters = new { clienteId, estado };

            if (!string.IsNullOrEmpty(estado))
            {
                whereClause += " AND r.estado = @estado";
            }

            var reservas = await connection.QueryAsync<MiReservaResponse>(
                $@"SELECT r.id, r.fecha_reserva, r.fecha_inicio, r.fecha_fin, r.estado,
                    r.monto_total, r.monto_pagado,
                    COUNT(dr.id) as cantidad_activos,
                    BOOL_OR(log.id IS NOT NULL) as requiere_entrega,
                    BOOL_OR(log.estado_entrega = 'Completada') as entrega_completada,
                    BOOL_OR(log.estado_devolucion = 'Completada') as devolucion_completada
                  FROM reservas r
                  LEFT JOIN detalle_reserva dr ON dr.reserva_id = r.id
                  LEFT JOIN logistica log ON log.reserva_id = r.id
                  WHERE {whereClause}
                  GROUP BY r.id, r.fecha_reserva, r.fecha_inicio, r.fecha_fin, r.estado, r.monto_total, r.monto_pagado
                  ORDER BY r.fecha_reserva DESC",
                parameters);

            var resultado = new List<MiReservaResponse>();
            foreach (var reserva in reservas)
            {
                // Obtener nombres de activos
                var activos = await connection.QueryAsync<string>(
                    @"SELECT a.nombre 
                      FROM detalle_reserva dr
                      JOIN activos a ON a.id = dr.activo_id
                      WHERE dr.reserva_id = @reservaId",
                    new { reservaId = reserva.Id });

                reserva.NombresActivos = activos.ToList();
                resultado.Add(reserva);
            }

            return resultado;
        }
        catch
        {
            return new List<MiReservaResponse>();
        }
    }

    public async Task<SeguimientoReservaResponse?> ObtenerSeguimientoReservaAsync(int reservaId, int clienteId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Verificar que la reserva pertenece al cliente
            var reserva = await connection.QueryFirstOrDefaultAsync(
                @"SELECT r.id, r.estado, r.fecha_reserva, r.fecha_inicio, r.fecha_fin,
                    r.monto_total, r.monto_pagado
                  FROM reservas r
                  WHERE r.id = @reservaId AND r.cliente_id = @clienteId",
                new { reservaId, clienteId });

            if (reserva == null)
                return null;

            var seguimiento = new SeguimientoReservaResponse
            {
                Reserva_Id = reserva.id,
                Estado_Actual = reserva.estado,
                Fecha_Reserva = reserva.fecha_reserva,
                Fecha_Inicio = reserva.fecha_inicio,
                Fecha_Fin = reserva.fecha_fin,
                Monto_Total = reserva.monto_total,
                Monto_Pagado = reserva.monto_pagado ?? 0,
                Saldo_Pendiente = reserva.monto_total - (reserva.monto_pagado ?? 0)
            };

            // Obtener activos
            var activos = await connection.QueryAsync<ActivoReservadoResponse>(
                @"SELECT dr.activo_id, a.nombre, dr.cantidad, dr.precio_unitario,
                    (dr.cantidad * dr.precio_unitario) as subtotal, a.estado as estado_activo
                  FROM detalle_reserva dr
                  JOIN activos a ON a.id = dr.activo_id
                  WHERE dr.reserva_id = @reservaId",
                new { reservaId });

            seguimiento.Activos = activos.ToList();

            // Obtener logística
            var logistica = await connection.QueryFirstOrDefaultAsync<LogisticaReservaResponse>(
                @"SELECT 
                    (fecha_entrega_programada IS NOT NULL) as entrega_programada,
                    fecha_entrega_programada, fecha_entrega_real, estado_entrega,
                    (fecha_devolucion_programada IS NOT NULL) as devolucion_programada,
                    fecha_devolucion_programada, fecha_devolucion_real, estado_devolucion,
                    direccion_entrega
                  FROM logistica
                  WHERE reserva_id = @reservaId",
                new { reservaId });

            seguimiento.Logistica = logistica;

            // Obtener pagos
            var pagos = await connection.QueryAsync<PagoReservaResponse>(
                @"SELECT tp.id, tp.fecha_pago, tp.monto, tp.metodo_pago, tp.estado, tp.referencia_pago as referencia
                  FROM transacciones_pago tp
                  WHERE tp.reserva_id = @reservaId
                  ORDER BY tp.fecha_pago DESC",
                new { reservaId });

            seguimiento.Pagos = pagos.ToList();

            // Obtener historial de estados (de auditoría)
            var historial = await connection.QueryAsync<CambioEstadoReservaResponse>(
                @"SELECT 
                    datos_nuevos->>'Estado' as estado,
                    fecha_accion as fecha_cambio,
                    u.username as usuario,
                    detalles as comentario
                  FROM auditoria a
                  LEFT JOIN usuarios u ON u.id = a.usuario_id
                  WHERE tabla_afectada = 'Reservas' 
                    AND registro_id = @reservaId 
                    AND accion = 'StatusChange'
                  ORDER BY fecha_accion ASC",
                new { reservaId });

            seguimiento.Historial_Estados = historial.ToList();

            return seguimiento;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SeguimientoReservaResponse> CrearReservaAsync(CrearReservaClienteRequest request, int clienteId, int usuarioId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Validar fechas
            if (request.Fecha_Inicio < DateTime.UtcNow.Date)
                throw new InvalidOperationException("La fecha de inicio no puede ser anterior a hoy");

            if (request.Fecha_Fin <= request.Fecha_Inicio)
                throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio");

            // Calcular monto total (sumar precios de todos los activos)
            decimal montoTotal = 0;
            foreach (var detalle in request.Activos)
            {
                var activo = await connection.QueryFirstOrDefaultAsync<(decimal precio_alquiler, string estado)>(
                    @"SELECT precio_alquiler, estado FROM activos WHERE id = @activoId",
                    new { activoId = detalle.Activo_Id });

                if (activo.estado != "Disponible")
                    throw new InvalidOperationException($"El activo {detalle.Activo_Id} no está disponible");

                var precioUnitario = detalle.Precio_Unitario_Sugerido ?? activo.precio_alquiler;
                montoTotal += precioUnitario * detalle.Cantidad;
            }

            // Crear reserva
            var reservaId = await connection.QueryFirstAsync<int>(
                @"INSERT INTO reservas (cliente_id, usuario_id, fecha_reserva, fecha_inicio, fecha_fin,
                    estado, monto_total, observaciones, fecha_creacion, fecha_actualizacion)
                  VALUES (@clienteId, @usuarioId, NOW(), @fechaInicio, @fechaFin,
                    'Pendiente', @montoTotal, @observaciones, NOW(), NOW())
                  RETURNING id",
                new
                {
                    clienteId,
                    usuarioId,
                    fechaInicio = request.Fecha_Inicio,
                    fechaFin = request.Fecha_Fin,
                    montoTotal,
                    observaciones = request.Observaciones
                });

            // Crear detalles de reserva
            foreach (var detalle in request.Activos)
            {
                var activo = await connection.QueryFirstAsync<decimal>(
                    @"SELECT precio_alquiler FROM activos WHERE id = @activoId",
                    new { activoId = detalle.Activo_Id });

                var precioUnitario = detalle.Precio_Unitario_Sugerido ?? activo;

                await connection.ExecuteAsync(
                    @"INSERT INTO detalle_reserva (reserva_id, activo_id, cantidad, precio_unitario, fecha_creacion, fecha_actualizacion)
                      VALUES (@reservaId, @activoId, @cantidad, @precioUnitario, NOW(), NOW())",
                    new
                    {
                        reservaId,
                        activoId = detalle.Activo_Id,
                        cantidad = detalle.Cantidad,
                        precioUnitario
                    });
            }

            // Si tiene dirección de entrega, crear logística
            if (!string.IsNullOrEmpty(request.Direccion_Entrega))
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO logistica (reserva_id, direccion_entrega, fecha_entrega_programada, estado_entrega,
                        fecha_devolucion_programada, estado_devolucion, fecha_creacion, fecha_actualizacion)
                      VALUES (@reservaId, @direccion, @fechaInicio, 'Pendiente', @fechaFin, 'Pendiente', NOW(), NOW())",
                    new
                    {
                        reservaId,
                        direccion = request.Direccion_Entrega,
                        fechaInicio = request.Fecha_Inicio,
                        fechaFin = request.Fecha_Fin
                    });
            }

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(
                "Reservas",
                reservaId,
                usuarioId,
                "Create",
                System.Text.Json.JsonSerializer.Serialize(request),
                null,
                $"Reserva creada desde portal del cliente",
                "Portal Cliente",
                "Sistema");

            // Retornar seguimiento
            return await ObtenerSeguimientoReservaAsync(reservaId, clienteId)
                ?? throw new InvalidOperationException("Error recuperando la reserva creada");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creando reserva: {ex.Message}");
        }
    }

    public async Task<VerificarDisponibilidadResponse> VerificarDisponibilidadAsync(DateTime fechaInicio, DateTime fechaFin, List<int> activoIds)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var disponibilidad = new VerificarDisponibilidadResponse
            {
                Fecha_Inicio = fechaInicio,
                Fecha_Fin = fechaFin,
                Todos_Disponibles = true
            };

            foreach (var activoId in activoIds)
            {
                // Obtener información del activo
                var activo = await connection.QueryFirstOrDefaultAsync<(string nombre, string estado, int cantidad_disponible)>(
                    @"SELECT nombre, estado, cantidad_disponible FROM activos WHERE id = @activoId",
                    new { activoId });

                if (activo.nombre == null)
                {
                    disponibilidad.Activos.Add(new DisponibilidadResponse
                    {
                        Activo_Id = activoId,
                        Nombre = "Activo no encontrado",
                        Disponible = false,
                        Cantidad_Disponible = 0,
                        Motivo_No_Disponible = "El activo no existe"
                    });
                    disponibilidad.Todos_Disponibles = false;
                    continue;
                }

                // Verificar si está disponible en general
                if (activo.estado != "Disponible")
                {
                    disponibilidad.Activos.Add(new DisponibilidadResponse
                    {
                        Activo_Id = activoId,
                        Nombre = activo.nombre,
                        Disponible = false,
                        Cantidad_Disponible = 0,
                        Motivo_No_Disponible = $"Estado del activo: {activo.estado}"
                    });
                    disponibilidad.Todos_Disponibles = false;
                    continue;
                }

                // Verificar reservas en el período
                var reservado = await connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SUM(dr.cantidad)
                      FROM detalle_reserva dr
                      JOIN reservas r ON r.id = dr.reserva_id
                      WHERE dr.activo_id = @activoId
                        AND r.estado NOT IN ('Cancelada', 'Completada')
                        AND (r.fecha_inicio <= @fechaFin AND r.fecha_fin >= @fechaInicio)",
                    new { activoId, fechaInicio, fechaFin });

                var cantidadReservada = reservado ?? 0;
                var cantidadDisponible = activo.cantidad_disponible - cantidadReservada;

                disponibilidad.Activos.Add(new DisponibilidadResponse
                {
                    Activo_Id = activoId,
                    Nombre = activo.nombre,
                    Disponible = cantidadDisponible > 0,
                    Cantidad_Disponible = Math.Max(0, cantidadDisponible),
                    Motivo_No_Disponible = cantidadDisponible <= 0 ? "Sin stock disponible para esas fechas" : null
                });

                if (cantidadDisponible <= 0)
                    disponibilidad.Todos_Disponibles = false;
            }

            return disponibilidad;
        }
        catch
        {
            return new VerificarDisponibilidadResponse
            {
                Fecha_Inicio = fechaInicio,
                Fecha_Fin = fechaFin,
                Todos_Disponibles = false
            };
        }
    }

    public async Task<List<MiCotizacionResponse>> ObtenerMisCotizacionesAsync(int clienteId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var cotizaciones = await connection.QueryAsync<MiCotizacionResponse>(
                @"SELECT sc.id, sc.fecha_solicitud, sc.estado, sc.descripcion_servicio,
                    sc.fecha_evento, sc.ubicacion_evento, sc.monto_estimado,
                    (sc.fecha_respuesta IS NOT NULL) as tiene_respuesta, sc.fecha_respuesta
                  FROM solicitud_cotizacion sc
                  WHERE sc.cliente_id = @clienteId
                  ORDER BY sc.fecha_solicitud DESC",
                new { clienteId });

            return cotizaciones.ToList();
        }
        catch
        {
            return new List<MiCotizacionResponse>();
        }
    }

    public async Task<MiCotizacionResponse> SolicitarCotizacionAsync(SolicitarCotizacionClienteRequest request, int clienteId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Validar que el servicio existe
            var servicio = await connection.QueryFirstOrDefaultAsync(
                @"SELECT id FROM servicios WHERE id = @servicioId",
                new { servicioId = request.Servicio_Id });

            if (servicio == null)
                throw new InvalidOperationException("Servicio no encontrado");

            // Crear cotización
            var cotizacionId = await connection.QueryFirstAsync<int>(
                @"INSERT INTO solicitud_cotizacion (cliente_id, servicio_id, descripcion_servicio,
                    fecha_solicitud, fecha_evento, ubicacion_evento, cantidad_personas_estimada,
                    observaciones, estado, fecha_creacion, fecha_actualizacion)
                  VALUES (@clienteId, @servicioId, @descripcion, NOW(), @fechaEvento, @ubicacion,
                    @cantidadPersonas, @observaciones, 'Pendiente', NOW(), NOW())
                  RETURNING id",
                new
                {
                    clienteId,
                    servicioId = request.Servicio_Id,
                    descripcion = request.Descripcion,
                    fechaEvento = request.Fecha_Evento,
                    ubicacion = request.Ubicacion_Evento,
                    cantidadPersonas = request.Cantidad_Personas_Estimada,
                    observaciones = request.Observaciones
                });

            // Obtener la cotización creada
            var cotizacion = await connection.QueryFirstAsync<MiCotizacionResponse>(
                @"SELECT sc.id, sc.fecha_solicitud, sc.estado, sc.descripcion_servicio,
                    sc.fecha_evento, sc.ubicacion_evento, sc.monto_estimado,
                    (sc.fecha_respuesta IS NOT NULL) as tiene_respuesta, sc.fecha_respuesta
                  FROM solicitud_cotizacion sc
                  WHERE sc.id = @cotizacionId",
                new { cotizacionId });

            return cotizacion;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error solicitando cotización: {ex.Message}");
        }
    }

    public async Task<EstadisticasClienteResponse> ObtenerEstadisticasAsync(int clienteId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var stats = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(r.id) as total_reservas,
                    SUM(CASE WHEN r.estado IN ('Pendiente', 'Confirmada', 'En_Proceso') THEN 1 ELSE 0 END) as activas,
                    SUM(CASE WHEN r.estado = 'Completada' THEN 1 ELSE 0 END) as completadas,
                    SUM(CASE WHEN r.estado = 'Cancelada' THEN 1 ELSE 0 END) as canceladas,
                    COALESCE(SUM(r.monto_pagado), 0) as total_gastado,
                    COALESCE(SUM(r.monto_total - COALESCE(r.monto_pagado, 0)), 0) as saldo_pendiente,
                    MAX(r.fecha_reserva) as ultima_reserva
                  FROM reservas r
                  WHERE r.cliente_id = @clienteId",
                new { clienteId });

            var cotizaciones = await connection.QueryFirstAsync<(int total, int pendientes)>(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN estado = 'Pendiente' THEN 1 ELSE 0 END) as pendientes
                  FROM solicitud_cotizacion
                  WHERE cliente_id = @clienteId",
                new { clienteId });

            var fechaRegistro = await connection.QueryFirstAsync<DateTime>(
                @"SELECT c.fecha_creacion FROM clientes c WHERE c.id = @clienteId",
                new { clienteId });

            return new EstadisticasClienteResponse
            {
                Total_Reservas = stats.total_reservas,
                Reservas_Activas = stats.activas ?? 0,
                Reservas_Completadas = stats.completadas ?? 0,
                Reservas_Canceladas = stats.canceladas ?? 0,
                Total_Gastado = stats.total_gastado,
                Saldo_Pendiente = stats.saldo_pendiente,
                Total_Cotizaciones = cotizaciones.total,
                Cotizaciones_Pendientes = cotizaciones.pendientes ?? 0,
                Ultima_Reserva = stats.ultima_reserva,
                Fecha_Registro = fechaRegistro
            };
        }
        catch
        {
            return new EstadisticasClienteResponse();
        }
    }

    public async Task<bool> CancelarReservaAsync(int reservaId, int clienteId, string motivo)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Verificar que la reserva pertenece al cliente y está en estado cancelable
            var reserva = await connection.QueryFirstOrDefaultAsync<(int id, string estado)>(
                @"SELECT id, estado FROM reservas 
                  WHERE id = @reservaId AND cliente_id = @clienteId",
                new { reservaId, clienteId });

            if (reserva.id == 0)
                throw new InvalidOperationException("Reserva no encontrada");

            if (reserva.estado != "Pendiente" && reserva.estado != "Confirmada")
                throw new InvalidOperationException($"No se puede cancelar una reserva en estado {reserva.estado}");

            // Actualizar estado
            await connection.ExecuteAsync(
                @"UPDATE reservas SET estado = 'Cancelada', observaciones = @motivo, fecha_actualizacion = NOW()
                  WHERE id = @reservaId",
                new { reservaId, motivo });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<PagoReservaResponse>> ObtenerHistorialPagosAsync(int clienteId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var pagos = await connection.QueryAsync<PagoReservaResponse>(
                @"SELECT tp.id, tp.fecha_pago, tp.monto, tp.metodo_pago, tp.estado, tp.referencia_pago as referencia
                  FROM transacciones_pago tp
                  JOIN reservas r ON r.id = tp.reserva_id
                  WHERE r.cliente_id = @clienteId
                  ORDER BY tp.fecha_pago DESC",
                new { clienteId });

            return pagos.ToList();
        }
        catch
        {
            return new List<PagoReservaResponse>();
        }
    }
}
