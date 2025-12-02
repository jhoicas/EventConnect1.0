# EventConnect Frontend - Micro-Frontends Architecture

<div align="center">

![Next.js](https://img.shields.io/badge/Next.js-15.x-black?logo=next.js)
![React](https://img.shields.io/badge/React-19.0-blue?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.7-blue?logo=typescript)
![Turborepo](https://img.shields.io/badge/Turborepo-2.3-red?logo=turborepo)
![Chakra UI](https://img.shields.io/badge/Chakra_UI-2.x-teal?logo=chakraui)

**Arquitectura de Micro-Frontends para EventConnect**  
*Sistema escalable, modular y de alto rendimiento*

</div>

---

## 🏗️ Arquitectura

### Monorepo con Turborepo

```
frontend/
├── apps/
│   ├── host/              # 🏠 Shell Application (Next.js 15)
│   ├── mfe-inventario/    # 📦 MFE - SIGI (Sistema de Inventarios)
│   ├── mfe-reservas/      # 📅 MFE - Gestión de Reservas
│   └── mfe-admin/         # ⚙️ MFE - Administración
├── packages/
│   ├── ui/                # 🎨 Sistema de Diseño (Chakra UI + Atoms)
│   └── shared/            # 🔧 Tipos, Validaciones y Utilidades
├── package.json           # Workspace raíz
├── turbo.json             # Pipeline de Turborepo
└── pnpm-workspace.yaml    # Configuración de pnpm
```

### Principios de Diseño

#### 1. **Separation of Concerns**
- **Host (Shell)**: Navegación, autenticación, carga de MFEs
- **MFEs**: Capacidades de negocio independientes
- **UI Package**: Sistema de diseño compartido
- **Shared Package**: Lógica reutilizable

#### 2. **Clean Architecture por MFE**
```
mfe-example/src/
├── /api              # 📡 Capa de Datos (RTK Query)
├── /store            # 🗃️ Redux Slices
├── /hooks            # 🪝 Custom Hooks (useOptimistic, useActionState)
├── /components       # 🧩 Atomic Design
│   ├── /pages        # Container Components (Smart)
│   ├── /molecules    # Composed Components
│   └── /atoms        # Base Components (Dumb)
└── /utils            # 🛠️ Helpers
```

#### 3. **Atomic Design Pattern**
- **Atoms**: `<Button>`, `<Input>`, `<Card>`
- **Molecules**: `<FormContainer>`, `<DataTable>`, `<Navbar>`
- **Pages**: Containers con lógica de negocio

---

## 🚀 Tecnologías Clave

### Core
- **Next.js 15** (App Router)
- **React 19** con nuevos hooks (`useOptimistic`, `useActionState`, `useFormStatus`)
- **TypeScript 5.7**
- **Turborepo 2.3** (Monorepo)
- **pnpm 8** (Gestor de paquetes)

### Estado y Datos
- **Redux Toolkit 2.5** (Estado global)
- **RTK Query** (Data fetching, caching, sincronización)
- **Zod** (Validación de schemas)

### UI/UX
- **Chakra UI 2.x** (Sistema de diseño)
- **Emotion** (CSS-in-JS)
- **Lucide React** (Iconos)
- **Framer Motion** (Animaciones)

---

## 📦 Instalación

### Requisitos Previos
- **Node.js**: >= 20.0.0
- **pnpm**: >= 8.0.0

```bash
# Instalar pnpm (si no lo tienes)
npm install -g pnpm@8.15.0
```

### Instalación del Monorepo

```bash
# Clonar el repositorio
cd C:\Users\yoiner.castillo\source\repos\EventConnect\frontend

# Instalar todas las dependencias
pnpm install

# Verificar la instalación
pnpm --version
```

---

## 🛠️ Scripts Disponibles

### Desarrollo

```bash
# Ejecutar todos los apps en modo desarrollo
pnpm dev

# Solo el Host
pnpm --filter @eventconnect/host dev

# MFE específico
pnpm --filter @eventconnect/mfe-inventario dev
```

### Build

```bash
# Build completo del monorepo
pnpm build

# Build del Host
pnpm --filter @eventconnect/host build

# Build de producción
pnpm build --force
```

### Linting

```bash
# Lint todo el código
pnpm lint

# Fix automático
pnpm lint --fix
```

### Clean

```bash
# Limpiar builds y cache
pnpm clean

# Limpiar node_modules completo
rm -rf node_modules
rm -rf apps/*/node_modules
rm -rf packages/*/node_modules
pnpm install
```

---

## 🎯 Estructura de Paquetes

### `@eventconnect/ui`

Sistema de diseño compartido con tema oscuro personalizado.

**Atoms:**
- `Button` - Botón reutilizable
- `Input` - Input con validación
- `Card` - Contenedor de contenido
- `Loading` - Indicador de carga
- `ErrorMessage` - Mensajes de error

**Molecules:**
- `FormContainer` - Wrapper de formularios
- `DataTable` - Tabla de datos genérica
- `Navbar` - Barra de navegación
- `Sidebar` - Menú lateral

**Tema:**
```typescript
import { theme } from '@eventconnect/ui';

// Colores principales
--bg: #0b1020       // Fondo oscuro
--card: #111833     // Tarjetas
--accent: #5b8cff   // Azul primario
--text: #e6e6f0     // Texto principal
--muted: #9aa3b2    // Texto secundario
```

### `@eventconnect/shared`

Tipos, validaciones y utilidades compartidas.

**Exports:**
```typescript
// Tipos
import { User, Producto, Activo } from '@eventconnect/shared';

// Validaciones Zod
import { loginSchema, productoSchema } from '@eventconnect/shared';

// Constantes
import { API_BASE_URL, ROUTES } from '@eventconnect/shared';

// Hooks
import { useDebounce } from '@eventconnect/shared';

// Formatters
import { formatCurrency, formatDate } from '@eventconnect/shared';
```

---

## 🔐 Autenticación y Estado Global

### Redux Store (Host)

```typescript
// apps/host/src/store/store.ts
export const store = configureStore({
  reducer: {
    auth: authReducer,
    [baseApi.reducerPath]: baseApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(baseApi.middleware),
});
```

### RTK Query - API Services

```typescript
// Ejemplo: apps/host/src/store/api/authApi.ts
export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/Auth/login',
        method: 'POST',
        body: credentials,
      }),
    }),
  }),
});

export const { useLoginMutation } = authApi;
```

### Uso en Componentes

```typescript
import { useLoginMutation } from '@/store/api/authApi';

function LoginForm() {
  const [login, { isLoading }] = useLoginMutation();

  const handleSubmit = async (data) => {
    const result = await login(data).unwrap();
    // Manejar resultado
  };
}
```

---

## 🪝 React 19 Hooks

### `useOptimistic`

Actualiza la UI inmediatamente con valores "optimistas".

```typescript
import { useOptimistic } from 'react';

function ProductList() {
  const [products, addOptimistic] = useOptimistic(
    items,
    (state, newItem) => [...state, newItem]
  );

  const handleAdd = async (product) => {
    addOptimistic(product); // UI se actualiza instantáneamente
    await saveProduct(product); // Request al backend
  };
}
```

### `useActionState`

Maneja estado de acciones (loading, error, data).

```typescript
import { useActionState } from 'react';

function CreateForm() {
  const [state, formAction, isPending] = useActionState(
    async (prevState, formData) => {
      const result = await createItem(formData);
      return { success: true, data: result };
    },
    { success: false, data: null }
  );

  return (
    <form action={formAction}>
      <Button isLoading={isPending}>Guardar</Button>
    </form>
  );
}
```

### `useFormStatus`

Proporciona estado de envío de formularios.

```typescript
import { useFormStatus } from 'react-dom';

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button isLoading={pending} disabled={pending}>
      {pending ? 'Guardando...' : 'Guardar'}
    </Button>
  );
}
```

---

## 🌐 Variables de Entorno

### Host (`.env.local`)

```env
# API Backend
NEXT_PUBLIC_API_URL=http://localhost:5555/api

# Environment
NEXT_PUBLIC_ENV=development

# Analytics (opcional)
NEXT_PUBLIC_ANALYTICS_ID=
```

---

## 🧪 Testing

```bash
# Ejecutar tests
pnpm test

# Coverage
pnpm test --coverage

# Watch mode
pnpm test --watch
```

### Estructura de Tests

```
src/
├── components/
│   ├── Button.tsx
│   └── Button.test.tsx
├── hooks/
│   ├── useInventory.ts
│   └── useInventory.test.ts
└── store/
    ├── slices/
    │   ├── authSlice.ts
    │   └── authSlice.test.ts
```

---

## 📊 Performance

### Code Splitting Automático

Next.js divide automáticamente el código por rutas.

```typescript
// Lazy loading manual
const MFEInventario = dynamic(() => import('@/mfe/inventario'), {
  loading: () => <Loading />,
  ssr: false,
});
```

### Optimizaciones en `next.config.js`

```javascript
experimental: {
  optimizePackageImports: ['@chakra-ui/react', 'lucide-react']
},
webpack: (config) => {
  config.optimization.splitChunks = {
    cacheGroups: {
      chakra: { name: 'chakra-ui', test: /[\\/]@chakra-ui[\\/]/ },
      redux: { name: 'redux', test: /[\\/]@reduxjs[\\/]/ },
    }
  };
  return config;
}
```

---

## 🚢 Deployment

### Vercel (Recomendado)

```bash
# Instalar Vercel CLI
npm i -g vercel

# Deploy
vercel --prod
```

### Docker

```dockerfile
FROM node:20-alpine AS base
RUN npm install -g pnpm@8.15.0

FROM base AS deps
WORKDIR /app
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY apps/host/package.json ./apps/host/
COPY packages/*/package.json ./packages/
RUN pnpm install --frozen-lockfile

FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN pnpm build --filter @eventconnect/host

FROM base AS runner
WORKDIR /app
ENV NODE_ENV production
COPY --from=builder /app/apps/host/.next ./apps/host/.next
COPY --from=builder /app/apps/host/public ./apps/host/public
EXPOSE 3000
CMD ["pnpm", "--filter", "@eventconnect/host", "start"]
```

---

## 📝 Convenciones de Código

### Commits (Conventional Commits)

```bash
feat(host): add login page
fix(ui): correct button hover state
docs(shared): update API types
refactor(mfe-inventario): simplify state management
```

### Naming

- **Componentes**: `PascalCase` (e.g., `LoginForm.tsx`)
- **Hooks**: `camelCase` con prefijo `use` (e.g., `useInventory.ts`)
- **Utilidades**: `camelCase` (e.g., `formatDate.ts`)
- **Constantes**: `UPPER_SNAKE_CASE` (e.g., `API_BASE_URL`)

---

## 🤝 Contribución

1. Crear feature branch: `git checkout -b feature/nueva-funcionalidad`
2. Commit con conventional commits: `git commit -m "feat: agregar X"`
3. Push: `git push origin feature/nueva-funcionalidad`
4. Crear Pull Request

---

## 📄 Licencia

MIT © 2025 EventConnect

---

## 🆘 Soporte

- **Email**: soporte@eventconnect.com
- **Docs**: [docs.eventconnect.com](https://docs.eventconnect.com)
- **Issues**: [GitHub Issues](https://github.com/eventconnect/frontend/issues)

---

<div align="center">

**⭐ Si te gusta este proyecto, déjanos una estrella en GitHub ⭐**

</div>
