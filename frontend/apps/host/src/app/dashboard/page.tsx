'use client';

import {
  Box,
  Grid,
  Heading,
  Text,
  VStack,
  useColorMode,
  SimpleGrid,
  HStack,
  Icon,
  Button,
  Badge,
  Progress,
  Divider,
  Avatar,
  Image,
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
  CloseButton,
  useDisclosure,
} from '@chakra-ui/react';
import { Card } from '@eventconnect/ui';
import { useAppSelector } from '../../store/store';
import {
  ShoppingBag,
  Calendar,
  MessageCircle,
  TrendingUp,
  Clock,
  CheckCircle,
  Star,
  Package,
  MapPin,
  Sparkles,
  ArrowRight,
  UserPlus,
} from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';

export default function DashboardPage() {
  const router = useRouter();
  const { user } = useAppSelector((state) => state.auth);
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const [pendingUsersCount, setPendingUsersCount] = useState<number>(0);
  const { isOpen: isAlertVisible, onClose: onAlertClose } = useDisclosure({ defaultIsOpen: true });

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  // Cargar usuarios pendientes para admins
  useEffect(() => {
    const fetchPendingUsers = async () => {
      if (user?.rol === 'SuperAdmin' || user?.rol === 'Admin-Proveedor') {
        try {
          const token = localStorage.getItem('token');
          const response = await fetch('http://localhost:5555/api/Usuario/pendientes/count', {
            headers: {
              'Authorization': `Bearer ${token}`,
            },
          });
          if (response.ok) {
            const data = await response.json();
            setPendingUsersCount(data.count);
          }
        } catch (error) {
          console.error('Error al obtener usuarios pendientes:', error);
        }
      }
    };

    fetchPendingUsers();
  }, [user?.rol]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  // TODO: Conectar con API real para obtener estadísticas del usuario
  const estadisticas = [
    {
      label: 'Cotizaciones Activas',
      value: '0',
      description: 'En proceso',
      icon: ShoppingBag,
      color: 'blue',
      action: () => router.push('/cliente/cotizaciones'),
    },
    {
      label: 'Reservas Confirmadas',
      value: '0',
      description: 'Próximos eventos',
      icon: Calendar,
      color: 'green',
      action: () => router.push('/cliente/reservas'),
    },
    {
      label: 'Mensajes sin leer',
      value: '0',
      description: 'Nuevos mensajes',
      icon: MessageCircle,
      color: 'purple',
      action: () => router.push('/cliente/mensajes'),
    },
    {
      label: 'Gastos Totales',
      value: '$0',
      description: 'Últimos 6 meses',
      icon: TrendingUp,
      color: 'orange',
      action: () => {},
    },
  ];

  // TODO: Obtener de API - reservasAPI.getByEstado('Pendiente')
  const reservaPendiente = null;

  // TODO: Obtener de API - reservasAPI.getByEstado('Confirmada')
  const proximaReserva = null;

  // TODO: Obtener de API - productosAPI.getPopulares()
  const productosPopulares: any[] = [];

  return (
    <VStack spacing={{ base: 4, md: 6 }} align="stretch" px={{ base: 4, md: 0 }}>
      {/* Alerta de Usuarios Pendientes - Solo para Admins */}
      {(user?.rol === 'SuperAdmin' || user?.rol === 'Admin-Proveedor') && pendingUsersCount > 0 && isAlertVisible && (
        <Alert
          status="warning"
          variant="left-accent"
          borderRadius="xl"
          boxShadow="md"
        >
          <AlertIcon as={UserPlus} />
          <Box flex="1">
            <AlertTitle fontSize="md" fontWeight="bold">
              {pendingUsersCount} {pendingUsersCount === 1 ? 'usuario empresa pendiente' : 'usuarios empresa pendientes'} de activación
            </AlertTitle>
            <AlertDescription fontSize="sm">
              Hay usuarios que requieren tu aprobación para acceder al sistema.
            </AlertDescription>
          </Box>
          <Button
            size="sm"
            colorScheme="orange"
            onClick={() => router.push('/dashboard/usuarios')}
            mr={2}
          >
            Gestionar Usuarios
          </Button>
          <CloseButton
            alignSelf="flex-start"
            position="relative"
            right={-1}
            top={-1}
            onClick={onAlertClose}
          />
        </Alert>
      )}

      {/* Estadísticas Rápidas */}
      <SimpleGrid columns={{ base: 1, sm: 2, lg: 4 }} spacing={{ base: 3, md: 4 }}>
        {estadisticas.map((stat) => (
          <Box
            key={stat.label}
            bg={cardBg}
            p={{ base: 4, md: 6 }}
            borderRadius="xl"
            borderWidth="1px"
            borderColor={borderColor}
            cursor="pointer"
            transition="all 0.2s"
            _hover={{ transform: 'translateY(-4px)', boxShadow: 'lg' }}
            onClick={stat.action}
          >
            <HStack justify="space-between" mb={3}>
              <Box
                p={3}
                borderRadius="lg"
                bg={`${stat.color}.50`}
              >
                <Icon as={stat.icon} boxSize={{ base: 5, md: 6 }} color={`${stat.color}.500`} />
              </Box>
              <Icon as={ArrowRight} color="gray.400" />
            </HStack>
            <Text fontSize={{ base: "2xl", md: "3xl" }} fontWeight="bold">
              {stat.value}
            </Text>
            <Text fontSize={{ base: "xs", md: "sm" }} fontWeight="medium" mb={1}>
              {stat.label}
            </Text>
            <Text fontSize="xs" color="gray.500">
              {stat.description}
            </Text>
          </Box>
        ))}
      </SimpleGrid>

      <Grid templateColumns={{ base: '1fr', lg: '2fr 1fr' }} gap={{ base: 4, md: 6 }}>
        {/* Columna Izquierda */}
        <VStack spacing={{ base: 4, md: 6 }} align="stretch">
          {/* Reserva Pendiente de Pago */}
          {reservaPendiente && (
            <Box
              bg="orange.50"
              borderWidth="2px"
              borderColor="orange.200"
              borderRadius="xl"
              p={{ base: 4, md: 6 }}
            >
              <HStack 
                justify="space-between" 
                mb={4}
                flexWrap={{ base: "wrap", md: "nowrap" }}
                gap={{ base: 2, md: 0 }}
              >
                <HStack>
                  <Icon as={Clock} color="orange.600" boxSize={{ base: 5, md: 6 }} />
                  <VStack align="start" spacing={0}>
                    <Text fontSize={{ base: "md", md: "lg" }} fontWeight="bold" color="orange.700">
                      Acción Requerida
                    </Text>
                    <Text fontSize={{ base: "xs", md: "sm" }} color="orange.600">
                      Tienes una reserva pendiente de pago
                    </Text>
                  </VStack>
                </HStack>
                <Badge colorScheme="orange" fontSize={{ base: "xs", md: "sm" }}>
                  Urgente
                </Badge>
              </HStack>

              <Box bg="white" p={{ base: 3, md: 4 }} borderRadius="lg" mb={3}>
                <HStack justify="space-between" mb={2} flexWrap="wrap">
                  <Text fontWeight="bold" fontSize={{ base: "sm", md: "md" }}>{reservaPendiente.nombre}</Text>
                  <Text fontSize={{ base: "xs", md: "sm" }} color="gray.500">
                    {reservaPendiente.codigo}
                  </Text>
                </HStack>
                <Text fontSize={{ base: "xs", md: "sm" }} color="gray.600" mb={2}>
                  📅 {new Date(reservaPendiente.fechaEvento).toLocaleDateString('es-ES', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </Text>
                <Text fontSize={{ base: "xl", md: "2xl" }} fontWeight="bold" color="blue.600">
                  ${reservaPendiente.total.toLocaleString()}
                </Text>
              </Box>

              <Progress value={75} size="sm" colorScheme="orange" borderRadius="full" mb={2} />
              <Text fontSize="xs" color="orange.600" mb={3}>
                Quedan {reservaPendiente.tiempoRestante} horas para asegurar tu reserva
              </Text>

              <Button
                colorScheme="orange"
                size={{ base: "md", md: "lg" }}
                w="full"
                onClick={() => router.push('/cliente/reservas')}
              >
                Subir Comprobante de Pago
              </Button>
            </Box>
          )}

          {/* Próxima Reserva */}
          {proximaReserva ? (
            <Box
              bg={cardBg}
              borderWidth="1px"
              borderColor={borderColor}
              borderRadius="xl"
              overflow="hidden"
            >
              <Image
                src={proximaReserva.imagen}
                alt={proximaReserva.nombre}
                h={{ base: "150px", md: "200px" }}
                w="100%"
                objectFit="cover"
              />
              <Box p={{ base: 4, md: 6 }}>
                <HStack justify="space-between" mb={2} flexWrap="wrap" gap={2}>
                  <Badge colorScheme="green">Confirmada</Badge>
                  <Text fontSize={{ base: "xs", md: "sm" }} color="gray.500">
                    {proximaReserva.codigo}
                  </Text>
                </HStack>
                <Heading size={{ base: "sm", md: "md" }} mb={2}>
                  {proximaReserva.nombre}
                </Heading>
                <HStack 
                  spacing={4} 
                  mb={4}
                  flexWrap={{ base: "wrap", md: "nowrap" }}
                  gap={{ base: 2, md: 0 }}
                >
                  <HStack>
                    <Icon as={Calendar} boxSize={4} color="gray.500" />
                    <Text fontSize={{ base: "xs", md: "sm" }} color="gray.600">
                      {new Date(proximaReserva.fechaEvento).toLocaleDateString('es-ES', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                      })}
                    </Text>
                  </HStack>
                  <HStack>
                    <Icon as={Package} boxSize={4} color="gray.500" />
                    <Text fontSize={{ base: "xs", md: "sm" }} color="gray.600">
                      {proximaReserva.items} productos
                    </Text>
                  </HStack>
                </HStack>
                <Button
                  variant="outline"
                  w="full"
                  size={{ base: "sm", md: "md" }}
                  onClick={() => router.push('/cliente/reservas')}
                >
                  Ver Detalles
                </Button>
              </Box>
            </Box>
          ) : (
            <Box
              bg={cardBg}
              borderWidth="1px"
              borderColor={borderColor}
              borderRadius="xl"
              p={{ base: 6, md: 8 }}
              textAlign="center"
            >
              <Icon as={Calendar} boxSize={{ base: 10, md: 12 }} color="gray.300" mb={4} />
              <Heading size={{ base: "sm", md: "md" }} mb={2} color="gray.600">
                No tienes reservas próximas
              </Heading>
              <Text fontSize={{ base: "sm", md: "md" }} color="gray.500" mb={4}>
                Explora nuestro catálogo y crea tu primera reserva
              </Text>
              <Button
                colorScheme="blue"
                size={{ base: "sm", md: "md" }}
                onClick={() => router.push('/cliente/explorar')}
              >
                Explorar Catálogo
              </Button>
            </Box>
          )}
        </VStack>

        {/* Columna Derecha */}
        <VStack spacing={{ base: 4, md: 6 }} align="stretch">
          {/* Accesos Rápidos */}
          <Box
            bg={cardBg}
            borderWidth="1px"
            borderColor={borderColor}
            borderRadius="xl"
            p={{ base: 4, md: 6 }}
          >
            <Text fontSize={{ base: "md", md: "lg" }} fontWeight="bold" mb={4}>
              Accesos Rápidos
            </Text>
            <VStack spacing={3}>
              <Button
                w="full"
                justifyContent="start"
                leftIcon={<Icon as={Sparkles} />}
                colorScheme="blue"
                size={{ base: "sm", md: "md" }}
                onClick={() => router.push('/cliente/explorar')}
              >
                Explorar Productos
              </Button>
              <Button
                w="full"
                justifyContent="start"
                leftIcon={<Icon as={ShoppingBag} />}
                size={{ base: "sm", md: "md" }}
                variant="outline"
                onClick={() => router.push('/cliente/cotizaciones')}
              >
                Mis Cotizaciones
              </Button>
              <Button
                w="full"
                justifyContent="start"
                leftIcon={<Icon as={MessageCircle} />}
                variant="outline"
                size={{ base: "sm", md: "md" }}
                onClick={() => router.push('/cliente/mensajes')}
              >
                Mensajes
              </Button>
            </VStack>
          </Box>

          {/* Productos Populares */}
          <Box
            bg={cardBg}
            borderWidth="1px"
            borderColor={borderColor}
            borderRadius="xl"
            p={{ base: 4, md: 6 }}
          >
            <HStack justify="space-between" mb={4}>
              <Text fontSize={{ base: "md", md: "lg" }} fontWeight="bold">
                Lo Más Buscado
              </Text>
              <Icon as={TrendingUp} color="green.500" boxSize={{ base: 5, md: 6 }} />
            </HStack>
            {productosPopulares.length > 0 ? (
              <>
                <VStack spacing={3} align="stretch">
                  {productosPopulares.map((producto, idx) => (
                    <Box key={idx}>
                      <HStack spacing={3}>
                        <Image
                          src={producto.imagen}
                          alt={producto.nombre}
                          boxSize={{ base: "40px", md: "50px" }}
                          borderRadius="md"
                          objectFit="cover"
                        />
                        <VStack flex={1} align="start" spacing={0}>
                          <Text fontSize={{ base: "xs", md: "sm" }} fontWeight="medium" noOfLines={1}>
                            {producto.nombre}
                          </Text>
                          <HStack spacing={1}>
                            <Icon as={Star} boxSize={3} color="yellow.400" fill="yellow.400" />
                            <Text fontSize="xs" color="gray.600">
                              {producto.calificacion}
                            </Text>
                          </HStack>
                          <Text fontSize="xs" fontWeight="bold" color="blue.600">
                            ${producto.precio.toLocaleString()}/día
                          </Text>
                        </VStack>
                      </HStack>
                      {idx < productosPopulares.length - 1 && <Divider mt={3} />}
                    </Box>
                  ))}
                </VStack>
                <Button
                  size="sm"
                  variant="ghost"
                  w="full"
                  mt={3}
                  onClick={() => router.push('/cliente/explorar')}
                >
                  Ver Todos
                </Button>
              </>
            ) : (
              <VStack py={6} spacing={2}>
                <Icon as={Package} boxSize={10} color="gray.300" />
                <Text fontSize="sm" color="gray.500" textAlign="center">
                  Explora nuestro catálogo para ver productos destacados
                </Text>
                <Button
                  size="sm"
                  colorScheme="blue"
                  mt={2}
                  onClick={() => router.push('/cliente/explorar')}
                >
                  Ver Catálogo
                </Button>
              </VStack>
            )}
          </Box>

          {/* Tips */}
          <Box
            bg="blue.50"
            borderWidth="1px"
            borderColor="blue.200"
            borderRadius="xl"
            p={6}
          >
            <HStack mb={3}>
              <Icon as={Sparkles} color="blue.600" />
              <Text fontSize="md" fontWeight="bold" color="blue.700">
                💡 Tip del día
              </Text>
            </HStack>
            <Text fontSize="sm" color="blue.700">
              ¿Sabías que puedes chatear con múltiples proveedores antes de reservar? 
              Compara precios y negocia directamente.
            </Text>
          </Box>
        </VStack>
      </Grid>
    </VStack>
  );
}

