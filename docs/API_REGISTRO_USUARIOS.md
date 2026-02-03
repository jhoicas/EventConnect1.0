# API de Registro de Usuarios - EventConnect

## Endpoints Disponibles

EventConnect ofrece dos endpoints para la creación de cuentas de usuario, cada uno diseñado para un tipo específico de usuario.

---

## 1. Registro de Usuario del Sistema

**Endpoint:** `POST /api/auth/register`

Este endpoint es para registrar usuarios del sistema (empleados, administradores, etc.).

### Request Body

```json
{
  "usuario": "jdoe",
  "email": "jdoe@ejemplo.com",
  "password": "Password123!",
  "nombre_Completo": "John Doe",
  "telefono": "+573001234567",
  "empresa_Id": 1,
  "rol_Id": 3
}
```

### Campos

| Campo | Tipo | Requerido | Descripción | Validación |
|-------|------|-----------|-------------|------------|
| `usuario` | string | Sí | Nombre de usuario único | Min: 3, Max: 50 caracteres |
| `email` | string | Sí | Correo electrónico único | Formato email válido |
| `password` | string | Sí | Contraseña | Mínimo 6 caracteres |
| `nombre_Completo` | string | Sí | Nombre completo del usuario | Mínimo 3 caracteres |
| `telefono` | string | No | Número de teléfono | Formato teléfono válido |
| `empresa_Id` | int | Sí | ID de la empresa | Mayor a 0 |
| `rol_Id` | int | No | ID del rol (default: 3) | - |

### Respuestas

#### ✅ Éxito (200 OK)

```json
{
  "message": "Usuario registrado exitosamente",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "expiration": "2026-02-02T15:30:00Z",
    "usuario": {
      "id": 123,
      "usuario": "jdoe",
      "email": "jdoe@ejemplo.com",
      "nombre_Completo": "John Doe",
      "telefono": "+573001234567",
      "avatar_URL": null,
      "empresa_Id": 1,
      "empresa_Nombre": "Mi Empresa S.A.S",
      "rol_Id": 3,
      "rol": "Cliente",
      "nivel_Acceso": 3
    }
  }
}
```

#### ❌ Error - Datos Inválidos (400 Bad Request)

```json
{
  "message": "Datos inválidos",
  "errors": [
    "El nombre de usuario es requerido",
    "El formato del email no es válido"
  ]
}
```

#### ❌ Error - Usuario ya existe (400 Bad Request)

```json
{
  "message": "El nombre de usuario o email ya está registrado"
}
```

---

## 2. Registro de Cliente

**Endpoint:** `POST /api/auth/register-cliente`

Este endpoint es para registrar clientes (personas o empresas) que usarán la plataforma. Crea automáticamente:
- Un registro en la tabla `Usuario`
- Un registro en la tabla `Cliente`

### Request Body - Persona Natural

```json
{
  "email": "cliente@ejemplo.com",
  "password": "Password123!",
  "nombre_Completo": "María García",
  "telefono": "+573001234567",
  "empresa_Id": 1,
  "tipo_Cliente": "Persona",
  "documento": "1234567890",
  "tipo_Documento": "CC",
  "direccion": "Calle 123 #45-67",
  "ciudad": "Bogotá"
}
```

### Request Body - Empresa

```json
{
  "email": "contacto@empresacliente.com",
  "password": "Password123!",
  "nombre_Completo": "Empresa Cliente S.A.S",
  "telefono": "+573001234567",
  "empresa_Id": 1,
  "tipo_Cliente": "Empresa",
  "documento": "900123456",
  "tipo_Documento": "NIT",
  "direccion": "Carrera 7 #32-16",
  "ciudad": "Medellín"
}
```

### Campos

| Campo | Tipo | Requerido | Descripción | Validación |
|-------|------|-----------|-------------|------------|
| `email` | string | Sí | Correo electrónico único | Formato email válido |
| `password` | string | Sí | Contraseña | Mínimo 6 caracteres |
| `nombre_Completo` | string | Sí | Nombre completo o razón social | Mínimo 3 caracteres |
| `telefono` | string | No | Número de teléfono | Formato teléfono válido |
| `empresa_Id` | int | Sí | ID de la empresa | Mayor a 0 |
| `tipo_Cliente` | string | Sí | Tipo de cliente | "Persona" o "Empresa" |
| `documento` | string | Sí | Número de documento | Mínimo 5 caracteres |
| `tipo_Documento` | string | Sí | Tipo de documento | "CC", "CE", "NIT", "PP" |
| `direccion` | string | No | Dirección física | - |
| `ciudad` | string | No | Ciudad de residencia | - |

### Tipos de Documento

- **CC**: Cédula de Ciudadanía
- **CE**: Cédula de Extranjería
- **NIT**: Número de Identificación Tributaria
- **PP**: Pasaporte

### Respuestas

#### ✅ Éxito - Persona (200 OK)

```json
{
  "message": "Registro exitoso. Bienvenido a EventConnect.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "expiration": "2026-02-02T15:30:00Z",
    "usuario": {
      "id": 124,
      "usuario": "cliente@ejemplo.com",
      "email": "cliente@ejemplo.com",
      "nombre_Completo": "María García",
      "telefono": "+573001234567",
      "avatar_URL": null,
      "empresa_Id": 1,
      "empresa_Nombre": "Mi Empresa S.A.S",
      "rol_Id": 4,
      "rol": "Cliente",
      "nivel_Acceso": 1
    }
  }
}
```

#### ✅ Éxito - Empresa (200 OK)

**Nota:** Las empresas requieren aprobación manual, por lo tanto NO se genera token de acceso inmediato.

