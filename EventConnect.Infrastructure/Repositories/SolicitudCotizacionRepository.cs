using Dapper;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class SolicitudCotizacionRepository : ISolicitudCotizacionRepository
{
    private readonly string _connectionString;

    public SolicitudCotizacionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public async Task<IEnumerable<Cotizacion>> GetByClienteIdAsync(int clienteId)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id, Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                   Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                   Fecha_Creacion, Fecha_Actualizacion
            FROM Cotizaciones
            WHERE Cliente_Id = @ClienteId
            ORDER BY Fecha_Solicitud DESC;";

        return await connection.QueryAsync<Cotizacion>(query, new { ClienteId = clienteId });
    }

    public async Task<Cotizacion?> GetByIdAsync(int id)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id, Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                   Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                   Fecha_Creacion, Fecha_Actualizacion
            FROM Cotizaciones
            WHERE Id = @Id;";

        return await connection.QueryFirstOrDefaultAsync<Cotizacion>(query, new { Id = id });
    }

    public async Task<IEnumerable<Cotizacion>> GetAllAsync()
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id, Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                   Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                   Fecha_Creacion, Fecha_Actualizacion
            FROM Cotizaciones
            ORDER BY Fecha_Solicitud DESC;";

        return await connection.QueryAsync<Cotizacion>(query);
    }

    public async Task<IEnumerable<Cotizacion>> GetByProductoIdAsync(int productoId)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id, Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                   Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                   Fecha_Creacion, Fecha_Actualizacion
            FROM Cotizaciones
            WHERE Producto_Id = @ProductoId
            ORDER BY Fecha_Solicitud DESC;";

        return await connection.QueryAsync<Cotizacion>(query, new { ProductoId = productoId });
    }

    public async Task<IEnumerable<Cotizacion>> GetByEstadoAsync(string estado)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id, Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                   Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                   Fecha_Creacion, Fecha_Actualizacion
            FROM Cotizaciones
            WHERE Estado = @Estado
            ORDER BY Fecha_Solicitud DESC;";

        return await connection.QueryAsync<Cotizacion>(query, new { Estado = estado });
    }

    public async Task<int> CreateAsync(Cotizacion cotizacion)
    {
        using var connection = GetConnection();
        const string query = @"
            INSERT INTO Cotizaciones (Cliente_Id, Producto_Id, Fecha_Solicitud, Cantidad_Solicitada, 
                                     Monto_Cotizacion, Estado, Observaciones, Fecha_Respuesta, 
                                     Fecha_Creacion, Fecha_Actualizacion)
            VALUES (@Cliente_Id, @Producto_Id, @Fecha_Solicitud, @Cantidad_Solicitada, 
                   @Monto_Cotizacion, @Estado, @Observaciones, @Fecha_Respuesta, 
                   @Fecha_Creacion, @Fecha_Actualizacion)
            RETURNING Id;";

        var now = DateTime.Now;
        cotizacion.Fecha_Solicitud = now;
        cotizacion.Fecha_Creacion = now;
        cotizacion.Fecha_Actualizacion = now;
        cotizacion.Monto_Cotizacion = 0; // Se actualiza cuando se responde

        return await connection.ExecuteScalarAsync<int>(query, cotizacion);
    }

    public async Task<bool> UpdateAsync(Cotizacion cotizacion)
    {
        using var connection = GetConnection();
        const string query = @"
            UPDATE Cotizaciones
            SET Monto_Cotizacion = @Monto_Cotizacion,
                Estado = @Estado,
                Observaciones = @Observaciones,
                Fecha_Respuesta = @Fecha_Respuesta,
                Fecha_Actualizacion = @Fecha_Actualizacion
            WHERE Id = @Id;";

        cotizacion.Fecha_Actualizacion = DateTime.Now;
        var rows = await connection.ExecuteAsync(query, cotizacion);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = GetConnection();
        const string query = "DELETE FROM Cotizaciones WHERE Id = @Id;";
        var rows = await connection.ExecuteAsync(query, new { Id = id });
        return rows > 0;
    }
}
