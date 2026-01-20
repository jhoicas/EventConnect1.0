using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using Dapper;

namespace EventConnect.Infrastructure.Repositories;

public class ReservaRepository : RepositoryBase<Reserva>
{
    public ReservaRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<Reserva>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = @"
            SELECT r.*, c.Nombre as Cliente_Nombre 
            FROM Reserva r
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            WHERE r.Empresa_Id = @EmpresaId
            ORDER BY r.Fecha_Creacion DESC";
        return await QueryAsync(sql, new { EmpresaId = empresaId });
    }

    public async Task<Reserva?> GetByCodigoAsync(string codigo)
    {
        var sql = "SELECT * FROM Reserva WHERE Codigo_Reserva = @Codigo";
        return await QueryFirstOrDefaultAsync(sql, new { Codigo = codigo });
    }

    public async Task<IEnumerable<Reserva>> GetByEstadoAsync(int empresaId, string estado)
    {
        var sql = "SELECT * FROM Reserva WHERE Empresa_Id = @EmpresaId AND Estado = @Estado ORDER BY Fecha_Evento";
        return await QueryAsync(sql, new { EmpresaId = empresaId, Estado = estado });
    }

    public async Task<IEnumerable<Reserva>> GetByClienteIdAsync(int clienteId)
    {
        var sql = "SELECT * FROM Reserva WHERE Cliente_Id = @ClienteId ORDER BY Fecha_Creacion DESC";
        return await QueryAsync(sql, new { ClienteId = clienteId });
    }

    // Métodos específicos para Cotizaciones
    public async Task<IEnumerable<CotizacionDTO>> GetCotizacionesByEmpresaIdAsync(int empresaId, bool? incluirVencidas = null)
    {
        var sql = @"
            SELECT 
                r.*,
                c.Nombre_Completo as Cliente_Nombre,
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
                c.Nombre_Completo as Cliente_Nombre,
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
                c.Nombre_Completo as Cliente_Nombre,
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
