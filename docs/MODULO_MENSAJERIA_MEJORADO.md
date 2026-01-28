# Módulo de Mensajería Mejorado - EventConnect

## Resumen de Cambios Implementados

### 1. Base de Datos
- ✅ **Índice agregado**: `idx_conversacion_cliente` en la tabla `Conversacion` para optimizar consultas por cliente
- ✅ **Verificación**: La tabla `Mensaje` tiene la relación correcta con `Usuario` a través de `Emisor_Usuario_Id`

### 2. DTOs Actualizados (`ChatDTOs.cs`)

```csharp
public class ConversacionDTO
{
    // Campos existentes...
    
    // NUEVOS CAMPOS para mostrar información de la contraparte
    public string? Nombre_Contraparte { get; set; }    // Nombre de la empresa o cliente
    public string? Avatar_Contraparte { get; set; }    // Logo de empresa o avatar de cliente
    public string? Email_Contraparte { get; set; }     // Email de contacto
}

public class CreateConversacionRequest
{
    public int? Empresa_Id { get; set; }  // NUEVO: Para cuando un Cliente inicia chat
    public string? Asunto { get; set; }
    public int? Reserva_Id { get; set; }
    public string? Mensaje_Inicial { get; set; }
}
```

### 3. Repositorio (`ConversacionRepository.cs`)

**Nuevos métodos agregados:**

#### `GetConversacionesByClienteIdAsync(int clienteId)`
- Devuelve conversaciones del cliente con sus proveedores (empresas)
- Incluye contador de mensajes no leídos
- Incluye datos de la empresa: Razón Social, Logo, Email

#### `GetConversacionesByEmpresaIdAsync(int empresaId)`
- Devuelve conversaciones de la empresa con sus clientes
- Incluye contador de mensajes no leídos del cliente
- Incluye datos del cliente: Nombre, Email

### 4. Controlador (`ChatController.cs`)

#### Endpoint Principal Mejorado: `GET /api/chat/conversaciones`

**Comportamiento según rol:**

1. **SuperAdmin**: Ve todas las conversaciones del sistema
2. **Cliente**: Ve sus conversaciones con empresas (proveedores)
3. **Admin-Proveedor**: Ve conversaciones con sus clientes

**Respuesta cuando no hay datos**: 
- ✅ Retorna `200 OK` con array vacío `[]` (facilita manejo en frontend)
- ✅ No retorna `400 BadRequest` cuando usuario no tiene empresa/cliente

#### Otros endpoints mejorados:
- `GET /api/chat/conversaciones/{id}` - Valida acceso por rol (Cliente o Empresa)
- `POST /api/chat/conversaciones` - Permite a Clientes iniciar conversaciones
- `GET /api/chat/mensajes/{conversacionId}` - Valida permisos por rol
- `POST /api/chat/mensajes` - Valida permisos por rol

---

## Ejemplos de Uso de la API

### 1. Cliente obtiene sus conversaciones

**Request:**
```http
GET /api/chat/conversaciones
Authorization: Bearer {token_cliente}
```

**Response:**
```json
[
  {
    "id": 1,
    "empresa_Id": 5,
    "cliente_Id": 10,
    "asunto": "Consulta sobre alquiler de sillas",
    "reserva_Id": 123,
    "estado": "Abierta",
    "fecha_Creacion": "2026-01-28T10:30:00",
    "mensajes_No_Leidos": 2,
    "nombre_Contraparte": "Eventos Caribe S.A.S",
    "avatar_Contraparte": "https://storage.example.com/logos/eventos-caribe.png",
    "email_Contraparte": "contacto@eventoscaribe.com",
    "ultimo_Mensaje": {
      "id": 456,
      "contenido": "Claro, tenemos disponibilidad para esa fecha",
      "emisor_Usuario_Id": 20,
      "emisor_Nombre": "Juan Pérez",
      "leido": false,
      "fecha_Envio": "2026-01-28T14:20:00"
    }
  }
]
```

### 2. Empresa obtiene conversaciones con clientes

**Request:**
```http
GET /api/chat/conversaciones
Authorization: Bearer {token_empresa}
```

**Response:**
```json
[
  {
    "id": 1,
    "empresa_Id": 5,
    "cliente_Id": 10,
    "asunto": "Consulta sobre alquiler de sillas",
    "estado": "Abierta",
    "mensajes_No_Leidos": 1,
    "nombre_Contraparte": "María González",
    "avatar_Contraparte": null,
    "email_Contraparte": "maria.gonzalez@email.com",
    "ultimo_Mensaje": {
      "contenido": "¿Tienen sillas disponibles para el 15 de febrero?",
      "leido": true,
      "fecha_Envio": "2026-01-28T10:30:00"
    }
  }
]
```

### 3. Cliente inicia conversación con una empresa

**Request:**
```http
POST /api/chat/conversaciones
Authorization: Bearer {token_cliente}
Content-Type: application/json

{
  "empresa_Id": 5,
  "asunto": "Consulta sobre mesas",
  "reserva_Id": null,
  "mensaje_Inicial": "Hola, quisiera información sobre el alquiler de mesas para 100 personas"
}
```

