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
import { useGetClientesQuery, useDeleteClienteMutation, type Cliente } from '../../store/api/clienteApi';
import { ClienteModal } from '../../components/ClienteModal';
import { useState, useRef } from 'react';
import { useAppSelector } from '../../store/store';

export default function ClientesPage() {
  const { colorMode } = useColorMode();
  const { user } = useAppSelector((state) => state.auth);
  const isSuperAdmin = user?.rol === 'SuperAdmin';
  const toast = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  
  const [selectedCliente, setSelectedCliente] = useState<Cliente | undefined>();
  const [clienteToDelete, setClienteToDelete] = useState<Cliente | null>(null);
  
  const { data: clientes, isLoading, error } = useGetClientesQuery();
  const [deleteCliente, { isLoading: isDeleting }] = useDeleteClienteMutation();
  
  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';
  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';

  const handleCreate = () => {
    setSelectedCliente(undefined);
    onOpen();
  };

  const handleEdit = (cliente: Cliente) => {
    setSelectedCliente(cliente);
    onOpen();
  };

  const handleDeleteClick = (cliente: Cliente) => {
    setClienteToDelete(cliente);
    onDeleteOpen();
  };

  const handleDeleteConfirm = async () => {
    if (!clienteToDelete) return;

    try {
      await deleteCliente(clienteToDelete.id).unwrap();
      toast({
        title: 'Cliente eliminado',
        description: `El cliente "${clienteToDelete.nombre}" fue eliminado exitosamente.`,
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
      onDeleteClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo eliminar el cliente',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const columns = [
    {
      header: 'Tipo',
      accessor: 'tipo_Cliente' as keyof Cliente,
      width: '100px',
      hideOnMobile: true,
      cell: (row: Cliente) => (
        <Badge colorScheme={row.tipo_Cliente === 'Empresa' ? 'purple' : 'blue'}>
          {row.tipo_Cliente}
        </Badge>
      ),
    },
    ...(isSuperAdmin ? [{
      header: 'Empresa',
      accessor: 'empresa_Nombre' as keyof Cliente,
      width: '150px',
      hideOnMobile: true,
      cell: (row: Cliente) => (
        <Badge colorScheme="green" variant="subtle">
          {row.empresa_Nombre || 'N/A'}
        </Badge>
      ),
    }] : []),
    {
      header: 'Nombre',
      accessor: 'nombre' as keyof Cliente,
    },
    {
      header: 'Documento',
      accessor: 'documento' as keyof Cliente,
      width: '130px',
      hideOnMobile: true,
      cell: (row: Cliente) => `${row.tipo_Documento}: ${row.documento}`,
    },
    {
      header: 'Email',
      accessor: 'email' as keyof Cliente,
      hideOnMobile: true,
      cell: (row: Cliente) => row.email || '-',
    },
    {
      header: 'Teléfono',
      accessor: 'telefono' as keyof Cliente,
      width: '120px',
      cell: (row: Cliente) => row.telefono || '-',
    },
    {
      header: 'Ciudad',
      accessor: 'ciudad' as keyof Cliente,
      width: '120px',
      hideOnMobile: true,
      cell: (row: Cliente) => row.ciudad || '-',
    },
    {
      header: 'Rating',
      accessor: 'rating' as keyof Cliente,
      width: '80px',
      hideOnMobile: true,
      cell: (row: Cliente) => (
        <Tooltip label={`${row.total_Alquileres} alquileres`}>
          <Badge colorScheme={row.rating >= 4 ? 'green' : row.rating >= 3 ? 'yellow' : 'red'}>
            {row.rating.toFixed(1)} ⭐
          </Badge>
        </Tooltip>
      ),
    },
    {
      header: 'Estado',
      accessor: 'estado' as keyof Cliente,
      width: '100px',
      hideOnMobile: true,
      cell: (row: Cliente) => (
        <Badge colorScheme={row.estado === 'Activo' ? 'green' : 'red'}>
          {row.estado}
        </Badge>
      ),
    },
    {
      header: 'Acciones',
      accessor: 'id' as keyof Cliente,
      width: '120px',
      cell: (row: Cliente) => (
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
          <Text color="red.400">Error al cargar los clientes</Text>
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
          <Heading size={{ base: "md", md: "lg" }}>Gestión de Clientes</Heading>
          <Button
            leftIcon={<AddIcon />}
            colorScheme="blue"
            onClick={handleCreate}
            size={{ base: "sm", md: "md" }}
            width={{ base: "full", sm: "auto" }}
          >
            Nuevo Cliente
          </Button>
        </HStack>

        <DataTable
          data={clientes || []}
          columns={columns}
        />
      </Box>

      <ClienteModal
        isOpen={isOpen}
        onClose={onClose}
        cliente={selectedCliente}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={bgColor} borderColor={borderColor} borderWidth="1px">
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Cliente
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar el cliente <strong>{clienteToDelete?.nombre}</strong>? 
              Esta acción no se puede deshacer.
            </AlertDialogBody>

            <AlertDialogFooter>
              <Button ref={cancelRef} onClick={onDeleteClose} isDisabled={isDeleting}>
                Cancelar
              </Button>
              <Button
                colorScheme="red"
                onClick={handleDeleteConfirm}
                ml={3}
                isLoading={isDeleting}
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
