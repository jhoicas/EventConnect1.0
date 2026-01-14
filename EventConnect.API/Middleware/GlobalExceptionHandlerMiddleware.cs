using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventConnect.API.Middleware;

/// <summary>
/// Middleware global para manejo centralizado de excepciones
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError; // 500 if unexpected
        var result = string.Empty;

        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { message = "No autorizado", error = exception.Message });
                break;
            case ArgumentNullException argEx:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { message = "Parámetro requerido faltante", error = argEx.Message });
                break;
            case ArgumentException argEx:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { message = "Parámetro inválido", error = argEx.Message });
                break;
            case InvalidOperationException invalidOpEx:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { message = "Operación inválida", error = invalidOpEx.Message });
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { message = "Recurso no encontrado" });
                break;
            default:
                // Para excepciones no esperadas, no exponer detalles en producción
                var isDevelopment = context.RequestServices
                    .GetRequiredService<IHostEnvironment>().IsDevelopment();
                
                result = JsonSerializer.Serialize(new
                {
                    message = "Ha ocurrido un error interno del servidor",
                    error = isDevelopment ? exception.Message : "Error interno del servidor",
                    stackTrace = isDevelopment ? exception.StackTrace : null
                });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
