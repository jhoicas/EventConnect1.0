using Dapper;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class ServicioRepository : IServicioRepository
{
    private readonly string _connectionString;

    public ServicioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public async Task<IEnumerable<Servicio>> GetAllAsync()
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id_Servicio, Titulo, Descripcion, Icono, Imagen_Url, Orden, Activo, Fecha_Creacion, Fecha_Actualizacion
            FROM Servicios
            ORDER BY Orden ASC, Fecha_Creacion DESC;";

        return await connection.QueryAsync<Servicio>(query);
    }

    public async Task<IEnumerable<Servicio>> GetByActivoAsync(bool activo)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id_Servicio, Titulo, Descripcion, Icono, Imagen_Url, Orden, Activo, Fecha_Creacion, Fecha_Actualizacion
            FROM Servicios
            WHERE Activo = @Activo
            ORDER BY Orden ASC, Fecha_Creacion DESC;";

        return await connection.QueryAsync<Servicio>(query, new { Activo = activo });
    }

    public async Task<Servicio?> GetByIdAsync(int id)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT Id_Servicio, Titulo, Descripcion, Icono, Imagen_Url, Orden, Activo, Fecha_Creacion, Fecha_Actualizacion
            FROM Servicios
            WHERE Id_Servicio = @Id;";

        return await connection.QueryFirstOrDefaultAsync<Servicio>(query, new { Id = id });
    }

    public async Task<int> CreateAsync(Servicio servicio)
    {
        using var connection = GetConnection();
        const string query = @"
            INSERT INTO Servicios (Titulo, Descripcion, Icono, Imagen_Url, Orden, Activo, Fecha_Creacion, Fecha_Actualizacion)
            VALUES (@Titulo, @Descripcion, @Icono, @Imagen_Url, @Orden, @Activo, @Fecha_Creacion, @Fecha_Actualizacion)
            RETURNING Id_Servicio;";

        var now = DateTime.Now;
        servicio.Fecha_Creacion = now;
        servicio.Fecha_Actualizacion = now;

        return await connection.ExecuteScalarAsync<int>(query, servicio);
    }

    public async Task<bool> UpdateAsync(Servicio servicio)
    {
        using var connection = GetConnection();
        const string query = @"
            UPDATE Servicios
            SET Titulo = @Titulo,
                Descripcion = @Descripcion,
                Icono = @Icono,
                Imagen_Url = @Imagen_Url,
                Orden = @Orden,
                Activo = @Activo,
                Fecha_Actualizacion = @Fecha_Actualizacion
            WHERE Id_Servicio = @Id_Servicio;";

        servicio.Fecha_Actualizacion = DateTime.Now;
        var rows = await connection.ExecuteAsync(query, servicio);
        return rows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = GetConnection();
        const string query = @"
            UPDATE Servicios
            SET Activo = FALSE,
                Fecha_Actualizacion = @Fecha_Actualizacion
            WHERE Id_Servicio = @Id;";

        var rows = await connection.ExecuteAsync(query, new { Id = id, Fecha_Actualizacion = DateTime.Now });
        return rows > 0;
    }
}
