using EventConnect.Domain.DTOs;
using EventConnect.Domain.Entities;
using EventConnect.Domain.Services;
using Dapper;
using Npgsql;

namespace EventConnect.Infrastructure.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly string _connectionString;

    public AuditoriaService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> RegistrarCambioAsync(
        string tablaAfectada,
        int registroId,
        int usuarioId,
        string accion,
        string datosNuevos,
        string? datosAnteriores = null,
        string? detalles = null,
        string? ipOrigen = null,
        string? userAgent = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Auditoria 
                (Tabla_Afectada, Registro_Id, Usuario_Id, Accion, Datos_Anteriores, Datos_Nuevos, 
                 Detalles, IP_Origen, User_Agent, Fecha_Accion)
                VALUES 
                (@TablaAfectada, @RegistroId, @UsuarioId, @Accion, @DatosAnteriores, @DatosNuevos,
                 @Detalles, @IPOrigen, @UserAgent, @FechaAccion);";

            var result = await connection.ExecuteAsync(sql, new
            {
                TablaAfectada = tablaAfectada,
                RegistroId = registroId,
                UsuarioId = usuarioId,
                Accion = accion,
                DatosAnteriores = datosAnteriores,
                DatosNuevos = datosNuevos,
                Detalles = detalles,
                IPOrigen = ipOrigen,
                UserAgent = userAgent,
                FechaAccion = DateTime.Now
            });

            return result > 0;
        }
        catch (Exception ex)
        {
            // Log el error pero no interrumpir el flujo principal
            System.Diagnostics.Debug.WriteLine($"Error registrando auditoría: {ex.Message}");
            return false;
        }
    }

    public async Task<HistorialResponse?> ObtenerHistorialAsync(string tablaAfectada, int registroId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    a.Id, a.Tabla_Afectada, a.Registro_Id, a.Usuario_Id, a.Accion,
                    a.Datos_Anteriores, a.Datos_Nuevos, a.Detalles, a.IP_Origen,
                    a.Fecha_Accion,
                    u.Usuario as Usuario_Nombre,
                    u.Email as Usuario_Email
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                WHERE a.Tabla_Afectada = @TablaAfectada 
                  AND a.Registro_Id = @RegistroId
                ORDER BY a.Fecha_Accion ASC;";

            var auditorias = await connection.QueryAsync<dynamic>(sql, new
            {
                TablaAfectada = tablaAfectada,
                RegistroId = registroId
            });

            if (!auditorias.Any())
                return null;

            var auditoriaList = auditorias.Cast<dynamic>().ToList();

            var timeline = auditoriaList.Select(a => new AuditoriaDto
            {
                Id = (int)a.id,
                Tabla_Afectada = a.tabla_afectada,
                Registro_Id = (int)a.registro_id,
                Usuario_Id = (int)a.usuario_id,
                Usuario_Nombre = a.usuario_nombre ?? "Sistema",
                Usuario_Email = a.usuario_email ?? "",
                Accion = a.accion,
                Datos_Anteriores = a.datos_anteriores,
                Datos_Nuevos = a.datos_nuevos,
                Detalles = a.detalles,
                IP_Origen = a.ip_origen,
                Fecha_Accion = (DateTime)a.fecha_accion
            }).ToList();

            var primerCambio = timeline.First();
            var ultimoCambio = timeline.Last();

            var usuarioCrea = timeline.FirstOrDefault(t => t.Accion == "Create");
            var usuarioUltimo = timeline.LastOrDefault();

            return new HistorialResponse
            {
                Registro_Id = registroId,
                Tabla_Afectada = tablaAfectada,
                Tipo_Entidad = tablaAfectada,
                Timeline = timeline,
                Total_Cambios = timeline.Count,
                Primer_Cambio = primerCambio.Fecha_Accion,
                Ultimo_Cambio = ultimoCambio.Fecha_Accion,
                Usuario_Creacion = usuarioCrea?.Usuario_Nombre ?? "Desconocido",
                Usuario_Ultima_Actualizacion = usuarioUltimo?.Usuario_Nombre ?? "Desconocido"
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo historial: {ex.Message}");
            return null;
        }
    }

    public async Task<PaginatedAuditoriaResponse<AuditoriaDto>> ObtenerHistorialFiltradoAsync(FiltroAuditoriaRequest filtro)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(filtro.Tabla_Afectada))
            {
                whereClause += " AND a.Tabla_Afectada = @TablaAfectada";
                parameters.Add("@TablaAfectada", filtro.Tabla_Afectada);
            }

            if (filtro.Registro_Id.HasValue)
            {
                whereClause += " AND a.Registro_Id = @RegistroId";
                parameters.Add("@RegistroId", filtro.Registro_Id.Value);
            }

            if (filtro.Usuario_Id.HasValue)
            {
                whereClause += " AND a.Usuario_Id = @UsuarioId";
                parameters.Add("@UsuarioId", filtro.Usuario_Id.Value);
            }

            if (!string.IsNullOrEmpty(filtro.Accion))
            {
                whereClause += " AND a.Accion = @Accion";
                parameters.Add("@Accion", filtro.Accion);
            }

            if (filtro.Desde.HasValue)
            {
                whereClause += " AND a.Fecha_Accion >= @Desde";
                parameters.Add("@Desde", filtro.Desde.Value);
            }

            if (filtro.Hasta.HasValue)
            {
                whereClause += " AND a.Fecha_Accion <= @Hasta";
                parameters.Add("@Hasta", filtro.Hasta.Value);
            }

            // Contar total
            var countSql = $@"SELECT COUNT(*) FROM Auditoria a {whereClause};";
            var totalRegistros = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // Obtener datos paginados
            var offset = (filtro.Pagina - 1) * filtro.Registros_Por_Pagina;
            var dataSql = $@"
                SELECT 
                    a.Id, a.Tabla_Afectada, a.Registro_Id, a.Usuario_Id, a.Accion,
                    a.Datos_Anteriores, a.Datos_Nuevos, a.Detalles, a.IP_Origen,
                    a.Fecha_Accion,
                    u.Usuario as Usuario_Nombre,
                    u.Email as Usuario_Email
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                {whereClause}
                ORDER BY a.Fecha_Accion DESC
                LIMIT @Limit OFFSET @Offset;";

            parameters.Add("@Limit", filtro.Registros_Por_Pagina);
            parameters.Add("@Offset", offset);

            var auditorias = await connection.QueryAsync<dynamic>(dataSql, parameters);

            var data = auditorias.Select(a => new AuditoriaDto
            {
                Id = (int)a.id,
                Tabla_Afectada = a.tabla_afectada,
                Registro_Id = (int)a.registro_id,
                Usuario_Id = (int)a.usuario_id,
                Usuario_Nombre = a.usuario_nombre ?? "Sistema",
                Usuario_Email = a.usuario_email ?? "",
                Accion = a.accion,
                Datos_Anteriores = a.datos_anteriores,
                Datos_Nuevos = a.datos_nuevos,
                Detalles = a.detalles,
                IP_Origen = a.ip_origen,
                Fecha_Accion = (DateTime)a.fecha_accion
            }).ToList();

            var totalPaginas = (int)Math.Ceiling((decimal)totalRegistros / filtro.Registros_Por_Pagina);

            return new PaginatedAuditoriaResponse<AuditoriaDto>
            {
                Data = data,
                Total_Registros = totalRegistros,
                Pagina_Actual = filtro.Pagina,
                Total_Paginas = totalPaginas,
                Registros_Por_Pagina = filtro.Registros_Por_Pagina
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo historial filtrado: {ex.Message}");
            return new PaginatedAuditoriaResponse<AuditoriaDto>();
        }
    }

    public async Task<ResumenAuditoriaResponse?> ObtenerResumenAsync(string tablaAfectada, int registroId)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    Tabla_Afectada, Registro_Id,
                    COUNT(*) as Total_Cambios,
                    SUM(CASE WHEN Accion = 'Delete' THEN 1 ELSE 0 END) as Total_Eliminaciones,
                    SUM(CASE WHEN Accion = 'Create' THEN 1 ELSE 0 END) as Total_Creaciones,
                    SUM(CASE WHEN Accion = 'Update' THEN 1 ELSE 0 END) as Total_Actualizaciones,
                    MIN(Fecha_Accion) as Primer_Cambio,
                    MAX(Fecha_Accion) as Ultimo_Cambio
                FROM Auditoria
                WHERE Tabla_Afectada = @TablaAfectada 
                  AND Registro_Id = @RegistroId
                GROUP BY Tabla_Afectada, Registro_Id;";

            var resumen = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
            {
                TablaAfectada = tablaAfectada,
                RegistroId = registroId
            });

            if (resumen == null)
                return null;

            // Obtener usuarios
            var usuariosSql = @"
                SELECT DISTINCT u.Usuario, COUNT(*) as Cambios
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                WHERE a.Tabla_Afectada = @TablaAfectada 
                  AND a.Registro_Id = @RegistroId
                GROUP BY u.Usuario
                ORDER BY COUNT(*) DESC;";

            var usuarios = await connection.QueryAsync<dynamic>(usuariosSql, new
            {
                TablaAfectada = tablaAfectada,
                RegistroId = registroId
            });

            var cambiosPorUsuario = usuarios
                .ToDictionary(u => (string)(u.usuario ?? "Sistema"), u => (int)u.cambios);

            var ultimosUsuarios = usuarios
                .Take(5)
                .Select(u => (string)(u.usuario ?? "Sistema"))
                .ToList();

            return new ResumenAuditoriaResponse
            {
                Registro_Id = registroId,
                Tabla_Afectada = tablaAfectada,
                Total_Cambios = (int)resumen.total_cambios,
                Total_Eliminaciones = (int)(resumen.total_eliminaciones ?? 0),
                Total_Creaciones = (int)(resumen.total_creaciones ?? 0),
                Total_Actualizaciones = (int)(resumen.total_actualizaciones ?? 0),
                Primer_Cambio = (DateTime)resumen.primer_cambio,
                Ultimo_Cambio = (DateTime)resumen.ultimo_cambio,
                Cambios_Por_Usuario = cambiosPorUsuario,
                Ultimos_Usuarios = ultimosUsuarios
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo resumen: {ex.Message}");
            return null;
        }
    }

    public async Task<List<AuditoriaDto>> ObtenerCambiosRecientesAsync(int top = 50)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    a.Id, a.Tabla_Afectada, a.Registro_Id, a.Usuario_Id, a.Accion,
                    a.Datos_Anteriores, a.Datos_Nuevos, a.Detalles, a.IP_Origen,
                    a.Fecha_Accion,
                    u.Usuario as Usuario_Nombre,
                    u.Email as Usuario_Email
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                ORDER BY a.Fecha_Accion DESC
                LIMIT @Top;";

            var auditorias = await connection.QueryAsync<dynamic>(sql, new { Top = top });

            return auditorias.Select(a => new AuditoriaDto
            {
                Id = (int)a.id,
                Tabla_Afectada = a.tabla_afectada,
                Registro_Id = (int)a.registro_id,
                Usuario_Id = (int)a.usuario_id,
                Usuario_Nombre = a.usuario_nombre ?? "Sistema",
                Usuario_Email = a.usuario_email ?? "",
                Accion = a.accion,
                Datos_Anteriores = a.datos_anteriores,
                Datos_Nuevos = a.datos_nuevos,
                Detalles = a.detalles,
                IP_Origen = a.ip_origen,
                Fecha_Accion = (DateTime)a.fecha_accion
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo cambios recientes: {ex.Message}");
            return new List<AuditoriaDto>();
        }
    }

    public async Task<List<AuditoriaDto>> ObtenerCambiosPorUsuarioAsync(int usuarioId, int top = 50)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    a.Id, a.Tabla_Afectada, a.Registro_Id, a.Usuario_Id, a.Accion,
                    a.Datos_Anteriores, a.Datos_Nuevos, a.Detalles, a.IP_Origen,
                    a.Fecha_Accion,
                    u.Usuario as Usuario_Nombre,
                    u.Email as Usuario_Email
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                WHERE a.Usuario_Id = @UsuarioId
                ORDER BY a.Fecha_Accion DESC
                LIMIT @Top;";

            var auditorias = await connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId, Top = top });

            return auditorias.Select(a => new AuditoriaDto
            {
                Id = (int)a.id,
                Tabla_Afectada = a.tabla_afectada,
                Registro_Id = (int)a.registro_id,
                Usuario_Id = (int)a.usuario_id,
                Usuario_Nombre = a.usuario_nombre ?? "Sistema",
                Usuario_Email = a.usuario_email ?? "",
                Accion = a.accion,
                Datos_Anteriores = a.datos_anteriores,
                Datos_Nuevos = a.datos_nuevos,
                Detalles = a.detalles,
                IP_Origen = a.ip_origen,
                Fecha_Accion = (DateTime)a.fecha_accion
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo cambios por usuario: {ex.Message}");
            return new List<AuditoriaDto>();
        }
    }

    public async Task<List<AuditoriaDto>> BuscarAsync(string termino, string? tablaAfectada = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClause = @"
                WHERE (a.Datos_Nuevos ILIKE @Termino 
                   OR a.Datos_Anteriores ILIKE @Termino 
                   OR a.Detalles ILIKE @Termino
                   OR u.Usuario ILIKE @Termino)";

            if (!string.IsNullOrEmpty(tablaAfectada))
                whereClause += " AND a.Tabla_Afectada = @TablaAfectada";

            var sql = $@"
                SELECT 
                    a.Id, a.Tabla_Afectada, a.Registro_Id, a.Usuario_Id, a.Accion,
                    a.Datos_Anteriores, a.Datos_Nuevos, a.Detalles, a.IP_Origen,
                    a.Fecha_Accion,
                    u.Usuario as Usuario_Nombre,
                    u.Email as Usuario_Email
                FROM Auditoria a
                LEFT JOIN Usuario u ON a.Usuario_Id = u.Id
                {whereClause}
                ORDER BY a.Fecha_Accion DESC
                LIMIT 100;";

            var auditorias = await connection.QueryAsync<dynamic>(sql, new
            {
                Termino = $"%{termino}%",
                TablaAfectada = tablaAfectada
            });

            return auditorias.Select(a => new AuditoriaDto
            {
                Id = (int)a.id,
                Tabla_Afectada = a.tabla_afectada,
                Registro_Id = (int)a.registro_id,
                Usuario_Id = (int)a.usuario_id,
                Usuario_Nombre = a.usuario_nombre ?? "Sistema",
                Usuario_Email = a.usuario_email ?? "",
                Accion = a.accion,
                Datos_Anteriores = a.datos_anteriores,
                Datos_Nuevos = a.datos_nuevos,
                Detalles = a.detalles,
                IP_Origen = a.ip_origen,
                Fecha_Accion = (DateTime)a.fecha_accion
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en búsqueda de auditoría: {ex.Message}");
            return new List<AuditoriaDto>();
        }
    }

    public async Task<int> LimpiarAuditoriaAntiguaAsync(int diasAntiguos = 90)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                DELETE FROM Auditoria
                WHERE Fecha_Accion < NOW() - INTERVAL '1 day' * @DiasAntiguos;";

            var resultado = await connection.ExecuteAsync(sql, new { DiasAntiguos = diasAntiguos });
            return resultado;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error limpiando auditoría antigua: {ex.Message}");
            return 0;
        }
    }
}
