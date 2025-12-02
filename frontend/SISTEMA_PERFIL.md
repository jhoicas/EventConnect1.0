# Sistema de Perfil de Usuario - EventConnect

## 📋 Descripción General

Sistema completo de gestión de perfil de usuario con las siguientes características:

### ✨ Funcionalidades Implementadas

1. **Modal de Perfil** (`ProfileModal.tsx`)
   - 3 tabs organizados: Información, Avatar, Seguridad
   - Disponible para **todos los roles** (SuperAdmin, Admin-Proveedor, Operario, Cliente, Auditor)
   - Accesible desde el menú del navbar (esquina superior derecha)

2. **Gestión de Avatar**
   - **Galería de Avatares Predeterminados** (24 opciones):
     - 6 avatares masculinos (estilo cartoon profesional)
     - 6 avatares femeninos (estilo cartoon profesional)
     - 6 avatares divertidos (robots/personajes)
     - 6 avatares profesionales (iniciales con colores)
   - **Subida de Foto Personalizada**:
     - Acepta JPG, PNG
     - Previsualización en tiempo real
     - Conversión a Base64 para almacenamiento

3. **Edición de Datos Personales**
   - Nombre completo (requerido)
   - Email (requerido)
   - Teléfono (opcional)
   - Validación en frontend

4. **Cambio de Contraseña**
   - Contraseña actual (seguridad)
   - Nueva contraseña (mínimo 8 caracteres)
   - Confirmación de contraseña
   - Validación de coincidencia

## 📁 Archivos Creados/Modificados

### Frontend

#### Nuevos Archivos
```
frontend/apps/host/src/components/profile/
└── ProfileModal.tsx          # Modal principal con 3 tabs
```

#### Archivos Modificados
```
frontend/packages/ui/src/components/molecules/
└── Navbar.tsx                # Agregado menú desplegable con avatar

frontend/apps/host/src/components/
└── DashboardLayout.tsx       # Integración del ProfileModal
```

### Backend

#### Archivos Modificados
```
EventConnect.API/Controllers/
└── AuthController.cs         # Agregados endpoints:
                              #   - POST /api/Auth/profile (actualizar perfil)
                              #   - POST /api/Auth/change-password

EventConnect.Domain/DTOs/
└── AuthDTOs.cs              # Agregados DTOs:
                              #   - UpdateProfileRequest
                              #   - ChangePasswordRequest
```

## 🎨 Galería de Avatares

### Tecnología Usada
- **DiceBear API**: Generador de avatares SVG gratuito
- **Ventajas**:
  - Sin dependencias de librerías externas
  - Avatares consistentes y profesionales
  - Generación dinámica basada en seeds
  - Múltiples estilos (avataaars, bottts, initials)

### Tipos de Avatares Disponibles

1. **Profesionales (Iniciales)**
   - Estilo similar a Gmail/Google
   - Colores: Azul, Púrpura, Rosa, Naranja, Verde, Cyan
   - Seed: Nombre del usuario
   - URL: `https://api.dicebear.com/7.x/initials/svg?seed={nombre}&backgroundColor={color}`

2. **Masculinos (Avataaars)**
   - Personas de estilo cartoon
   - Fondos pastel suaves
   - Seeds: Felix, John, Charlie, Max, Sam
   - URL: `https://api.dicebear.com/7.x/avataaars/svg?seed={seed}&backgroundColor={color}`

3. **Femeninos (Avataaars)**
   - Personas de estilo cartoon
   - Fondos pastel suaves
   - Seeds: Sophie, Emma, Luna, Mia, Zoe, Lily
   - URL: `https://api.dicebear.com/7.x/avataaars/svg?seed={seed}&backgroundColor={color}`

4. **Divertidos (Bottts)**
   - Robots y personajes abstractos
   - Ideales para usuarios que prefieren anonimato
   - Seeds: Felix, Aneka, Whiskers, Fluffy, Buddy, Lucky
   - URL: `https://api.dicebear.com/7.x/bottts/svg?seed={seed}&backgroundColor={color}`

