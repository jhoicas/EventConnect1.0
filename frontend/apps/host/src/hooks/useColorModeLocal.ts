import { useEffect, useState } from 'react';

type ColorMode = 'light' | 'dark' | 'blue';

/**
 * Hook personalizado para obtener el color mode del localStorage
 * y calcular los colores de fondo y bordes para modales
 */
export function useColorModeLocal() {
  const [colorMode, setColorMode] = useState<ColorMode>('light');

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  // Colores calculados basados en el colorMode
  const colors = {
    bgColor: colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff',
    inputBg: colorMode === 'dark' ? '#242b3d' : colorMode === 'blue' ? '#1e3140' : '#f5f6f8',
    borderColor: colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0',
  };

  return {
    colorMode,
    ...colors,
  };
}
