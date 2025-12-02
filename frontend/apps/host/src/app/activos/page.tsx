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
  ButtonGroup,
  Tag,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { Warehouse } from 'lucide-react';
import { ActivoModal } from '@/components/ActivoModal';
import { 
  useGetActivosQuery, 
  useDeleteActivoMutation,
  type Activo,
} from '@/store/api/activoApi';
import { useGetCategoriasQuery } from '@/store/api/categoriaApi';
import { useRef } from 'react';

export default function ActivosPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();
  
  const [selectedActivo, setSelectedActivo] = useState<Activo | undefined>();
  const [activoToDelete, setActivoToDelete] = useState<Activo | null>(null);
  const [estadoFilter, setEstadoFilter] = useState<string>('Todos');

  const { data: activos = [], isLoading } = useGetActivosQuery();
  const { data: categorias = [] } = useGetCategoriasQuery();
  const [deleteActivo] = useDeleteActivoMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (activo: Activo) => {
    setSelectedActivo(activo);
    onOpen();
  };

  const handleDelete = (activo: Activo) => {
    setActivoToDelete(activo);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (activoToDelete) {
      try {
        await deleteActivo(activoToDelete.id).unwrap();
        toast({
          title: 'Activo eliminado',
          description: `El activo ${activoToDelete.codigo_Activo} fue eliminado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar el activo',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setActivoToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedActivo(undefined);
    onClose();
  };

  const getEstadoBadgeColor = (estado: string) => {
    switch (estado) {
      case 'Activo':
        return 'green';
      case 'En Mantenimiento':
        return 'yellow';
      case 'Dado de Baja':
        return 'red';
      default:
        return 'gray';
    }
  };

  const formatPrice = (price?: number) => {
    if (!price) return 'N/A';
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatDate = (date?: string) => {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getCategoriaNombre = (categoriaId: number) => {
    const categoria = categorias.find(c => c.id === categoriaId);
    return categoria ? categoria.nombre : 'Sin categoría';
  };

  const activosFiltrados = estadoFilter === 'Todos' 
    ? activos 
    : activos.filter(a => a.estado === estadoFilter);

  const estados = ['Todos', 'Activo', 'En Mantenimiento', 'Dado de Baja'];

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <Warehouse size={28} />
          Activos Fijos
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Sistema Integrado de Gestión de Inventarios (SIGI)
        </Text>
      </Box>

      <HStack justify="space-between" flexWrap="wrap" gap={4}>
        <ButtonGroup size="sm" isAttached variant="outline">
          {estados.map((estado) => (
            <Button
              key={estado}
              onClick={() => setEstadoFilter(estado)}
              colorScheme={estadoFilter === estado ? 'blue' : 'gray'}
              variant={estadoFilter === estado ? 'solid' : 'outline'}
            >
              {estado}
            </Button>
          ))}
        </ButtonGroup>

        <Button
          leftIcon={<AddIcon />}
          colorScheme="blue"
          onClick={onOpen}
          size="md"
        >
          Nuevo Activo
        </Button>
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
              <Th>Código</Th>
              <Th>Nombre</Th>
              <Th>Categoría</Th>
              <Th>Marca/Modelo</Th>
              <Th>Estado</Th>
              <Th>Ubicación</Th>
              <Th isNumeric>Valor</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  Cargando activos...
                </Td>
              </Tr>
            ) : activosFiltrados.length === 0 ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  No hay activos {estadoFilter !== 'Todos' ? `con estado "${estadoFilter}"` : 'registrados'}
                </Td>
              </Tr>
            ) : (
              activosFiltrados.map((activo) => (
                <Tr key={activo.id}>
                  <Td fontWeight="semibold">
                    <Tooltip label={activo.numero_Serie ? `S/N: ${activo.numero_Serie}` : 'Sin número de serie'}>
                      {activo.codigo_Activo}
                    </Tooltip>
                  </Td>
                  <Td>
                    <Text fontWeight="medium">{activo.nombre}</Text>
                    {activo.descripcion && (
                      <Text fontSize="xs" color={mutedColor} noOfLines={1}>
                        {activo.descripcion}
                      </Text>
                    )}
                  </Td>
                  <Td>
                    <Tag size="sm" colorScheme="purple">
                      {getCategoriaNombre(activo.categoria_Id)}
                    </Tag>
                  </Td>
                  <Td>
                    {activo.marca || activo.modelo ? (
                      <>
                        <Text fontSize="sm" fontWeight="medium">{activo.marca || 'N/A'}</Text>
                        <Text fontSize="xs" color={mutedColor}>{activo.modelo || 'N/A'}</Text>
                      </>
                    ) : (
                      <Text fontSize="sm" color={mutedColor}>N/A</Text>
                    )}
                  </Td>
                  <Td>
                    <Badge colorScheme={getEstadoBadgeColor(activo.estado)}>
                      {activo.estado}
                    </Badge>
                  </Td>
                  <Td>
                    <Text fontSize="sm">{activo.ubicacion_Fisica || 'Sin ubicación'}</Text>
                  </Td>
                  <Td isNumeric>
                    <Tooltip label={`Adquirido: ${formatDate(activo.fecha_Adquisicion)}`}>
                      <Text fontSize="sm" fontWeight="bold">{formatPrice(activo.valor_Adquisicion)}</Text>
                    </Tooltip>
                  </Td>
                  <Td>
                    <HStack spacing={2}>
                      <Tooltip label="Editar activo">
                        <IconButton
                          aria-label="Editar"
                          icon={<EditIcon />}
                          size="sm"
                          colorScheme="blue"
                          variant="ghost"
                          onClick={() => handleEdit(activo)}
                        />
                      </Tooltip>
                      <Tooltip label="Eliminar activo">
                        <IconButton
                          aria-label="Eliminar"
                          icon={<DeleteIcon />}
                          size="sm"
                          colorScheme="red"
                          variant="ghost"
                          onClick={() => handleDelete(activo)}
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

      <ActivoModal
        isOpen={isOpen}
        onClose={handleModalClose}
        activo={selectedActivo}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Activo
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar el activo <strong>{activoToDelete?.codigo_Activo}</strong>? Esta acción no se puede deshacer.
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
