'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Input,
  InputGroup,
  InputLeftElement,
  HStack,
  Button,
  SimpleGrid,
  Image,
  Text,
  VStack,
  Badge,
  Icon,
  useColorMode,
  IconButton,
  Flex,
  Tag,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalCloseButton,
  useDisclosure,
  Avatar,
  Divider,
  Textarea,
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  Spinner,
  Center,
} from '@chakra-ui/react';
import { Search, MapPin, Calendar, DollarSign, Star, Package, Mic, Utensils, Armchair, Sparkles, MessageCircle, Send, AlertCircle, Zap, PartyPopper, Building2 } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { productosAPI, empresasAPI, type Producto, type Empresa } from '../../../services/api';

export default function ExplorarPage() {
  const router = useRouter();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('todos');
  const [filtroEmpresa, setFiltroEmpresa] = useState<number | null>(null);
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [selectedProducto, setSelectedProducto] = useState<any>(null);
  const [mensaje, setMensaje] = useState('');
  const [mensajesChat, setMensajesChat] = useState<any[]>([]);
  
  // Estados para datos de API
  const [productos, setProductos] = useState<Producto[]>([]);
  const [empresas, setEmpresas] = useState<Empresa[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  // Cargar datos de la API
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const [productosData, empresasData] = await Promise.all([
          productosAPI.getAll(),
          empresasAPI.getAll(),
        ]);
        
        setProductos(productosData);
        setEmpresas(empresasData);
        
        // Verificar si hay datos
        if (productosData.length === 0) {
          console.warn('No se encontraron productos en la base de datos');
        }
        if (empresasData.length === 0) {
          console.warn('No se encontraron empresas en la base de datos');
        }
      } catch (err) {
        console.error('Error loading data:', err);
        setError('Error al cargar los datos. Verifica que el backend esté corriendo en http://localhost:5555');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  const categorias = [
    { id: 'todos', label: 'Todos', icon: Sparkles, color: 'purple' },
    { id: 'mobiliario', label: 'Mobiliario', icon: Armchair, color: 'blue' },
    { id: 'vajilla', label: 'Vajilla', icon: Utensils, color: 'pink' },
    { id: 'decoracion', label: 'Decoración', icon: Package, color: 'orange' },
    { id: 'tecnologia', label: 'Tecnología', icon: Zap, color: 'green' },
    { id: 'recreacion', label: 'Recreación', icon: PartyPopper, color: 'red' },
  ];

  // Mapear categorías desde los campos de la BD
  const mapCategoria = (sku: string): string => {
    if (sku.includes('SIL') || sku.includes('MES')) return 'mobiliario';
    if (sku.includes('VAJ') || sku.includes('CUB') || sku.includes('CRI')) return 'vajilla';
    if (sku.includes('DEC') || sku.includes('MAN') || sku.includes('CAM')) return 'decoracion';
    if (sku.includes('SON') || sku.includes('ILU') || sku.includes('PRO') || sku.includes('DJ') || sku.includes('LED')) return 'tecnologia';
    if (sku.includes('INF') || sku.includes('ALI')) return 'recreacion';
    return 'otros';
  };

  // Obtener nombre de empresa por ID
  const getEmpresaNombre = (empresaId: number): string => {
    const empresa = empresas.find(e => e.id === empresaId);
    return empresa?.razon_Social || 'Empresa Desconocida';
  };

  // Filtrar productos
  const productosFiltrados = productos.filter((p) => {
    const categoria = mapCategoria(p.sku);
    const matchCategoria = selectedCategory === 'todos' || categoria === selectedCategory;
    const matchSearch = searchQuery === '' || 
      p.nombre.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (p.descripcion && p.descripcion.toLowerCase().includes(searchQuery.toLowerCase())) ||
      getEmpresaNombre(p.empresa_Id).toLowerCase().includes(searchQuery.toLowerCase());
    const matchEmpresa = filtroEmpresa === null || p.empresa_Id === filtroEmpresa;
    
    return matchCategoria && matchSearch && matchEmpresa && p.activo;
  });

  const abrirChat = (producto: Producto) => {
    setSelectedProducto(producto);
    setMensajesChat([
      {
        id: 1,
        tipo: 'sistema',
        mensaje: `¡Hola! Estás interesado en "${producto.nombre}" de ${getEmpresaNombre(producto.empresa_Id)}. La tarifa base es $${producto.precio_Alquiler_Dia.toLocaleString()} por día. ¿Cuándo es tu evento y cuántas unidades necesitas?`,
        fecha: new Date(),
      },
    ]);
    setMensaje('');
    onOpen();
  };

  const enviarMensaje = () => {
    if (!mensaje.trim()) return;

    const nuevoMensaje = {
      id: mensajesChat.length + 1,
      tipo: 'cliente',
      mensaje: mensaje,
      fecha: new Date(),
    };

    setMensajesChat([...mensajesChat, nuevoMensaje]);

    // Simular respuesta del proveedor
    setTimeout(() => {
      const respuesta = {
        id: mensajesChat.length + 2,
        tipo: 'proveedor',
        mensaje: '¡Perfecto! Con esa información puedo prepararte una cotización especial. Dame un momento para calcular el mejor precio para tu evento. ¿Necesitas algo más adicional?',
        fecha: new Date(),
      };
      setMensajesChat((prev) => [...prev, respuesta]);
    }, 1500);

    setMensaje('');
  };

  return (
    <Box bg={bgColor} minH="100vh">
      <Container maxW="container.xl" py={6}>
        {loading ? (
          <Center minH="60vh">
            <VStack spacing={4}>
              <Spinner size="xl" color="blue.500" thickness="4px" />
              <Text color="gray.500">Cargando productos...</Text>
            </VStack>
          </Center>
        ) : error ? (
          <Center minH="60vh">
            <Alert status="error" maxW="2xl" borderRadius="lg">
              <AlertIcon />
              <Box flex="1">
                <AlertTitle mb={2}>Error al cargar los productos</AlertTitle>
                <AlertDescription>
                  <VStack align="start" spacing={2}>
                    <Text>{error}</Text>
                    <Box mt={3} p={3} bg="red.50" borderRadius="md" fontSize="sm">
                      <Text fontWeight="bold" mb={2}>Pasos para solucionar:</Text>
                      <VStack align="start" spacing={1} pl={2}>
                        <Text>1. Verifica que el backend esté corriendo:</Text>
                        <Text ml={4} fontFamily="mono" color="red.700">cd EventConnect.API && dotnet run</Text>
                        <Text>2. Confirma que el servidor responda en:</Text>
                        <Text ml={4} fontFamily="mono" color="red.700">http://localhost:5555/api/Producto</Text>
                        <Text>3. Asegúrate de que la BD tenga datos:</Text>
                        <Text ml={4} fontFamily="mono" color="red.700">Ejecuta database/inserts_data_real.sql</Text>
                      </VStack>
                    </Box>
                    <Button 
                      mt={3} 
                      colorScheme="red" 
                      size="sm"
                      onClick={() => window.location.reload()}
                    >
                      Reintentar
                    </Button>
                  </VStack>
                </AlertDescription>
              </Box>
            </Alert>
          </Center>
        ) : (
        <VStack spacing={6} align="stretch">
          {/* Barra de Búsqueda Grande */}
          <Box>
            <InputGroup size="lg">
              <InputLeftElement>
                <Icon as={Search} color="gray.400" />
              </InputLeftElement>
              <Input
                placeholder="¿Qué buscas hoy? Ej. Sillas, Sonido, Mesas..."
                bg={cardBg}
                borderColor={borderColor}
                _focus={{ borderColor: 'blue.400', boxShadow: '0 0 0 1px #3182ce' }}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </InputGroup>
          </Box>

          {/* Alerta de Precios Negociables */}
          <Alert status="info" borderRadius="lg" bg="blue.50" borderColor="blue.200" borderWidth="1px">
            <AlertIcon as={AlertCircle} color="blue.500" />
            <Box>
              <AlertTitle fontSize="sm" color="blue.800">
                💰 Precios Negociables
              </AlertTitle>
              <AlertDescription fontSize="xs" color="blue.700">
                Los precios mostrados son referenciales. Puedes chatear con cada proveedor para negociar según cantidades y recibir cotizaciones personalizadas.
              </AlertDescription>
            </Box>
          </Alert>

          {/* Filtros Rápidos */}
          <HStack spacing={3} overflowX="auto" pb={2}>
            <Button
              leftIcon={<Icon as={MapPin} />}
              size="sm"
              variant="outline"
              borderColor={borderColor}
              bg={cardBg}
            >
              Ubicación
            </Button>
            <Button
              leftIcon={<Icon as={Calendar} />}
              size="sm"
              variant="outline"
              borderColor={borderColor}
              bg={cardBg}
            >
              Fecha Evento
            </Button>
            <Button
              leftIcon={<Icon as={DollarSign} />}
              size="sm"
              variant="outline"
              borderColor={borderColor}
              bg={cardBg}
            >
              Rango de Precio
            </Button>
          </HStack>

          {/* Filtros por Empresa */}
          <Box>
            <Text fontSize="lg" fontWeight="bold" mb={3}>
              Filtrar por Empresa
            </Text>
            <HStack spacing={3} overflowX="auto" pb={2}>
              <Button
                size="sm"
                variant={filtroEmpresa === null ? 'solid' : 'outline'}
                colorScheme={filtroEmpresa === null ? 'purple' : 'gray'}
                onClick={() => setFiltroEmpresa(null)}
              >
                Todas las empresas
              </Button>
              {empresas.map((empresa) => (
                <Button
                  key={empresa.id}
                  size="sm"
                  variant={filtroEmpresa === empresa.id ? 'solid' : 'outline'}
                  colorScheme={filtroEmpresa === empresa.id ? 'blue' : 'gray'}
                  onClick={() => setFiltroEmpresa(empresa.id)}
                  leftIcon={<Icon as={Building2} boxSize={4} />}
                >
                  {empresa.razon_Social}
                </Button>
              ))}
            </HStack>
          </Box>

          {/* Categorías */}
          <Box>
            <Text fontSize="lg" fontWeight="bold" mb={3}>
              Categorías
            </Text>
            <HStack spacing={4} overflowX="auto" pb={2}>
              {categorias.map((cat) => (
                <VStack
                  key={cat.id}
                  cursor="pointer"
                  onClick={() => setSelectedCategory(cat.id)}
                  spacing={2}
                  minW="80px"
                >
                  <Box
                    p={4}
                    borderRadius="full"
                    bg={selectedCategory === cat.id ? `${cat.color}.100` : cardBg}
                    borderWidth="2px"
                    borderColor={selectedCategory === cat.id ? `${cat.color}.400` : borderColor}
                    transition="all 0.2s"
                    _hover={{ transform: 'scale(1.05)' }}
                  >
                    <Icon as={cat.icon} boxSize={6} color={`${cat.color}.500`} />
                  </Box>
                  <Text
                    fontSize="xs"
                    fontWeight={selectedCategory === cat.id ? 'bold' : 'normal'}
                    color={selectedCategory === cat.id ? `${cat.color}.600` : 'gray.600'}
                  >
                    {cat.label}
                  </Text>
                </VStack>
              ))}
            </HStack>
          </Box>

          {/* Resultados */}
          <Box>
            <Flex justify="space-between" align="center" mb={4}>
              <Text fontSize="lg" fontWeight="bold">
                {productosFiltrados.length} resultados
              </Text>
              <Button size="sm" variant="ghost">
                Ordenar por
              </Button>
            </Flex>

            <SimpleGrid columns={{ base: 1, sm: 2, lg: 3, xl: 4 }} spacing={6}>
              {productosFiltrados.map((producto) => (
                <Box
                  key={producto.id}
                  bg={cardBg}
                  borderRadius="xl"
                  overflow="hidden"
                  borderWidth="1px"
                  borderColor={borderColor}
                  transition="all 0.2s"
                  _hover={{ transform: 'translateY(-4px)', boxShadow: 'lg' }}
                >
                  <Image
                    src={producto.imagen_URL || 'https://via.placeholder.com/600x400?text=Sin+Imagen'}
                    alt={producto.nombre}
                    h="200px"
                    w="100%"
                    objectFit="cover"
                    cursor="pointer"
                    onClick={() => router.push(`/cliente/explorar/${producto.id}`)}
                  />
                  <VStack p={4} align="stretch" spacing={2}>
                    <Badge colorScheme="purple" alignSelf="start" fontSize="xs">
                      {getEmpresaNombre(producto.empresa_Id)}
                    </Badge>
                    <Text fontWeight="bold" fontSize="md" noOfLines={2} minH="48px">
                      {producto.nombre}
                    </Text>
                    <Text fontSize="xs" color="gray.600" noOfLines={2} minH="32px">
                      {producto.descripcion}
                    </Text>
                    <HStack justify="space-between">
                      <Tag size="sm" colorScheme="green" fontSize="xs">
                        Stock: {producto.cantidad_Stock}
                      </Tag>
                      <Tag size="sm" colorScheme="purple" fontSize="xs">
                        {producto.sku}
                      </Tag>
                    </HStack>
                    <Divider />
                    <VStack align="stretch" spacing={1}>
                      <HStack justify="space-between">
                        <Text fontSize="xs" color="gray.500">
                          Tarifa base:
                        </Text>
                        <Text fontSize="lg" fontWeight="bold" color="blue.600">
                          ${producto.precio_Alquiler_Dia.toLocaleString()}/día
                        </Text>
                      </HStack>
                      <Text fontSize="xs" color="green.600" textAlign="right">
                        Precio negociable
                      </Text>
                    </VStack>
                    <HStack spacing={2}>
                      <Button
                        size="sm"
                        colorScheme="blue"
                        flex={1}
                        onClick={(e) => {
                          e.stopPropagation();
                        }}
                      >
                        Agregar
                      </Button>
                      <IconButton
                        aria-label="Chat con proveedor"
                        icon={<Icon as={MessageCircle} />}
                        size="sm"
                        colorScheme="green"
                        variant="outline"
                        onClick={(e) => {
                          e.stopPropagation();
                          abrirChat(producto);
                        }}
                      />
                    </HStack>
                  </VStack>
                </Box>
              ))}
            </SimpleGrid>
          </Box>
        </VStack>
        )}
      </Container>

      {/* Modal de Chat con Proveedor */}
      <Modal isOpen={isOpen} onClose={onClose} size="xl" scrollBehavior="inside">
        <ModalOverlay />
        <ModalContent maxW="600px">
          <ModalHeader>
              <HStack spacing={3}>
              <Avatar size="sm" name={getEmpresaNombre(selectedProducto?.empresa_Id)} src={empresas.find(e => e.id === selectedProducto?.empresa_Id)?.logo_URL} />
              <VStack align="start" spacing={0}>
                <Text fontSize="md">{getEmpresaNombre(selectedProducto?.empresa_Id)}</Text>
                <Text fontSize="xs" color="gray.500" fontWeight="normal">
                  Interesado en: {selectedProducto?.nombre}
                </Text>
              </VStack>
            </HStack>
          </ModalHeader>
          <ModalCloseButton />
          <ModalBody pb={6}>
            {/* Información del producto */}
            <Box bg="blue.50" p={4} borderRadius="md" mb={4}>
              <HStack spacing={3} mb={2}>
                <Image
                  src={selectedProducto?.imagen_URL || 'https://via.placeholder.com/60'}
                  alt={selectedProducto?.nombre}
                  boxSize="60px"
                  borderRadius="md"
                  objectFit="cover"
                />
                <VStack align="start" spacing={0} flex={1}>
                  <Text fontWeight="bold" fontSize="sm">
                    {selectedProducto?.nombre}
                  </Text>
                  <Text fontSize="xs" color="gray.600" noOfLines={2}>
                    {selectedProducto?.descripcion}
                  </Text>
                </VStack>
              </HStack>
              <Divider my={2} />
              <HStack justify="space-between">
                <VStack align="start" spacing={0}>
                  <Text fontSize="xs" color="gray.600">
                    Tarifa Base:
                  </Text>
                  <Text fontSize="xl" fontWeight="bold" color="blue.600">
                    ${selectedProducto?.precio_Alquiler_Dia.toLocaleString()}/día
                  </Text>
                </VStack>
                <VStack align="end" spacing={0}>
                  <Text fontSize="xs" color="green.600">
                    Precio negociable
                  </Text>
                </VStack>
              </HStack>
            </Box>

            {/* Alerta informativa */}
            <Alert status="info" borderRadius="md" mb={4} size="sm">
              <AlertIcon />
              <Box fontSize="xs">
                <Text fontWeight="bold">💬 Negocia directamente con {getEmpresaNombre(selectedProducto?.empresa_Id)}</Text>
                <Text>Los precios varían según cantidad, fechas y servicios adicionales. ¡Solicita tu cotización personalizada!</Text>
              </Box>
            </Alert>

            {/* Chat */}
            <VStack spacing={3} align="stretch" minH="300px" maxH="400px" overflowY="auto" mb={4}>
              {mensajesChat.map((msg) => (
                <Box key={msg.id}>
                  {msg.tipo === 'sistema' && (
                    <Box bg="gray.100" p={3} borderRadius="md" fontSize="sm" textAlign="center" color="gray.700">
                      {msg.mensaje}
                    </Box>
                  )}
                  {msg.tipo === 'cliente' && (
                    <Flex justify="flex-end">
                      <Box bg="blue.500" color="white" p={3} borderRadius="lg" maxW="70%" fontSize="sm">
                        {msg.mensaje}
                      </Box>
                    </Flex>
                  )}
                  {msg.tipo === 'proveedor' && (
                    <Flex justify="flex-start">
                      <Box bg="gray.200" p={3} borderRadius="lg" maxW="70%" fontSize="sm">
                        <HStack spacing={2} mb={1}>
                          <Avatar size="xs" name={getEmpresaNombre(selectedProducto?.empresa_Id)} src={empresas.find(e => e.id === selectedProducto?.empresa_Id)?.logo_URL} />
                          <Text fontWeight="bold" fontSize="xs">{getEmpresaNombre(selectedProducto?.empresa_Id)}</Text>
                        </HStack>
                        {msg.mensaje}
                      </Box>
                    </Flex>
                  )}
                </Box>
              ))}
            </VStack>

            <Divider mb={4} />

            {/* Input de mensaje */}
            <HStack>
              <Textarea
                placeholder="Escribe tu mensaje... Ej: Necesito 50 sillas para el 15 de diciembre"
                value={mensaje}
                onChange={(e) => setMensaje(e.target.value)}
                size="sm"
                resize="none"
                rows={2}
                onKeyPress={(e) => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    enviarMensaje();
                  }
                }}
              />
              <IconButton
                aria-label="Enviar mensaje"
                icon={<Icon as={Send} />}
                colorScheme="blue"
                onClick={enviarMensaje}
                isDisabled={!mensaje.trim()}
              />
            </HStack>

            <Text fontSize="xs" color="gray.500" mt={2}>
              💬 El proveedor podrá enviarte una cotización personalizada con precios negociados según tus necesidades.
            </Text>
          </ModalBody>
        </ModalContent>
      </Modal>
    </Box>
  );
}
