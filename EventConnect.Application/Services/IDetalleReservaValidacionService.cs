using EventConnect.Domain.Entities;

namespace EventConnect.Application.Services;

/// <summary>
/// Servicio para validar la integridad Producto-Activo en reservas
/// Garantiza que un Activo solo se reserve bajo su Producto padre
/// </summary>
public interface IDetalleReservaValidacionService
{
    /// <summary>
    /// Valida que un Activo pertenezca al Producto especificado
    /// </summary>
    Task<(bool esValido, string mensaje, int? productoIdReal)> ValidarProductoActivoAsync(
        int? productoId, 
        int? activoId);
    
    /// <summary>
    /// Obtiene el Producto_Id real de un Activo
    /// </summary>
    Task<int?> ObtenerProductoIdDeActivoAsync(int activoId);
    
    /// <summary>
    /// Valida que un Activo esté disponible para reserva
    /// </summary>
    Task<bool> ValidarDisponibilidadActivoAsync(int activoId);
    
    /// <summary>
    /// Obtiene activos disponibles de un producto específico
    /// </summary>
    Task<IEnumerable<Activo>> ObtenerActivosDisponiblesAsync(int productoId);
    
    /// <summary>
    /// Cuenta cuántos activos disponibles tiene un producto
    /// </summary>
    Task<int> ContarActivosDisponiblesAsync(int productoId);
    
    /// <summary>
    /// Valida y normaliza un DetalleReserva antes de guardarlo
    /// Auto-completa el Producto_Id si solo se especifica Activo_Id
    /// </summary>
    Task<(bool esValido, string mensaje, DetalleReserva detalleNormalizado)> ValidarYNormalizarDetalleAsync(
        DetalleReserva detalle);
}
