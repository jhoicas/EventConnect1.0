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
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { Calendar } from 'lucide-react';
import { ReservaModal } from '@/components/ReservaModal';
import { 
  useGetReservasQuery, 
  useDeleteReservaMutation,
  type Reserva,
} from '@/store/api/reservaApi';
import { useGetClientesQuery } from '@/store/api/clienteApi';
import { useRef } from 'react';

export default function ReservasPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();
  
  const [selectedReserva, setSelectedReserva] = useState<Reserva | undefined>();
  const [reservaToDelete, setReservaToDelete] = useState<Reserva | null>(null);
  const [estadoFilter, setEstadoFilter] = useState<string>('Todos');

  const { data: reservas = [], isLoading } = useGetReservasQuery();
  const { data: clientes = [] } = useGetClientesQuery();
  const [deleteReserva] = useDeleteReservaMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (reserva: Reserva) => {
    setSelectedReserva(reserva);
    onOpen();
  };

  const handleDelete = (reserva: Reserva) => {
    setReservaToDelete(reserva);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (reservaToDelete) {
      try {
        await deleteReserva(reservaToDelete.id).unwrap();
        toast({
          title: 'Reserva eliminada',
          description: `La reserva ${reservaToDelete.codigo_Reserva} fue eliminada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar la reserva',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setReservaToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedReserva(undefined);
    onClose();
  };

  const getEstadoBadgeColor = (estado: string) => {
    switch (estado) {
      case 'Solicitado':
        return 'yellow';
      case 'Confirmado':
        return 'blue';
      case 'Entregado':
        return 'green';
      case 'Devuelto':
        return 'gray';
      case 'Cancelado':
        return 'red';
      default:
        return 'gray';
    }
  };

  const getEstadoPagoBadgeColor = (estado: string) => {
    switch (estado) {
      case 'Pagado':
        return 'green';
      case 'Parcial':
        return 'yellow';
      case 'Pendiente':
        return 'red';
      default:
        return 'gray';
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getClienteName = (clienteId: number) => {
    const cliente = clientes.find(c => c.id === clienteId);
    return cliente ? cliente.nombre : 'Cliente no encontrado';
  };

  const reservasFiltradas = estadoFilter === 'Todos' 
    ? reservas 
    : reservas.filter(r => r.estado === estadoFilter);

  const estados = ['Todos', 'Solicitado', 'Confirmado', 'Entregado', 'Devuelto', 'Cancelado'];

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <Calendar size={28} />
          Reservas
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Administra las reservas y eventos de tus clientes
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
          Nueva Reserva
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
              <Th>Cliente</Th>
              <Th>Fecha Evento</Th>
              <Th>Estado</Th>
              <Th>Estado Pago</Th>
              <Th isNumeric>Total</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={7} textAlign="center">
                  Cargando reservas...
                </Td>
              </Tr>
            ) : reservasFiltradas.length === 0 ? (
              <Tr>
                <Td colSpan={7} textAlign="center">
                  No hay reservas {estadoFilter !== 'Todos' ? `con estado "${estadoFilter}"` : 'registradas'}
                </Td>
              </Tr>
            ) : (
              reservasFiltradas.map((reserva) => (
                <Tr key={reserva.id}>
                  <Td fontWeight="semibold">{reserva.codigo_Reserva}</Td>
                  <Td>{getClienteName(reserva.cliente_Id)}</Td>
                  <Td>{formatDate(reserva.fecha_Evento)}</Td>
                  <Td>
                    <Badge colorScheme={getEstadoBadgeColor(reserva.estado)}>
                      {reserva.estado}
                    </Badge>
                  </Td>
                  <Td>
                    <Badge colorScheme={getEstadoPagoBadgeColor(reserva.estado_Pago)}>
                      {reserva.estado_Pago}
                    </Badge>
                  </Td>
                  <Td isNumeric fontWeight="bold">{formatPrice(reserva.total)}</Td>
                  <Td>
                    <HStack spacing={2}>
                      <Tooltip label="Editar reserva">
                        <IconButton
                          aria-label="Editar"
                          icon={<EditIcon />}
                          size="sm"
                          colorScheme="blue"
                          variant="ghost"
                          onClick={() => handleEdit(reserva)}
                        />
                      </Tooltip>
                      <Tooltip label="Eliminar reserva">
                        <IconButton
                          aria-label="Eliminar"
                          icon={<DeleteIcon />}
                          size="sm"
                          colorScheme="red"
                          variant="ghost"
                          onClick={() => handleDelete(reserva)}
                        />
                      </Tooltip>
                    </HStack>
                  </Td>
                </Tr>
                  ))
                )}
              </Tbody>
            </Table>
          </Box>      <ReservaModal
        isOpen={isOpen}
        onClose={handleModalClose}
        reserva={selectedReserva}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Reserva
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar la reserva <strong>{reservaToDelete?.codigo_Reserva}</strong>? Esta acción no se puede deshacer.
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
