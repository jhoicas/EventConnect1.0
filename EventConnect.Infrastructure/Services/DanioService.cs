using System.Data;
using Dapper;
using Npgsql;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace EventConnect.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de daños y discrepancias
/// Integrado con auditoría y gestión de activos
/// </summary>
public class DanioService : IDanioService
{
    private readonly string _connectionString;
    private readonly IAuditoriaService _auditoriaService;
    private const string TablaAuditoria = "Danios";

    public DanioService(IConfiguration configuration, IAuditoriaService auditoriaService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _auditoriaService = auditoriaService;
    }

    public async Task<DanioDetalladoResponse> RegistrarDanioAsync(CrearDanioRequest request, int usuarioReportadorId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Verificar que la reserva, activo y cliente existan
            var verificacion = await connection.QueryFirstOrDefaultAsync(
                @"SELECT r.id as reserva_id, a.id as activo_id, c.id as cliente_id
                  FROM reservas r
                  JOIN activos a ON a.id = @activoId
                  JOIN clientes c ON c.id = @clienteId
                  WHERE r.id = @reservaId",
                new { request.Reserva_Id, request.Activo_Id, request.Cliente_Id });

            if (verificacion == null)
                throw new InvalidOperationException("Reserva, activo o cliente no válidos");

            // Insertar el daño
            var danioId = await connection.QueryFirstAsync<int>(
                @"INSERT INTO danios (reserva_id, activo_id, cliente_id, descripcion, estado, 
                    fecha_reporte, imagen_url, monto_estimado, usuario_reportador_id, observaciones, 
                    fecha_creacion, fecha_actualizacion)
                  VALUES (@reservaId, @activoId, @clienteId, @descripcion, 'Reportado', 
                    @fechaReporte, @imagenUrl, @montoEstimado, @usuarioReportadorId, @observaciones,
                    @fechaCreacion, @fechaActualizacion)
                  RETURNING id",
                new
                {
                    request.Reserva_Id,
                    request.Activo_Id,
                    request.Cliente_Id,
                    request.Descripcion,
                    fechaReporte = DateTime.UtcNow,
                    request.Imagen_URL,
                    request.Monto_Estimado,
                    usuarioReportadorId,
                    request.Observaciones,
                    fechaCreacion = DateTime.UtcNow,
                    fechaActualizacion = DateTime.UtcNow
                });

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(
                TablaAuditoria,
                danioId,
                usuarioReportadorId,
                "Create",
                System.Text.Json.JsonSerializer.Serialize(request),
                null,
                $"Daño reportado en activo {request.Activo_Id}",
                "Sistema",
                "Sistema");

