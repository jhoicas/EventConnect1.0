using Dapper;
using EventConnect.Domain.Entities;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class FacturaRepository : RepositoryBase<Factura>
{
    public FacturaRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Factura>> GetByEmpresaAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Factura 
            WHERE Empresa_Id = @EmpresaId 
            ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<Factura>(query, new { EmpresaId = empresaId });
    }

    public async Task<Factura?> GetByPrefijoConsecutivoAsync(int empresaId, string prefijo, int consecutivo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Factura 
            WHERE Empresa_Id = @EmpresaId 
            AND Prefijo = @Prefijo 
            AND Consecutivo = @Consecutivo";
        return await connection.QueryFirstOrDefaultAsync<Factura>(query, new { EmpresaId = empresaId, Prefijo = prefijo, Consecutivo = consecutivo });
    }

    /// <summary>
    /// Obtiene el siguiente número consecutivo disponible para un prefijo y empresa
    /// </summary>
    public async Task<int> GetSiguienteConsecutivoAsync(int empresaId, string prefijo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT COALESCE(MAX(Consecutivo), 0) + 1 
            FROM Factura 
            WHERE Empresa_Id = @EmpresaId 
            AND Prefijo = @Prefijo";
        return await connection.QueryFirstOrDefaultAsync<int>(query, new { EmpresaId = empresaId, Prefijo = prefijo });
    }

    /// <summary>
    /// Crea una factura con sus detalles en una transacción
    /// </summary>
    public async Task<int> CreateWithDetailsAsync(Factura factura, IEnumerable<DetalleFactura> detalles)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insertar factura
            var facturaId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Factura (
                    Empresa_Id, Cliente_Id, Reserva_Id, Prefijo, Consecutivo, CUFE,
                    Fecha_Emision, Fecha_Vencimiento, Subtotal, Impuestos, Total,
                    Estado, Datos_Cliente_Snapshot, Observaciones, Creado_Por_Id,
                    Fecha_Creacion, Fecha_Actualizacion
                ) VALUES (
                    @Empresa_Id, @Cliente_Id, @Reserva_Id, @Prefijo, @Consecutivo, @CUFE,
                    @Fecha_Emision, @Fecha_Vencimiento, @Subtotal, @Impuestos, @Total,
                    @Estado, @Datos_Cliente_Snapshot, @Observaciones, @Creado_Por_Id,
                    @Fecha_Creacion, @Fecha_Actualizacion
                ) RETURNING Id",
                factura, transaction
            );

            // Insertar detalles
            foreach (var detalle in detalles)
            {
                detalle.Factura_Id = facturaId;
                await connection.ExecuteAsync(
                    @"INSERT INTO Detalle_Factura (
                        Factura_Id, Producto_Id, Servicio, Cantidad, Precio_Unitario,
                        Subtotal, Tasa_Impuesto, Impuesto, Total, Unidad_Medida,
                        Observaciones, Fecha_Creacion
                    ) VALUES (
                        @Factura_Id, @Producto_Id, @Servicio, @Cantidad, @Precio_Unitario,
                        @Subtotal, @Tasa_Impuesto, @Impuesto, @Total, @Unidad_Medida,
                        @Observaciones, @Fecha_Creacion
                    )",
                    detalle, transaction
                );
            }

            transaction.Commit();
            return facturaId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Obtiene una factura con sus detalles
    /// </summary>
    public async Task<FacturaConDetallesDto?> GetWithDetailsAsync(int facturaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        
        var sql = @"
            -- Información de la Factura
            SELECT 
                f.*,
                c.Nombre AS Cliente_Nombre,
                c.Documento AS Cliente_Documento,
                c.Tipo_Documento AS Cliente_Tipo_Documento,
                c.Email AS Cliente_Email,
                c.Direccion AS Cliente_Direccion,
                c.Ciudad AS Cliente_Ciudad
            FROM Factura f
            INNER JOIN Cliente c ON f.Cliente_Id = c.Id
            WHERE f.Id = @FacturaId;
            
            -- Detalles de la Factura
            SELECT * FROM Detalle_Factura 
            WHERE Factura_Id = @FacturaId 
            ORDER BY Id;";

        using var multi = await connection.QueryMultipleAsync(sql, new { FacturaId = facturaId });
        
        var factura = await multi.ReadFirstOrDefaultAsync<FacturaConDetallesDto>();
        if (factura == null)
            return null;

        factura.Detalles = (await multi.ReadAsync<DetalleFactura>()).ToList();
        return factura;
    }

    /// <summary>
    /// DTO interno para factura con detalles
    /// </summary>
    public class FacturaConDetallesDto : Factura
    {
        public string? Cliente_Nombre { get; set; }
        public string? Cliente_Documento { get; set; }
        public string? Cliente_Tipo_Documento { get; set; }
        public string? Cliente_Email { get; set; }
        public string? Cliente_Direccion { get; set; }
        public string? Cliente_Ciudad { get; set; }
        public List<DetalleFactura> Detalles { get; set; } = new();
    }
}
