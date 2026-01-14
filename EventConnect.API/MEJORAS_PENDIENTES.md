# 📋 Mejoras Pendientes - EventConnect Backend

## ⚠️ Rate Limiting

El código actual de Rate Limiting usa APIs que pueden no estar disponibles en .NET 9.0 de la misma forma. 

**Opciones:**
1. Simplificar usando un middleware básico de rate limiting
2. Usar un paquete NuGet de terceros (ej: AspNetCoreRateLimit)
3. Implementar rate limiting manual con MemoryCache

**Estado**: Pendiente de corrección

---

## 📝 DTOs Separados

Crear DTOs para separar entidades del dominio de los contratos de la API.

**Prioridad**: Media
**Esfuerzo**: Alto (requiere refactorizar ~20 controllers)

**Ejemplo de estructura necesaria:**
```
EventConnect.API/DTOs/
├── Cliente/
│   ├── CreateClienteRequest.cs
│   ├── UpdateClienteRequest.cs
│   └── ClienteResponse.cs
├── Producto/
│   ├── CreateProductoRequest.cs
│   ├── UpdateProductoRequest.cs
│   └── ProductoResponse.cs
└── ...
```

---

## ✅ FluentValidation

Agregar validación con FluentValidation para mejorar la validación de modelos.

**Pasos:**
1. Instalar paquete: `dotnet add package FluentValidation.AspNetCore`
2. Crear validadores para cada DTO
3. Registrar validadores en Program.cs

**Prioridad**: Media
**Esfuerzo**: Medio

---

## ✅ Dependency Injection Optimizada

Crear interfaces para todos los repositorios y registrar usando interfaces.

**Ejemplo:**
```csharp
// ❌ Actual
builder.Services.AddScoped(_ => new ClienteRepository(connectionString));

// ✅ Debe ser
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
```

**Prioridad**: Baja
**Esfuerzo**: Medio

---

*Este documento se actualiza según se implementan las mejoras.*
