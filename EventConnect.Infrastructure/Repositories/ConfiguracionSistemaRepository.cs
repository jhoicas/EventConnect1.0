using Dapper;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Repositories;

namespace EventConnect.Infrastructure.Repositories;

public class ConfiguracionSistemaRepository : RepositoryBase<ConfiguracionSistema>, IConfiguracionSistemaRepository
{
    public ConfiguracionSistemaRepository(string connectionString) : base(connectionString)
    {
    }

    public new async Task<IEnumerable<ConfiguracionSistema>> GetAllAsync()
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Configuracion_Sistema ORDER BY Clave";
        return await connection.QueryAsync<ConfiguracionSistema>(query);
    }

    public async Task<IEnumerable<ConfiguracionSistema>> GetByEmpresaIdAsync(int empresaId)
    {
        using var connection = GetConnection();
        const string query = @"
            SELECT * FROM Configuracion_Sistema 
            WHERE Empresa_Id = @EmpresaId OR Es_Global = TRUE
            ORDER BY Clave";
        return await connection.QueryAsync<ConfiguracionSistema>(query, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<ConfiguracionSistema>> GetGlobalesAsync()
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Configuracion_Sistema WHERE Es_Global = TRUE ORDER BY Clave";
        return await connection.QueryAsync<ConfiguracionSistema>(query);
    }

    public new async Task<ConfiguracionSistema?> GetByIdAsync(int id)
    {
        using var connection = GetConnection();
        const string query = "SELECT * FROM Configuracion_Sistema WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ConfiguracionSistema>(query, new { Id = id });
    }

    public async Task<ConfiguracionSistema?> GetByClaveAsync(string clave, int? empresaId = null)
    {
        using var connection = GetConnection();
        string query;
        object parameters;

        if (empresaId.HasValue)
        {
            query = @"
                SELECT * FROM Configuracion_Sistema 
                WHERE Clave = @Clave AND (Empresa_Id = @EmpresaId OR Es_Global = TRUE)
                ORDER BY Es_Global ASC
                LIMIT 1";
            parameters = new { Clave = clave, EmpresaId = empresaId.Value };
        }
        else
        {
            query = "SELECT * FROM Configuracion_Sistema WHERE Clave = @Clave AND Es_Global = TRUE LIMIT 1";
            parameters = new { Clave = clave };
        }

        return await connection.QuerySingleOrDefaultAsync<ConfiguracionSistema>(query, parameters);
    }

    public async Task<int> CreateAsync(ConfiguracionSistema configuracion)
    {
        using var connection = GetConnection();
        const string query = @"
            INSERT INTO Configuracion_Sistema 
            (Empresa_Id, Clave, Valor, Descripcion, Tipo_Dato, Es_Global, Fecha_Actualizacion)
            VALUES 
            (@Empresa_Id, @Clave, @Valor, @Descripcion, @Tipo_Dato, @Es_Global, @Fecha_Actualizacion)
            RETURNING Id;";
        
        configuracion.Fecha_Actualizacion = DateTime.Now;
        return await connection.ExecuteScalarAsync<int>(query, configuracion);
    }

    public async Task<bool> UpdateAsync(ConfiguracionSistema configuracion)
    {
        using var connection = GetConnection();
        const string query = @"
            UPDATE Configuracion_Sistema 
            SET Empresa_Id = @Empresa_Id,
                Clave = @Clave,
                Valor = @Valor,
                Descripcion = @Descripcion,
                Tipo_Dato = @Tipo_Dato,
                Es_Global = @Es_Global,
                Fecha_Actualizacion = @Fecha_Actualizacion
            WHERE Id = @Id";
        
        configuracion.Fecha_Actualizacion = DateTime.Now;
        var rowsAffected = await connection.ExecuteAsync(query, configuracion);
        return rowsAffected > 0;
    }

    public new async Task<bool> DeleteAsync(int id)
    {
        using var connection = GetConnection();
        const string query = "DELETE FROM Configuracion_Sistema WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
        return rowsAffected > 0;
    }
}
