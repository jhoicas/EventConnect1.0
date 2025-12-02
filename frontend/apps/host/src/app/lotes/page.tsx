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
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
  Progress,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { TrendingUp, Package, AlertTriangle, Clock } from 'lucide-react';
import { LoteModal } from '@/components/LoteModal';
import { 
  useGetLotesQuery, 
  useDeleteLoteMutation,
  type Lote,
} from '@/store/api/loteApi';
import { useGetProductosQuery } from '@/store/api/productoApi';
import { useGetBodegasQuery } from '@/store/api/bodegaApi';
import { useRef } from 'react';

export default function LotesPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();
  
  const [selectedLote, setSelectedLote] = useState<Lote | undefined>();
  const [loteToDelete, setLoteToDelete] = useState<Lote | null>(null);

  const { data: lotes = [], isLoading } = useGetLotesQuery();
  const { data: productos = [] } = useGetProductosQuery();
  const { data: bodegas = [] } = useGetBodegasQuery();
  const [deleteLote] = useDeleteLoteMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (lote: Lote) => {
    setSelectedLote(lote);
    onOpen();
  };

  const handleDelete = (lote: Lote) => {
    setLoteToDelete(lote);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (loteToDelete) {
      try {
        await deleteLote(loteToDelete.id).unwrap();
        toast({
          title: 'Lote eliminado',
          description: `El lote ${loteToDelete.codigo_Lote} fue eliminado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar el lote',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setLoteToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedLote(undefined);
    onClose();
  };

  const getEstadoBadgeColor = (estado: string) => {
    switch (estado) {
      case 'Disponible':
        return 'green';
      case 'Agotado':
        return 'red';
      case 'Reservado':
        return 'yellow';
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

  const formatDate = (date?: string) => {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getProductoNombre = (productoId: number) => {
    const producto = productos.find(p => p.id === productoId);
    return producto ? producto.nombre : 'Desconocido';
  };

  const getBodegaNombre = (bodegaId?: number) => {
    if (!bodegaId) return 'Sin asignar';
    const bodega = bodegas.find(b => b.id === bodegaId);
    return bodega ? bodega.nombre : 'Desconocida';
  };

  const getDiasParaVencer = (fechaVencimiento?: string) => {
    if (!fechaVencimiento) return null;
    const hoy = new Date();
    const vencimiento = new Date(fechaVencimiento);
    const diffTime = vencimiento.getTime() - hoy.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  const getVencimientoColor = (dias: number | null) => {
    if (dias === null) return 'gray';
    if (dias < 0) return 'red';
    if (dias <= 7) return 'red';
    if (dias <= 30) return 'yellow';
    return 'green';
  };

  const lotesVencidos = lotes.filter(l => {
    const dias = getDiasParaVencer(l.fecha_Vencimiento);
    return dias !== null && dias < 0;
  });

  const lotesPorVencer = lotes.filter(l => {
    const dias = getDiasParaVencer(l.fecha_Vencimiento);
    return dias !== null && dias >= 0 && dias <= 30;
  });

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <TrendingUp size={28} />
          Lotes
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Control de lotes y fechas de vencimiento
        </Text>
      </Box>

      {lotesVencidos.length > 0 && (
        <Alert status="error" borderRadius="lg">
          <AlertIcon as={AlertTriangle} />
          <Box flex="1">
            <AlertTitle>Lotes Vencidos</AlertTitle>
            <AlertDescription>
              Hay {lotesVencidos.length} lote(s) con fecha de vencimiento vencida. Revisa el inventario.
            </AlertDescription>
          </Box>
        </Alert>
      )}

      {lotesPorVencer.length > 0 && (
        <Alert status="warning" borderRadius="lg">
          <AlertIcon as={Clock} />
          <Box flex="1">
            <AlertTitle>Lotes por Vencer</AlertTitle>
            <AlertDescription>
              Hay {lotesPorVencer.length} lote(s) que vencerán en los próximos 30 días.
            </AlertDescription>
          </Box>
        </Alert>
      )}

      <HStack justify="flex-end">
        <Button
          leftIcon={<AddIcon />}
          colorScheme="blue"
          onClick={onOpen}
          size="md"
        >
          Nuevo Lote
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
              <Th>Producto</Th>
              <Th>Bodega</Th>
              <Th>Vencimiento</Th>
              <Th isNumeric>Cantidad</Th>
              <Th isNumeric>Costo Unit.</Th>
              <Th>Estado</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  Cargando lotes...
                </Td>
              </Tr>
            ) : lotes.length === 0 ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  No hay lotes registrados
                </Td>
              </Tr>
            ) : (
              lotes.map((lote) => {
                const dias = getDiasParaVencer(lote.fecha_Vencimiento);
                const porcentajeUsado = ((lote.cantidad_Inicial - lote.cantidad_Actual) / lote.cantidad_Inicial) * 100;
                
                return (
                  <Tr key={lote.id}>
                    <Td fontWeight="semibold">{lote.codigo_Lote}</Td>
                    <Td>
                      <HStack>
                        <Package size={16} />
                        <Text fontSize="sm">{getProductoNombre(lote.producto_Id)}</Text>
                      </HStack>
                    </Td>
                    <Td fontSize="sm">{getBodegaNombre(lote.bodega_Id)}</Td>
                    <Td>
                      {lote.fecha_Vencimiento ? (
                        <Tooltip label={dias !== null ? `${dias} días ${dias >= 0 ? 'restantes' : 'vencido'}` : ''}>
                          <Box>
                            <Text fontSize="sm">{formatDate(lote.fecha_Vencimiento)}</Text>
                            {dias !== null && (
                              <Badge colorScheme={getVencimientoColor(dias)} fontSize="xs">
                                {dias < 0 ? 'Vencido' : dias <= 7 ? 'Urgente' : dias <= 30 ? 'Próximo' : 'OK'}
                              </Badge>
                            )}
                          </Box>
                        </Tooltip>
                      ) : (
                        <Text fontSize="sm" color={mutedColor}>Sin vencimiento</Text>
                      )}
                    </Td>
                    <Td isNumeric>
                      <Box>
                        <Text fontWeight="bold">{lote.cantidad_Actual}</Text>
                        <Text fontSize="xs" color={mutedColor}>de {lote.cantidad_Inicial}</Text>
                        <Progress 
                          value={100 - porcentajeUsado} 
                          size="xs" 
                          colorScheme={porcentajeUsado > 80 ? 'red' : porcentajeUsado > 50 ? 'yellow' : 'green'}
                          mt={1}
                        />
                      </Box>
                    </Td>
                    <Td isNumeric fontSize="sm">{formatPrice(lote.costo_Unitario)}</Td>
                    <Td>
                      <Badge colorScheme={getEstadoBadgeColor(lote.estado)}>
                        {lote.estado}
                      </Badge>
                    </Td>
                    <Td>
                      <HStack spacing={2}>
                        <Tooltip label="Editar lote">
                          <IconButton
                            aria-label="Editar"
                            icon={<EditIcon />}
                            size="sm"
                            colorScheme="blue"
                            variant="ghost"
                            onClick={() => handleEdit(lote)}
                          />
                        </Tooltip>
                        <Tooltip label="Eliminar lote">
                          <IconButton
                            aria-label="Eliminar"
                            icon={<DeleteIcon />}
                            size="sm"
                            colorScheme="red"
                            variant="ghost"
                            onClick={() => handleDelete(lote)}
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

      <LoteModal
        isOpen={isOpen}
        onClose={handleModalClose}
        lote={selectedLote}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Lote
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar el lote <strong>{loteToDelete?.codigo_Lote}</strong>? Esta acción no se puede deshacer.
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
