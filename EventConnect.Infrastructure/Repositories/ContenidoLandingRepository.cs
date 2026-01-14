using Dapper;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;

namespace EventConnect.Infrastructure.Repositories;

public class ContenidoLandingRepository : RepositoryBase<ContenidoLanding>, IContenidoLandingRepository
{
    public ContenidoLandingRepository(string connectionString) : base(connectionString)
    {
    }

    public new async Task<IEnumerable<ContenidoLanding>> GetAllAsync()
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Contenido_Landing ORDER BY Seccion, Orden";
        return await connection.QueryAsync<ContenidoLanding>(query);
    }

    public async Task<IEnumerable<ContenidoLanding>> GetBySeccionAsync(string seccion)
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Contenido_Landing WHERE Seccion = @Seccion ORDER BY Orden";
        return await connection.QueryAsync<ContenidoLanding>(query, new { Seccion = seccion });
    }

    public async Task<IEnumerable<ContenidoLanding>> GetActivosAsync()
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Contenido_Landing WHERE Activo = TRUE ORDER BY Seccion, Orden";
        return await connection.QueryAsync<ContenidoLanding>(query);
    }

    public new async Task<ContenidoLanding?> GetByIdAsync(int id)
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Contenido_Landing WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ContenidoLanding>(query, new { Id = id });
    }

    public async Task<int> CreateAsync(ContenidoLanding contenido)
    {
        using var connection = GetConnection();
        const string query = @"
            INSERT INTO Contenido_Landing 
            (Seccion, Titulo, Subtitulo, Descripcion, Imagen_URL, Icono_Nombre, Orden, Activo, Fecha_Actualizacion)
            VALUES 
            (@Seccion, @Titulo, @Subtitulo, @Descripcion, @Imagen_URL, @Icono_Nombre, @Orden, @Activo, @Fecha_Actualizacion)
            RETURNING Id;";
        
        contenido.Fecha_Actualizacion = DateTime.Now;
        return await connection.ExecuteScalarAsync<int>(query, contenido);
    }

    public async Task<bool> UpdateAsync(ContenidoLanding contenido)
    {
        using var connection = GetConnection();
        const string query = @"
            UPDATE Contenido_Landing 
            SET Seccion = @Seccion,
                Titulo = @Titulo,
                Subtitulo = @Subtitulo,
                Descripcion = @Descripcion,
                Imagen_URL = @Imagen_URL,
                Icono_Nombre = @Icono_Nombre,
                Orden = @Orden,
                Activo = @Activo,
                Fecha_Actualizacion = @Fecha_Actualizacion
            WHERE Id = @Id";
        
        contenido.Fecha_Actualizacion = DateTime.Now;
        var rowsAffected = await connection.ExecuteAsync(query, contenido);
        return rowsAffected > 0;
    }

    public new async Task<bool> DeleteAsync(int id)
    {
        using var connection = GetConnection();
        const string query = "DELETE FROM Contenido_Landing WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }
}