**Response:**
```json
{
  "id": 25,
  "message": "Conversación creada correctamente"
}
```

### 4. Enviar mensaje en una conversación

**Request:**
```http
POST /api/chat/mensajes
Authorization: Bearer {token}
Content-Type: application/json

{
  "conversacion_Id": 1,
  "contenido": "Perfecto, necesito 50 sillas tipo banquete"
}
```

**Response:**
```json
{
  "id": 457,
  "message": "Mensaje enviado correctamente"
}
```

### 5. Obtener mensajes de una conversación

**Request:**
```http
GET /api/chat/mensajes/1
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 455,
    "conversacion_Id": 1,
    "emisor_Usuario_Id": 15,
    "emisor_Nombre": "María González",
    "contenido": "¿Tienen sillas disponibles para el 15 de febrero?",
    "leido": true,
    "fecha_Envio": "2026-01-28T10:30:00"
  },
  {
    "id": 456,
    "emisor_Usuario_Id": 20,
    "emisor_Nombre": "Juan Pérez",
    "contenido": "Claro, tenemos disponibilidad para esa fecha",
    "leido": false,
    "fecha_Envio": "2026-01-28T14:20:00"
  }
]
```

---

## Migración SQL

Para aplicar el índice en la base de datos existente, ejecutar:

```bash
psql -h <host> -U <usuario> -d <database> -f database/migrations/20260128_add_conversacion_cliente_index.sql
```

O copiar y ejecutar manualmente:
```sql
CREATE INDEX IF NOT EXISTS idx_conversacion_cliente ON Conversacion(Cliente_Id);
```

---

## Validaciones de Seguridad Implementadas

### Control de Acceso por Rol:

1. **Cliente**:
   - ✅ Solo ve conversaciones donde `Cliente_Id` = su ID
   - ✅ Solo puede enviar/recibir mensajes en sus conversaciones
   - ✅ Puede iniciar nuevas conversaciones con empresas

2. **Empresa (Admin-Proveedor)**:
   - ✅ Solo ve conversaciones donde `Empresa_Id` = su empresa
   - ✅ Solo puede interactuar con conversaciones de su empresa
   - ✅ Recibe notificación de mensajes no leídos de clientes

3. **SuperAdmin**:
   - ✅ Acceso total a todas las conversaciones
   - ✅ Puede monitorear comunicaciones

### Prevención de Fugas de Información:
- ✅ Los contadores de mensajes no leídos solo cuentan mensajes del emisor contrario
- ✅ Los datos de contraparte solo incluyen información pública (nombre, email, logo)
- ✅ Validación estricta de permisos antes de devolver datos sensibles

---

## Frontend - Consideraciones de Implementación

### Lista de Conversaciones (Chat List)
```typescript
// Ejemplo de componente React
interface Conversacion {
  id: number;
  nombre_Contraparte: string;
  avatar_Contraparte?: string;
  mensajes_No_Leidos: number;
  ultimo_Mensaje?: {
    contenido: string;
    fecha_Envio: string;
  };
}

// Ya no necesitas hacer fetch adicionales para obtener:
// - Nombre de la contraparte
// - Avatar/Logo
// - Contador de mensajes no leídos
```

### Manejo de Respuestas Vacías
```typescript
const obtenerConversaciones = async () => {
  const response = await fetch('/api/chat/conversaciones', {
    headers: { Authorization: `Bearer ${token}` }
  });
  
  const conversaciones = await response.json();
  
  // Siempre recibirás un array, incluso si está vacío
  if (conversaciones.length === 0) {
    mostrarMensaje("No tienes conversaciones aún");
  }
};
```

---

## Rendimiento y Optimización

### Índices Creados:
- `idx_conversacion_empresa` - Para filtrar por empresa
- `idx_conversacion_cliente` - **NUEVO** - Para filtrar por cliente
- `idx_conversacion_estado` - Para filtrar por estado
- `idx_mensaje_conversacion` - Para obtener mensajes de una conversación

### Consultas Optimizadas:
- ✅ Contador de mensajes no leídos calculado en SQL (no en memoria)
- ✅ JOIN directo con tablas Cliente/Empresa para obtener datos
- ✅ ORDER BY en índice `Fecha_Creacion`

---

## Testing

### Casos de Prueba Recomendados:

1. **Cliente sin conversaciones**: Debe retornar `[]`
2. **Cliente con múltiples conversaciones**: Debe mostrar todas con datos de empresas
3. **Empresa sin clientes**: Debe retornar `[]`
4. **Empresa con múltiples clientes**: Debe mostrar todas con datos de clientes
5. **Mensajes no leídos**: Contador debe ser preciso
6. **Permisos**: Cliente no debe ver conversaciones de otro cliente
7. **Permisos**: Empresa no debe ver conversaciones de otra empresa