## 🔧 Integración en el Navbar

### Cambios Realizados

**Antes:**
```tsx
<Flex align="center" gap={2}>
  <User size={18} />
  <Text fontSize="sm">{username}</Text>
</Flex>
<IconButton icon={<LogOut />} onClick={onLogout} />
```

**Después:**
```tsx
<ChakraMenu>
  <MenuButton>
    <Flex align="center" gap={2}>
      <Avatar src={userAvatar} size="sm" name={username} />
      <Box textAlign="left">
        <Text fontSize="sm" fontWeight="medium">{username}</Text>
        <Text fontSize="xs" color="gray">{userRole}</Text>
      </Box>
    </Flex>
  </MenuButton>
  <MenuList>
    <MenuItem icon={<Settings />} onClick={onProfileClick}>
      Mi Perfil
    </MenuItem>
    <MenuItem icon={<LogOut />} onClick={onLogout}>
      Cerrar Sesión
    </MenuItem>
  </MenuList>
</ChakraMenu>
```

### Nuevas Props en Navbar
```typescript
export interface NavbarProps {
  title: string;
  username?: string;
  userAvatar?: string;      // 🆕 URL del avatar
  userRole?: string;         // 🆕 Rol del usuario (ej: "Super Administrador")
  onMenuClick: () => void;
  onProfileClick?: () => void; // 🆕 Callback para abrir modal de perfil
  onLogout: () => void;
}
```

## 📱 Flujo de Usuario

### Paso 1: Acceder al Perfil
1. Usuario hace clic en su avatar/nombre (esquina superior derecha)
2. Se despliega menú con opciones:
   - **Mi Perfil** → Abre ProfileModal
   - **Cerrar Sesión** → Logout

### Paso 2: Editar Información (Tab 1)
1. Formulario con 3 campos:
   - Nombre Completo (requerido)
   - Email (requerido)
   - Teléfono (opcional)
2. Botón "Guardar Cambios"
3. Toast de confirmación

### Paso 3: Cambiar Avatar (Tab 2)
**Opción A: Subir Foto**
1. Click en "Subir Foto Personalizada"
2. Seleccionar imagen (JPG/PNG, máx 2MB)
3. Previsualización instantánea
4. Botón "Guardar Avatar"

**Opción B: Elegir Avatar Predeterminado**
1. Scroll por galerías organizadas:
   - Iniciales (6 colores)
   - Masculinos (6 opciones)
   - Femeninos (6 opciones)
   - Divertidos (6 opciones)
2. Click en avatar deseado (se marca con borde azul)
3. Botón "Guardar Avatar"

### Paso 4: Cambiar Contraseña (Tab 3)
1. Ingresar contraseña actual
2. Ingresar nueva contraseña (mín 8 caracteres)
3. Confirmar nueva contraseña
4. Validaciones:
   - Contraseñas coinciden
   - Longitud mínima
5. Botón "Cambiar Contraseña"
6. Toast de confirmación

## 🔐 Seguridad

### Validaciones Frontend
```typescript
// Información
if (!formData.nombre_Completo || !formData.email) {
  toast({ title: "Error", description: "Nombre y email son obligatorios" });
  return;
}

// Contraseña
if (passwordData.newPassword !== passwordData.confirmPassword) {
  toast({ title: "Error", description: "Las contraseñas no coinciden" });
  return;
}

if (passwordData.newPassword.length < 8) {
  toast({ title: "Error", description: "Mínimo 8 caracteres" });
  return;
}
```

### Endpoints Backend (TODO)
```csharp
// POST /api/Auth/profile
[HttpPut("profile")]
public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
{
    // TODO: Validar que el usuario solo pueda actualizar su propio perfil
    // TODO: Validar email único
    // TODO: Sanitizar datos
    // TODO: Guardar avatar en storage o DB
}

// POST /api/Auth/change-password
[HttpPost("change-password")]
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
{
    // TODO: Verificar contraseña actual con BCrypt
    // TODO: Hashear nueva contraseña
    // TODO: Actualizar en DB
    // TODO: Invalidar tokens anteriores
}
```

