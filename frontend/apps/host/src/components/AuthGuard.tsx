'use client';

import { useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAppSelector } from '../store/store';
import { Loading } from '@eventconnect/ui';
import { ROUTES } from '@eventconnect/shared';

export function AuthGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, isLoading, user } = useAppSelector((state) => state.auth);

  useEffect(() => {
    if (!isLoading) {
      // Si no está autenticado, redirigir a login
      if (!isAuthenticated && pathname !== ROUTES.LOGIN) {
        router.push(ROUTES.LOGIN);
        return;
      }

      // Si está autenticado, redirigir según rol
      if (isAuthenticated && user) {
        const isAdminRoute = pathname.startsWith('/dashboard') || 
                           pathname.startsWith('/categorias') || 
                           pathname.startsWith('/productos') ||
                           pathname.startsWith('/clientes') ||
                           pathname.startsWith('/reservas') ||
                           pathname.startsWith('/activos') ||
                           pathname.startsWith('/bodegas') ||
                           pathname.startsWith('/lotes') ||
                           pathname.startsWith('/mantenimientos') ||
                           pathname.startsWith('/configuracion') ||
                           pathname.startsWith('/chat') ||
                           pathname.startsWith('/gestion-landing');

        // Ruta para usuarios normales (singular)
        const isClienteRoute = pathname.startsWith('/cliente/');

        // Si es SuperAdmin o Admin y está en ruta de cliente (singular), redirigir a dashboard
        if ((user.rol === 'SuperAdmin' || user.rol === 'Admin') && isClienteRoute) {
          router.push(ROUTES.DASHBOARD);
          return;
        }

        // Si es Usuario/Cliente y está en ruta admin, redirigir a vista cliente
        if (user.rol === 'Usuario' && isAdminRoute) {
          router.push('/cliente/explorar');
          return;
        }
      }
    }
  }, [isAuthenticated, isLoading, pathname, router, user]);

  if (isLoading) {
    return <Loading text="Verificando sesión..." />;
  }

  if (!isAuthenticated && pathname !== ROUTES.LOGIN) {
    return null;
  }

  return <>{children}</>;
}
