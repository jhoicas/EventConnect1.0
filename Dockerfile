# Dockerfile para DigitalOcean - Backend API
# Optimizado para producción siguiendo mejores prácticas de .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos (capa de cache)
COPY EventConnect.sln .
COPY EventConnect.API/EventConnect.API.csproj EventConnect.API/
COPY EventConnect.Domain/EventConnect.Domain.csproj EventConnect.Domain/
COPY EventConnect.Application/EventConnect.Application.csproj EventConnect.Application/
COPY EventConnect.Infrastructure/EventConnect.Infrastructure.csproj EventConnect.Infrastructure/

# Restaurar dependencias (caché esta capa si no cambian los .csproj)
RUN dotnet restore EventConnect.sln --verbosity quiet

# Copiar todo el código fuente
COPY EventConnect.API/ EventConnect.API/
COPY EventConnect.Domain/ EventConnect.Domain/
COPY EventConnect.Application/ EventConnect.Application/
COPY EventConnect.Infrastructure/ EventConnect.Infrastructure/

# Compilar y publicar (optimizado para producción)
WORKDIR /src/EventConnect.API
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage (imagen más pequeña)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar solo los archivos publicados
COPY --from=build /app/publish .

# Cambiar ownership al usuario no-root
RUN chown -R appuser:appuser /app

# Cambiar a usuario no-root
USER appuser

# Exponer puerto
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "EventConnect.API.dll"]