            return await ObtenerDanioPorIdAsync(danioId) 
                ?? throw new InvalidOperationException("Error recuperando el daño creado");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error registrando daño: {ex.Message}");
        }
    }

    public async Task<PaginatedDanioResponse> ObtenerDaniosAsync(FiltrarDaniosRequest filtro)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construir WHERE dinámicamente
            var whereClauses = new List<string> { "1=1" };
            var parameters = new DynamicParameters();

            if (filtro.Reserva_Id.HasValue)
            {
                whereClauses.Add("d.reserva_id = @reservaId");
                parameters.Add("@reservaId", filtro.Reserva_Id);
            }
            if (filtro.Activo_Id.HasValue)
            {
                whereClauses.Add("d.activo_id = @activoId");
                parameters.Add("@activoId", filtro.Activo_Id);
            }
            if (filtro.Cliente_Id.HasValue)
            {
                whereClauses.Add("d.cliente_id = @clienteId");
                parameters.Add("@clienteId", filtro.Cliente_Id);
            }
            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                whereClauses.Add("d.estado = @estado");
                parameters.Add("@estado", filtro.Estado);
            }
            if (filtro.Fecha_Desde.HasValue)
            {
                whereClauses.Add("d.fecha_reporte >= @fechaDesde");
                parameters.Add("@fechaDesde", filtro.Fecha_Desde.Value.Date);
            }
            if (filtro.Fecha_Hasta.HasValue)
            {
                whereClauses.Add("d.fecha_reporte <= @fechaHasta");
                parameters.Add("@fechaHasta", filtro.Fecha_Hasta.Value.AddDays(1).Date);
            }
            if (filtro.Usuario_Reportador_Id.HasValue)
            {
                whereClauses.Add("d.usuario_reportador_id = @usuarioReportador");
                parameters.Add("@usuarioReportador", filtro.Usuario_Reportador_Id);
            }

            var whereClause = string.Join(" AND ", whereClauses);

            // Obtener total
            var total = await connection.QueryFirstAsync<int>(
                $@"SELECT COUNT(*) FROM danios d WHERE {whereClause}",
                parameters);

            // Obtener página
            var offset = (filtro.Pagina - 1) * filtro.Cantidad_Por_Pagina;
            parameters.Add("@offset", offset);
            parameters.Add("@limit", filtro.Cantidad_Por_Pagina);

            var danios = await connection.QueryAsync<DanioResponse>(
                $@"SELECT d.id, d.reserva_id, d.activo_id, d.cliente_id, d.descripcion, d.estado,
                    d.fecha_reporte, d.monto_estimado, d.monto_final,
                    a.nombre as nombre_activo, c.empresa_id as nombre_cliente
                  FROM danios d
                  LEFT JOIN activos a ON a.id = d.activo_id
                  LEFT JOIN clientes c ON c.id = d.cliente_id
                  WHERE {whereClause}
                  ORDER BY d.fecha_reporte DESC
                  LIMIT @limit OFFSET @offset",
                parameters);

            return new PaginatedDanioResponse
            {
                Items = danios.ToList(),
                Total = total,
                Pagina = filtro.Pagina,
                Cantidad_Por_Pagina = filtro.Cantidad_Por_Pagina
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error obteniendo daños: {ex.Message}");
        }
    }

    public async Task<DanioDetalladoResponse?> ObtenerDanioPorIdAsync(int danioId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<DanioDetalladoResponse>(
                @"SELECT d.id, d.reserva_id, d.activo_id, d.cliente_id, d.descripcion, d.estado,
                    d.fecha_reporte, d.fecha_resolucion, d.imagen_url, d.monto_estimado, d.monto_final,
                    d.resolucion, d.usuario_reportador_id, d.usuario_evaluador_id, d.observaciones,
                    d.fecha_creacion, d.fecha_actualizacion,
                    u1.username as nombre_reportador, u2.username as nombre_evaluador,
                    a.nombre as nombre_activo, c.empresa_id as nombre_cliente
                  FROM danios d
                  LEFT JOIN usuarios u1 ON u1.id = d.usuario_reportador_id
                  LEFT JOIN usuarios u2 ON u2.id = d.usuario_evaluador_id
                  LEFT JOIN activos a ON a.id = d.activo_id
                  LEFT JOIN clientes c ON c.id = d.cliente_id
                  WHERE d.id = @danioId",
                new { danioId });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<DanioResponse>> ObtenerDanioPorReservaAsync(int reservaId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var danios = await connection.QueryAsync<DanioResponse>(
                @"SELECT d.id, d.reserva_id, d.activo_id, d.cliente_id, d.descripcion, d.estado,
                    d.fecha_reporte, d.monto_estimado, d.monto_final,
                    a.nombre as nombre_activo, c.empresa_id as nombre_cliente
                  FROM danios d
                  LEFT JOIN activos a ON a.id = d.activo_id
                  LEFT JOIN clientes c ON c.id = d.cliente_id
                  WHERE d.reserva_id = @reservaId
                  ORDER BY d.fecha_reporte DESC",
                new { reservaId });

            return danios.ToList();
        }
        catch
        {
            return new List<DanioResponse>();
        }
    }

    public async Task<ResumenDanioActivoResponse> ObtenerDanioPorActivoAsync(int activoId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var danios = await connection.QueryAsync<DanioResponse>(
                @"SELECT d.id, d.reserva_id, d.activo_id, d.cliente_id, d.descripcion, d.estado,
                    d.fecha_reporte, d.monto_estimado, d.monto_final,
                    a.nombre as nombre_activo, c.empresa_id as nombre_cliente
                  FROM danios d
                  LEFT JOIN activos a ON a.id = d.activo_id
                  LEFT JOIN clientes c ON c.id = d.cliente_id
                  WHERE d.activo_id = @activoId
                  ORDER BY d.fecha_reporte DESC",
                new { activoId });

            var resumen = await connection.QueryFirstOrDefaultAsync(
                @"SELECT d.activo_id, a.nombre, 
                    COUNT(*) as total,
                    SUM(CASE WHEN d.estado = 'Confirmado' THEN 1 ELSE 0 END) as confirmados,
                    SUM(CASE WHEN d.estado = 'En_Reparacion' THEN 1 ELSE 0 END) as en_reparacion,
                    COALESCE(SUM(d.monto_final), 0) as monto_total
                  FROM danios d
                  LEFT JOIN activos a ON a.id = d.activo_id
                  WHERE d.activo_id = @activoId
                  GROUP BY d.activo_id, a.nombre",
                new { activoId });

            return new ResumenDanioActivoResponse
            {
                Activo_Id = activoId,
                NombreActivo = resumen?.nombre,
                Total_Danios = resumen?.total ?? 0,
                Confirmados = resumen?.confirmados ?? 0,
                En_Reparacion = resumen?.en_reparacion ?? 0,
                Monto_Total = resumen?.monto_total ?? 0,
                Danios = danios.ToList()
            };
        }
        catch
        {
            return new ResumenDanioActivoResponse { Activo_Id = activoId };
        }
    }

    public async Task<DanioDetalladoResponse> ActualizarEstadoDanioAsync(int danioId, ActualizarDanioRequest request)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Obtener daño actual
            var danioBefore = await ObtenerDanioPorIdAsync(danioId);
            if (danioBefore == null)
                throw new InvalidOperationException("Daño no encontrado");

            // Actualizar estado
            await connection.ExecuteAsync(
                @"UPDATE danios SET estado = @estado, resolucion = @resolucion, 
                    monto_final = @montoFinal, usuario_evaluador_id = @usuarioEvaluador,
                    observaciones = @observaciones, fecha_actualizacion = @fechaActualizacion
                  WHERE id = @danioId",
                new
                {
                    @estado = request.Estado,
                    request.Resolucion,
                    request.Monto_Final,
                    usuarioEvaluador = request.Usuario_Evaluador_Id,
                    request.Observaciones,
                    fechaActualizacion = DateTime.UtcNow,
                    danioId
                });

            // Registrar en auditoría
            var usuario_id = request.Usuario_Evaluador_Id ?? danioBefore.Usuario_Reportador_Id;
            await _auditoriaService.RegistrarCambioAsync(
                TablaAuditoria,
                danioId,
                usuario_id,
                "StatusChange",
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Json.JsonSerializer.Serialize(danioBefore),
                $"Estado actualizado a: {request.Estado}",
                "Sistema",
                "Sistema");

            return await ObtenerDanioPorIdAsync(danioId)
                ?? throw new InvalidOperationException("Error recuperando daño actualizado");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error actualizando daño: {ex.Message}");
        }
    }

    public async Task<EstadisticasDaniosResponse> ObtenerEstadisticasAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var stats = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN estado = 'Reportado' THEN 1 ELSE 0 END) as reportados,
                    SUM(CASE WHEN estado = 'En_Evaluacion' THEN 1 ELSE 0 END) as en_evaluacion,
                    SUM(CASE WHEN estado = 'Confirmado' THEN 1 ELSE 0 END) as confirmados,
                    SUM(CASE WHEN estado = 'Rechazado' THEN 1 ELSE 0 END) as rechazados,
                    SUM(CASE WHEN estado = 'En_Reparacion' THEN 1 ELSE 0 END) as en_reparacion,
                    SUM(CASE WHEN estado = 'Reparado' THEN 1 ELSE 0 END) as reparados,
                    SUM(CASE WHEN estado = 'Perdida_Total' THEN 1 ELSE 0 END) as perdida_total,
                    COALESCE(SUM(monto_estimado), 0) as monto_estimado,
                    COALESCE(SUM(monto_final), 0) as monto_final,
                    COALESCE(AVG(EXTRACT(DAY FROM fecha_resolucion - fecha_reporte)), 0) as dias_promedio,
                    SUM(CASE WHEN DATE_TRUNC('month', fecha_reporte) = DATE_TRUNC('month', NOW()) THEN 1 ELSE 0 END) as danios_mes
                  FROM danios");

            return new EstadisticasDaniosResponse
            {
                Total_Danios = stats.total,
                Reportados = stats.reportados ?? 0,
                En_Evaluacion = stats.en_evaluacion ?? 0,
                Confirmados = stats.confirmados ?? 0,
                Rechazados = stats.rechazados ?? 0,
                En_Reparacion = stats.en_reparacion ?? 0,
                Reparados = stats.reparados ?? 0,
                Perdida_Total = stats.perdida_total ?? 0,
                Monto_Total_Estimado = stats.monto_estimado,
                Monto_Total_Final = stats.monto_final,
                Promedio_Resolucion_Dias = (decimal)(stats.dias_promedio ?? 0),
                Danios_Este_Mes = stats.danios_mes ?? 0
            };
        }
        catch
        {
            return new EstadisticasDaniosResponse();
        }
    }

    public async Task<DanioDetalladoResponse> ResolverDanioAsync(int danioId, string resolucion, decimal montoFinal, int usuarioEvaluadorId)
    {
        var request = new ActualizarDanioRequest
        {
            Estado = "Reparado",
            Resolucion = resolucion,
            Monto_Final = montoFinal,
            Usuario_Evaluador_Id = usuarioEvaluadorId
        };

        return await ActualizarEstadoDanioAsync(danioId, request);
    }

    public async Task<DanioDetalladoResponse> RechazarDanioAsync(int danioId, string motivo, int usuarioEvaluadorId)
    {
        var request = new ActualizarDanioRequest
        {
            Estado = "Rechazado",
            Resolucion = motivo,
            Usuario_Evaluador_Id = usuarioEvaluadorId
        };

        return await ActualizarEstadoDanioAsync(danioId, request);
    }
}
