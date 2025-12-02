namespace EventConnect.Application.Services;

/// <summary>
/// Servicio para cálculo automático de depreciación de activos
/// </summary>
public interface IDepreciacionService
{
    /// <summary>
    /// Calcula y registra la depreciación mensual de todos los activos
    /// </summary>
    Task CalcularDepreciacionMensualAsync();
    
    /// <summary>
    /// Calcula la depreciación de un activo específico
    /// </summary>
    Task<decimal> CalcularDepreciacionActivoAsync(int activoId);
    
    /// <summary>
    /// Obtiene el valor en libros actual de un activo
    /// </summary>
    Task<decimal> ObtenerValorLibrosAsync(int activoId);
}
