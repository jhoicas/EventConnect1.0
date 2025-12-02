using EventConnect.Domain.Entities;

namespace EventConnect.Infrastructure.Repositories;

public class ProductoRepository : RepositoryBase<Producto>
{
    public ProductoRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<IEnumerable<Producto>> GetByEmpresaIdAsync(int empresaId)
    {
        var sql = @"
            SELECT p.*, c.Nombre as Categoria_Nombre 
            FROM Producto p
            INNER JOIN Categoria c ON p.Categoria_Id = c.Id
            WHERE p.Empresa_Id = @EmpresaId AND p.Activo = 1
            ORDER BY p.Nombre";
        return await QueryAsync(sql, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId)
    {
        var sql = "SELECT * FROM Producto WHERE Categoria_Id = @CategoriaId AND Activo = 1";
        return await QueryAsync(sql, new { CategoriaId = categoriaId });
    }

    public async Task<IEnumerable<Producto>> GetStockBajoAsync(int empresaId)
    {
        var sql = "SELECT * FROM Producto WHERE Empresa_Id = @EmpresaId AND Cantidad_Stock <= Stock_Minimo AND Activo = 1";
        return await QueryAsync(sql, new { EmpresaId = empresaId });
    }
}
