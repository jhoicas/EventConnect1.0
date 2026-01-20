using Dapper;
using EventConnect.Domain.Entities;
using EventConnect.Domain.DTOs;
using Npgsql;

namespace EventConnect.Infrastructure.Repositories;

public class ActivoRepository : RepositoryBase<Activo>
{
    public ActivoRepository(string connectionString) : base(connectionString) { }

    public async Task<IEnumerable<Activo>> GetByEmpresaIdAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Empresa_Id = @EmpresaId ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId });
    }

    public async Task<IEnumerable<Activo>> GetByBodegaIdAsync(int bodegaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Bodega_Id = @BodegaId AND Activo = true ORDER BY Codigo_Activo";
        return await connection.QueryAsync<Activo>(query, new { BodegaId = bodegaId });
    }

    public async Task<IEnumerable<Activo>> GetByEstadoDisponibilidadAsync(int empresaId, string estadoDisponibilidad)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Empresa_Id = @EmpresaId AND Estado_Disponibilidad = @EstadoDisponibilidad ORDER BY Fecha_Creacion DESC";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId, EstadoDisponibilidad = estadoDisponibilidad });
    }

    public async Task<Activo?> GetByCodigoActivoAsync(string codigoActivo)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Codigo_Activo = @CodigoActivo";
        return await connection.QueryFirstOrDefaultAsync<Activo>(query, new { CodigoActivo = codigoActivo });
    }

    public async Task<Activo?> GetByQRCodeAsync(string qrCode)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Activo WHERE Codigo_QR = @QRCode OR QR_Code_URL = @QRCode";
        return await connection.QueryFirstOrDefaultAsync<Activo>(query, new { QRCode = qrCode });
    }

    public async Task<IEnumerable<Activo>> GetActivosParaDepreciacionAsync(int empresaId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            SELECT * FROM Activo 
            WHERE Empresa_Id = @EmpresaId 
            AND Activo = true
            AND Valor_Residual IS NOT NULL
            AND Vida_Util_Meses IS NOT NULL
            AND Fecha_Compra IS NOT NULL";
        return await connection.QueryAsync<Activo>(query, new { EmpresaId = empresaId });
    }

    /// <summary>
    /// Obtiene la hoja de vida completa de un activo con todos sus historiales
    /// usando QueryMultipleAsync para eficiencia en una sola conexión
    /// </summary>
    public async Task<HojaVidaActivoDto?> GetHojaVidaAsync(int activoId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        
        var sql = @"
            -- Información del Activo
            SELECT 
                a.Id,
                a.Empresa_Id AS EmpresaId,
                a.Producto_Id AS ProductoId,
                p.Nombre AS ProductoNombre,
                a.Bodega_Id AS BodegaId,
                b.Nombre AS BodegaNombre,
                a.Codigo_Activo AS CodigoActivo,
                a.Codigo_QR AS CodigoQR,
                a.Numero_Serie AS NumeroSerie,
                a.Estado_Fisico AS EstadoFisico,
                a.Estado_Disponibilidad AS EstadoDisponibilidad,
                a.Fecha_Compra AS FechaCompra,
                a.Costo_Adquisicion AS CostoAdquisicion,
                a.Valor_Residual AS ValorResidual,
                a.Vida_Util_Meses AS VidaUtilMeses,
                a.Proveedor,
                a.Fecha_Creacion AS FechaCreacion
            FROM Activo a
            LEFT JOIN Producto p ON a.Producto_Id = p.Id
            LEFT JOIN Bodega b ON a.Bodega_Id = b.Id
            WHERE a.Id = @ActivoId;
            
            -- Historial de Movimientos
            SELECT 
                mi.Id,
                mi.Fecha_Movimiento AS FechaMovimiento,
                mi.Tipo_Movimiento AS TipoMovimiento,
                bo.Nombre AS UbicacionOrigen,
                bd.Nombre AS UbicacionDestino,
                mi.Motivo,
                mi.Usuario_Id AS UsuarioId,
                u.Nombre AS UsuarioNombre
            FROM Movimiento_Inventario mi
            LEFT JOIN Bodega bo ON mi.Bodega_Origen_Id = bo.Id
            LEFT JOIN Bodega bd ON mi.Bodega_Destino_Id = bd.Id
            LEFT JOIN Usuario u ON mi.Usuario_Id = u.Id
            WHERE mi.Activo_Id = @ActivoId
            ORDER BY mi.Fecha_Movimiento DESC;
            
            -- Historial de Mantenimientos
            SELECT 
                m.Id,
                m.Fecha_Programada AS FechaProgramada,
                m.Fecha_Realizada AS FechaRealizada,
                m.Tipo_Mantenimiento AS TipoMantenimiento,
                m.Descripcion,
                m.Costo,
                m.Estado,
                m.Observaciones,
                m.Proveedor_Servicio AS ProveedorServicio
            FROM Mantenimiento m
            WHERE m.Activo_Id = @ActivoId
            ORDER BY COALESCE(m.Fecha_Realizada, m.Fecha_Programada) DESC NULLS LAST;
            
            -- Historial de Reservas
            SELECT 
                dr.Id,
                dr.Reserva_Id AS ReservaId,
                r.Codigo_Reserva AS CodigoReserva,
                r.Fecha_Evento AS FechaEvento,
                r.Fecha_Entrega AS FechaEntrega,
                r.Fecha_Devolucion_Real AS FechaDevolucion,
                r.Cliente_Id AS ClienteId,
                c.Nombre AS ClienteNombre,
                c.Email AS ClienteEmail,
                r.Estado AS EstadoReserva,
                dr.Precio_Unitario AS PrecioUnitario,
                dr.Dias_Alquiler AS DiasAlquiler
            FROM Detalle_Reserva dr
            INNER JOIN Reserva r ON dr.Reserva_Id = r.Id
            INNER JOIN Cliente c ON r.Cliente_Id = c.Id
            WHERE dr.Activo_Id = @ActivoId
            ORDER BY r.Fecha_Evento DESC;";

        using var multi = await connection.QueryMultipleAsync(sql, new { ActivoId = activoId });
        
        var activoInfo = await multi.ReadFirstOrDefaultAsync<HojaVidaActivoDto>();
        if (activoInfo == null)
            return null;

        activoInfo.HistorialMovimientos = (await multi.ReadAsync<HistorialMovimientoDto>()).ToList();
        activoInfo.HistorialMantenimientos = (await multi.ReadAsync<HistorialMantenimientoDto>()).ToList();
        activoInfo.HistorialReservas = (await multi.ReadAsync<HistorialReservaDto>()).ToList();

        return activoInfo;
    }
}
