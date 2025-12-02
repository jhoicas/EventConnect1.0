'use client';

import { Provider } from 'react-redux';
import { store } from '../store/store';
import { ChakraProvider, ColorModeScript } from '@chakra-ui/react';
import { theme } from '@eventconnect/ui';
import { useEffect, useState } from 'react';
import { restoreSession, setLoading } from '../store/slices/authSlice';
import { User } from '@eventconnect/shared';

export function Providers({ children }: { children: React.ReactNode }) {
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    
    // Restaurar tema desde localStorage o usar light por defecto
    const savedTheme = localStorage.getItem('chakra-ui-color-mode');
    if (savedTheme && ['dark', 'light', 'blue'].includes(savedTheme)) {
      document.documentElement.setAttribute('data-theme', savedTheme);
    } else {
      // Si no hay tema guardado, establecer light por defecto
      localStorage.setItem('chakra-ui-color-mode', 'light');
      document.documentElement.setAttribute('data-theme', 'light');
    }

    // Restaurar sesión desde localStorage
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (token && userStr && userStr !== 'undefined' && userStr !== 'null') {
      try {
        const user: User = JSON.parse(userStr);
        store.dispatch(restoreSession({ user, token }));
      } catch (error) {
        console.error('Error al restaurar sesión:', error);
        // Limpiar localStorage corrupto
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        store.dispatch(setLoading(false));
      }
    } else {
      store.dispatch(setLoading(false));
    }
  }, []);

  // Evitar flash de contenido sin estilo
  if (!mounted) {
    return null;
  }

  return (
    <Provider store={store}>
      <ChakraProvider theme={theme}>
        {children}
      </ChakraProvider>
    </Provider>
  );
}
