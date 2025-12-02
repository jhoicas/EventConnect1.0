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
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
  Tag,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { Wrench, AlertCircle, Settings, CheckCircle } from 'lucide-react';
import { MantenimientoModal } from '@/components/MantenimientoModal';
import { 
  useGetMantenimientosQuery, 
  useDeleteMantenimientoMutation,
  type Mantenimiento,
} from '@/store/api/mantenimientoApi';
import { useGetActivosQuery } from '@/store/api/activoApi';
import { useRef } from 'react';

export default function MantenimientosPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();
  
  const [selectedMantenimiento, setSelectedMantenimiento] = useState<Mantenimiento | undefined>();
  const [mantenimientoToDelete, setMantenimientoToDelete] = useState<Mantenimiento | null>(null);
  const [estadoFilter, setEstadoFilter] = useState<string>('Todos');

  const { data: mantenimientos = [], isLoading } = useGetMantenimientosQuery();
  const { data: activos = [] } = useGetActivosQuery();
  const [deleteMantenimiento] = useDeleteMantenimientoMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (mantenimiento: Mantenimiento) => {
    setSelectedMantenimiento(mantenimiento);
    onOpen();
  };

  const handleDelete = (mantenimiento: Mantenimiento) => {
    setMantenimientoToDelete(mantenimiento);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (mantenimientoToDelete) {
      try {
        await deleteMantenimiento(mantenimientoToDelete.id).unwrap();
        toast({
          title: 'Mantenimiento eliminado',
          description: `El mantenimiento fue eliminado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar el mantenimiento',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setMantenimientoToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedMantenimiento(undefined);
    onClose();
  };

  const getEstadoBadgeColor = (estado: string) => {
    switch (estado) {
      case 'Pendiente':
        return 'yellow';
      case 'En Proceso':
        return 'blue';
      case 'Completado':
        return 'green';
      case 'Cancelado':
        return 'red';
      default:
        return 'gray';
    }
  };

  const getTipoBadgeColor = (tipo: string) => {
    switch (tipo) {
      case 'Preventivo':
        return 'green';
      case 'Correctivo':
        return 'orange';
      case 'Predictivo':
        return 'purple';
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

  const getActivoNombre = (activoId: number) => {
    const activo = activos.find(a => a.id === activoId);
    return activo ? activo.nombre : 'Desconocido';
  };

  const getDiasParaProgramado = (fechaProgramada?: string) => {
    if (!fechaProgramada) return null;
    const hoy = new Date();
    const programado = new Date(fechaProgramada);
    const diffTime = programado.getTime() - hoy.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  const mantenimientosFiltrados = estadoFilter === 'Todos' 
    ? mantenimientos 
    : mantenimientos.filter(m => m.estado === estadoFilter);

  const mantenimientosVencidos = mantenimientos.filter(m => {
    const dias = getDiasParaProgramado(m.fecha_Programada);
    return m.estado === 'Pendiente' && dias !== null && dias < 0;
  });

  const mantenimientosPorVencer = mantenimientos.filter(m => {
    const dias = getDiasParaProgramado(m.fecha_Programada);
    return m.estado === 'Pendiente' && dias !== null && dias >= 0 && dias <= 7;
  });

  const estados = ['Todos', 'Pendiente', 'En Proceso', 'Completado', 'Cancelado'];

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <Wrench size={28} />
          Mantenimientos
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Programa y registra mantenimientos de activos
        </Text>
      </Box>

      {mantenimientosVencidos.length > 0 && (
        <Alert status="error" borderRadius="lg">
          <AlertIcon as={AlertCircle} />
          <Box flex="1">
            <AlertTitle>Mantenimientos Vencidos</AlertTitle>
            <AlertDescription>
              Hay {mantenimientosVencidos.length} mantenimiento(s) con fecha programada vencida.
            </AlertDescription>
          </Box>
        </Alert>
      )}

      {mantenimientosPorVencer.length > 0 && (
        <Alert status="warning" borderRadius="lg">
          <AlertIcon as={Settings} />
          <Box flex="1">
            <AlertTitle>Mantenimientos Próximos</AlertTitle>
            <AlertDescription>
              Hay {mantenimientosPorVencer.length} mantenimiento(s) programados para los próximos 7 días.
            </AlertDescription>
          </Box>
        </Alert>
      )}

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
          Nuevo Mantenimiento
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
              <Th>Activo</Th>
              <Th>Tipo</Th>
              <Th>Fecha Programada</Th>
              <Th>Fecha Realizada</Th>
              <Th>Proveedor</Th>
              <Th isNumeric>Costo</Th>
              <Th>Estado</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  Cargando mantenimientos...
                </Td>
              </Tr>
            ) : mantenimientosFiltrados.length === 0 ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  No hay mantenimientos {estadoFilter !== 'Todos' ? `con estado "${estadoFilter}"` : 'registrados'}
                </Td>
              </Tr>
            ) : (
              mantenimientosFiltrados.map((mantenimiento) => {
                const dias = getDiasParaProgramado(mantenimiento.fecha_Programada);
                const urgente = mantenimiento.estado === 'Pendiente' && dias !== null && dias <= 3;
                
                return (
                  <Tr key={mantenimiento.id} bg={urgente ? 'red.50' : undefined}>
                    <Td>
                      <HStack>
                        <Settings size={16} />
                        <Text fontSize="sm" fontWeight="medium">
                          {getActivoNombre(mantenimiento.activo_Id)}
                        </Text>
                      </HStack>
                      {mantenimiento.descripcion && (
                        <Text fontSize="xs" color={mutedColor} noOfLines={1}>
                          {mantenimiento.descripcion}
                        </Text>
                      )}
                    </Td>
                    <Td>
                      <Badge colorScheme={getTipoBadgeColor(mantenimiento.tipo_Mantenimiento)}>
                        {mantenimiento.tipo_Mantenimiento}
                      </Badge>
                    </Td>
                    <Td>
                      {mantenimiento.fecha_Programada ? (
                        <Tooltip label={dias !== null ? `${dias >= 0 ? `En ${dias} días` : `Vencido hace ${Math.abs(dias)} días`}` : ''}>
                          <Box>
                            <Text fontSize="sm">{formatDate(mantenimiento.fecha_Programada)}</Text>
                            {urgente && dias !== null && (
                              <Tag size="sm" colorScheme="red" mt={1}>
                                {dias < 0 ? 'Vencido' : `${dias} días`}
                              </Tag>
                            )}
                          </Box>
                        </Tooltip>
                      ) : (
                        <Text fontSize="sm" color={mutedColor}>Sin programar</Text>
                      )}
                    </Td>
                    <Td>
                      {mantenimiento.fecha_Realizada ? (
                        <HStack>
                          <CheckCircle size={14} color="green" />
                          <Text fontSize="sm">{formatDate(mantenimiento.fecha_Realizada)}</Text>
                        </HStack>
                      ) : (
                        <Text fontSize="sm" color={mutedColor}>Pendiente</Text>
                      )}
                    </Td>
                    <Td fontSize="sm">{mantenimiento.proveedor_Servicio || 'N/A'}</Td>
                    <Td isNumeric fontSize="sm">{formatPrice(mantenimiento.costo)}</Td>
                    <Td>
                      <Badge colorScheme={getEstadoBadgeColor(mantenimiento.estado)}>
                        {mantenimiento.estado}
                      </Badge>
                    </Td>
                    <Td>
                      <HStack spacing={2}>
                        <Tooltip label="Editar mantenimiento">
                          <IconButton
                            aria-label="Editar"
                            icon={<EditIcon />}
                            size="sm"
                            colorScheme="blue"
                            variant="ghost"
                            onClick={() => handleEdit(mantenimiento)}
                          />
                        </Tooltip>
                        <Tooltip label="Eliminar mantenimiento">
                          <IconButton
                            aria-label="Eliminar"
                            icon={<DeleteIcon />}
                            size="sm"
                            colorScheme="red"
                            variant="ghost"
                            onClick={() => handleDelete(mantenimiento)}
                          />
                        </Tooltip>
                      </HStack>
                    </Td>
                  </Tr>
                );
              })
            )}
          </Tbody>
        </Table>
      </Box>

      <MantenimientoModal
        isOpen={isOpen}
        onClose={handleModalClose}
        mantenimiento={selectedMantenimiento}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Mantenimiento
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar este mantenimiento? Esta acción no se puede deshacer.
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
