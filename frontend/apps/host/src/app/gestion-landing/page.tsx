'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Heading,
  Text,
  VStack,
  useColorMode,
  useDisclosure,
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
  Badge,
  IconButton,
  HStack,
  useToast,
  AlertDialog,
  AlertDialogBody,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogContent,
  AlertDialogOverlay,
  Tooltip,
  Image,
  Select,
  Switch,
  FormControl,
  FormLabel,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon, ViewIcon } from '@chakra-ui/icons';
import { FileText, Eye, EyeOff } from 'lucide-react';
import {
  useGetContenidosQuery,
  useDeleteContenidoMutation,
  type ContenidoLanding,
} from '@/store/api/contenidoLandingApi';
import { useRef } from 'react';

export default function GestionLandingPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();

  const [contenidoToDelete, setContenidoToDelete] = useState<ContenidoLanding | null>(null);
  const [seccionFilter, setSeccionFilter] = useState<string>('Todos');

  const { data: contenidos = [], isLoading } = useGetContenidosQuery();
  const [deleteContenido] = useDeleteContenidoMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleDelete = (contenido: ContenidoLanding) => {
    setContenidoToDelete(contenido);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (contenidoToDelete) {
      try {
        await deleteContenido(contenidoToDelete.id).unwrap();
        toast({
          title: 'Contenido eliminado',
          description: `El contenido "${contenidoToDelete.titulo}" fue eliminado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar el contenido',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setContenidoToDelete(null);
    }
  };

  const getSeccionBadgeColor = (seccion: string) => {
    switch (seccion) {
      case 'hero':
        return 'purple';
      case 'servicios':
        return 'blue';
      case 'nosotros':
        return 'green';
      case 'planes':
        return 'orange';
      case 'testimonios':
        return 'cyan';
      default:
        return 'gray';
    }
  };

  const contenidosFiltrados = seccionFilter === 'Todos'
    ? contenidos
    : contenidos.filter((c) => c.seccion === seccionFilter);

  const secciones = ['Todos', 'hero', 'servicios', 'nosotros', 'planes', 'testimonios'];

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <FileText size={28} />
          Gestión de Landing Page
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Edita el contenido que los visitantes ven en la página de inicio
        </Text>
      </Box>

      <HStack justify="space-between" flexWrap="wrap" gap={4}>
        <FormControl maxW="300px">
          <FormLabel fontSize="sm">Filtrar por sección</FormLabel>
          <Select value={seccionFilter} onChange={(e) => setSeccionFilter(e.target.value)} size="sm">
            {secciones.map((seccion) => (
              <option key={seccion} value={seccion}>
                {seccion.charAt(0).toUpperCase() + seccion.slice(1)}
              </option>
            ))}
          </Select>
        </FormControl>

        <HStack>
          <Button
            leftIcon={<ViewIcon />}
            variant="outline"
            onClick={() => window.open('/', '_blank')}
            size="md"
          >
            Ver Landing
          </Button>
          <Button leftIcon={<AddIcon />} colorScheme="blue" size="md">
            Nuevo Contenido
          </Button>
        </HStack>
      </HStack>

      <Box
        bg={cardBg}
        borderWidth="1px"
        borderColor={borderColor}
        borderRadius="lg"
        overflow="hidden"
      >
        <Table variant="simple">
          <Thead>
            <Tr>
              <Th>Sección</Th>
              <Th>Título</Th>
              <Th>Subtítulo</Th>
              <Th>Imagen</Th>
              <Th>Icono</Th>
              <Th>Orden</Th>
              <Th>Estado</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  Cargando contenidos...
                </Td>
              </Tr>
            ) : contenidosFiltrados.length === 0 ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  No hay contenidos en esta sección
                </Td>
              </Tr>
            ) : (
              contenidosFiltrados.map((contenido) => (
                <Tr key={contenido.id}>
                  <Td>
                    <Badge colorScheme={getSeccionBadgeColor(contenido.seccion)}>
                      {contenido.seccion}
                    </Badge>
                  </Td>
                  <Td fontSize="sm" fontWeight="semibold" maxW="200px" noOfLines={1}>
                    {contenido.titulo}
                  </Td>
                  <Td fontSize="sm" maxW="250px" noOfLines={2} color={mutedColor}>
                    {contenido.subtitulo || 'N/A'}
                  </Td>
                  <Td>
                    {contenido.imagen_URL ? (
                      <Image
                        src={contenido.imagen_URL}
                        alt={contenido.titulo}
                        boxSize="50px"
                        objectFit="cover"
                        borderRadius="md"
                        fallbackSrc="https://via.placeholder.com/50"
                      />
                    ) : (
                      <Text fontSize="sm" color={mutedColor}>
                        Sin imagen
                      </Text>
                    )}
                  </Td>
                  <Td>
                    <Badge variant="outline">{contenido.icono_Nombre || 'N/A'}</Badge>
                  </Td>
                  <Td>
                    <Badge colorScheme="gray">{contenido.orden}</Badge>
                  </Td>
                  <Td>
                    {contenido.activo ? (
                      <HStack>
                        <Eye size={16} color="green" />
                        <Badge colorScheme="green">Visible</Badge>
                      </HStack>
                    ) : (
                      <HStack>
                        <EyeOff size={16} color="gray" />
                        <Badge colorScheme="gray">Oculto</Badge>
                      </HStack>
                    )}
                  </Td>
                  <Td>
                    <HStack spacing={2}>
                      <Tooltip label="Editar contenido">
                        <IconButton
                          aria-label="Editar"
                          icon={<EditIcon />}
                          size="sm"
                          colorScheme="blue"
                          variant="ghost"
                        />
                      </Tooltip>
                      <Tooltip label="Eliminar contenido">
                        <IconButton
                          aria-label="Eliminar"
                          icon={<DeleteIcon />}
                          size="sm"
                          colorScheme="red"
                          variant="ghost"
                          onClick={() => handleDelete(contenido)}
                        />
                      </Tooltip>
                    </HStack>
                  </Td>
                </Tr>
              ))
            )}
          </Tbody>
        </Table>
      </Box>

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Contenido
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar el contenido "{contenidoToDelete?.titulo}"? Esta acción no se puede deshacer.
            </AlertDialogBody>

            <AlertDialogFooter>
              <Button ref={cancelRef} onClick={onDeleteClose}>
                Cancelar
              </Button>
              <Button colorScheme="red" onClick={confirmDelete} ml={3}>
                Eliminar
              </Button>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialogOverlay>
      </AlertDialog>
    </VStack>
  );
}
