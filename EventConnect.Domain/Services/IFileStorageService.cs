namespace EventConnect.Domain.Services;

/// <summary>
/// Interfaz para servicios de almacenamiento de archivos
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo en el almacenamiento
    /// </summary>
    /// <param name="file">Archivo a guardar</param>
    /// <param name="folder">Carpeta donde guardar (ej: "evidencias", "productos")</param>
    /// <param name="fileName">Nombre del archivo (opcional, se genera automáticamente si es null)</param>
    /// <returns>URL relativa del archivo guardado</returns>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string folder, string? fileName = null);

    /// <summary>
    /// Elimina un archivo del almacenamiento
    /// </summary>
    /// <param name="fileUrl">URL relativa del archivo</param>
    /// <returns>True si se eliminó exitosamente</returns>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Valida si un archivo es una imagen válida
    /// </summary>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="contentType">Tipo de contenido</param>
    /// <returns>True si es una imagen válida</returns>
    bool IsValidImage(string fileName, string contentType);

    /// <summary>
    /// Obtiene el tamaño máximo permitido para archivos (en bytes)
    /// </summary>
    long MaxFileSize { get; }
}
