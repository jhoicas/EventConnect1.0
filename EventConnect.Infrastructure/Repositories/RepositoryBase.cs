using Dapper;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace EventConnect.Infrastructure.Repositories;

public class RepositoryBase<T> where T : class
{
    protected readonly string _connectionString;
    protected readonly string _tableName;

    public RepositoryBase(string connectionString)
    {
        _connectionString = connectionString;
        _tableName = GetTableName();
    }

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    private string GetTableName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        return tableAttr?.Name ?? type.Name;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = $"SELECT * FROM {_tableName}";
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<int> AddAsync(T entity)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var properties = GetPropertiesForInsert(entity);
        var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        // PostgreSQL uses RETURNING instead of LAST_INSERT_ID()
        var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}) RETURNING Id;";
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var properties = GetPropertiesForUpdate(entity);
        var setClause = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
        var sql = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    protected async Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, parameters);
    }

    protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? parameters = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<TResult>(sql, parameters);
    }

    protected async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, parameters);
    }

    private IEnumerable<PropertyInfo> GetPropertiesForInsert(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.CanWrite && p.GetValue(entity) != null);
        return properties;
    }

    private IEnumerable<PropertyInfo> GetPropertiesForUpdate(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.CanWrite);
        return properties;
    }

    private string GetColumnName(PropertyInfo property)
    {
        var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttr?.Name ?? property.Name;
    }
}
