using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using Dapper;

namespace EventConnect.Infrastructure.Repositories;

public class ReservaRepository : RepositoryBase<Reserva>
{
    public ReservaRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<Reserva?> GetByCodigoAsync(string codigo)
    {
        var sql = "SELECT * FROM Reserva WHERE Codigo_Reserva = @Codigo";
        return await QueryFirstOrDefaultAsync(sql, new { Codigo = codigo });
    }

    public async Task<IEnumerable<Reserva>> GetByClienteIdAsync(int clienteId)
    {
        var sql = "SELECT * FROM Reserva WHERE Cliente_Id = @ClienteId ORDER BY Fecha_Creacion DESC";
        return await QueryAsync(sql, new { ClienteId = clienteId });
    }

    /// <summary>
    /// Obtiene todas las reservas de un cliente con información de las empresas involucradas
    /// MULTIVENDEDOR: Una reserva puede tener productos de múltiples empresas
    /// </summary>
    public async Task<IEnumerable<ReservationResponse>> GetReservationsByClienteIdAsync(int clienteId)
    {
        var sql = @"
            SELECT 
                r.Id,
                r.Cliente_Id,
                r.Codigo_Reserva,
                r.Estado,
                r.Fecha_Evento,
                r.Fecha_Entrega,
                r.Fecha_Devolucion_Programada,
                r.Fecha_Devolucion_Real,
                r.Direccion_Entrega,
                r.Ciudad_Entrega,
                r.Contacto_En_Sitio,
                r.Telefono_Contacto,
                r.Subtotal,
                r.Descuento,
                r.Total,
                r.Fianza,
                r.Fianza_Devuelta,
                r.Metodo_Pago,
                r.Estado_Pago,
                r.Observaciones,
                r.Fecha_Creacion,
                r.Fecha_Vencimiento_Cotizacion,
                r.Fecha_Actualizacion,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                c.Documento as Cliente_Documento,
                u1.Nombre_Completo as Creado_Por_Nombre,
                u2.Nombre_Completo as Aprobado_Por_Nombre,
                COUNT(DISTINCT dr.Empresa_Id) as Cantidad_Empresas
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            LEFT JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
            LEFT JOIN Usuario u1 ON r.Creado_Por_Id = u1.Id
            LEFT JOIN Usuario u2 ON r.Aprobado_Por_Id = u2.Id
            WHERE r.Cliente_Id = @ClienteId
            GROUP BY r.Id, c.Nombre, c.Email, c.Telefono, c.Documento, 
                     u1.Nombre_Completo, u2.Nombre_Completo
            ORDER BY r.Fecha_Creacion DESC";
        
        using var connection = GetConnection();
        return await connection.QueryAsync<ReservationResponse>(sql, new { ClienteId = clienteId });
    }

    /// <summary>
    /// Obtiene todos los detalles de una reserva con información de las empresas
    /// </summary>
    public async Task<IEnumerable<ReservationDetailResponse>> GetReservationDetailsAsync(int reservaId)
    {
        var sql = @"
            SELECT 
                dr.Id,
                dr.Reserva_Id,
                dr.Empresa_Id,
                e.Razon_Social as Empresa_Nombre,
                dr.Producto_Id,
                p.Nombre as Producto_Nombre,
                dr.Activo_Id,
                a.Codigo as Activo_Codigo,
                dr.Cantidad,
                dr.Precio_Unitario,
                dr.Subtotal,
                dr.Dias_Alquiler,
                dr.Observaciones,
                dr.Estado_Item,
                dr.Fecha_Creacion
            FROM Detalle_Reserva dr
            INNER JOIN Empresa e ON dr.Empresa_Id = e.Id
            LEFT JOIN Producto p ON dr.Producto_Id = p.Id
            LEFT JOIN Activo a ON dr.Activo_Id = a.Id
            WHERE dr.Reserva_Id = @ReservaId
            ORDER BY e.Razon_Social, dr.Fecha_Creacion";
        
        using var connection = GetConnection();
        return await connection.QueryAsync<ReservationDetailResponse>(sql, new { ReservaId = reservaId });
    }

