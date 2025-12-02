using EventConnect.Application.Services;
using EventConnect.Domain.Entities;
using EventConnect.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace EventConnect.Application.Services.Implementation;

public class DetalleReservaValidacionService : IDetalleReservaValidacionService
{
    private readonly ActivoRepository _activoRepository;
    private readonly ProductoRepository _productoRepository;
    private readonly ILogger<DetalleReservaValidacionService> _logger;

    public DetalleReservaValidacionService(
        ActivoRepository activoRepository,
        ProductoRepository productoRepository,
        ILogger<DetalleReservaValidacionService> logger)
    {
        _activoRepository = activoRepository;
        _productoRepository = productoRepository;
        _logger = logger;
    }

    public async Task<(bool esValido, string mensaje, int? productoIdReal)> ValidarProductoActivoAsync(
        int? productoId, 
        int? activoId)
    {
        // Validar que al menos uno esté presente
        if (!productoId.HasValue && !activoId.HasValue)
        {
            return (false, "Debe especificar al menos un Producto_Id o Activo_Id", null);
        }

        // Si solo se especifica Producto_Id (reserva genérica de stock)
        if (productoId.HasValue && !activoId.HasValue)
        {
            var producto = await _productoRepository.GetByIdAsync(productoId.Value);
            if (producto == null || !producto.Activo)
            {
                return (false, $"El Producto ID {productoId} no existe o está inactivo", null);
            }
            return (true, "Validación exitosa - Reserva genérica de producto", productoId);
        }

        // Si se especifica Activo_Id, validar integridad
        if (activoId.HasValue)
        {
            var activo = await _activoRepository.GetByIdAsync(activoId.Value);
            
            // Validar que el activo exista y esté activo
            if (activo == null || !activo.Esta_Activo)
            {
                return (false, $"El Activo ID {activoId} no existe o está inactivo", null);
            }

            var productoIdReal = activo.Producto_Id;

            // Si se especificó también un Producto_Id, validar que coincida
            if (productoId.HasValue && productoId.Value != productoIdReal)
            {
                var productoDeclarado = await _productoRepository.GetByIdAsync(productoId.Value);
                var productoReal = await _productoRepository.GetByIdAsync(productoIdReal);
                
                var mensaje = $"Integridad violada: El Activo '{activo.Codigo_Activo}' pertenece al producto " +
                             $"'{productoReal?.Nombre}' (ID {productoIdReal}), pero se intentó asociar con " +
                             $"'{productoDeclarado?.Nombre}' (ID {productoId})";
                
                _logger.LogWarning(mensaje);
                return (false, mensaje, productoIdReal);
            }

            // Validar disponibilidad del activo
            if (activo.Estado_Disponibilidad != "Disponible")
            {
                return (false, 
                    $"El Activo '{activo.Codigo_Activo}' no está disponible (Estado: {activo.Estado_Disponibilidad})", 
                    productoIdReal);
            }

            return (true, "Validación exitosa - Activo disponible", productoIdReal);
        }

        return (false, "Error de validación desconocido", null);
    }

    public async Task<int?> ObtenerProductoIdDeActivoAsync(int activoId)
    {
        try
        {
            var activo = await _activoRepository.GetByIdAsync(activoId);
            return activo?.Producto_Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Producto_Id del Activo {ActivoId}", activoId);
            return null;
        }
    }

    public async Task<bool> ValidarDisponibilidadActivoAsync(int activoId)
    {
        try
        {
            var activo = await _activoRepository.GetByIdAsync(activoId);
            return activo != null 
                && activo.Esta_Activo 
                && activo.Estado_Disponibilidad == "Disponible";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar disponibilidad del Activo {ActivoId}", activoId);
            return false;
        }
    }

    public async Task<IEnumerable<Activo>> ObtenerActivosDisponiblesAsync(int productoId)
    {
        try
        {
            var activos = await _activoRepository.GetAllAsync();
            return activos.Where(a => 
                a.Producto_Id == productoId 
                && a.Esta_Activo 
                && a.Estado_Disponibilidad == "Disponible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener activos disponibles del Producto {ProductoId}", productoId);
            return Enumerable.Empty<Activo>();
        }
    }

    public async Task<int> ContarActivosDisponiblesAsync(int productoId)
    {
        var activos = await ObtenerActivosDisponiblesAsync(productoId);
        return activos.Count();
    }

    public async Task<(bool esValido, string mensaje, DetalleReserva detalleNormalizado)> ValidarYNormalizarDetalleAsync(
        DetalleReserva detalle)
    {
        try
        {
            // Crear copia para normalizar
            var detalleNormalizado = new DetalleReserva
            {
                Id = detalle.Id,
                Reserva_Id = detalle.Reserva_Id,
                Producto_Id = detalle.Producto_Id,
                Activo_Id = detalle.Activo_Id,
                Cantidad = detalle.Cantidad,
                Precio_Unitario = detalle.Precio_Unitario,
                Subtotal = detalle.Subtotal,
                Dias_Alquiler = detalle.Dias_Alquiler,
                Observaciones = detalle.Observaciones,
                Estado_Item = detalle.Estado_Item,
                Fecha_Creacion = detalle.Fecha_Creacion
            };

            // Validar integridad
            var (esValido, mensaje, productoIdReal) = await ValidarProductoActivoAsync(
                detalle.Producto_Id, 
                detalle.Activo_Id);

            if (!esValido)
            {
                return (false, mensaje, detalleNormalizado);
            }

            // Auto-completar Producto_Id si solo se especificó Activo_Id
            if (detalle.Activo_Id.HasValue && !detalle.Producto_Id.HasValue && productoIdReal.HasValue)
            {
                detalleNormalizado.Producto_Id = productoIdReal.Value;
                _logger.LogInformation(
                    "Auto-completado Producto_Id={ProductoId} para Activo_Id={ActivoId}", 
                    productoIdReal.Value, 
                    detalle.Activo_Id.Value);
            }

            // Validar cantidad cuando es reserva de activo específico
            if (detalle.Activo_Id.HasValue && detalle.Cantidad != 1)
            {
                return (false, 
                    "Cuando se reserva un activo específico, la cantidad debe ser 1", 
                    detalleNormalizado);
            }

            // Validar que Subtotal sea coherente
            var subtotalEsperado = detalle.Precio_Unitario * detalle.Cantidad * detalle.Dias_Alquiler;
            if (Math.Abs(detalle.Subtotal - subtotalEsperado) > 0.01m)
            {
                _logger.LogWarning(
                    "Subtotal inconsistente. Esperado: {Esperado}, Recibido: {Recibido}", 
                    subtotalEsperado, 
                    detalle.Subtotal);
                
                detalleNormalizado.Subtotal = subtotalEsperado;
            }

            return (true, "Detalle validado y normalizado correctamente", detalleNormalizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar y normalizar DetalleReserva");
            return (false, $"Error de validación: {ex.Message}", detalle);
        }
    }
}
