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
  Stat,
  StatLabel,
  StatNumber,
  StatHelpText,
  SimpleGrid,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { FileBox, Warehouse, MapPin } from 'lucide-react';
import { BodegaModal } from '@/components/BodegaModal';
import { 
  useGetBodegasQuery, 
  useDeleteBodegaMutation,
  type Bodega,
} from '@/store/api/bodegaApi';
import { useRef } from 'react';

export default function BodegasPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();
  
  const [selectedBodega, setSelectedBodega] = useState<Bodega | undefined>();
  const [bodegaToDelete, setBodegaToDelete] = useState<Bodega | null>(null);

  const { data: bodegas = [], isLoading } = useGetBodegasQuery();
  const [deleteBodega] = useDeleteBodegaMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (bodega: Bodega) => {
    setSelectedBodega(bodega);
    onOpen();
  };

  const handleDelete = (bodega: Bodega) => {
    setBodegaToDelete(bodega);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (bodegaToDelete) {
      try {
        await deleteBodega(bodegaToDelete.id).unwrap();
        toast({
          title: 'Bodega eliminada',
          description: `La bodega ${bodegaToDelete.nombre} fue eliminada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar la bodega',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setBodegaToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedBodega(undefined);
    onClose();
  };

  const getEstadoBadgeColor = (estado: string) => {
    return estado === 'Activo' ? 'green' : 'red';
  };

  const bodegasActivas = bodegas.filter(b => b.estado === 'Activo').length;
  const capacidadTotal = bodegas.reduce((sum, b) => sum + (b.capacidad_M3 || 0), 0);

  return (
    <VStack spacing={{ base: 4, md: 6 }} align="stretch" px={{ base: 4, md: 0 }}>
      <Box>
        <Heading size={{ base: "md", md: "lg" }} mb={2} display="flex" alignItems="center" gap={3}>
          <FileBox size={20} />
          Bodegas
        </Heading>
        <Text fontSize={{ base: "xs", md: "sm" }} color={mutedColor}>
          Gestiona las ubicaciones y almacenes de inventario
        </Text>
      </Box>

      <SimpleGrid columns={{ base: 1, md: 3 }} spacing={{ base: 3, md: 6 }}>
        <Box
          bg={cardBg}
          borderWidth="1px"
          borderColor={borderColor}
          borderRadius="lg"
          p={{ base: 4, md: 5 }}
        >
          <Stat>
            <HStack>
              <Box color="blue.500">
                <Warehouse size={{ base: 20, md: 24 }} />
              </Box>
              <Box>
                <StatLabel fontSize={{ base: "xs", md: "sm" }}>Total Bodegas</StatLabel>
                <StatNumber fontSize={{ base: "xl", md: "2xl" }}>{bodegas.length}</StatNumber>
                <StatHelpText fontSize={{ base: "xs", md: "sm" }}>{bodegasActivas} activas</StatHelpText>
              </Box>
            </HStack>
          </Stat>
        </Box>

        <Box
          bg={cardBg}
          borderWidth="1px"
          borderColor={borderColor}
          borderRadius="lg"
          p={5}
        >
          <Stat>
            <HStack>
              <Box color="purple.500">
                <FileBox size={24} />
              </Box>
              <Box>
                <StatLabel>Capacidad Total</StatLabel>
                <StatNumber>{capacidadTotal.toFixed(2)} m³</StatNumber>
                <StatHelpText>Espacio disponible</StatHelpText>
              </Box>
            </HStack>
          </Stat>
        </Box>

        <Box
          bg={cardBg}
          borderWidth="1px"
          borderColor={borderColor}
          borderRadius="lg"
          p={5}
        >
          <Stat>
            <HStack>
              <Box color="green.500">
                <MapPin size={24} />
              </Box>
              <Box>
                <StatLabel>Ciudades</StatLabel>
                <StatNumber>{new Set(bodegas.map(b => b.ciudad).filter(Boolean)).size}</StatNumber>
                <StatHelpText>Ubicaciones únicas</StatHelpText>
              </Box>
            </HStack>
          </Stat>
        </Box>
      </SimpleGrid>

      <HStack justify="flex-end">
        <Button
          leftIcon={<AddIcon />}
          colorScheme="blue"
          onClick={onOpen}
          size="md"
        >
          Nueva Bodega
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
              <Th>Ciudad</Th>
              <Th>Dirección</Th>
              <Th>Teléfono</Th>
              <Th isNumeric>Capacidad (m³)</Th>
              <Th>Estado</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  Cargando bodegas...
                </Td>
              </Tr>
            ) : bodegas.length === 0 ? (
              <Tr>
                <Td colSpan={8} textAlign="center">
                  No hay bodegas registradas
                </Td>
              </Tr>
            ) : (
              bodegas.map((bodega) => (
                <Tr key={bodega.id}>
                  <Td fontWeight="semibold">{bodega.codigo_Bodega}</Td>
                  <Td fontWeight="medium">{bodega.nombre}</Td>
                  <Td>{bodega.ciudad || 'N/A'}</Td>
                  <Td>
                    <Text fontSize="sm" noOfLines={1}>
                      {bodega.direccion || 'Sin dirección'}
                    </Text>
                  </Td>
                  <Td fontSize="sm">{bodega.telefono || 'N/A'}</Td>
                  <Td isNumeric>
                    <Text fontWeight="bold">
                      {bodega.capacidad_M3 ? bodega.capacidad_M3.toFixed(2) : 'N/A'}
                    </Text>
                  </Td>
                  <Td>
                    <Badge colorScheme={getEstadoBadgeColor(bodega.estado)}>
                      {bodega.estado}
                    </Badge>
                  </Td>
                  <Td>
                    <HStack spacing={2}>
                      <Tooltip label="Editar bodega">
                        <IconButton
                          aria-label="Editar"
                          icon={<EditIcon />}
                          size="sm"
                          colorScheme="blue"
                          variant="ghost"
                          onClick={() => handleEdit(bodega)}
                        />
                      </Tooltip>
                      <Tooltip label="Eliminar bodega">
                        <IconButton
                          aria-label="Eliminar"
                          icon={<DeleteIcon />}
                          size="sm"
                          colorScheme="red"
                          variant="ghost"
                          onClick={() => handleDelete(bodega)}
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

      <BodegaModal
        isOpen={isOpen}
        onClose={handleModalClose}
        bodega={selectedBodega}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Bodega
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar la bodega <strong>{bodegaToDelete?.nombre}</strong>? Esta acción no se puede deshacer.
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
