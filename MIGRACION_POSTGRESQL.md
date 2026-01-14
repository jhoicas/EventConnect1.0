# Migración de MySQL a PostgreSQL - EventConnect Backend

## 🔄 Estado de la Migración

Esta migración está en progreso. Se requiere actualizar múltiples archivos para cambiar de MySQL a PostgreSQL.

## 📋 Cambios Realizados

1. ✅ Paquetes NuGet actualizados:
   - ❌ Eliminado: `MySqlConnector` 
   - ❌ Eliminado: `AspNetCore.HealthChecks.MySql`
   - ✅ Agregado: `Npgsql` (10.0.1)
   - ✅ Agregado: `AspNetCore.HealthChecks.Npgsql` (9.0.0)

2. ✅ `RepositoryBase.cs` actualizado:
   - `MySqlConnection` → `NpgsqlConnection`
   - `LAST_INSERT_ID()` → `RETURNING Id`

3. ✅ `ContenidoLandingRepository.cs` actualizado:
   - `LAST_INSERT_ID()` → `RETURNING Id`

4. ✅ `ConfiguracionSistemaRepository.cs` actualizado:
   - `LAST_INSERT_ID()` → `RETURNING Id`

5. ✅ `UsuarioRepository.cs` actualizado:
   - `using MySqlConnector` → `using Npgsql`

## 📋 Cambios Pendientes

### Archivos que necesitan actualización de `using` y `MySqlConnection`:

1. `EventConnect.Infrastructure/Repositories/BodegaRepository.cs`
2. `EventConnect.Infrastructure/Repositories/MantenimientoRepository.cs`
3. `EventConnect.Infrastructure/Repositories/MovimientoInventarioRepository.cs`
4. `EventConnect.Infrastructure/Repositories/ActivoRepository.cs`
5. `EventConnect.Infrastructure/Repositories/LoteRepository.cs`
6. `EventConnect.Infrastructure/Repositories/DetalleReservaRepository.cs`
7. `EventConnect.Infrastructure/Services/AuthService.cs`

### Queries SQL que necesitan cambios específicos de PostgreSQL:

1. **CURDATE()** → **CURRENT_DATE** (PostgreSQL)
   - Ubicación: `MantenimientoRepository.cs` (línea 49), `LoteRepository.cs` (líneas 24, 36)

2. **DATE_ADD()** → **+ INTERVAL** (PostgreSQL)
   - Ubicación: `LoteRepository.cs` (línea 36)
   - MySQL: `DATE_ADD(CURDATE(), INTERVAL @Dias DAY)`
   - PostgreSQL: `CURRENT_DATE + INTERVAL '@Dias days'`

3. **CONCAT()** - PostgreSQL también soporta CONCAT(), pero se puede usar `||` para concatenación
   - Ubicación: `DetalleReservaRepository.cs` (línea 120)

4. **UPDATE con INNER JOIN** - PostgreSQL usa sintaxis diferente
   - Ubicación: `DetalleReservaRepository.cs` (línea 139-144)
   - MySQL: `UPDATE detalle_reserva dr INNER JOIN activo a ON ... SET ...`
   - PostgreSQL: `UPDATE detalle_reserva dr SET ... FROM activo a WHERE ...`

5. **NOW()** - PostgreSQL también soporta NOW(), pero puede usar CURRENT_TIMESTAMP

## 🔧 Cadena de Conexión PostgreSQL

URI proporcionada:
```
postgres://uecct3vhln2750:pa6b7d86f527f2bc8b418feadd03667970f779c77b3d79787ff9e3242b4417a6c@c7itisjfjj8ril.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com:5432/d21i4sul1k9fam
```

Formato connection string de Npgsql:
```
Host=c7itisjfjj8ril.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com;Port=5432;Database=d21i4sul1k9fam;Username=uecct3vhln2750;Password=pa6b7d86f527f2bc8b418feadd03667970f779c77b3d79787ff9e3242b4417a6c;SslMode=Require
```

**⚠️ IMPORTANTE**: Esta cadena de conexión contiene credenciales sensibles. Debe configurarse usando User Secrets o Variables de Entorno, NO en `appsettings.json`.

## 📝 Próximos Pasos

1. Actualizar todos los archivos restantes con `MySqlConnection` → `NpgsqlConnection`
2. Actualizar queries SQL específicas de MySQL a PostgreSQL
3. Actualizar Health Checks en `Program.cs`
4. Actualizar cadena de conexión en `appsettings.json` (con placeholder)
5. Documentar configuración en User Secrets
6. Probar la aplicación con PostgreSQL
