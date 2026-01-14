# Dockerfile para DigitalOcean
# Nota: Este Dockerfile debe estar en EventConnect.API pero copia desde la raíz
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos
COPY EventConnect.sln .
COPY EventConnect.API/EventConnect.API.csproj EventConnect.API/
COPY EventConnect.Domain/EventConnect.Domain.csproj EventConnect.Domain/
COPY EventConnect.Application/EventConnect.Application.csproj EventConnect.Application/
COPY EventConnect.Infrastructure/EventConnect.Infrastructure.csproj EventConnect.Infrastructure/

# Restaurar dependencias
RUN dotnet restore EventConnect.sln

# Copiar todo el código fuente
COPY EventConnect.API/ EventConnect.API/
COPY EventConnect.Domain/ EventConnect.Domain/
COPY EventConnect.Application/ EventConnect.Application/
COPY EventConnect.Infrastructure/ EventConnect.Infrastructure/

# Compilar y publicar
WORKDIR /src/EventConnect.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventConnect.API.dll"]
