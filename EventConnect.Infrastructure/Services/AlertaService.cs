using System.Data;
using Dapper;
using Npgsql;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace EventConnect.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de alertas de mantenimiento y depreciación
/// Genera alertas automáticas para mantener activos en óptimas condiciones
/// </summary>
public class AlertaService : IAlertaService
{
    private readonly string _connectionString;
    private readonly IAuditoriaService _auditoriaService;
    private const string TablaAuditoria = "Alertas";

    // Configuración de períodos (en días)
    private const int DIAS_MANTENIMIENTO = 180; // 6 meses
    private const int DIAS_DEPRECIACION_CRITICA = 30; // 1 mes antes del fin
    private const int DIAS_VENCIMIENTO_ALERTA = 30; // 1 mes antes de vencer

    public AlertaService(IConfiguration configuration, IAuditoriaService auditoriaService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _auditoriaService = auditoriaService;
    }

    public async Task<PaginatedAlertaResponse> ObtenerAlertasAsync(FiltrarAlertasRequest filtro)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construir WHERE dinámicamente
            var whereClauses = new List<string> { "1=1" };
            var parameters = new DynamicParameters();

            if (filtro.Activo_Id.HasValue)
            {
                whereClauses.Add("a.activo_id = @activoId");
                parameters.Add("@activoId", filtro.Activo_Id);
            }
            if (!string.IsNullOrEmpty(filtro.Tipo))
            {
                whereClauses.Add("a.tipo = @tipo");
                parameters.Add("@tipo", filtro.Tipo);
            }
            if (!string.IsNullOrEmpty(filtro.Severidad))
            {
                whereClauses.Add("a.severidad = @severidad");
                parameters.Add("@severidad", filtro.Severidad);
            }
            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                whereClauses.Add("a.estado = @estado");
                parameters.Add("@estado", filtro.Estado);
            }
            if (filtro.Usuario_Asignado_Id.HasValue)
            {
                whereClauses.Add("a.usuario_asignado_id = @usuarioAsignado");
                parameters.Add("@usuarioAsignado", filtro.Usuario_Asignado_Id);
            }
            if (filtro.Fecha_Desde.HasValue)
            {
                whereClauses.Add("a.fecha_alerta >= @fechaDesde");
                parameters.Add("@fechaDesde", filtro.Fecha_Desde.Value.Date);
            }
            if (filtro.Fecha_Hasta.HasValue)
            {
                whereClauses.Add("a.fecha_alerta <= @fechaHasta");
                parameters.Add("@fechaHasta", filtro.Fecha_Hasta.Value.AddDays(1).Date);
            }
            if (filtro.Solo_Pendientes.HasValue && filtro.Solo_Pendientes.Value)
            {
                whereClauses.Add("a.estado = 'Pendiente'");
            }

            var whereClause = string.Join(" AND ", whereClauses);

            // Obtener total
            var total = await connection.QueryFirstAsync<int>(
                $@"SELECT COUNT(*) FROM alertas a WHERE {whereClause}",
                parameters);

            // Obtener página
            var offset = (filtro.Pagina - 1) * filtro.Cantidad_Por_Pagina;
            parameters.Add("@offset", offset);
            parameters.Add("@limit", filtro.Cantidad_Por_Pagina);

            var alertas = await connection.QueryAsync<AlertaResponse>(
                $@"SELECT a.id, a.activo_id, a.tipo, a.descripcion, a.severidad, a.estado,
                    a.fecha_alerta, a.fecha_vencimiento, a.prioridad,
                    act.nombre as nombre_activo,
                    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  WHERE {whereClause}
                  ORDER BY a.prioridad DESC, a.fecha_alerta DESC
                  LIMIT @limit OFFSET @offset",
                parameters);

            return new PaginatedAlertaResponse
            {
                Items = alertas.ToList(),
                Total = total,
                Pagina = filtro.Pagina,
                Cantidad_Por_Pagina = filtro.Cantidad_Por_Pagina
            };
        }
        catch
        {
            return new PaginatedAlertaResponse();
        }
    }

    public async Task<AlertaDetalladoResponse?> ObtenerAlertaPorIdAsync(int alertaId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<AlertaDetalladoResponse>(
                @"SELECT a.id, a.activo_id, a.tipo, a.descripcion, a.severidad, a.estado,
                    a.fecha_alerta, a.fecha_vencimiento, a.fecha_resolucion, a.usuario_asignado_id,
                    a.detalles_tecnicos, a.observaciones, a.acciones_recomendadas,
                    a.notificacion_enviada, a.fecha_notificacion, a.prioridad,
                    act.nombre as nombre_activo, u.username as nombre_usuario_asignado,
                    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  LEFT JOIN usuarios u ON u.id = a.usuario_asignado_id
                  WHERE a.id = @alertaId",
                new { alertaId });
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResumenAlertasPorActivoResponse> ObtenerAlertasPorActivoAsync(int activoId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var alertas = await connection.QueryAsync<AlertaResponse>(
                @"SELECT a.id, a.activo_id, a.tipo, a.descripcion, a.severidad, a.estado,
                    a.fecha_alerta, a.fecha_vencimiento, a.prioridad,
                    act.nombre as nombre_activo,
                    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  WHERE a.activo_id = @activoId AND a.estado != 'Resuelta'
                  ORDER BY a.prioridad DESC",
                new { activoId });

            var stats = await connection.QueryFirstOrDefaultAsync(
                @"SELECT act.nombre, COUNT(*) as total,
                    SUM(CASE WHEN a.severidad = 'Critica' THEN 1 ELSE 0 END) as criticas,
                    SUM(CASE WHEN a.estado = 'Pendiente' THEN 1 ELSE 0 END) as pendientes
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  WHERE a.activo_id = @activoId
                  GROUP BY act.nombre",
                new { activoId });

            return new ResumenAlertasPorActivoResponse
            {
                Activo_Id = activoId,
                NombreActivo = stats?.nombre,
                Total_Alertas = stats?.total ?? 0,
                Pendientes = stats?.pendientes ?? 0,
                Criticas = stats?.criticas ?? 0,
                Alertas = alertas.ToList()
            };
        }
        catch
        {
            return new ResumenAlertasPorActivoResponse { Activo_Id = activoId };
        }
    }

    public async Task<List<AlertaResponse>> ObtenerAlertasCriticasAsync(int limit = 20)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var alertas = await connection.QueryAsync<AlertaResponse>(
                @"SELECT a.id, a.activo_id, a.tipo, a.descripcion, a.severidad, a.estado,
                    a.fecha_alerta, a.fecha_vencimiento, a.prioridad,
                    act.nombre as nombre_activo,
                    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  WHERE (a.severidad = 'Critica' OR a.estado = 'Vencida') AND a.estado != 'Resuelta'
                  ORDER BY a.prioridad DESC, a.fecha_alerta ASC
                  LIMIT @limit",
                new { limit });

            return alertas.ToList();
        }
        catch
        {
            return new List<AlertaResponse>();
        }
    }

    public async Task<ResultadoGeneracionAlertasResponse> GenerarAlertasAutomaticasAsync()
    {
        var resultado = new ResultadoGeneracionAlertasResponse();

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Obtener todos los activos activos
            var activos = await connection.QueryAsync<(int id, DateTime fecha_ultimo_mantenimiento, DateTime fecha_fin_vida_util)>(
                @"SELECT id, fecha_ultimo_mantenimiento, fecha_fin_vida_util 
                  FROM activos 
                  WHERE estado IN ('Disponible', 'En_Uso', 'Mantenimiento') 
                  AND fecha_fin_vida_util IS NOT NULL");

            foreach (var activo in activos)
            {
                resultado.Total_Procesados++;

                try
                {
                    // Verificar alerta de mantenimiento
                    var dias_desde_mantenimiento = (DateTime.UtcNow - activo.fecha_ultimo_mantenimiento).Days;
                    if (dias_desde_mantenimiento > DIAS_MANTENIMIENTO)
                    {
                        var alertaExistente = await connection.QueryFirstOrDefaultAsync<int?>(
                            @"SELECT id FROM alertas 
                              WHERE activo_id = @activoId AND tipo = 'Mantenimiento' 
                              AND estado != 'Resuelta'",
                            new { activoId = activo.id });

                        if (alertaExistente == null)
                        {
                            await connection.ExecuteAsync(
                                @"INSERT INTO alertas (activo_id, tipo, descripcion, severidad, estado,
                                    fecha_alerta, fecha_vencimiento, acciones_recomendadas, prioridad,
                                    fecha_creacion, fecha_actualizacion)
                                  VALUES (@activoId, 'Mantenimiento',
                                    'Mantenimiento requerido - últimas revisión hace ' || @dias || ' días',
                                    'Alta', 'Pendiente', NOW(), NOW() + INTERVAL '7 days',
                                    'Programar revisión técnica', 8, NOW(), NOW())",
                                new { activoId = activo.id, dias = dias_desde_mantenimiento });

                            resultado.Alertas_Creadas++;
                            resultado.Mensajes.Add($"Alerta de mantenimiento creada para activo {activo.id}");
                        }
                    }

                    // Verificar alerta de depreciación
                    var dias_para_fin = (activo.fecha_fin_vida_util - DateTime.UtcNow).Days;
                    if (dias_para_fin > 0 && dias_para_fin <= DIAS_DEPRECIACION_CRITICA)
                    {
                        var alertaExistente = await connection.QueryFirstOrDefaultAsync<int?>(
                            @"SELECT id FROM alertas 
                              WHERE activo_id = @activoId AND tipo = 'Depreciacion' 
                              AND estado != 'Resuelta'",
                            new { activoId = activo.id });

                        if (alertaExistente == null)
                        {
                            var severidad = dias_para_fin <= 7 ? "Critica" : "Alta";
                            await connection.ExecuteAsync(
                                @"INSERT INTO alertas (activo_id, tipo, descripcion, severidad, estado,
                                    fecha_alerta, fecha_vencimiento, acciones_recomendadas, prioridad,
                                    fecha_creacion, fecha_actualizacion)
                                  VALUES (@activoId, 'Depreciacion',
                                    'Fin de vida útil próximo en ' || @dias || ' días',
                                    @severidad, 'Pendiente', NOW(), @fechaVencimiento,
                                    'Evaluar reemplazo del activo', 9, NOW(), NOW())",
                                new
                                {
                                    activoId = activo.id,
                                    dias = dias_para_fin,
                                    severidad,
                                    fechaVencimiento = activo.fecha_fin_vida_util
                                });

                            resultado.Alertas_Creadas++;
                            resultado.Mensajes.Add($"Alerta de depreciación creada para activo {activo.id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultado.Errores++;
                    resultado.Mensajes.Add($"Error procesando activo {activo.id}: {ex.Message}");
                }
            }

            // Marcar alertas como vencidas si pasó la fecha
            await connection.ExecuteAsync(
                @"UPDATE alertas SET estado = 'Vencida', fecha_actualizacion = NOW()
                  WHERE fecha_vencimiento < NOW() AND estado IN ('Pendiente', 'Asignada', 'En_Proceso')");
        }
        catch (Exception ex)
        {
            resultado.Errores++;
            resultado.Mensajes.Add($"Error general: {ex.Message}");
        }

        return resultado;
    }

    public async Task<List<AlertaAutomaticaResponse>> GenerarAlertasParaActivoAsync(int activoId)
    {
        var alertas = new List<AlertaAutomaticaResponse>();

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var activo = await connection.QueryFirstOrDefaultAsync(
                @"SELECT fecha_ultimo_mantenimiento, fecha_fin_vida_util 
                  FROM activos WHERE id = @activoId",
                new { activoId });

            if (activo == null) return alertas;

            var dias_desde_mant = (DateTime.UtcNow - activo.fecha_ultimo_mantenimiento).Days;
            var dias_para_fin = (activo.fecha_fin_vida_util - DateTime.UtcNow).Days;

            if (dias_desde_mant > DIAS_MANTENIMIENTO)
            {
                var id = await connection.QueryFirstAsync<int>(
                    @"INSERT INTO alertas (activo_id, tipo, descripcion, severidad, estado, fecha_alerta, prioridad, fecha_creacion, fecha_actualizacion)
                      VALUES (@activoId, 'Mantenimiento', 'Mantenimiento requerido', 'Alta', 'Pendiente', NOW(), 8, NOW(), NOW())
                      RETURNING id",
                    new { activoId });

                alertas.Add(new AlertaAutomaticaResponse
                {
                    Id = id,
                    Tipo = "Mantenimiento",
                    Descripcion = $"Mantenimiento requerido - {dias_desde_mant} días sin revisión",
                    Severidad = "Alta",
                    Activo_Id = activoId,
                    Fecha_Generacion = DateTime.UtcNow
                });
            }

            if (dias_para_fin > 0 && dias_para_fin <= DIAS_DEPRECIACION_CRITICA)
            {
                var severidad = dias_para_fin <= 7 ? "Critica" : "Alta";
                var id = await connection.QueryFirstAsync<int>(
                    @"INSERT INTO alertas (activo_id, tipo, descripcion, severidad, estado, fecha_alerta, fecha_vencimiento, prioridad, fecha_creacion, fecha_actualizacion)
                      VALUES (@activoId, 'Depreciacion', 'Fin de vida útil próximo', @severidad, 'Pendiente', NOW(), NOW() + INTERVAL '30 days', 9, NOW(), NOW())
                      RETURNING id",
                    new { activoId, severidad });

                alertas.Add(new AlertaAutomaticaResponse
                {
                    Id = id,
                    Tipo = "Depreciacion",
                    Descripcion = $"Fin de vida útil en {dias_para_fin} días",
                    Severidad = severidad,
                    Activo_Id = activoId,
                    Fecha_Generacion = DateTime.UtcNow
                });
            }
        }
        catch
        {
            // No lanzar excepción
        }

        return alertas;
    }

    public async Task<AlertaDetalladoResponse> ActualizarEstadoAlertaAsync(int alertaId, ActualizarAlertaRequest request)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var alerta_before = await ObtenerAlertaPorIdAsync(alertaId);
            if (alerta_before == null)
                throw new InvalidOperationException("Alerta no encontrada");

            var estado = request.Marcar_Como_Resuelta.HasValue && request.Marcar_Como_Resuelta.Value 
                ? "Resuelta" 
                : request.Estado ?? alerta_before.Estado;

            var fecha_resolucion = estado == "Resuelta" ? DateTime.UtcNow : (DateTime?)null;

            await connection.ExecuteAsync(
                @"UPDATE alertas SET estado = @estado, usuario_asignado_id = @usuarioAsignado,
                    observaciones = @observaciones, prioridad = @prioridad,
                    fecha_resolucion = @fechaResolucion, fecha_actualizacion = NOW()
                  WHERE id = @alertaId",
                new
                {
                    @estado = estado,
                    usuarioAsignado = request.Usuario_Asignado_Id,
                    request.Observaciones,
                    prioridad = request.Prioridad ?? alerta_before.Prioridad,
                    fechaResolucion = fecha_resolucion,
                    alertaId
                });

            return await ObtenerAlertaPorIdAsync(alertaId)
                ?? throw new InvalidOperationException("Error recuperando alerta");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error actualizando alerta: {ex.Message}");
        }
    }

    public async Task<AlertaDetalladoResponse> ResolverAlertaAsync(int alertaId, string observaciones, int usuarioId)
    {
        var request = new ActualizarAlertaRequest
        {
            Estado = "Resuelta",
            Observaciones = observaciones,
            Marcar_Como_Resuelta = true
        };

        return await ActualizarEstadoAlertaAsync(alertaId, request);
    }

    public async Task<ResumenAlertasResponse> ObtenerEstadisticasAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var stats = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN estado = 'Pendiente' THEN 1 ELSE 0 END) as pendientes,
                    SUM(CASE WHEN estado = 'Asignada' THEN 1 ELSE 0 END) as asignadas,
                    SUM(CASE WHEN estado = 'En_Proceso' THEN 1 ELSE 0 END) as en_proceso,
                    SUM(CASE WHEN estado = 'Resuelta' THEN 1 ELSE 0 END) as resueltas,
                    SUM(CASE WHEN estado = 'Vencida' THEN 1 ELSE 0 END) as vencidas,
                    SUM(CASE WHEN estado = 'Ignorada' THEN 1 ELSE 0 END) as ignoradas,
                    SUM(CASE WHEN severidad = 'Critica' THEN 1 ELSE 0 END) as criticas,
                    SUM(CASE WHEN severidad = 'Alta' THEN 1 ELSE 0 END) as altas,
                    SUM(CASE WHEN severidad = 'Media' THEN 1 ELSE 0 END) as medias,
                    SUM(CASE WHEN severidad = 'Baja' THEN 1 ELSE 0 END) as bajas,
                    SUM(CASE WHEN tipo = 'Mantenimiento' THEN 1 ELSE 0 END) as mantenimiento,
                    SUM(CASE WHEN tipo = 'Depreciacion' THEN 1 ELSE 0 END) as depreciacion,
                    SUM(CASE WHEN tipo = 'Vencimiento' THEN 1 ELSE 0 END) as vencimiento,
                    SUM(CASE WHEN tipo = 'Garantia' THEN 1 ELSE 0 END) as garantia,
                    COALESCE(AVG(EXTRACT(DAY FROM fecha_resolucion - fecha_alerta)), 0) as tiempo_promedio,
                    COUNT(DISTINCT activo_id) as activos_con_alertas
                  FROM alertas");

            return new ResumenAlertasResponse
            {
                Total_Alertas = stats.total,
                Pendientes = stats.pendientes ?? 0,
                Asignadas = stats.asignadas ?? 0,
                En_Proceso = stats.en_proceso ?? 0,
                Resueltas = stats.resueltas ?? 0,
                Vencidas = stats.vencidas ?? 0,
                Ignoradas = stats.ignoradas ?? 0,
                Alertas_Criticas = stats.criticas ?? 0,
                Alertas_Altas = stats.altas ?? 0,
                Alertas_Medias = stats.medias ?? 0,
                Alertas_Bajas = stats.bajas ?? 0,
                Mantenimiento_Necesario = stats.mantenimiento ?? 0,
                Deprecio_Proximo = stats.depreciacion ?? 0,
                Garantia_Vencida = stats.garantia ?? 0,
                Vencimiento_Proximo = stats.vencimiento ?? 0,
                Tiempo_Promedio_Resolucion_Dias = stats.tiempo_promedio ?? 0,
                Activos_Con_Alertas = stats.activos_con_alertas ?? 0
            };
        }
        catch
        {
            return new ResumenAlertasResponse();
        }
    }

    public async Task<List<AlertaResponse>> ObtenerAlertasProximasAVencerAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var alertas = await connection.QueryAsync<AlertaResponse>(
                @"SELECT a.id, a.activo_id, a.tipo, a.descripcion, a.severidad, a.estado,
                    a.fecha_alerta, a.fecha_vencimiento, a.prioridad,
                    act.nombre as nombre_activo,
                    CAST(EXTRACT(DAY FROM a.fecha_vencimiento - NOW()) AS INTEGER) as dias_para_vencer
                  FROM alertas a
                  LEFT JOIN activos act ON act.id = a.activo_id
                  WHERE a.fecha_vencimiento BETWEEN NOW() AND NOW() + INTERVAL '2 days'
                  AND a.estado != 'Resuelta'
                  ORDER BY a.fecha_vencimiento ASC");

            return alertas.ToList();
        }
        catch
        {
            return new List<AlertaResponse>();
        }
    }

    public async Task<AlertaDetalladoResponse> AsignarAlertaAsync(int alertaId, int usuarioAsignadoId)
    {
        var request = new ActualizarAlertaRequest
        {
            Usuario_Asignado_Id = usuarioAsignadoId,
            Estado = "Asignada"
        };

        return await ActualizarEstadoAlertaAsync(alertaId, request);
    }

    public async Task<int> LimpiarAlertasAntiguasAsync(int diasAntiguos = 90)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var affected = await connection.ExecuteAsync(
                @"DELETE FROM alertas 
                  WHERE estado = 'Resuelta' 
                  AND fecha_resolucion < NOW() - (@dias || ' days')::INTERVAL",
                new { dias = diasAntiguos });

            return affected;
        }
        catch
        {
            return 0;
        }
    }
}
