namespace EventConnect.Domain.DTOs;

/// <summary>
/// Métricas generales del dashboard
/// </summary>
public class DashboardMetricasResponse
{
    // Métricas de ingresos
    public decimal Ingresos_Totales { get; set; }
    public decimal Ingresos_Este_Mes { get; set; }
    public decimal Ingresos_Mes_Anterior { get; set; }
    public decimal Porcentaje_Cambio_Ingresos { get; set; }

    // Métricas de reservas
    public int Total_Reservas { get; set; }
    public int Reservas_Activas { get; set; }
    public int Reservas_Este_Mes { get; set; }
    public int Reservas_Mes_Anterior { get; set; }
    public decimal Porcentaje_Cambio_Reservas { get; set; }

    // Métricas de clientes
    public int Total_Clientes { get; set; }
    public int Clientes_Nuevos_Este_Mes { get; set; }
    public int Clientes_Activos { get; set; }

    // Métricas de activos
    public int Total_Activos { get; set; }
    public int Activos_Disponibles { get; set; }
    public int Activos_En_Uso { get; set; }
    public int Activos_Mantenimiento { get; set; }
    public decimal Tasa_Ocupacion_Promedio { get; set; }

    // Alertas y problemas
    public int Alertas_Pendientes { get; set; }
    public int Alertas_Criticas { get; set; }
    public int Danios_Sin_Resolver { get; set; }

    // Métricas financieras
    public decimal Saldo_Por_Cobrar { get; set; }
    public decimal Ticket_Promedio { get; set; }
}

/// <summary>
/// Reporte de rentabilidad por período
/// </summary>
public class ReporteRentabilidadResponse
{
    public DateTime Fecha_Inicio { get; set; }
    public DateTime Fecha_Fin { get; set; }
    public decimal Ingresos_Totales { get; set; }
    public decimal Ingresos_Confirmados { get; set; }
    public decimal Ingresos_Pendientes { get; set; }
    public int Total_Reservas { get; set; }
    public int Reservas_Completadas { get; set; }
    public decimal Ticket_Promedio { get; set; }
    public List<RentabilidadPorCategoriaResponse> Por_Categoria { get; set; } = new();
    public List<RentabilidadMensualResponse> Por_Mes { get; set; } = new();
}

/// <summary>
/// Rentabilidad por categoría de activo
/// </summary>
public class RentabilidadPorCategoriaResponse
{
    public int Categoria_Id { get; set; }
    public string Nombre_Categoria { get; set; } = null!;
    public decimal Ingresos { get; set; }
    public int Cantidad_Reservas { get; set; }
    public decimal Porcentaje_Total { get; set; }
}

/// <summary>
/// Rentabilidad mensual
/// </summary>
public class RentabilidadMensualResponse
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string Nombre_Mes { get; set; } = null!;
    public decimal Ingresos { get; set; }
    public int Cantidad_Reservas { get; set; }
    public decimal Ticket_Promedio { get; set; }
}

/// <summary>
/// Comportamiento de clientes
/// </summary>
public class ComportamientoClienteResponse
{
    public int Cliente_Id { get; set; }
    public string Nombre_Cliente { get; set; } = null!;
    public int Total_Reservas { get; set; }
    public decimal Total_Gastado { get; set; }
    public decimal Ticket_Promedio { get; set; }
    public DateTime Ultima_Reserva { get; set; }
    public DateTime Fecha_Registro { get; set; }
    public int Dias_Cliente { get; set; }
    public string Segmento { get; set; } = null!; // VIP, Frecuente, Ocasional, Nuevo
}

/// <summary>
/// Top clientes por diferentes métricas
/// </summary>
public class TopClientesResponse
{
    public List<ComportamientoClienteResponse> Por_Ingresos { get; set; } = new();
    public List<ComportamientoClienteResponse> Por_Frecuencia { get; set; } = new();
    public List<ComportamientoClienteResponse> Clientes_Nuevos { get; set; } = new();
}

/// <summary>
/// Activos más rentados
/// </summary>
public class ActivoRentadoResponse
{
    public int Activo_Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Categoria { get; set; }
    public int Veces_Rentado { get; set; }
    public decimal Ingresos_Generados { get; set; }
    public decimal Tasa_Ocupacion { get; set; }
    public int Dias_Total_Renta { get; set; }
}

