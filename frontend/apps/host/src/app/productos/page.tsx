'use client';

import { 
  Box, 
  Heading, 
  Button, 
  useColorMode,
  Container,
  HStack,
  useDisclosure,
  Spinner,
  Center,
  Text,
  IconButton,
  useToast,
  AlertDialog,
  AlertDialogBody,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogContent,
  AlertDialogOverlay,
  Badge,
  Tooltip,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { DataTable } from '@eventconnect/ui';
import { useGetProductosQuery, useDeleteProductoMutation, type Producto } from '../../store/api/productoApi';
import { ProductoModal } from '../../components/ProductoModal';
import { useState, useRef } from 'react';

export default function ProductosPage() {
  const { colorMode } = useColorMode();
  const toast = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  
  const [selectedProducto, setSelectedProducto] = useState<Producto | undefined>();
  const [productoToDelete, setProductoToDelete] = useState<Producto | null>(null);
  
  const { data: productos, isLoading, error } = useGetProductosQuery();
  const [deleteProducto, { isLoading: isDeleting }] = useDeleteProductoMutation();
  
  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';
  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';

  const handleCreate = () => {
    setSelectedProducto(undefined);
    onOpen();
  };

  const handleEdit = (producto: Producto) => {
    setSelectedProducto(producto);
    onOpen();
  };

  const handleDeleteClick = (producto: Producto) => {
    setProductoToDelete(producto);
    onDeleteOpen();
  };

  const handleDeleteConfirm = async () => {
    if (!productoToDelete) return;

    try {
      await deleteProducto(productoToDelete.id).unwrap();
      toast({
        title: 'Producto eliminado',
        description: `El producto "${productoToDelete.nombre}" fue eliminado exitosamente.`,
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
      onDeleteClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo eliminar el producto',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const columns = [
    {
      header: 'SKU',
      accessor: 'sku' as keyof Producto,
      width: '100px',
      hideOnMobile: true,
    },
    {
      header: 'Nombre',
      accessor: 'nombre' as keyof Producto,
    },
    {
      header: 'Precio/Día',
      accessor: 'precio_Alquiler_Dia' as keyof Producto,
      width: '120px',
      cell: (row: Producto) => formatPrice(row.precio_Alquiler_Dia),
    },
    {
      header: 'Stock',
      accessor: 'cantidad_Stock' as keyof Producto,
      width: '80px',
      cell: (row: Producto) => {
        const isLow = row.cantidad_Stock <= row.stock_Minimo;
        return (
          <Tooltip label={isLow ? 'Stock bajo' : 'Stock normal'}>
            <Badge colorScheme={isLow ? 'red' : 'green'}>
              {row.cantidad_Stock}
            </Badge>
          </Tooltip>
        );
      },
    },
    {
      header: 'Tipo',
      accessor: 'es_Alquilable' as keyof Producto,
      width: '100px',
      hideOnMobile: true,
      cell: (row: Producto) => {
        if (row.es_Alquilable && row.es_Vendible) return 'Ambos';
        if (row.es_Alquilable) return 'Alquiler';
        if (row.es_Vendible) return 'Venta';
        return '-';
      },
    },
    {
      header: 'Estado',
      accessor: 'activo' as keyof Producto,
      width: '100px',
      hideOnMobile: true,
      cell: (row: Producto) => (
        <Badge colorScheme={row.activo ? 'green' : 'red'}>
          {row.activo ? 'Activo' : 'Inactivo'}
        </Badge>
      ),
    },
    {
      header: 'Acciones',
      accessor: 'id' as keyof Producto,
      width: '120px',
      cell: (row: Producto) => (
        <HStack spacing={2}>
          <IconButton
            aria-label="Editar"
            icon={<EditIcon />}
            size="sm"
            colorScheme="blue"
            variant="ghost"
            onClick={() => handleEdit(row)}
          />
          <IconButton
            aria-label="Eliminar"
            icon={<DeleteIcon />}
            size="sm"
            colorScheme="red"
            variant="ghost"
            onClick={() => handleDeleteClick(row)}
          />
        </HStack>
      ),
    },
  ];

  if (isLoading) {
    return (
      <Container maxW="7xl" py={{ base: 4, md: 8 }} px={{ base: 4, md: 6 }}>
        <Center h="400px">
          <Spinner size="xl" color="brand.400" />
        </Center>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxW="7xl" py={{ base: 4, md: 8 }} px={{ base: 4, md: 6 }}>
        <Box p={8} borderRadius="lg" borderWidth="1px" borderColor="red.300" bg={bgColor} textAlign="center">
          <Text color="red.400">Error al cargar los productos</Text>
        </Box>
      </Container>
    );
  }
  
  return (
    <Container maxW="7xl" py={{ base: 4, md: 8 }} px={{ base: 4, md: 6 }}>
      <Box
        p={{ base: 4, md: 6 }}
        borderRadius="lg"
        borderWidth="1px"
        borderColor={borderColor}
        bg={bgColor}
      >
        <HStack 
          justify="space-between" 
          mb={6}
          flexWrap={{ base: "wrap", md: "nowrap" }}
          gap={{ base: 3, md: 0 }}
        >
          <Heading size={{ base: "md", md: "lg" }}>Gestión de Productos</Heading>
          <Button
            leftIcon={<AddIcon />}
            colorScheme="blue"
            onClick={handleCreate}
            size={{ base: "sm", md: "md" }}
            width={{ base: "full", sm: "auto" }}
          >
            Nuevo Producto
          </Button>
        </HStack>

        <DataTable
          data={productos || []}
          columns={columns}
        />
      </Box>

      <ProductoModal
        isOpen={isOpen}
        onClose={onClose}
        producto={selectedProducto}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent 
            bg={bgColor} 
            borderColor={borderColor} 
            borderWidth="1px"
            mx={4}
          >
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Producto
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar el producto <strong>{productoToDelete?.nombre}</strong>? 
              Esta acción no se puede deshacer.
            </AlertDialogBody>

            <AlertDialogFooter gap={{ base: 2, sm: 0 }}>
              <Button 
                ref={cancelRef} 
                onClick={onDeleteClose} 
                isDisabled={isDeleting}
                size={{ base: "sm", md: "md" }}
              >
                Cancelar
              </Button>
              <Button
                colorScheme="red"
                onClick={handleDeleteConfirm}
                ml={{ base: 0, sm: 3 }}
                isLoading={isDeleting}
                size={{ base: "sm", md: "md" }}
              >
                Eliminar
              </Button>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialogOverlay>
      </AlertDialog>
    </Container>
  );
}
