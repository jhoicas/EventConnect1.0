using Dapper;
using EventConnect.Domain.Entities;
using MySqlConnector;

namespace EventConnect.Infrastructure.Repositories;

public class DetalleReservaRepository : RepositoryBase<DetalleReserva>
{
    public DetalleReservaRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<DetalleReserva>> GetByReservaIdAsync(int reservaId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM detalle_reserva 
            WHERE Reserva_Id = @ReservaId 
            ORDER BY Id";
        
        return await connection.QueryAsync<DetalleReserva>(sql, new { ReservaId = reservaId });
    }

    public async Task<IEnumerable<DetalleReserva>> GetByProductoIdAsync(int productoId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM detalle_reserva 
            WHERE Producto_Id = @ProductoId 
            ORDER BY Id DESC";
        
        return await connection.QueryAsync<DetalleReserva>(sql, new { ProductoId = productoId });
    }

    public async Task<IEnumerable<DetalleReserva>> GetByActivoIdAsync(int activoId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT * FROM detalle_reserva 
            WHERE Activo_Id = @ActivoId 
            ORDER BY Id DESC";
        
        return await connection.QueryAsync<DetalleReserva>(sql, new { ActivoId = activoId });
    }

    public async Task<bool> ValidarIntegridadAsync(int detalleId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT 
                CASE 
                    WHEN dr.Activo_Id IS NOT NULL 
                         AND dr.Producto_Id IS NOT NULL 
                         AND dr.Producto_Id != a.Producto_Id 
                    THEN 0
                    WHEN dr.Producto_Id IS NULL AND dr.Activo_Id IS NULL 
                    THEN 0
                    ELSE 1
                END AS EsValido
            FROM detalle_reserva dr
            LEFT JOIN activo a ON dr.Activo_Id = a.Id
            WHERE dr.Id = @DetalleId";
        
        var resultado = await connection.QueryFirstOrDefaultAsync<int>(sql, new { DetalleId = detalleId });
        return resultado == 1;
    }

    /// <summary>
    /// Obtiene detalles de reserva con información completa de producto y activo
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetDetallesConInfoCompletaAsync(int reservaId)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT 
                dr.*,
                p.Nombre AS Producto_Nombre,
                p.SKU AS Producto_SKU,
                p.Imagen_URL AS Producto_Imagen,
                a.Codigo_Activo,
                a.Numero_Serie,
                a.Estado_Fisico,
                a.Estado_Disponibilidad,
                CASE 
                    WHEN dr.Activo_Id IS NOT NULL 
                         AND dr.Producto_Id IS NOT NULL 
                         AND dr.Producto_Id != a.Producto_Id 
                    THEN 'INTEGRIDAD_VIOLADA'
                    WHEN dr.Producto_Id IS NULL AND dr.Activo_Id IS NULL 
                    THEN 'INCOMPLETO'
                    ELSE 'OK'
                END AS Estado_Integridad
            FROM detalle_reserva dr
            LEFT JOIN producto p ON dr.Producto_Id = p.Id
            LEFT JOIN activo a ON dr.Activo_Id = a.Id
            WHERE dr.Reserva_Id = @ReservaId
            ORDER BY dr.Id";
        
        return await connection.QueryAsync(sql, new { ReservaId = reservaId });
    }

    /// <summary>
    /// Obtiene todos los detalles con problemas de integridad
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetDetallesConProblemasIntegridadAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            SELECT 
                dr.Id AS Detalle_Id,
                dr.Reserva_Id,
                r.Codigo_Reserva,
                dr.Producto_Id,
                p.Nombre AS Producto_Nombre,
                dr.Activo_Id,
                a.Codigo_Activo,
                a.Producto_Id AS Activo_Producto_Real,
                pa.Nombre AS Activo_Producto_Nombre,
                'INTEGRIDAD_VIOLADA' AS Estado_Integridad,
                CONCAT('Producto declarado: ', p.Nombre, ' pero Activo pertenece a: ', pa.Nombre) AS Descripcion_Error
            FROM detalle_reserva dr
            INNER JOIN reserva r ON dr.Reserva_Id = r.Id
            LEFT JOIN producto p ON dr.Producto_Id = p.Id
            LEFT JOIN activo a ON dr.Activo_Id = a.Id
            LEFT JOIN producto pa ON a.Producto_Id = pa.Id
            WHERE dr.Activo_Id IS NOT NULL 
              AND dr.Producto_Id IS NOT NULL 
              AND dr.Producto_Id != a.Producto_Id";
        
        return await connection.QueryAsync(sql);
    }

    /// <summary>
    /// Corrige automáticamente detalles con problemas de integridad
    /// </summary>
    public async Task<int> CorregirIntegridadAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"
            UPDATE detalle_reserva dr
            INNER JOIN activo a ON dr.Activo_Id = a.Id
            SET dr.Producto_Id = a.Producto_Id
            WHERE dr.Activo_Id IS NOT NULL 
              AND (dr.Producto_Id IS NULL OR dr.Producto_Id != a.Producto_Id)";
        
        return await connection.ExecuteAsync(sql);
    }
}