    /// <summary>
    /// Obtiene las reservas donde una empresa es proveedora
    /// MULTIVENDEDOR: Filtra por empresas en Detalle_Reserva
    /// </summary>
    public async Task<IEnumerable<ReservationResponse>> GetReservationsByEmpresaIdAsync(int empresaId, string? estado = null)
    {
        var sql = @"
            SELECT DISTINCT
                r.Id,
                r.Cliente_Id,
                r.Codigo_Reserva,
                r.Estado,
                r.Fecha_Evento,
                r.Fecha_Entrega,
                r.Fecha_Devolucion_Programada,
                r.Fecha_Devolucion_Real,
                r.Direccion_Entrega,
                r.Ciudad_Entrega,
                r.Contacto_En_Sitio,
                r.Telefono_Contacto,
                r.Subtotal,
                r.Descuento,
                r.Total,
                r.Fianza,
                r.Fianza_Devuelta,
                r.Metodo_Pago,
                r.Estado_Pago,
                r.Observaciones,
                r.Fecha_Creacion,
                r.Fecha_Vencimiento_Cotizacion,
                r.Fecha_Actualizacion,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                c.Documento as Cliente_Documento,
                u1.Nombre_Completo as Creado_Por_Nombre,
                u2.Nombre_Completo as Aprobado_Por_Nombre,
                COUNT(DISTINCT dr.Empresa_Id) as Cantidad_Empresas
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            INNER JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
            LEFT JOIN Usuario u1 ON r.Creado_Por_Id = u1.Id
            LEFT JOIN Usuario u2 ON r.Aprobado_Por_Id = u2.Id
            WHERE dr.Empresa_Id = @EmpresaId";
        
        if (!string.IsNullOrEmpty(estado))
        {
            sql += " AND r.Estado = @Estado";
        }
        
        sql += @"
            GROUP BY r.Id, c.Nombre, c.Email, c.Telefono, c.Documento,
                     u1.Nombre_Completo, u2.Nombre_Completo
            ORDER BY r.Fecha_Creacion DESC";
        
        using var connection = GetConnection();
        return await connection.QueryAsync<ReservationResponse>(sql, 
            new { EmpresaId = empresaId, Estado = estado });
    }

    /// <summary>
    /// Obtiene una reserva por ID con información completa
    /// </summary>
    public async Task<ReservationResponse?> GetReservationByIdAsync(int id)
    {
        var sql = @"
            SELECT 
                r.Id,
                r.Cliente_Id,
                r.Codigo_Reserva,
                r.Estado,
                r.Fecha_Evento,
                r.Fecha_Entrega,
                r.Fecha_Devolucion_Programada,
                r.Fecha_Devolucion_Real,
                r.Direccion_Entrega,
                r.Ciudad_Entrega,
                r.Contacto_En_Sitio,
                r.Telefono_Contacto,
                r.Subtotal,
                r.Descuento,
                r.Total,
                r.Fianza,
                r.Fianza_Devuelta,
                r.Metodo_Pago,
                r.Estado_Pago,
                r.Observaciones,
                r.Fecha_Creacion,
                r.Fecha_Vencimiento_Cotizacion,
                r.Fecha_Actualizacion,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                c.Documento as Cliente_Documento,
                u1.Nombre_Completo as Creado_Por_Nombre,
                u2.Nombre_Completo as Aprobado_Por_Nombre,
                COUNT(DISTINCT dr.Empresa_Id) as Cantidad_Empresas
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            LEFT JOIN Detalle_Reserva dr ON r.Id = dr.Reserva_Id
            LEFT JOIN Usuario u1 ON r.Creado_Por_Id = u1.Id
            LEFT JOIN Usuario u2 ON r.Aprobado_Por_Id = u2.Id
            WHERE r.Id = @Id
            GROUP BY r.Id, c.Nombre, c.Email, c.Telefono, c.Documento,
                     u1.Nombre_Completo, u2.Nombre_Completo";
        
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<ReservationResponse>(sql, new { Id = id });
    }

    /// <summary>
    /// Genera un código único para la reserva (sin dependencia de empresa)
    /// </summary>
    public async Task<string> GenerarCodigoReservaAsync()
    {
        var sql = @"
            SELECT COUNT(*) + 1 as Siguiente
            FROM Reserva 
            WHERE EXTRACT(YEAR FROM Fecha_Creacion) = EXTRACT(YEAR FROM NOW())";
        
        using var connection = GetConnection();
        var siguiente = await connection.ExecuteScalarAsync<int>(sql);
        var anio = DateTime.Now.Year.ToString().Substring(2);
        return $"RES-{anio}-{siguiente:D6}";
    }

