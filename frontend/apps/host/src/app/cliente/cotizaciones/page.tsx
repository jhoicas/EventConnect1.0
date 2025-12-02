'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Text,
  VStack,
  HStack,
  Button,
  Icon,
  useColorMode,
  Alert,
  AlertIcon,
  Center,
  Card,
  CardBody,
  Divider,
} from '@chakra-ui/react';
import { Plus, AlertCircle, ShoppingBag } from 'lucide-react';
import { useRouter } from 'next/navigation';

export default function CotizacionesPage() {
  const router = useRouter();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  return (
    <Box bg={bgColor} minH="100vh">
      <Container maxW="container.xl" py={6}>
        <VStack spacing={6} align="stretch">
          {/* Header */}
          <HStack justify="space-between">
            <VStack align="start" spacing={0}>
              <Text fontSize="2xl" fontWeight="bold">
                Mis Cotizaciones
              </Text>
              <Text fontSize="sm" color="gray.500">
                Gestiona tus cotizaciones y solicita reservas
              </Text>
            </VStack>
            <Button 
              leftIcon={<Icon as={Plus} />} 
              colorScheme="blue" 
              size="sm"
              onClick={() => router.push('/cliente/explorar')}
            >
              Explorar Productos
            </Button>
          </HStack>

          {/* Estado Vacío / Información */}
          <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
            <CardBody>
              <Center py={12}>
                <VStack spacing={4} maxW="500px">
                  <Icon as={AlertCircle} boxSize={16} color="blue.400" />
                  <Text fontSize="xl" fontWeight="bold" textAlign="center">
                    Carrito de Cotizaciones
                  </Text>
                  <Text fontSize="md" color="gray.500" textAlign="center">
                    La funcionalidad de carrito y cotizaciones está en desarrollo. Por ahora puedes explorar nuestro catálogo de productos y contactar directamente a los proveedores.
                  </Text>
                  <Alert status="info" borderRadius="lg" mt={4}>
                    <AlertIcon />
                    <VStack align="start" spacing={1}>
                      <Text fontSize="sm" fontWeight="bold">Próximas Funcionalidades:</Text>
                      <Text fontSize="xs">• Agregar productos al carrito</Text>
                      <Text fontSize="xs">• Guardar cotizaciones por proveedor</Text>
                      <Text fontSize="xs">• Solicitar múltiples productos en una sola reserva</Text>
                      <Text fontSize="xs">• Comparar precios entre proveedores</Text>
                    </VStack>
                  </Alert>
                  <Divider my={4} />
                  <Button 
                    leftIcon={<Icon as={ShoppingBag} />} 
                    colorScheme="blue" 
                    size="lg"
                    onClick={() => router.push('/cliente/explorar')}
                  >
                    Ir al Catálogo de Productos
                  </Button>
                </VStack>
              </Center>
            </CardBody>
          </Card>
        </VStack>
      </Container>
    </Box>
  );
}
