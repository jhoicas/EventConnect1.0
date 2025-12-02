'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Box, Container } from '@chakra-ui/react';
import { Navbar, Sidebar } from '@eventconnect/ui';
import type { MenuItemType } from '@eventconnect/ui';
import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  Calendar,
  Warehouse,
  FileBox,
  TrendingUp,
  Wrench,
  Settings,
  MessageCircle,
  UserCog,
} from 'lucide-react';
import { useAppSelector, useAppDispatch } from '../store/store';
import { logout, setCredentials } from '../store/slices/authSlice';
import { ROUTES } from '@eventconnect/shared';
import { usePathname } from 'next/navigation';
import ProfileModal from './profile/ProfileModal';
import { API_BASE_URL } from '../config/env';

export function DashboardLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const dispatch = useAppDispatch();
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [isProfileModalOpen, setIsProfileModalOpen] = useState(false);
  const [pendingUsersCount, setPendingUsersCount] = useState<number>(0);
  const { user } = useAppSelector((state) => state.auth);

  // Cargar conteo de usuarios pendientes (solo para admins)
  useEffect(() => {
    const fetchPendingUsers = async () => {
      if (user?.rol === 'SuperAdmin' || user?.rol === 'Admin-Proveedor') {
        try {
          const token = localStorage.getItem('token');
          const response = await fetch(`${API_BASE_URL}Usuario/pendientes/count`, {
            headers: {
              'Authorization': `Bearer ${token}`,
            },
          });
          if (response.ok) {
            const data = await response.json();
            setPendingUsersCount(data.count);
          }
        } catch (error) {
          console.dir(error); 
          console.error('Error al obtener usuarios pendientes:', error);
        }
      }
    };

    fetchPendingUsers();
    
    // Actualizar cada 30 segundos
    const interval = setInterval(fetchPendingUsers, 30000);
    return () => clearInterval(interval);
  }, [user?.rol]);

  // Menú para clientes
  const clienteMenuItems: MenuItemType[] = [
    { label: 'Explorar', icon: LayoutDashboard, href: '/cliente/explorar', isActive: pathname === '/cliente/explorar' },
    { label: 'Cotizar', icon: ShoppingCart, href: '/cliente/cotizaciones', isActive: pathname === '/cliente/cotizaciones' },
    { label: 'Reservas', icon: Calendar, href: '/cliente/reservas', isActive: pathname === '/cliente/reservas' },
    { label: 'Mensajes', icon: MessageCircle, href: '/cliente/mensajes', isActive: pathname === '/cliente/mensajes' },
  ];

  // Menú para administradores
  const adminMenuItems: MenuItemType[] = [
    { label: 'Dashboard', icon: LayoutDashboard, href: ROUTES.DASHBOARD, isActive: pathname === ROUTES.DASHBOARD },
    { label: 'Chat', icon: MessageCircle, href: ROUTES.CHAT, isActive: pathname === ROUTES.CHAT },
    { label: 'Categorías', icon: Package, href: ROUTES.CATEGORIAS, isActive: pathname === ROUTES.CATEGORIAS },
    { label: 'Productos', icon: ShoppingCart, href: ROUTES.PRODUCTOS, isActive: pathname === ROUTES.PRODUCTOS },
    { label: 'Clientes', icon: Users, href: ROUTES.CLIENTES, isActive: pathname === ROUTES.CLIENTES },
    { label: 'Reservas', icon: Calendar, href: ROUTES.RESERVAS, isActive: pathname === ROUTES.RESERVAS },
    { label: 'Activos', icon: Warehouse, href: ROUTES.ACTIVOS, isActive: pathname === ROUTES.ACTIVOS },
    { label: 'Bodegas', icon: FileBox, href: ROUTES.BODEGAS, isActive: pathname === ROUTES.BODEGAS },
    { label: 'Lotes', icon: TrendingUp, href: ROUTES.LOTES, isActive: pathname === ROUTES.LOTES },
    { label: 'Mantenimientos', icon: Wrench, href: ROUTES.MANTENIMIENTOS, isActive: pathname === ROUTES.MANTENIMIENTOS },
  ];

  if (user?.rol === 'SuperAdmin' || user?.rol === 'Admin-Proveedor') {
    adminMenuItems.push(
      {
        label: 'Usuarios',
        icon: UserCog,
        href: ROUTES.USUARIOS,
        isActive: pathname === ROUTES.USUARIOS,
      },
      {
        label: 'Configuración',
        icon: Settings,
        href: ROUTES.CONFIGURACION,
        isActive: pathname === ROUTES.CONFIGURACION,
      }
    );
  }

  // Seleccionar menú según rol
  const menuItems = (user?.rol === 'Cliente' || user?.rol === 'Usuario') 
    ? clienteMenuItems 
    : adminMenuItems;

  const handleLogout = () => {
    dispatch(logout());
    router.push(ROUTES.LOGIN);
  };

  const handleMenuItemClick = (href: string) => {
    router.push(href);
  };

  const handleNotificationClick = () => {
    // Redirigir a la página de usuarios para gestionar los pendientes
    router.push(ROUTES.USUARIOS);
  };

  const handleProfileSave = (data: any) => {
    console.log('Datos de perfil actualizados:', data);
    // Actualizar Redux con los nuevos datos del perfil
    dispatch(setCredentials({
      user: {
        ...user,
        nombre_Completo: data.nombre_Completo,
        telefono: data.telefono,
        avatar_URL: data.avatar_URL,
      },
      token: localStorage.getItem('token') || '',
    }));
  };

  return (
    <Box minH="100vh">
      {/* Navbar ocupa todo el ancho */}
      <Navbar
        title="EventConnect"
        username={user?.nombre_Completo}
        userAvatar={user?.avatar_URL}
        userRole={user?.rol}
        userEmail={user?.email}
        pendingUsersCount={user?.rol === 'SuperAdmin' || user?.rol === 'Admin-Proveedor' ? pendingUsersCount : undefined}
        onMenuClick={() => setIsSidebarOpen(!isSidebarOpen)}
        onProfileClick={() => setIsProfileModalOpen(true)}
        onNotificationClick={handleNotificationClick}
        onLogout={handleLogout}
      />

      {/* Contenedor flex para sidebar y contenido */}
      <Box display="flex">
        {/* Sidebar fijo a la izquierda */}
        <Sidebar
          isOpen={isSidebarOpen}
          onClose={() => setIsSidebarOpen(false)}
          items={menuItems}
          onItemClick={handleMenuItemClick}
        />

        {/* Contenido principal con margen izquierdo */}
        <Box flex="1" ml={{ base: 0, md: isSidebarOpen ? "250px" : 0 }} transition="margin-left 0.3s">
          <Container maxW="container.xl" py={6}>
            {children}
          </Container>
        </Box>
      </Box>

      <ProfileModal
        isOpen={isProfileModalOpen}
        onClose={() => setIsProfileModalOpen(false)}
        user={user}
        onSave={handleProfileSave}
      />
    </Box>
  );
}
