using System.Text.Json;
using EventConnect.Domain.Services;

namespace EventConnect.API.Helpers;

/// <summary>
/// Helper para registrar cambios de auditoría en los controllers
/// Simplifica la integración del sistema de auditoría
/// </summary>
public class AuditoriaHelper
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaHelper(IAuditoriaService auditoriaService, IHttpContextAccessor httpContextAccessor)
    {
        _auditoriaService = auditoriaService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Registra un cambio de creación
    /// </summary>
    public async Task RegistrarCreacionAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        object datosNuevos,
        string? detalles = null)
    {
        await RegistrarCambioAsync(
            tablaAfectada,
            registroId,
            usuarioId,
            "Create",
            datosNuevos,
            datosAnteriores: null,
            detalles: detalles ?? $"{tablaAfectada} creado");
    }

    /// <summary>
    /// Registra un cambio de actualización
    /// </summary>
    public async Task RegistrarActualizacionAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        object datosAnteriores,
        object datosNuevos,
        string? detalles = null)
    {
        await RegistrarCambioAsync(
            tablaAfectada,
            registroId,
            usuarioId,
            "Update",
            datosNuevos,
            datosAnteriores: datosAnteriores,
            detalles: detalles ?? $"{tablaAfectada} actualizado");
    }

    /// <summary>
    /// Registra un cambio de estado
    /// </summary>
    public async Task RegistrarCambioEstadoAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        string estadoAnterior,
        string estadoNuevo,
        string? detalles = null)
    {
        var datosAnteriores = new { Estado = estadoAnterior };
        var datosNuevos = new { Estado = estadoNuevo };

        await RegistrarCambioAsync(
            tablaAfectada,
            registroId,
            usuarioId,
            "StatusChange",
            datosNuevos,
            datosAnteriores: datosAnteriores,
            detalles: detalles ?? $"Estado cambiado de '{estadoAnterior}' a '{estadoNuevo}'");
    }

    /// <summary>
    /// Registra un cambio de eliminación
    /// </summary>
    public async Task RegistrarEliminacionAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        object datosAnteriores,
        string? detalles = null)
    {
        await RegistrarCambioAsync(
            tablaAfectada,
            registroId,
            usuarioId,
            "Delete",
            datosNuevos: new { Eliminado = true },
            datosAnteriores: datosAnteriores,
            detalles: detalles ?? $"{tablaAfectada} eliminado");
    }

    /// <summary>
    /// Registra un evento de entrega
    /// </summary>
    public async Task RegistrarEntregaAsync(
        int registroId,
        int usuarioId,
        object detallesEntrega,
        string? observaciones = null)
    {
        await RegistrarCambioAsync(
            "Logistica",
            registroId,
            usuarioId,
            "Entrega",
            detallesEntrega,
            detalles: observaciones ?? "Entrega realizada");
    }

    /// <summary>
    /// Registra un evento de devolución
    /// </summary>
    public async Task RegistrarDevolucionAsync(
        int registroId,
        int usuarioId,
        object detallesDevolucio,
        string? observaciones = null)
    {
        await RegistrarCambioAsync(
            "Logistica",
            registroId,
            usuarioId,
            "Devolución",
            detallesDevolucio,
            detalles: observaciones ?? "Devolución recibida");
    }

    /// <summary>
    /// Método privado para registrar cambios
    /// </summary>
    private async Task RegistrarCambioAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        string accion,
        object datosNuevos,
        object? datosAnteriores = null,
        string? detalles = null)
    {
        try
        {
            var ipOrigen = ObtenerIPOrigen();
            var userAgent = ObtenerUserAgent();

            var datosNuevosJson = SerializarDatos(datosNuevos);
            var datosAnterioresJson = datosAnteriores != null ? SerializarDatos(datosAnteriores) : null;

            await _auditoriaService.RegistrarCambioAsync(
                tablaAfectada,
                registroId,
                usuarioId,
                accion,
                datosNuevosJson,
                datosAnterioresJson,
                detalles,
                ipOrigen,
                userAgent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error registrando auditoría: {ex.Message}");
            // No lanzar excepción para no interrumpir el flujo principal
        }
    }

    /// <summary>
    /// Serializa un objeto a JSON
    /// </summary>
    private string SerializarDatos(object datos)
    {
        try
        {
            if (datos is string str)
                return str;
            return JsonSerializer.Serialize(datos, new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return JsonSerializer.Serialize(new { Error = "No se pudo serializar" });
        }
    }

    /// <summary>
    /// Obtiene la dirección IP del cliente
    /// </summary>
    private string ObtenerIPOrigen()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return "Desconocida";

            var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(remoteIpAddress) && context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                remoteIpAddress = context.Request.Headers["X-Forwarded-For"].ToString().Split(',').First();
            }

            return remoteIpAddress ?? "Desconocida";
        }
        catch
        {
            return "Desconocida";
        }
    }

    /// <summary>
    /// Obtiene el User Agent del cliente
    /// </summary>
    private string ObtenerUserAgent()
    {
        try
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            return userAgent ?? "Desconocido";
        }
        catch
        {
            return "Desconocido";
        }
    }
}
