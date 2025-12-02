'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Text,
  VStack,
  Button,
  Avatar,
  FormControl,
  FormLabel,
  Input,
  useColorMode,
  Card,
  CardBody,
  Divider,
  HStack,
  Icon,
  Badge,
  SimpleGrid,
  Flex,
} from '@chakra-ui/react';
import { User, Mail, Phone, MapPin, CreditCard, HelpCircle, LogOut, Edit2 } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useAppSelector } from '../../../store/store';

export default function PerfilPage() {
  const router = useRouter();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { user } = useAppSelector((state) => state.auth);

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  // Obtener datos del usuario autenticado
  const usuario = {
    nombre: user?.nombre_Completo || 'Usuario',
    email: user?.email || '',
    telefono: user?.telefono || 'No registrado',
    avatar: (user?.avatar_URL && user.avatar_URL.trim() !== '') 
      ? user.avatar_URL 
      : `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.nombre_Completo || 'U')}&background=6366f1&color=fff&size=150`,
  };

  console.log('Usuario actual:', user);
  console.log('Avatar a mostrar:', usuario.avatar);

  const handleUpdateAvatar = async () => {
    console.log('=== INICIO handleUpdateAvatar ===');
    console.log('avatarUrl:', avatarUrl);
    console.log('user:', user);
    
    if (!avatarUrl.trim()) {
      console.log('ERROR: URL vacía');
      toast({
        title: 'Error',
        description: 'Por favor ingresa una URL válida',
        status: 'error',
        duration: 3000,
      });
      return;
    }

    setIsUpdating(true);
    console.log('Enviando petición a:', `http://localhost:5555/api/Usuario/${user?.id}/perfil`);
    
    try {
      const token = localStorage.getItem('token');
      console.log('Token:', token ? 'Existe' : 'No existe');
      
      const requestBody = {
        Avatar_URL: avatarUrl,
      };
      console.log('Body de la petición:', JSON.stringify(requestBody));
      
      const response = await fetch(`http://localhost:5555/api/Usuario/${user?.id}/perfil`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody),
      });

      console.log('Response status:', response.status);
      console.log('Response ok:', response.ok);

      if (response.ok) {
        const data = await response.json();
        console.log('Respuesta del servidor:', data);
        
        // Actualizar Redux con el nuevo avatar
        const token = localStorage.getItem('token');
        const updatedUser = { ...user!, avatar_URL: avatarUrl };
        
        console.log('Actualizando Redux con:', updatedUser);
        
        dispatch(setCredentials({
          user: updatedUser,
          token: token || '',
        }));
        
        toast({
          title: 'Éxito',
          description: 'Avatar actualizado correctamente',
          status: 'success',
          duration: 3000,
        });
        
        onClose();
        setAvatarUrl('');
      } else {
        const errorData = await response.json();
        console.error('Error del servidor:', errorData);
        throw new Error(errorData.message || 'Error al actualizar avatar');
      }
    } catch (error: any) {
      console.error('Error en catch:', error);
      toast({
        title: 'Error',
        description: error.message || 'No se pudo actualizar el avatar',
        status: 'error',
        duration: 3000,
      });
    } finally {
      setIsUpdating(false);
      console.log('=== FIN handleUpdateAvatar ===');
    }
  };

  const direcciones = [
    { id: 1, nombre: 'Casa', direccion: 'Calle 123 #45-67, Bogotá', principal: true },
    { id: 2, nombre: 'Oficina', direccion: 'Carrera 7 #80-20, Bogotá', principal: false },
  ];

  return (
    <Box bg={bgColor} minH="100vh">
      <Container maxW="container.lg" py={6}>
        <VStack spacing={6} align="stretch">
          {/* Header */}
          <VStack align="start" spacing={0}>
            <Text fontSize="2xl" fontWeight="bold">
              Mi Perfil
            </Text>
            <Text fontSize="sm" color="gray.500">
              Gestiona tu información personal y preferencias
            </Text>
          </VStack>

          {/* Avatar y Datos Principales */}
          <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
            <CardBody>
              <VStack spacing={4}>
                <Avatar src={usuario.avatar} size="2xl" />
                <VStack spacing={0}>
                  <Text fontSize="xl" fontWeight="bold">
                    {usuario.nombre}
                  </Text>
                  <Badge colorScheme="blue">Cliente Verificado</Badge>
                </VStack>
              </VStack>
            </CardBody>
          </Card>

          {/* Información Personal */}
          <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
            <CardBody>
              <VStack spacing={4} align="stretch">
                <HStack justify="space-between">
                  <Text fontSize="lg" fontWeight="bold">
                    Información Personal
                  </Text>
                  <Button size="sm" leftIcon={<Icon as={Edit2} />} variant="ghost">
                    Editar
                  </Button>
                </HStack>
                <Divider />
                <SimpleGrid columns={{ base: 1, md: 2 }} spacing={4}>
                  <FormControl>
                    <FormLabel fontSize="sm">Nombre Completo</FormLabel>
                    <Input value={usuario.nombre} isReadOnly />
                  </FormControl>
                  <FormControl>
                    <FormLabel fontSize="sm">Email</FormLabel>
                    <Input value={usuario.email} isReadOnly />
                  </FormControl>
                  <FormControl>
                    <FormLabel fontSize="sm">Teléfono</FormLabel>
                    <Input value={usuario.telefono} isReadOnly />
                  </FormControl>
                </SimpleGrid>
              </VStack>
            </CardBody>
          </Card>

          {/* Direcciones Guardadas - Comentado temporalmente hasta implementar funcionalidad
          <Card bg={cardBg} borderColor={borderColor} borderWidth="1px">
            <CardBody>
              <VStack spacing={4} align="stretch">
                <HStack justify="space-between">
                  <Text fontSize="lg" fontWeight="bold">
                    Direcciones Guardadas
                  </Text>
                  <Button size="sm" leftIcon={<Icon as={MapPin} />} colorScheme="blue">
                    Nueva Dirección
                  </Button>
                </HStack>
                <Divider />
                <VStack spacing={3} align="stretch">
                  {direcciones.map((dir) => (
                    <Box
                      key={dir.id}
                      p={4}
                      borderWidth="1px"
                      borderColor={borderColor}
                      borderRadius="lg"
                    >
                      <Flex justify="space-between" align="start">
                        <VStack align="start" spacing={1}>
                          <HStack>
                            <Text fontWeight="bold">{dir.nombre}</Text>
                            {dir.principal && (
                              <Badge colorScheme="green" fontSize="xs">
                                Principal
                              </Badge>
                            )}
                          </HStack>
                          <Text fontSize="sm" color="gray.600">
                            {dir.direccion}
                          </Text>
                        </VStack>
                        <Button size="xs" variant="ghost">
                          Editar
                        </Button>
                      </Flex>
                    </Box>
                  ))}
                </VStack>
              </VStack>
            </CardBody>
          </Card>
          */}

          {/* Opciones */}
          <VStack spacing={3}>
            <Button
              w="full"
              justifyContent="start"
              leftIcon={<Icon as={CreditCard} />}
              variant="outline"
              size="lg"
            >
              Métodos de Pago
            </Button>
            <Button
              w="full"
              justifyContent="start"
              leftIcon={<Icon as={HelpCircle} />}
              variant="outline"
              size="lg"
            >
              Ayuda y Soporte
            </Button>
            <Button
              w="full"
              justifyContent="start"
              leftIcon={<Icon as={LogOut} />}
              colorScheme="red"
              variant="outline"
              size="lg"
              onClick={() => {
                localStorage.clear();
                router.push('/login');
              }}
            >
              Cerrar Sesión
            </Button>
          </VStack>
        </VStack>
      </Container>
    </Box>
  );
}
