# Dockerfile para DigitalOcean
# El build context es EventConnect.API, pero necesitamos los proyectos padre
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar desde el directorio padre (..) ya que el context es EventConnect.API
COPY ../EventConnect.sln .
COPY ../EventConnect.Domain/EventConnect.Domain.csproj EventConnect.Domain/
COPY ../EventConnect.Application/EventConnect.Application.csproj EventConnect.Application/
COPY ../EventConnect.Infrastructure/EventConnect.Infrastructure.csproj EventConnect.Infrastructure/
COPY EventConnect.API.csproj EventConnect.API/

# Restaurar dependencias
RUN dotnet restore EventConnect.sln

# Copiar todo el código fuente
COPY ../EventConnect.Domain/ EventConnect.Domain/
COPY ../EventConnect.Application/ EventConnect.Application/
COPY ../EventConnect.Infrastructure/ EventConnect.Infrastructure/
COPY . EventConnect.API/

# Compilar y publicar
WORKDIR /src/EventConnect.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventConnect.API.dll"]