## 🎯 Próximos Pasos

### Backend (Pendiente)
1. **Implementar AuthService.UpdateProfileAsync()**
   - Actualizar Usuario en DB (Nombre_Completo, Email, Telefono, Avatar_URL)
   - Validar email único
   - Retornar usuario actualizado

2. **Implementar AuthService.ChangePasswordAsync()**
   - Verificar contraseña actual con BCrypt.Verify()
   - Hashear nueva contraseña con BCrypt.HashPassword()
   - Actualizar Hash_Password en DB
   - Log de auditoría (cambio de contraseña)

3. **Storage de Imágenes**
   - Opción A: Guardar Base64 en DB (campo `Avatar_URL` tipo TEXT)
   - Opción B: Subir a servidor y guardar path/URL
   - Opción C: Integrar con servicio cloud (Azure Blob, AWS S3)

4. **Agregar campo Avatar_URL a tabla Usuario**
   ```sql
   ALTER TABLE Usuario ADD COLUMN Avatar_URL TEXT NULL;
   ```

### Frontend (Pendiente)
1. **Conectar con API real**
   ```typescript
   const handleProfileSave = async (data: any) => {
     try {
       const response = await fetch('/api/Auth/profile', {
         method: 'PUT',
         headers: { 'Content-Type': 'application/json' },
         body: JSON.stringify({
           usuarioId: user.id,
           ...data
         })
       });
       
       if (response.ok) {
         // Actualizar estado global (Redux)
         dispatch(updateUser(data));
         toast({ title: "Perfil actualizado", status: "success" });
       }
     } catch (error) {
       toast({ title: "Error", status: "error" });
     }
   };
   ```

2. **Actualizar Redux Store**
   - Agregar action `updateUser` en authSlice
   - Persistir avatar en localStorage
   - Refrescar UI automáticamente

3. **Mejorar UX**
   - Loading states durante guardado
   - Confirmación antes de cambiar avatar
   - Preview de cambios antes de guardar
   - Crop/resize de imágenes grandes

## 📊 Estado Actual

### ✅ Completado
- [x] Diseño UI del ProfileModal
- [x] 3 tabs funcionales (Información, Avatar, Seguridad)
- [x] Galería de 24 avatares predeterminados
- [x] Subida de foto personalizada con preview
- [x] Formulario de edición de datos
- [x] Formulario de cambio de contraseña
- [x] Validaciones frontend
- [x] Integración en Navbar con menú desplegable
- [x] Disponible para todos los roles
- [x] Endpoints backend (estructura básica)
- [x] DTOs para UpdateProfile y ChangePassword

### ⏳ Pendiente
- [ ] Implementar lógica en AuthService (backend)
- [ ] Agregar campo Avatar_URL a DB
- [ ] Conectar frontend con API real
- [ ] Actualizar Redux store después de guardar
- [ ] Tests unitarios
- [ ] Manejo de imágenes grandes (compresión)

## 🎨 Captura de Funcionalidades

### Navbar con Menú Desplegable
```
┌─────────────────────────────────────────────┐
│ ☰ EventConnect           [🌙]  👤 María     │
│                                  Admin       │
│                           ┌──────────────┐   │
│                           │ ⚙ Mi Perfil  │   │
│                           │ 🚪 Cerrar S. │   │
│                           └──────────────┘   │
└─────────────────────────────────────────────┘
```