```json
{
  "message": "Registro exitoso. Tu cuenta de empresa está pendiente de aprobación. Te notificaremos cuando sea activada.",
  "data": {
    "token": null,
    "refreshToken": null,
    "expiration": null,
    "usuario": {
      "id": 125,
      "usuario": "contacto@empresacliente.com",
      "email": "contacto@empresacliente.com",
      "nombre_Completo": "Empresa Cliente S.A.S",
      "telefono": "+573001234567",
      "avatar_URL": null,
      "empresa_Id": 1,
      "empresa_Nombre": "Mi Empresa S.A.S",
      "rol_Id": 4,
      "rol": "Cliente",
      "nivel_Acceso": 1
    },
    "message": "Registro exitoso. Tu cuenta está pendiente de aprobación por un administrador."
  }
}
```

#### ❌ Error - Datos Inválidos (400 Bad Request)

```json
{
  "message": "Datos inválidos",
  "errors": [
    "El email es requerido",
    "La contraseña debe tener al menos 6 caracteres",
    "El tipo de cliente debe ser 'Persona' o 'Empresa'"
  ]
}
```

#### ❌ Error - Cliente ya existe (400 Bad Request)

```json
{
  "message": "El email o documento ya está registrado en el sistema"
}
```

---

## Diferencias entre los Endpoints

| Característica | `/api/auth/register` | `/api/auth/register-cliente` |
|----------------|---------------------|------------------------------|
| **Propósito** | Usuarios del sistema (empleados) | Clientes de la plataforma |
| **Tabla Usuario** | ✅ | ✅ |
| **Tabla Cliente** | ❌ | ✅ |
| **Campo Usuario único** | ✅ | ❌ (usa email) |
| **Documento** | ❌ | ✅ |
| **Aprobación** | Inmediata | Persona: Inmediata, Empresa: Manual |
| **Token generado** | Siempre | Persona: Sí, Empresa: No |

---

## Ejemplos de Uso

### cURL - Registro de Usuario

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "jdoe",
    "email": "jdoe@ejemplo.com",
    "password": "Password123!",
    "nombre_Completo": "John Doe",
    "telefono": "+573001234567",
    "empresa_Id": 1,
    "rol_Id": 3
  }'
```

### cURL - Registro de Cliente Persona

```bash
curl -X POST http://localhost:5000/api/auth/register-cliente \
  -H "Content-Type: application/json" \
  -d '{
    "email": "cliente@ejemplo.com",
    "password": "Password123!",
    "nombre_Completo": "María García",
    "telefono": "+573001234567",
    "empresa_Id": 1,
    "tipo_Cliente": "Persona",
    "documento": "1234567890",
    "tipo_Documento": "CC",
    "direccion": "Calle 123 #45-67",
    "ciudad": "Bogotá"
  }'
```

### cURL - Registro de Cliente Empresa

```bash
curl -X POST http://localhost:5000/api/auth/register-cliente \
  -H "Content-Type: application/json" \
  -d '{
    "email": "contacto@empresacliente.com",
    "password": "Password123!",
    "nombre_Completo": "Empresa Cliente S.A.S",
    "telefono": "+573001234567",
    "empresa_Id": 1,
    "tipo_Cliente": "Empresa",
    "documento": "900123456",
    "tipo_Documento": "NIT",
    "direccion": "Carrera 7 #32-16",
    "ciudad": "Medellín"
  }'
```

---

## Validaciones Implementadas

### A nivel de DTO (Data Annotations)

- ✅ Campos requeridos
- ✅ Formato de email
- ✅ Longitud mínima/máxima
- ✅ Formato de teléfono
- ✅ Valores permitidos (tipo cliente, tipo documento)
- ✅ Rangos numéricos

### A nivel de Controller

- ✅ Validación de ModelState
- ✅ Validación de campos obligatorios
- ✅ Validación de longitud de contraseña
- ✅ Validación de formato de email (regex)
- ✅ Validación de tipo de cliente
- ✅ Validación de tipo de documento

### A nivel de Service

- ✅ Verificación de duplicados (usuario, email, documento)
- ✅ Hash seguro de contraseñas
- ✅ Transacciones para integridad de datos
- ✅ Estado inicial según tipo de cliente

---

## Notas Importantes

1. **Contraseñas**: Todas las contraseñas se almacenan hasheadas usando BCrypt.

2. **Estados de Usuario**:
   - Persona: `Activo` (puede iniciar sesión inmediatamente)
   - Empresa: `Inactivo` (requiere aprobación de administrador)

3. **Rol por defecto**: Los clientes se registran con `Rol_Id = 4` (Cliente)

4. **Email como Usuario**: En el registro de clientes, el email se usa también como nombre de usuario

5. **Transacciones**: El registro de cliente usa transacciones para garantizar que se creen ambos registros (Usuario y Cliente) o ninguno

6. **Tokens JWT**: Se generan automáticamente para personas, permitiendo login inmediato

---

## Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| 200 | Registro exitoso |
| 400 | Datos inválidos o duplicados |
| 500 | Error interno del servidor |

---

## Seguridad

- ✅ Contraseñas hasheadas con BCrypt
- ✅ Validación de formato de email
- ✅ Longitud mínima de contraseña (6 caracteres)
- ✅ Validación de duplicados antes de insertar
- ✅ Transacciones para integridad de datos
- ✅ Logging de intentos de registro
- ✅ ModelState validation automática

---

## Próximos Pasos Después del Registro

1. **Si es Persona**: Usar el token para acceder a la plataforma inmediatamente
2. **Si es Empresa**: Esperar aprobación del administrador
3. **Verificar email** (si se implementa verificación por email)
4. **Completar perfil** con información adicional
