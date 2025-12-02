# Host Shell - EventConnect

Aplicación principal (Shell) que orquesta los Micro-Frontends.

## Responsabilidades

- 🎨 Carga del sistema de diseño (Chakra UI)
- 🧭 Gestión de navegación principal
- 📦 Montaje dinámico de MFEs
- 🔐 Estado de autenticación global (Redux)

## Estructura

```
src/
├── /app              # App Router de Next.js 15
├── /components       # Componentes del Shell
├── /store            # Redux Store Global
└── /styles           # Tema y estilos globales
```

## Scripts

```bash
pnpm dev      # Desarrollo (http://localhost:3000)
pnpm build    # Build de producción
pnpm start    # Servidor de producción
```