### Modal de Perfil
```
┌────────────────────────────────────────────┐
│  Mi Perfil                             ✖   │
├────────────────────────────────────────────┤
│  [Información] [Avatar] [Seguridad]        │
│                                            │
│  Tab 1: Información                        │
│  ┌─────────────────────────────────────┐  │
│  │ Nombre Completo *                   │  │
│  │ [María Fernanda Rodríguez        ]  │  │
│  └─────────────────────────────────────┘  │
│  ┌─────────────────────────────────────┐  │
│  │ Email *                             │  │
│  │ [maria@eventoselegantes.com      ]  │  │
│  └─────────────────────────────────────┘  │
│  ┌─────────────────────────────────────┐  │
│  │ Teléfono                            │  │
│  │ [+57 310 123 4567                ]  │  │
│  └─────────────────────────────────────┘  │
│                                            │
│  [💾 Guardar Cambios]                     │
│                                            │
│  Tab 2: Avatar                             │
│  ┌─────────────────────────────────────┐  │
│  │        [Avatar Actual]               │  │
│  │           👤 MR                      │  │
│  └─────────────────────────────────────┘  │
│                                            │
│  📸 Subir Foto Personalizada               │
│  [Elegir archivo...]                       │
│                                            │
│  Iniciales (Estilo Gmail)                  │
│  👤 👤 👤 👤 👤 👤                         │
│                                            │
│  Avatares Masculinos                       │
│  😀 😊 🧔 👨 👦 🙂                         │
│                                            │
│  Avatares Femeninos                        │
│  👩 👧 👱‍♀️ 👩‍🦰 👩‍🦱 👩‍🦳                   │
│                                            │
│  Avatares Divertidos                       │
│  🤖 🦊 🐱 🐶 🐻 🦁                         │
│                                            │
│  [💾 Guardar Avatar]                      │
│                                            │
│  Tab 3: Seguridad                          │
│  ┌─────────────────────────────────────┐  │
│  │ Contraseña Actual *                 │  │
│  │ [••••••••                        ]  │  │
│  └─────────────────────────────────────┘  │
│  ┌─────────────────────────────────────┐  │
│  │ Nueva Contraseña *                  │  │
│  │ [••••••••                        ]  │  │
│  │ Mínimo 8 caracteres                 │  │
│  └─────────────────────────────────────┘  │
│  ┌─────────────────────────────────────┐  │
│  │ Confirmar Nueva Contraseña *        │  │
│  │ [••••••••                        ]  │  │
│  └─────────────────────────────────────┘  │
│                                            │
│  [🔒 Cambiar Contraseña]                  │
└────────────────────────────────────────────┘
```

## 🚀 Cómo Usar (Para Desarrolladores)

### Integrar en Nuevo Componente
```tsx
import ProfileModal from '@/components/profile/ProfileModal';

function MiComponente() {
  const [isOpen, setIsOpen] = useState(false);
  const user = useAppSelector(state => state.auth.user);

  const handleSave = (data) => {
    console.log('Guardar perfil:', data);
    // Llamar a API
  };

  return (
    <>
      <Button onClick={() => setIsOpen(true)}>
        Editar Perfil
      </Button>
      
      <ProfileModal
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        user={user}
        onSave={handleSave}
      />
    </>
  );
}
```

### Agregar Nuevos Avatares
```typescript
// En ProfileModal.tsx
const DEFAULT_AVATARS = {
  // ... existentes ...
  
  nuevaCategoria: [
    "https://api.dicebear.com/7.x/[estilo]/svg?seed=[seed]&backgroundColor=[color]",
    // ... más avatares
  ],
};

// Luego agregar sección en el JSX:
<Box>
  <FormLabel>Nueva Categoría</FormLabel>
  <SimpleGrid columns={6} spacing={3}>
    {DEFAULT_AVATARS.nuevaCategoria.map((avatar, index) => (
      <Avatar
        key={index}
        src={avatar}
        cursor="pointer"
        border={selectedAvatar === avatar ? "3px solid" : "none"}
        borderColor="blue.500"
        onClick={() => {
          setSelectedAvatar(avatar);
          setCustomImage("");
        }}
      />
    ))}
  </SimpleGrid>
</Box>
```

## 📚 Recursos

- **DiceBear Documentation**: https://www.dicebear.com/
- **Chakra UI Avatar**: https://chakra-ui.com/docs/components/avatar
- **Chakra UI Tabs**: https://chakra-ui.com/docs/components/tabs
- **FileReader API**: https://developer.mozilla.org/en-US/docs/Web/API/FileReader

---

**Autor**: GitHub Copilot  
**Fecha**: Noviembre 2025  
**Proyecto**: EventConnect - Sistema de Gestión de Eventos
