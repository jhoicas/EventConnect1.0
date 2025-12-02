using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using Dapper;

namespace EventConnect.Infrastructure.Repositories;

public class TransaccionPagoRepository : RepositoryBase<TransaccionPago>
{
    public TransaccionPagoRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<TransaccionPagoDTO>> GetByReservaIdAsync(int reservaId)
    {
        var sql = @"
            SELECT 
                tp.*,
                u.Nombre_Completo as Registrado_Por_Nombre,
                c.Nombre_Completo as Cliente_Nombre
            FROM transaccion_pago tp
            INNER JOIN Usuario u ON tp.Registrado_Por_Usuario_Id = u.Id
            INNER JOIN Reserva r ON tp.Reserva_Id = r.Id
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            WHERE tp.Reserva_Id = @ReservaId
            ORDER BY tp.Fecha_Transaccion DESC";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<TransaccionPagoDTO>(connection, sql, new { ReservaId = reservaId });
    }

    public async Task<IEnumerable<TransaccionPagoDTO>> GetByEmpresaIdAsync(int empresaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var sql = @"
            SELECT 
                tp.*,
                u.Nombre_Completo as Registrado_Por_Nombre,
                c.Nombre_Completo as Cliente_Nombre
            FROM transaccion_pago tp
            INNER JOIN Usuario u ON tp.Registrado_Por_Usuario_Id = u.Id
            INNER JOIN Reserva r ON tp.Reserva_Id = r.Id
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            WHERE r.Empresa_Id = @EmpresaId";

        if (fechaInicio.HasValue)
            sql += " AND tp.Fecha_Transaccion >= @FechaInicio";
        if (fechaFin.HasValue)
            sql += " AND tp.Fecha_Transaccion <= @FechaFin";

        sql += " ORDER BY tp.Fecha_Transaccion DESC";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<TransaccionPagoDTO>(connection, sql, new { EmpresaId = empresaId, FechaInicio = fechaInicio, FechaFin = fechaFin });
    }

    public async Task<ResumenPagosDTO> GetResumenPagosAsync(int reservaId)
    {
        var sql = @"
            SELECT 
                r.Id as Reserva_Id,
                r.Total as Total_Reserva,
                COALESCE(SUM(CASE WHEN tp.Tipo = 'Pago' THEN tp.Monto ELSE 0 END), 0) as Total_Pagado,
                r.Total - COALESCE(SUM(CASE WHEN tp.Tipo = 'Pago' THEN tp.Monto ELSE 0 END), 0) as Saldo_Pendiente,
                (COALESCE(SUM(CASE WHEN tp.Tipo = 'Pago' THEN tp.Monto ELSE 0 END), 0) / r.Total * 100) as Porcentaje_Pagado
            FROM Reserva r
            LEFT JOIN transaccion_pago tp ON r.Id = tp.Reserva_Id
            WHERE r.Id = @ReservaId
            GROUP BY r.Id, r.Total";
        
        using var connection = GetConnection();
        var resumen = await Dapper.SqlMapper.QueryFirstOrDefaultAsync<ResumenPagosDTO>(connection, sql, new { ReservaId = reservaId });
        
        if (resumen != null)
        {
            resumen.Transacciones = (await GetByReservaIdAsync(reservaId)).ToList();
        }
        
        return resumen ?? new ResumenPagosDTO { Reserva_Id = reservaId };
    }

    public async Task<decimal> GetTotalPagadoAsync(int reservaId)
    {
        var sql = @"
            SELECT COALESCE(SUM(Monto), 0)
            FROM transaccion_pago
            WHERE Reserva_Id = @ReservaId AND Tipo = 'Pago'";
        
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<decimal>(sql, new { ReservaId = reservaId });
    }

    public async Task<IEnumerable<TransaccionPagoDTO>> GetAllAsync()
    {
        var sql = @"
            SELECT 
                tp.*,
                u.Nombre_Completo as Registrado_Por_Nombre,
                c.Nombre_Completo as Cliente_Nombre
            FROM transaccion_pago tp
            INNER JOIN Usuario u ON tp.Registrado_Por_Usuario_Id = u.Id
            INNER JOIN Reserva r ON tp.Reserva_Id = r.Id
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            ORDER BY tp.Fecha_Transaccion DESC";
        
        using var connection = GetConnection();
        return await Dapper.SqlMapper.QueryAsync<TransaccionPagoDTO>(connection, sql);
    }
}
