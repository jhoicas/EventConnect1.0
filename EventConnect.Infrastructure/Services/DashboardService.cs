using System.Data;
using System.Globalization;
using Dapper;
using Npgsql;
using EventConnect.Domain.DTOs;
using EventConnect.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace EventConnect.Infrastructure.Services;

/// <summary>
/// Servicio de Dashboard y Reportes (Analítica y BI)
/// Proporciona métricas, KPIs y análisis del negocio
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly string _connectionString;
    private static readonly CultureInfo CultureEs = new CultureInfo("es-ES");

    public DashboardService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<DashboardMetricasResponse> ObtenerMetricasGeneralesAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var metricas = new DashboardMetricasResponse();

            // Métricas de ingresos
            var ingresos = await connection.QueryFirstAsync(
                @"SELECT 
                    COALESCE(SUM(monto_pagado), 0) as total,
                    COALESCE(SUM(CASE WHEN DATE_TRUNC('month', fecha_reserva) = DATE_TRUNC('month', NOW()) THEN monto_pagado ELSE 0 END), 0) as este_mes,
                    COALESCE(SUM(CASE WHEN DATE_TRUNC('month', fecha_reserva) = DATE_TRUNC('month', NOW() - INTERVAL '1 month') THEN monto_pagado ELSE 0 END), 0) as mes_anterior
                  FROM reservas WHERE estado != 'Cancelada'");

            metricas.Ingresos_Totales = ingresos.total;
            metricas.Ingresos_Este_Mes = ingresos.este_mes;
            metricas.Ingresos_Mes_Anterior = ingresos.mes_anterior;
            metricas.Porcentaje_Cambio_Ingresos = ingresos.mes_anterior > 0 
                ? ((ingresos.este_mes - ingresos.mes_anterior) / ingresos.mes_anterior) * 100 
                : 0;

            // Métricas de reservas
            var reservas = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN estado IN ('Pendiente', 'Confirmada', 'En_Proceso') THEN 1 ELSE 0 END) as activas,
                    SUM(CASE WHEN DATE_TRUNC('month', fecha_reserva) = DATE_TRUNC('month', NOW()) THEN 1 ELSE 0 END) as este_mes,
                    SUM(CASE WHEN DATE_TRUNC('month', fecha_reserva) = DATE_TRUNC('month', NOW() - INTERVAL '1 month') THEN 1 ELSE 0 END) as mes_anterior
                  FROM reservas WHERE estado != 'Cancelada'");

            metricas.Total_Reservas = reservas.total;
            metricas.Reservas_Activas = reservas.activas ?? 0;
            metricas.Reservas_Este_Mes = reservas.este_mes ?? 0;
            metricas.Reservas_Mes_Anterior = reservas.mes_anterior ?? 0;
            metricas.Porcentaje_Cambio_Reservas = reservas.mes_anterior > 0 
                ? ((decimal)(reservas.este_mes - reservas.mes_anterior) / reservas.mes_anterior) * 100 
                : 0;

            // Métricas de clientes
            var clientes = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN DATE_TRUNC('month', fecha_creacion) = DATE_TRUNC('month', NOW()) THEN 1 ELSE 0 END) as nuevos_este_mes,
                    COUNT(DISTINCT CASE WHEN EXISTS(SELECT 1 FROM reservas WHERE reservas.cliente_id = clientes.id AND estado IN ('Pendiente', 'Confirmada', 'En_Proceso')) THEN id END) as activos
                  FROM clientes");

            metricas.Total_Clientes = clientes.total;
            metricas.Clientes_Nuevos_Este_Mes = clientes.nuevos_este_mes ?? 0;
            metricas.Clientes_Activos = clientes.activos ?? 0;

            // Métricas de activos
            var activos = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN estado = 'Disponible' THEN 1 ELSE 0 END) as disponibles,
                    SUM(CASE WHEN estado = 'En_Uso' THEN 1 ELSE 0 END) as en_uso,
                    SUM(CASE WHEN estado IN ('Mantenimiento', 'En_Mantenimiento') THEN 1 ELSE 0 END) as mantenimiento
                  FROM activos");

            metricas.Total_Activos = activos.total;
            metricas.Activos_Disponibles = activos.disponibles ?? 0;
            metricas.Activos_En_Uso = activos.en_uso ?? 0;
            metricas.Activos_Mantenimiento = activos.mantenimiento ?? 0;
            metricas.Tasa_Ocupacion_Promedio = activos.total > 0 
                ? ((decimal)(activos.en_uso ?? 0) / activos.total) * 100 
                : 0;

            // Alertas y problemas
            var alertas = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(CASE WHEN estado = 'Pendiente' THEN 1 END) as pendientes,
                    COUNT(CASE WHEN severidad = 'Critica' AND estado != 'Resuelta' THEN 1 END) as criticas
                  FROM alertas");

            var danios = await connection.QueryFirstAsync<int>(
                @"SELECT COUNT(*) FROM danios WHERE estado NOT IN ('Reparado', 'Rechazado')");

            metricas.Alertas_Pendientes = alertas.pendientes ?? 0;
            metricas.Alertas_Criticas = alertas.criticas ?? 0;
            metricas.Danios_Sin_Resolver = danios;

            // Métricas financieras
            var financieras = await connection.QueryFirstAsync(
                @"SELECT 
                    COALESCE(SUM(monto_total - COALESCE(monto_pagado, 0)), 0) as saldo_cobrar,
                    COALESCE(AVG(monto_total), 0) as ticket_promedio
                  FROM reservas WHERE estado != 'Cancelada'");

            metricas.Saldo_Por_Cobrar = financieras.saldo_cobrar;
            metricas.Ticket_Promedio = financieras.ticket_promedio;

            return metricas;
        }
        catch
        {
            return new DashboardMetricasResponse();
        }
    }

    public async Task<DashboardCompletoResponse> ObtenerDashboardCompletoAsync()
    {
        var dashboard = new DashboardCompletoResponse
        {
            Metricas = await ObtenerMetricasGeneralesAsync(),
            KPIs = await ObtenerKPIsAsync(),
            Top_Activos = await ObtenerActivosMasRentadosAsync(5),
            Top_Clientes = await ObtenerTopClientesAsync(5),
            Estados_Reservas = await ObtenerEstadisticasEstadosAsync(),
            Fecha_Generacion = DateTime.UtcNow
        };

        return dashboard;
    }

    public async Task<ReporteRentabilidadResponse> GenerarReporteRentabilidadAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var reporte = new ReporteRentabilidadResponse
            {
                Fecha_Inicio = fechaInicio,
                Fecha_Fin = fechaFin
            };

            // Resumen general
            var general = await connection.QueryFirstAsync(
                @"SELECT 
                    COALESCE(SUM(monto_total), 0) as total,
                    COALESCE(SUM(CASE WHEN estado = 'Completada' THEN monto_total ELSE 0 END), 0) as confirmados,
                    COALESCE(SUM(CASE WHEN estado IN ('Pendiente', 'Confirmada') THEN monto_total ELSE 0 END), 0) as pendientes,
                    COUNT(*) as total_reservas,
                    SUM(CASE WHEN estado = 'Completada' THEN 1 ELSE 0 END) as completadas,
                    AVG(monto_total) as ticket_promedio
                  FROM reservas 
                  WHERE fecha_reserva BETWEEN @fechaInicio AND @fechaFin 
                    AND estado != 'Cancelada'",
                new { fechaInicio, fechaFin });

            reporte.Ingresos_Totales = general.total;
            reporte.Ingresos_Confirmados = general.confirmados;
            reporte.Ingresos_Pendientes = general.pendientes;
            reporte.Total_Reservas = general.total_reservas;
            reporte.Reservas_Completadas = general.completadas ?? 0;
            reporte.Ticket_Promedio = general.ticket_promedio ?? 0;

            // Por categoría
            var porCategoria = await connection.QueryAsync<RentabilidadPorCategoriaResponse>(
                @"SELECT 
                    c.id as categoria_id, c.nombre as nombre_categoria,
                    COALESCE(SUM(dr.cantidad * dr.precio_unitario), 0) as ingresos,
                    COUNT(DISTINCT r.id) as cantidad_reservas
                  FROM categorias c
                  LEFT JOIN activos a ON a.categoria_id = c.id
                  LEFT JOIN detalle_reserva dr ON dr.activo_id = a.id
                  LEFT JOIN reservas r ON r.id = dr.reserva_id AND r.fecha_reserva BETWEEN @fechaInicio AND @fechaFin AND r.estado != 'Cancelada'
                  GROUP BY c.id, c.nombre
                  HAVING COALESCE(SUM(dr.cantidad * dr.precio_unitario), 0) > 0
                  ORDER BY ingresos DESC",
                new { fechaInicio, fechaFin });

            var totalIngresos = porCategoria.Sum(c => c.Ingresos);
            foreach (var categoria in porCategoria)
            {
                categoria.Porcentaje_Total = totalIngresos > 0 ? (categoria.Ingresos / totalIngresos) * 100 : 0;
            }
            reporte.Por_Categoria = porCategoria.ToList();

            // Por mes
            var porMes = await connection.QueryAsync(
                @"SELECT 
                    EXTRACT(YEAR FROM fecha_reserva)::INTEGER as año,
                    EXTRACT(MONTH FROM fecha_reserva)::INTEGER as mes,
                    COALESCE(SUM(monto_total), 0) as ingresos,
                    COUNT(*) as cantidad_reservas,
                    AVG(monto_total) as ticket_promedio
                  FROM reservas 
                  WHERE fecha_reserva BETWEEN @fechaInicio AND @fechaFin AND estado != 'Cancelada'
                  GROUP BY EXTRACT(YEAR FROM fecha_reserva), EXTRACT(MONTH FROM fecha_reserva)
                  ORDER BY año, mes",
                new { fechaInicio, fechaFin });

            reporte.Por_Mes = porMes.Select(m => new RentabilidadMensualResponse
            {
                Año = m.año,
                Mes = m.mes,
                Nombre_Mes = new DateTime(m.año, m.mes, 1).ToString("MMMM", CultureEs),
                Ingresos = m.ingresos,
                Cantidad_Reservas = m.cantidad_reservas,
                Ticket_Promedio = m.ticket_promedio ?? 0
            }).ToList();

            return reporte;
        }
        catch
        {
            return new ReporteRentabilidadResponse 
            { 
                Fecha_Inicio = fechaInicio, 
                Fecha_Fin = fechaFin 
            };
        }
    }

    public async Task<TendenciasResponse> ObtenerTendenciasAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var tendencias = new TendenciasResponse();

            // Tendencia diaria (últimos 30 días)
            var diaria = await connection.QueryAsync(
                @"SELECT 
                    DATE(fecha_reserva) as fecha,
                    COUNT(*) as reservas,
                    COALESCE(SUM(monto_total), 0) as ingresos,
                    COUNT(DISTINCT cliente_id) as nuevos_clientes
                  FROM reservas 
                  WHERE fecha_reserva >= NOW() - INTERVAL '30 days' AND estado != 'Cancelada'
                  GROUP BY DATE(fecha_reserva)
                  ORDER BY fecha DESC");

            tendencias.Tendencia_Diaria = diaria.Select(d => new TendenciaDiariaResponse
            {
                Fecha = d.fecha,
                Reservas = d.reservas,
                Ingresos = d.ingresos,
                Nuevos_Clientes = d.nuevos_clientes
            }).ToList();

            // Tendencia mensual (últimos 12 meses)
            var mensual = await connection.QueryAsync(
                @"SELECT 
                    EXTRACT(YEAR FROM fecha_reserva)::INTEGER as año,
                    EXTRACT(MONTH FROM fecha_reserva)::INTEGER as mes,
                    COUNT(*) as reservas,
                    COALESCE(SUM(monto_total), 0) as ingresos
                  FROM reservas 
                  WHERE fecha_reserva >= NOW() - INTERVAL '12 months' AND estado != 'Cancelada'
                  GROUP BY EXTRACT(YEAR FROM fecha_reserva), EXTRACT(MONTH FROM fecha_reserva)
                  ORDER BY año DESC, mes DESC");

            tendencias.Tendencia_Mensual = mensual.Select(m => new TendenciaMensualResponse
            {
                Año = m.año,
                Mes = m.mes,
                Nombre_Mes = new DateTime(m.año, m.mes, 1).ToString("MMMM yyyy", CultureEs),
                Reservas = m.reservas,
                Ingresos = m.ingresos,
                Nuevos_Clientes = 0 // TODO: calcular desde clientes
            }).ToList();

            return tendencias;
        }
        catch
        {
            return new TendenciasResponse();
        }
    }

    public async Task<KPIsResponse> ObtenerKPIsAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var kpis = new KPIsResponse();

            // Tasa de conversión cotización → reserva
            var conversion = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(CASE WHEN sc.estado = 'Convertida' THEN 1 END)::DECIMAL as convertidas,
                    COUNT(*)::DECIMAL as total
                  FROM solicitud_cotizacion sc");

            kpis.Tasa_Conversion_Cotizacion = conversion.total > 0 
                ? (conversion.convertidas / conversion.total) * 100 
                : 0;

            // Tasas de reservas
            var tasas = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(*)::DECIMAL as total,
                    COUNT(CASE WHEN estado = 'Completada' THEN 1 END)::DECIMAL as completadas,
                    COUNT(CASE WHEN estado = 'Cancelada' THEN 1 END)::DECIMAL as canceladas
                  FROM reservas");

            kpis.Tasa_Completitud_Reservas = tasas.total > 0 
                ? (tasas.completadas / tasas.total) * 100 
                : 0;
            kpis.Tasa_Cancelacion = tasas.total > 0 
                ? (tasas.canceladas / tasas.total) * 100 
                : 0;

            // Tiempos promedio
            var tiempos = await connection.QueryFirstOrDefaultAsync(
                @"SELECT 
                    COALESCE(AVG(EXTRACT(DAY FROM fecha_entrega_real - fecha_entrega_programada)), 0) as entrega,
                    COALESCE(AVG(EXTRACT(DAY FROM d.fecha_resolucion - d.fecha_reporte)), 0) as danios
                  FROM logistica l
                  LEFT JOIN danios d ON 1=1");

            kpis.Tiempo_Promedio_Entrega_Dias = tiempos?.entrega ?? 0;
            kpis.Tiempo_Promedio_Resolucion_Danios_Dias = tiempos?.danios ?? 0;

            // KPIs financieros
            var financieros = await connection.QueryFirstAsync(
                @"SELECT 
                    COALESCE(AVG(monto_total), 0) / NULLIF(COUNT(DISTINCT cliente_id), 0) as revenue_per_cliente,
                    COUNT(DISTINCT cliente_id) as total_clientes
                  FROM reservas WHERE estado != 'Cancelada'");

            kpis.Revenue_Per_Cliente = financieros.revenue_per_cliente ?? 0;

            // Utilización de activos
            var utilizacion = await connection.QueryFirstAsync(
                @"SELECT 
                    COUNT(CASE WHEN estado = 'En_Uso' THEN 1 END)::DECIMAL as en_uso,
                    COUNT(*)::DECIMAL as total
                  FROM activos");

            kpis.Utilizacion_Promedio_Activos = utilizacion.total > 0 
                ? (utilizacion.en_uso / utilizacion.total) * 100 
                : 0;

            return kpis;
        }
        catch
        {
            return new KPIsResponse();
        }
    }

    public async Task<List<ActivoRentadoResponse>> ObtenerActivosMasRentadosAsync(int top = 10)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var activos = await connection.QueryAsync<ActivoRentadoResponse>(
                @"SELECT 
                    a.id as activo_id, a.nombre, c.nombre as categoria,
                    COUNT(DISTINCT r.id) as veces_rentado,
                    COALESCE(SUM(dr.cantidad * dr.precio_unitario), 0) as ingresos_generados,
                    COALESCE(SUM(EXTRACT(DAY FROM r.fecha_fin - r.fecha_inicio)), 0) as dias_total_renta
                  FROM activos a
                  LEFT JOIN categorias c ON c.id = a.categoria_id
                  LEFT JOIN detalle_reserva dr ON dr.activo_id = a.id
                  LEFT JOIN reservas r ON r.id = dr.reserva_id AND r.estado != 'Cancelada'
                  GROUP BY a.id, a.nombre, c.nombre
                  ORDER BY veces_rentado DESC, ingresos_generados DESC
                  LIMIT @top",
                new { top });

            return activos.ToList();
        }
        catch
        {
            return new List<ActivoRentadoResponse>();
        }
    }

    public async Task<TopClientesResponse> ObtenerTopClientesAsync(int top = 10)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var topClientes = new TopClientesResponse();

            // Por ingresos
            var porIngresos = await connection.QueryAsync(
                @"SELECT 
                    c.id as cliente_id, 
                    COALESCE(c.empresa_id, c.id)::TEXT as nombre_cliente,
                    COUNT(r.id) as total_reservas,
                    COALESCE(SUM(r.monto_pagado), 0) as total_gastado,
                    COALESCE(AVG(r.monto_total), 0) as ticket_promedio,
                    MAX(r.fecha_reserva) as ultima_reserva,
                    c.fecha_creacion as fecha_registro,
                    EXTRACT(DAY FROM NOW() - c.fecha_creacion)::INTEGER as dias_cliente,
                    'VIP' as segmento
                  FROM clientes c
                  LEFT JOIN reservas r ON r.cliente_id = c.id AND r.estado != 'Cancelada'
                  GROUP BY c.id, c.empresa_id, c.fecha_creacion
                  ORDER BY total_gastado DESC
                  LIMIT @top",
                new { top });

            topClientes.Por_Ingresos = porIngresos.Select(c => new ComportamientoClienteResponse
            {
                Cliente_Id = c.cliente_id,
                Nombre_Cliente = c.nombre_cliente,
                Total_Reservas = c.total_reservas,
                Total_Gastado = c.total_gastado,
                Ticket_Promedio = c.ticket_promedio,
                Ultima_Reserva = c.ultima_reserva,
                Fecha_Registro = c.fecha_registro,
                Dias_Cliente = c.dias_cliente,
                Segmento = c.segmento
            }).ToList();

            // Por frecuencia
            topClientes.Por_Frecuencia = topClientes.Por_Ingresos
                .OrderByDescending(c => c.Total_Reservas)
                .Take(top)
                .ToList();

            return topClientes;
        }
        catch
        {
            return new TopClientesResponse();
        }
    }

    public async Task<EstadisticasEstadosResponse> ObtenerEstadisticasEstadosAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var estados = await connection.QueryAsync(
                @"SELECT estado, COUNT(*) as cantidad 
                  FROM reservas 
                  GROUP BY estado");

            var stats = new EstadisticasEstadosResponse();
            foreach (var estado in estados)
            {
                stats.Por_Estado[estado.estado] = estado.cantidad;

                switch (estado.estado)
                {
                    case "Pendiente": stats.Pendientes = estado.cantidad; break;
                    case "Confirmada": stats.Confirmadas = estado.cantidad; break;
                    case "En_Proceso": stats.En_Proceso = estado.cantidad; break;
                    case "Completada": stats.Completadas = estado.cantidad; break;
                    case "Cancelada": stats.Canceladas = estado.cantidad; break;
                }
            }

            return stats;
        }
        catch
        {
            return new EstadisticasEstadosResponse();
        }
    }

    public async Task<List<DistribucionGeograficaResponse>> ObtenerDistribucionGeograficaAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var distribucion = await connection.QueryAsync<DistribucionGeograficaResponse>(
                @"SELECT 
                    COALESCE(c.ciudad, 'Sin especificar') as ciudad,
                    COUNT(DISTINCT c.id) as cantidad_clientes,
                    COUNT(r.id) as cantidad_reservas,
                    COALESCE(SUM(r.monto_total), 0) as ingresos_totales
                  FROM clientes c
                  LEFT JOIN reservas r ON r.cliente_id = c.id AND r.estado != 'Cancelada'
                  GROUP BY c.ciudad
                  ORDER BY ingresos_totales DESC");

            return distribucion.ToList();
        }
        catch
        {
            return new List<DistribucionGeograficaResponse>();
        }
    }

    public async Task<List<ComportamientoClienteResponse>> ObtenerComportamientoClientesAsync(string? segmento = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var comportamiento = await connection.QueryAsync(
                @"SELECT 
                    c.id as cliente_id,
                    COALESCE(c.empresa_id, c.id)::TEXT as nombre_cliente,
                    COUNT(r.id) as total_reservas,
                    COALESCE(SUM(r.monto_pagado), 0) as total_gastado,
                    COALESCE(AVG(r.monto_total), 0) as ticket_promedio,
                    MAX(r.fecha_reserva) as ultima_reserva,
                    c.fecha_creacion as fecha_registro,
                    EXTRACT(DAY FROM NOW() - c.fecha_creacion)::INTEGER as dias_cliente,
                    CASE 
                        WHEN COALESCE(SUM(r.monto_pagado), 0) > 10000 THEN 'VIP'
                        WHEN COUNT(r.id) >= 5 THEN 'Frecuente'
                        WHEN COUNT(r.id) > 0 THEN 'Ocasional'
                        ELSE 'Nuevo'
                    END as segmento
                  FROM clientes c
                  LEFT JOIN reservas r ON r.cliente_id = c.id AND r.estado != 'Cancelada'
                  GROUP BY c.id, c.empresa_id, c.fecha_creacion
                  ORDER BY total_gastado DESC");

            var resultado = comportamiento.Select(c => new ComportamientoClienteResponse
            {
                Cliente_Id = c.cliente_id,
                Nombre_Cliente = c.nombre_cliente,
                Total_Reservas = c.total_reservas,
                Total_Gastado = c.total_gastado,
                Ticket_Promedio = c.ticket_promedio,
                Ultima_Reserva = c.ultima_reserva,
                Fecha_Registro = c.fecha_registro,
                Dias_Cliente = c.dias_cliente,
                Segmento = c.segmento
            }).ToList();

            if (!string.IsNullOrEmpty(segmento))
            {
                resultado = resultado.Where(c => c.Segmento == segmento).ToList();
            }

            return resultado;
        }
        catch
        {
            return new List<ComportamientoClienteResponse>();
        }
    }

    public async Task<List<RentabilidadPorCategoriaResponse>> ObtenerRentabilidadPorCategoriaAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var rentabilidad = await connection.QueryAsync<RentabilidadPorCategoriaResponse>(
                @"SELECT 
                    c.id as categoria_id, c.nombre as nombre_categoria,
                    COALESCE(SUM(dr.cantidad * dr.precio_unitario), 0) as ingresos,
                    COUNT(DISTINCT r.id) as cantidad_reservas
                  FROM categorias c
                  LEFT JOIN activos a ON a.categoria_id = c.id
                  LEFT JOIN detalle_reserva dr ON dr.activo_id = a.id
                  LEFT JOIN reservas r ON r.id = dr.reserva_id AND r.estado != 'Cancelada'
                  GROUP BY c.id, c.nombre
                  HAVING COALESCE(SUM(dr.cantidad * dr.precio_unitario), 0) > 0
                  ORDER BY ingresos DESC");

            var total = rentabilidad.Sum(r => r.Ingresos);
            foreach (var item in rentabilidad)
            {
                item.Porcentaje_Total = total > 0 ? (item.Ingresos / total) * 100 : 0;
            }

            return rentabilidad.ToList();
        }
        catch
        {
            return new List<RentabilidadPorCategoriaResponse>();
        }
    }
}
