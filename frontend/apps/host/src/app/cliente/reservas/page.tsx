'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Text,
  VStack,
  HStack,
  Button,
  Badge,
  Icon,
  useColorMode,
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  Card,
  CardBody,
  Divider,
  Flex,
  Progress,
  Alert,
  AlertIcon,
  Image,
  SimpleGrid,
  IconButton,
  useDisclosure,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Input,
  Spinner,
  Center,
} from '@chakra-ui/react';
import { Clock, CheckCircle, Upload, Phone, MapPin, QrCode, Calendar, Package } from 'lucide-react';
import { reservasAPI, type Reserva } from '../../../services/api';

export default function ReservasPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [selectedReserva, setSelectedReserva] = useState<Reserva | null>(null);
  
  // Estados para datos de API
  const [reservas, setReservas] = useState<Reserva[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  // Cargar reservas de la API
  useEffect(() => {
    const loadReservas = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Verificar que el token existe
        const token = localStorage.getItem('token');
        if (!token) {
          console.warn('No hay token de autenticación');
          setError('No estás autenticado. Por favor, inicia sesión nuevamente.');
          setLoading(false);
          return;
        }
        
        const data = await reservasAPI.getAll();
        setReservas(data);
      } catch (err) {
        console.error('Error loading reservas:', err);
        setError('Error al cargar las reservas. Por favor, intenta de nuevo.');
      } finally {
        setLoading(false);
      }
    };

    loadReservas();
  }, []);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  // Filtrar reservas por estado
  const reservasPendientes = reservas.filter(r => r.estado === 'Pendiente' || r.estado === 'Cotizada');
  const reservasConfirmadas = reservas.filter(r => r.estado === 'Confirmada' || r.estado === 'En_Curso');
  const reservasHistorial = reservas.filter(r => r.estado === 'Completada' || r.estado === 'Cancelada');

  return (
    <Box bg={bgColor} minH="100vh">
      <Container maxW="container.xl" py={6}>
        <VStack spacing={6} align="stretch">
          {/* Header */}
          <VStack align="start" spacing={0}>
            <Text fontSize="2xl" fontWeight="bold">
              Mis Reservas
            </Text>
            <Text fontSize="sm" color="gray.500">
              Gestiona tus reservas y mantente al día con tus eventos
            </Text>
          </VStack>

          {/* Loading State */}
          {loading && (
            <Center py={20}>
              <VStack spacing={4}>
                <Spinner size="xl" color="blue.500" thickness="4px" />
                <Text color="gray.500">Cargando reservas...</Text>
              </VStack>
            </Center>
          )}

          {/* Error State */}
          {error && (
            <Alert status="error" borderRadius="lg" mb={6}>
              <AlertIcon />
              {error}
            </Alert>
          )}

          {/* Content */}
          {!loading && !error && (
            <Tabs variant="soft-rounded" colorScheme="blue">
              <TabList>
                <Tab>
                  <HStack>
                    <Icon as={Clock} />
                    <Text>Pendientes de Pago</Text>
                    <Badge colorScheme="orange">{reservasPendientes.length}</Badge>
                  </HStack>
                </Tab>
                <Tab>
                  <HStack>
                    <Icon as={CheckCircle} />
                    <Text>Confirmadas</Text>
                    <Badge colorScheme="green">{reservasConfirmadas.length}</Badge>
                  </HStack>
                </Tab>
                <Tab>
                <HStack>
                  <Icon as={Package} />
                  <Text>Historial</Text>
                  <Badge>{reservasHistorial.length}</Badge>
                </HStack>
              </Tab>
            </TabList>

            <TabPanels mt={6}>
              {/* Tab: Pendientes */}
              <TabPanel p={0}>
                <VStack spacing={4} align="stretch">
                  {reservasPendientes.length === 0 ? (
                    <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
                      <CardBody>
                        <Center py={8}>
                          <VStack spacing={2}>
                            <Text fontSize="lg" color="gray.500">No tienes reservas pendientes</Text>
                            <Text fontSize="sm" color="gray.400">¡Explora nuestro catálogo y crea tu primera reserva!</Text>
                          </VStack>
                        </Center>
                      </CardBody>
                    </Card>
                  ) : (
                    reservasPendientes.map((reserva) => (
                    <Card key={reserva.id} bg={cardBg} borderColor={borderColor} borderWidth="1px">
                      <CardBody>
                        <VStack spacing={4} align="stretch">
                          {/* Header */}
                          <Flex justify="space-between" align="start">
                            <VStack align="start" spacing={0}>
                              <HStack>
                                <Text fontSize="lg" fontWeight="bold">
                                  Reserva {reserva.codigo_Reserva}
                                </Text>
                                <Badge colorScheme="orange" fontSize="sm">
                                  🟠 {reserva.estado}
                                </Badge>
                              </HStack>
                              <Text fontSize="sm" color="gray.500">
                                Código: {reserva.codigo_Reserva}
                              </Text>
                            </VStack>
                            <Text fontSize="2xl" fontWeight="bold" color="blue.600">
                              ${reserva.total.toLocaleString()}
                            </Text>
                          </Flex>

                          {/* Cuenta Regresiva */}
                          <Alert status="warning" borderRadius="lg">
                            <AlertIcon as={Clock} />
                            <Box flex="1">
                              <Text fontSize="sm" fontWeight="bold">
                                Completa el pago para asegurar tu reserva
                              </Text>
                              <Progress
                                value={50}
                                size="sm"
                                colorScheme="orange"
                                mt={2}
                                borderRadius="full"
                              />
                            </Box>
                          </Alert>

                          {/* Info Evento */}
                          <SimpleGrid columns={2} spacing={3}>
                            <HStack>
                              <Icon as={Calendar} color="blue.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontSize="xs" color="gray.500">
                                  Fecha del Evento
                                </Text>
                                <Text fontSize="sm" fontWeight="medium">
                                  {reserva.fecha_Evento ? new Date(reserva.fecha_Evento).toLocaleDateString('es-ES', {
                                    year: 'numeric',
                                    month: 'long',
                                    day: 'numeric',
                                  }) : 'No especificada'}
                                </Text>
                              </VStack>
                            </HStack>
                            <HStack>
                              <Icon as={MapPin} color="blue.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontSize="xs" color="gray.500">
                                  Ubicación
                                </Text>
                                <Text fontSize="sm" fontWeight="medium">
                                  {reserva.ciudad_Entrega || 'Por definir'}
                                </Text>
                              </VStack>
                            </HStack>
                          </SimpleGrid>

                          <Divider />

                          {/* Observaciones */}
                          {reserva.observaciones && (
                            <Box>
                              <Text fontSize="xs" color="gray.500" mb={1}>Observaciones:</Text>
                              <Text fontSize="sm" color="gray.600">{reserva.observaciones}</Text>
                            </Box>
                          )}

                          <Divider />

                          {/* Acción Principal */}
                          <Button
                            leftIcon={<Icon as={Upload} />}
                            colorScheme="orange"
                            size="lg"
                            onClick={() => {
                              setSelectedReserva(reserva);
                              onOpen();
                            }}
                          >
                            📤 Subir Comprobante de Pago
                          </Button>
                          <Text fontSize="xs" color="gray.500" textAlign="center">
                            Una vez subas el comprobante, el proveedor verificará tu pago en 2-4 horas
                          </Text>
                        </VStack>
                      </CardBody>
                    </Card>
                  )))}
                </VStack>
              </TabPanel>

              {/* Tab: Confirmadas */}
              <TabPanel p={0}>
                <VStack spacing={4} align="stretch">
                  {reservasConfirmadas.length === 0 ? (
                    <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
                      <CardBody>
                        <Center py={8}>
                          <VStack spacing={2}>
                            <Text fontSize="lg" color="gray.500">No tienes reservas confirmadas</Text>
                            <Text fontSize="sm" color="gray.400">Las reservas aparecerán aquí una vez confirmado el pago</Text>
                          </VStack>
                        </Center>
                      </CardBody>
                    </Card>
                  ) : (
                    reservasConfirmadas.map((reserva) => (
                    <Card
                      key={reserva.id}
                      bg={cardBg}
                      borderColor="green.200"
                      borderWidth="2px"
                      boxShadow="md"
                    >
                      <CardBody>
                        <VStack spacing={4} align="stretch">
                          {/* Header */}
                          <Flex justify="space-between" align="start">
                            <VStack align="start" spacing={0}>
                              <HStack>
                                <Text fontSize="lg" fontWeight="bold">
                                  Reserva {reserva.codigo_Reserva}
                                </Text>
                                <Badge colorScheme="green" fontSize="sm">
                                  🟢 {reserva.estado}
                                </Badge>
                              </HStack>
                              <Text fontSize="sm" color="gray.500">
                                Código: {reserva.codigo_Reserva}
                              </Text>
                            </VStack>
                            <Text fontSize="2xl" fontWeight="bold" color="green.600">
                              ${reserva.total.toLocaleString()}
                            </Text>
                          </Flex>

                          {/* Info Evento */}
                          <SimpleGrid columns={2} spacing={3}>
                            <HStack>
                              <Icon as={Calendar} color="green.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontSize="xs" color="gray.500">
                                  Fecha del Evento
                                </Text>
                                <Text fontSize="sm" fontWeight="medium">
                                  {reserva.fecha_Evento ? new Date(reserva.fecha_Evento).toLocaleDateString('es-ES', {
                                    year: 'numeric',
                                    month: 'long',
                                    day: 'numeric',
                                  }) : 'No especificada'}
                                </Text>
                              </VStack>
                            </HStack>
                            <HStack>
                              <Icon as={MapPin} color="green.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontSize="xs" color="gray.500">
                                  Ubicación
                                </Text>
                                <Text fontSize="sm" fontWeight="medium">
                                  {reserva.ciudad_Entrega || 'Por definir'}
                                </Text>
                              </VStack>
                            </HStack>
                          </SimpleGrid>

                          <Divider />

                          {/* Direccion de Entrega */}
                          {reserva.direccion_Entrega && (
                            <Box bg="green.50" p={4} borderRadius="lg">
                              <Text fontSize="sm" fontWeight="bold" color="green.700" mb={2}>
                                📍 Dirección de Entrega
                              </Text>
                              <Text fontSize="sm">{reserva.direccion_Entrega}</Text>
                            </Box>
                          )}

                          {/* Observaciones */}
                          {reserva.observaciones && (
                            <Box>
                              <Text fontSize="xs" color="gray.500" mb={1}>Observaciones:</Text>
                              <Text fontSize="sm" color="gray.600">{reserva.observaciones}</Text>
                            </Box>
                          )}

                          <Divider />

                          {/* QR Code Placeholder */}
                          <VStack>
                            <Text fontSize="sm" fontWeight="bold">
                              Código QR para Recogida
                            </Text>
                            <Box 
                              w="150px" 
                              h="150px" 
                              bg="gray.200" 
                              borderRadius="md" 
                              display="flex" 
                              alignItems="center" 
                              justifyContent="center"
                            >
                              <Icon as={QrCode} boxSize={16} color="gray.400" />
                            </Box>
                            <Text fontSize="xs" color="gray.500" textAlign="center">
                              Muestra este código al recoger los equipos
                            </Text>
                          </VStack>

                          {/* Acciones */}
                          <Button leftIcon={<Icon as={Phone} />} colorScheme="green" size="lg">
                            Contactar Proveedor
                          </Button>
                        </VStack>
                      </CardBody>
                    </Card>
                  )))}
                </VStack>
              </TabPanel>

              {/* Tab: Historial */}
              <TabPanel p={0}>
                <VStack spacing={4} align="stretch">
                  {reservasHistorial.length === 0 ? (
                    <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
                      <CardBody>
                        <Center py={8}>
                          <VStack spacing={2}>
                            <Text fontSize="lg" color="gray.500">No tienes reservas en el historial</Text>
                            <Text fontSize="sm" color="gray.400">Aquí aparecerán tus reservas completadas o canceladas</Text>
                          </VStack>
                        </Center>
                      </CardBody>
                    </Card>
                  ) : (
                    reservasHistorial.map((reserva) => (
                    <Card key={reserva.id} bg={cardBg} borderColor={borderColor} borderWidth="1px">
                      <CardBody>
                        <Flex justify="space-between" align="center">
                          <VStack align="start" spacing={0}>
                            <Text fontSize="md" fontWeight="bold">
                              Reserva {reserva.codigo_Reserva}
                            </Text>
                            <Text fontSize="sm" color="gray.500">
                              {reserva.codigo_Reserva} • {reserva.fecha_Evento ? new Date(reserva.fecha_Evento).toLocaleDateString() : 'Sin fecha'}
                            </Text>
                            <Badge colorScheme={reserva.estado === 'Completada' ? 'green' : 'red'} mt={2}>
                              {reserva.estado}
                            </Badge>
                          </VStack>
                          <VStack align="end">
                            <Text fontSize="lg" fontWeight="bold">
                              ${reserva.total.toLocaleString()}
                            </Text>
                            <Text fontSize="xs" color="gray.500">
                              {new Date(reserva.fecha_Creacion).toLocaleDateString('es-ES')}
                            </Text>
                          </VStack>
                        </Flex>
                      </CardBody>
                    </Card>
                  )))}
                </VStack>
              </TabPanel>
            </TabPanels>
          </Tabs>
        )}
        </VStack>
      </Container>

      {/* Modal Subir Comprobante */}
      <Modal isOpen={isOpen} onClose={onClose} size="lg">
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Subir Comprobante de Pago</ModalHeader>
          <ModalBody>
            <VStack spacing={4}>
              <Alert status="info" borderRadius="lg">
                <AlertIcon />
                <Text fontSize="sm">
                  Sube una foto clara del comprobante de transferencia o consignación. El proveedor
                  verificará tu pago en 2-4 horas.
                </Text>
              </Alert>
              <Input type="file" accept="image/*" />
              <Input placeholder="Número de referencia (opcional)" />
            </VStack>
          </ModalBody>
          <ModalFooter>
            <Button variant="ghost" mr={3} onClick={onClose}>
              Cancelar
            </Button>
            <Button colorScheme="blue" leftIcon={<Icon as={Upload} />}>
              Subir Comprobante
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </Box>
  );
}
