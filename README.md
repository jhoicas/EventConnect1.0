#  EventConnect - Sistema de Gestión de Eventos y Mobiliario

Sistema integral de gestión empresarial desarrollado con **.NET 9.0** y **Clean Architecture**, que incluye módulos de alquiler de mobiliario y el Sistema Integrado de Gestión de Inventarios (SIGI).

##  Tabla de Contenidos

- [Características](#-características)
- [Arquitectura](#-arquitectura)
- [Tecnologías](#-tecnologías)
- [Requisitos](#-requisitos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Endpoints API](#-endpoints-api)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Servicios de Aplicación](#-servicios-de-aplicación)

##  Características

### Módulo Core (Alquiler de Mobiliario)
-  Gestión de categorías y productos
-  Sistema de reservas y clientes
-  Autenticación JWT con roles (SuperAdmin, Admin, Usuario)
-  Multi-tenant (soporte de múltiples empresas)

### Módulo SIGI (Sistema Integrado de Gestión de Inventarios)
-  Gestión de activos fijos
-  Control de bodegas y ubicaciones
-  Administración de lotes con fechas de vencimiento
-  Movimientos de inventario (entradas/salidas/transferencias)
-  Programación y seguimiento de mantenimientos

### Servicios de Aplicación
-  **AuditoriaService**: Registro de acciones con integridad SHA-256
-  **DepreciacionService**: Cálculo automático de depreciación de activos
-  **NotificacionService**: Notificaciones multi-canal (Email/SMS/Push)

##  Arquitectura

El proyecto implementa **Clean Architecture** con 4 capas:

```

      EventConnect.API                  Controladores y Middleware
      (ASP.NET Core Web API)         

              

   EventConnect.Application             Servicios de Negocio
   (Lógica de Aplicación)            

              

   EventConnect.Infrastructure          Repositorios y Datos
   (Dapper + MySQL)                  

              

      EventConnect.Domain               Entidades y Contratos
      (Modelos y Repositorios)       

```

##  Tecnologías

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **MySQL 8.0** - Base de datos
- **Dapper** - Micro-ORM para acceso a datos
- **JWT Bearer** - Autenticación y autorización
- **BCrypt.Net** - Hash de contraseñas
- **Swagger/OpenAPI** - Documentación interactiva de API

##  Requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL 8.0+](https://dev.mysql.com/downloads/)
- Visual Studio 2022 / VS Code / Rider

##  Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/jhoicas/EventConnect.git
cd EventConnect
```

### 2. Configurar la base de datos

Ejecutar los scripts SQL en orden:

```bash
mysql -u root -p < database/EJECUTAR_PRIMERO.sql
mysql -u root -p < database/RENOMBRAR_TABLAS.sql
mysql -u root -p < database/crear_tablas_faltantes.sql
mysql -u root -p < database/schema_completo.sql
```

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Compilar el proyecto

```bash
dotnet build
```

### 5. Ejecutar la aplicación

```bash
cd EventConnect.API
dotnet run
```

La API estará disponible en: **http://localhost:5555**

##  Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "EventConnectConnection": "Server=localhost;Port=3306;Database=db_eventconnect;User=root;Password=root;"
  },
  "Jwt": {
    "Key": "tu-clave-secreta-super-segura-de-al-menos-32-caracteres",
    "Issuer": "EventConnectAPI",
    "Audience": "EventConnectClients",
    "ExpiryMinutes": 1440
  }
}
```

### Usuarios Predeterminados

| Usuario      | Contraseña      | Rol         |
|--------------|-----------------|-------------|
| superadmin   | SuperAdmin123$  | SuperAdmin  |
| admin        | Admin123$       | Admin       |
| usuario      | Usuario123$     | Usuario     |

##  Endpoints API

###  Autenticación

| Método | Endpoint          | Descripción              |
|--------|-------------------|--------------------------|
| POST   | /api/Auth/login   | Login y obtención de JWT |
| POST   | /api/Auth/register| Registro de usuarios     |

###  Módulo Core

| Método | Endpoint              | Descripción                    |
|--------|-----------------------|--------------------------------|
| GET    | /api/Categoria        | Listar categorías              |
| POST   | /api/Categoria        | Crear categoría                |
| GET    | /api/Producto         | Listar productos               |
| POST   | /api/Producto         | Crear producto                 |
| GET    | /api/Cliente          | Listar clientes                |
| POST   | /api/Cliente          | Crear cliente                  |
| GET    | /api/Reserva          | Listar reservas                |
| POST   | /api/Reserva          | Crear reserva                  |

###  Módulo SIGI

| Método | Endpoint                    | Descripción                        |
|--------|-----------------------------|-------------------------------------|
| GET    | /api/Activo                 | Listar activos                     |
| POST   | /api/Activo                 | Crear activo                       |
| GET    | /api/Bodega                 | Listar bodegas                     |
| POST   | /api/Bodega                 | Crear bodega                       |
| GET    | /api/Lote                   | Listar lotes                       |
| POST   | /api/Lote                   | Crear lote                         |
| GET    | /api/Lote/proximos-vencer   | Lotes próximos a vencer            |
| GET    | /api/MovimientoInventario   | Listar movimientos                 |
| POST   | /api/MovimientoInventario   | Registrar movimiento               |
| GET    | /api/Mantenimiento          | Listar mantenimientos              |
| GET    | /api/Mantenimiento/pendientes| Mantenimientos pendientes         |
| GET    | /api/Mantenimiento/vencidos | Mantenimientos vencidos            |

###  Documentación Interactiva

Swagger UI disponible en: **http://localhost:5555/swagger**

##  Estructura del Proyecto

```
EventConnect/
 EventConnect.API/                    # Capa de Presentación
    Controllers/                     # 11 Controladores REST
       AuthController.cs
       CategoriaController.cs
       ProductoController.cs
       ClienteController.cs
       ReservaController.cs
       ActivoController.cs         # SIGI
       BodegaController.cs         # SIGI
       LoteController.cs           # SIGI
       MovimientoInventarioController.cs  # SIGI
       MantenimientoController.cs  # SIGI
    Middleware/
       GlobalExceptionHandler.cs
    Program.cs                       # Configuración y DI

 EventConnect.Application/            # Capa de Aplicación
    Services/
        IAuditoriaService.cs
        IDepreciacionService.cs
        INotificacionService.cs
        Implementation/
            AuditoriaService.cs      # SHA-256 integrity
            DepreciacionService.cs   # Cálculo depreciación
            NotificacionService.cs   # Multi-canal

 EventConnect.Infrastructure/         # Capa de Infraestructura
    Repositories/
       RepositoryBase.cs            # Clase base genérica
       UsuarioRepository.cs
       CategoriaRepository.cs
       ProductoRepository.cs
       ActivoRepository.cs          # SIGI
       BodegaRepository.cs          # SIGI
       LoteRepository.cs            # SIGI
       ... (17 repositorios total)
    Services/
        AuthService.cs               # BCrypt hashing

 EventConnect.Domain/                 # Capa de Dominio
    Entities/                        # 19 Entidades
       Usuario.cs
       Categoria.cs
       Producto.cs
       Activo.cs                    # SIGI
       Bodega.cs                    # SIGI
       ...
    Configuration/
        AppSettings.cs

 database/                            # Scripts SQL
     EJECUTAR_PRIMERO.sql
     RENOMBRAR_TABLAS.sql
     crear_tablas_faltantes.sql
     schema_completo.sql              # 30 tablas
```

##  Servicios de Aplicación

### AuditoriaService
Registra todas las acciones críticas del sistema con hash SHA-256 para garantizar integridad.

```csharp
await _auditoriaService.RegistrarAccionAsync(
    usuarioId: 1,
    accion: "LOGIN",
    entidadAfectada: "Usuario",
    detalles: "Login exitoso desde IP: 192.168.1.1"
);
```

### DepreciacionService
Calcula automáticamente la depreciación de activos fijos.

```csharp
// Calcular depreciación mensual de todos los activos
await _depreciacionService.CalcularDepreciacionMensualAsync();

// Obtener valor en libros de un activo
var valorLibros = await _depreciacionService.ObtenerValorLibrosAsync(activoId: 5);
```

### NotificacionService
Envía notificaciones a través de múltiples canales.

```csharp
// Notificar lotes próximos a vencer
await _notificacionService.NotificarLotesProximosVencerAsync(
    empresaId: 1,
    dias: 7
);

// Enviar notificación personalizada
await _notificacionService.EnviarNotificacionAsync(
    usuarioId: 10,
    titulo: "Alerta de Stock",
    mensaje: "El producto X está por debajo del stock mínimo",
    tipo: "Sistema"
);
```

##  Base de Datos

### Estadísticas
- **30 tablas** principales
- **19 entidades** de dominio
- **Vistas**: 3 vistas materializadas
- **Triggers**: 5 triggers para auditoría
- **Stored Procedures**: 8 procedimientos almacenados

### Tablas Principales

**Módulo Core:**
- Usuario, Empresa, Categoria, Producto
- Cliente, Reserva, Detalle_Reserva
- Contenido_Landing

**Módulo SIGI:**
- Activo, Bodega, Lote
- Movimiento_Inventario, Mantenimiento
- Depreciacion, Notificacion, Log_Auditoria

##  Seguridad

-  Autenticación JWT con tokens Bearer
-  Hash de contraseñas con BCrypt (salt rounds: 12)
-  Roles y permisos (SuperAdmin, Admin, Usuario)
-  Auditoría con integridad SHA-256
-  Validación de entrada en todos los endpoints
-  Multi-tenancy por Empresa_Id

##  Testing

```bash
# Ejecutar todas las pruebas
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

##  Estado del Proyecto

**Backend API:**  100% Completo (7/7 endpoints funcionando)

| Componente                | Estado | Progreso |
|---------------------------|--------|----------|
| Database Schema           |      | 100%     |
| Domain Layer              |      | 100%     |
| Infrastructure Layer      |      | 100%     |
| Application Layer         |      | 100%     |
| API Controllers           |      | 100%     |
| Authentication (JWT)      |      | 100%     |
| Documentation             |      | 100%     |
| Frontend (Next.js)        |      | Pendiente|

##  Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

##  Licencia

Este proyecto está bajo la Licencia MIT.

##  Autor

**Yoiner Castillo**
- GitHub: [@jhoicas](https://github.com/jhoicas)

##  Agradecimientos

- Clean Architecture por Robert C. Martin
- Dapper por Marc Gravell
- ASP.NET Core Team

---

 Si este proyecto te fue útil, considera darle una estrella en GitHub!