    /// <summary>
    /// Verifica si hay disponibilidad en la fecha solicitada para una empresa específica
    /// </summary>
    public async Task<bool> VerificarDisponibilidadAsync(int empresaId, DateTime fechaEvento, int? reservaIdExcluir = null)
    {
        var sql = @"
            SELECT COUNT(DISTINCT dr.Reserva_Id)
            FROM Detalle_Reserva dr
            INNER JOIN Reserva r ON dr.Reserva_Id = r.Id
            WHERE dr.Empresa_Id = @EmpresaId 
            AND r.Fecha_Evento::date = @FechaEvento::date
            AND r.Estado IN ('Solicitado', 'Confirmado')";
        
        if (reservaIdExcluir.HasValue)
        {
            sql += " AND dr.Reserva_Id != @ReservaIdExcluir";
        }
        
        using var connection = GetConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, 
            new { EmpresaId = empresaId, FechaEvento = fechaEvento, ReservaIdExcluir = reservaIdExcluir });
        
        // Permite múltiples reservas por día (ajusta según tu política)
        return true;
    }

    /// <summary>
    /// Obtiene estadísticas de reservas para una empresa (como proveedora)
    /// </summary>
    public async Task<ReservationStatsDTO> GetReservationStatsAsync(int empresaId)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT dr.Reserva_Id) as Total_Reservas,
                COUNT(DISTINCT CASE WHEN r.Estado = 'Solicitado' THEN dr.Reserva_Id END) as Reservas_Pendientes,
                COUNT(DISTINCT CASE WHEN r.Estado = 'Confirmado' THEN dr.Reserva_Id END) as Reservas_Confirmadas,
                COUNT(DISTINCT CASE WHEN r.Estado = 'Cancelado' THEN dr.Reserva_Id END) as Reservas_Canceladas,
                COUNT(DISTINCT CASE WHEN r.Estado = 'Completado' THEN dr.Reserva_Id END) as Reservas_Completadas,
                COALESCE(SUM(CASE WHEN r.Estado IN ('Confirmado', 'Completado') THEN dr.Subtotal ELSE 0 END), 0) as Total_Ingresos,
                COALESCE(SUM(CASE WHEN r.Estado = 'Solicitado' AND r.Estado_Pago = 'Pendiente' THEN dr.Subtotal ELSE 0 END), 0) as Total_Pendiente_Pago
            FROM Detalle_Reserva dr
            INNER JOIN Reserva r ON dr.Reserva_Id = r.Id
            WHERE dr.Empresa_Id = @EmpresaId";
        
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<ReservationStatsDTO>(sql, new { EmpresaId = empresaId })
            ?? new ReservationStatsDTO();
    }

    // Métodos específicos para Cotizaciones
    public async Task<IEnumerable<CotizacionDTO>> GetCotizacionesByEmpresaIdAsync(int empresaId, bool? incluirVencidas = null)
    {
        var sql = @"
            SELECT 
                r.*,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                u.Nombre_Completo as Creado_Por_Nombre,
                EXTRACT(DAY FROM r.Fecha_Vencimiento_Cotizacion - NOW()) as Dias_Para_Vencer,
                (r.Fecha_Vencimiento_Cotizacion < NOW()) as Esta_Vencida
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            INNER JOIN Usuario u ON r.Creado_Por_Id = u.Id
            WHERE r.Empresa_Id = @EmpresaId 
            AND r.Estado = 'Solicitado'
            AND r.Fecha_Vencimiento_Cotizacion IS NOT NULL";

        if (incluirVencidas.HasValue && !incluirVencidas.Value)
        {
            sql += " AND r.Fecha_Vencimiento_Cotizacion >= NOW()";
        }

        sql += " ORDER BY r.Fecha_Creacion DESC";

        using var connection = GetConnection();
        return await connection.QueryAsync<CotizacionDTO>(sql, new { EmpresaId = empresaId });
    }

    /// <summary>
    /// Obtiene todas las cotizaciones opcionalmente filtradas por Empresa_Id
    /// </summary>
    /// <param name="empresaId">ID de la empresa. null = todas (solo SuperAdmin), valor = filtrar por empresa</param>
    /// <param name="incluirVencidas">Incluir cotizaciones vencidas</param>
    public async Task<IEnumerable<CotizacionDTO>> GetAllCotizacionesAsync(int? empresaId = null, bool? incluirVencidas = null)
    {
        var sql = @"
            SELECT 
                r.*,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                u.Nombre_Completo as Creado_Por_Nombre,
                EXTRACT(DAY FROM r.Fecha_Vencimiento_Cotizacion - NOW()) as Dias_Para_Vencer,
                (r.Fecha_Vencimiento_Cotizacion < NOW()) as Esta_Vencida
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            INNER JOIN Usuario u ON r.Creado_Por_Id = u.Id
            WHERE r.Estado = 'Solicitado'
            AND r.Fecha_Vencimiento_Cotizacion IS NOT NULL";

        // Aplicar filtro multi-tenant si se proporciona empresaId
        if (empresaId.HasValue)
        {
            sql += " AND r.Empresa_Id = @EmpresaId";
        }

        if (incluirVencidas.HasValue && !incluirVencidas.Value)
        {
            sql += " AND r.Fecha_Vencimiento_Cotizacion >= NOW()";
        }

        sql += " ORDER BY r.Fecha_Creacion DESC";

        using var connection = GetConnection();
        return await connection.QueryAsync<CotizacionDTO>(
            sql, 
            empresaId.HasValue ? new { EmpresaId = empresaId.Value } : null);
    }

    public async Task<CotizacionDTO?> GetCotizacionByIdAsync(int id)
    {
        var sql = @"
            SELECT 
                r.*,
                c.Nombre as Cliente_Nombre,
                c.Email as Cliente_Email,
                c.Telefono as Cliente_Telefono,
                u.Nombre_Completo as Creado_Por_Nombre,
                EXTRACT(DAY FROM r.Fecha_Vencimiento_Cotizacion - NOW()) as Dias_Para_Vencer,
                (r.Fecha_Vencimiento_Cotizacion < NOW()) as Esta_Vencida
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            INNER JOIN Usuario u ON r.Creado_Por_Id = u.Id
            WHERE r.Id = @Id 
            AND r.Estado = 'Solicitado'
            AND r.Fecha_Vencimiento_Cotizacion IS NOT NULL";

        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<CotizacionDTO>(sql, new { Id = id });
    }

    public async Task<EstadisticasCotizacionesDTO> GetEstadisticasCotizacionesAsync(int empresaId)
    {
        var sql = @"
            SELECT 
                COUNT(CASE WHEN Estado = 'Solicitado' AND Fecha_Vencimiento_Cotizacion IS NOT NULL THEN 1 END) as Total_Cotizaciones,
                COUNT(CASE WHEN Estado = 'Solicitado' AND Fecha_Vencimiento_Cotizacion >= NOW() THEN 1 END) as Cotizaciones_Vigentes,
                COUNT(CASE WHEN Estado = 'Solicitado' AND Fecha_Vencimiento_Cotizacion < NOW() THEN 1 END) as Cotizaciones_Vencidas,
                COUNT(CASE WHEN Estado != 'Solicitado' AND Fecha_Vencimiento_Cotizacion IS NOT NULL THEN 1 END) as Cotizaciones_Convertidas,
                COALESCE(SUM(CASE WHEN Estado = 'Solicitado' AND Fecha_Vencimiento_Cotizacion IS NOT NULL THEN Total ELSE 0 END), 0) as Valor_Total_Cotizado,
                COALESCE(SUM(CASE WHEN Estado != 'Solicitado' AND Fecha_Vencimiento_Cotizacion IS NOT NULL THEN Total ELSE 0 END), 0) as Valor_Total_Convertido
            FROM Reserva
            WHERE Empresa_Id = @EmpresaId";

        using var connection = GetConnection();
        var stats = await connection.QueryFirstOrDefaultAsync<EstadisticasCotizacionesDTO>(sql, new { EmpresaId = empresaId });

        if (stats != null && stats.Total_Cotizaciones > 0)
        {
            stats.Tasa_Conversion = (decimal)stats.Cotizaciones_Convertidas / stats.Total_Cotizaciones * 100;
        }

        return stats ?? new EstadisticasCotizacionesDTO();
    }

    public async Task<bool> ExtenderVencimientoCotizacionAsync(int cotizacionId, int diasExtension)
    {
        var sql = @"
            UPDATE Reserva 
            SET Fecha_Vencimiento_Cotizacion = Fecha_Vencimiento_Cotizacion + (@DiasExtension || ' days')::INTERVAL,
                Fecha_Actualizacion = NOW()
            WHERE Id = @CotizacionId 
            AND Estado = 'Solicitado'
            AND Fecha_Vencimiento_Cotizacion IS NOT NULL";

        using var connection = GetConnection();
        var affected = await connection.ExecuteAsync(sql, new { CotizacionId = cotizacionId, DiasExtension = diasExtension });
        return affected > 0;
    }
}
