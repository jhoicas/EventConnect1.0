using EventConnect.Domain.Services;
using Microsoft.AspNetCore.Hosting;

namespace EventConnect.Infrastructure.Services;

/// <summary>
/// Implementación de almacenamiento local de archivos
/// Guarda archivos en wwwroot/{folder}
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedImageContentTypes = { 
        "image/jpeg", "image/png", "image/gif", "image/webp" 
    };

    public long MaxFileSize => MAX_FILE_SIZE;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream, 
        string originalFileName, 
        string folder, 
        string? fileName = null)
    {
        try
        {
            // Validar que es una imagen
            if (!IsValidImage(originalFileName, ""))
            {
                throw new ArgumentException($"El archivo {originalFileName} no es una imagen válida");
            }

            // Generar nombre único si no se proporciona
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var extension = Path.GetExtension(originalFileName);
                fileName = $"{Guid.NewGuid()}{extension}";
            }

            // Crear ruta del directorio
            var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("Directorio creado: {Path}", uploadPath);
            }

            // Ruta completa del archivo
            var filePath = Path.Combine(uploadPath, fileName);

            // Guardar archivo
            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            // Retornar URL relativa (compatible con wwwroot)
            var relativeUrl = $"/{folder}/{fileName}";
            _logger.LogInformation("Archivo guardado: {FilePath}, URL: {Url}", filePath, relativeUrl);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo {FileName} en carpeta {Folder}", originalFileName, folder);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            // Remover la barra inicial si existe
            var cleanUrl = fileUrl.TrimStart('/');
            
            // Construir ruta física
            var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, cleanUrl);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Archivo eliminado: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("Archivo no encontrado para eliminar: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo {FileUrl}", fileUrl);
            return false;
        }
    }

    public bool IsValidImage(string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        // Validar extensión
        if (!AllowedImageExtensions.Contains(extension))
            return false;

        // Si se proporciona contentType, validarlo también
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (!AllowedImageContentTypes.Contains(contentType.ToLowerInvariant()))
                return false;
        }

        return true;
    }
}