/// <summary>
/// Tendencias temporales
/// </summary>
public class TendenciasResponse
{
    public List<TendenciaDiariaResponse> Tendencia_Diaria { get; set; } = new();
    public List<TendenciaSemanalResponse> Tendencia_Semanal { get; set; } = new();
    public List<TendenciaMensualResponse> Tendencia_Mensual { get; set; } = new();
}

/// <summary>
/// Tendencia diaria (últimos 30 días)
/// </summary>
public class TendenciaDiariaResponse
{
    public DateTime Fecha { get; set; }
    public int Reservas { get; set; }
    public decimal Ingresos { get; set; }
    public int Nuevos_Clientes { get; set; }
}

/// <summary>
/// Tendencia semanal (últimas 12 semanas)
/// </summary>
public class TendenciaSemanalResponse
{
    public int Año { get; set; }
    public int Semana { get; set; }
    public DateTime Fecha_Inicio_Semana { get; set; }
    public int Reservas { get; set; }
    public decimal Ingresos { get; set; }
}

/// <summary>
/// Tendencia mensual (últimos 12 meses)
/// </summary>
public class TendenciaMensualResponse
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string Nombre_Mes { get; set; } = null!;
    public int Reservas { get; set; }
    public decimal Ingresos { get; set; }
    public int Nuevos_Clientes { get; set; }
}

/// <summary>
/// KPIs (Key Performance Indicators)
/// </summary>
public class KPIsResponse
{
    // KPIs operacionales
    public decimal Tasa_Conversion_Cotizacion { get; set; } // % cotizaciones → reservas
    public decimal Tasa_Completitud_Reservas { get; set; } // % reservas completadas
    public decimal Tasa_Cancelacion { get; set; }
    public decimal Tiempo_Promedio_Entrega_Dias { get; set; }
    public decimal Tiempo_Promedio_Resolucion_Danios_Dias { get; set; }

    // KPIs financieros
    public decimal Margen_Promedio { get; set; }
    public decimal Revenue_Per_Cliente { get; set; }
    public decimal Crecimiento_Mensual_Ingresos { get; set; }

    // KPIs de satisfacción
    public decimal Tasa_Retencion_Clientes { get; set; }
    public decimal Clientes_Recurrentes_Porcentaje { get; set; }

    // KPIs de inventario
    public decimal Utilizacion_Promedio_Activos { get; set; }
    public decimal ROI_Activos { get; set; }
    public int Activos_Con_Alertas_Porcentaje { get; set; }
}

/// <summary>
/// Estadísticas de estados de reservas
/// </summary>
public class EstadisticasEstadosResponse
{
    public int Pendientes { get; set; }
    public int Confirmadas { get; set; }
    public int En_Proceso { get; set; }
    public int Completadas { get; set; }
    public int Canceladas { get; set; }
    public Dictionary<string, int> Por_Estado { get; set; } = new();
}

/// <summary>
/// Distribución geográfica de clientes
/// </summary>
public class DistribucionGeograficaResponse
{
    public string Ciudad { get; set; } = null!;
    public int Cantidad_Clientes { get; set; }
    public int Cantidad_Reservas { get; set; }
    public decimal Ingresos_Totales { get; set; }
}

/// <summary>
/// Análisis de productos/servicios
/// </summary>
public class AnalisisProductosResponse
{
    public int Producto_Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Cantidad_Vendida { get; set; }
    public decimal Ingresos { get; set; }
    public decimal Margen { get; set; }
}

/// <summary>
/// Reporte completo del dashboard
/// </summary>
public class DashboardCompletoResponse
{
    public DashboardMetricasResponse Metricas { get; set; } = new();
    public KPIsResponse KPIs { get; set; } = new();
    public List<ActivoRentadoResponse> Top_Activos { get; set; } = new();
    public TopClientesResponse Top_Clientes { get; set; } = new();
    public EstadisticasEstadosResponse Estados_Reservas { get; set; } = new();
    public DateTime Fecha_Generacion { get; set; } = DateTime.UtcNow;
}
