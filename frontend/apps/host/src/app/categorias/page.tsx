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
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon } from '@chakra-ui/icons';
import { DataTable } from '@eventconnect/ui';
import { useGetCategoriasQuery, useDeleteCategoriaMutation, type Categoria } from '../../store/api/categoriaApi';
import { CategoriaModal } from '../../components/CategoriaModal';
import { useState, useRef } from 'react';

export default function CategoriasPage() {
  const { colorMode } = useColorMode();
  const toast = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  
  const [selectedCategoria, setSelectedCategoria] = useState<Categoria | undefined>();
  const [categoriaToDelete, setCategoriaToDelete] = useState<Categoria | null>(null);
  
  const { data: categorias, isLoading, error } = useGetCategoriasQuery();
  const [deleteCategoria, { isLoading: isDeleting }] = useDeleteCategoriaMutation();
  
  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';
  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';
  const mutedColor = colorMode === 'dark' ? '#9ca3af' : colorMode === 'blue' ? '#94a3b8' : '#718096';

  const handleCreate = () => {
    setSelectedCategoria(undefined);
    onOpen();
  };

  const handleEdit = (categoria: Categoria) => {
    setSelectedCategoria(categoria);
    onOpen();
  };

  const handleDeleteClick = (categoria: Categoria) => {
    setCategoriaToDelete(categoria);
    onDeleteOpen();
  };

  const handleDeleteConfirm = async () => {
    if (!categoriaToDelete) return;

    try {
      await deleteCategoria(categoriaToDelete.id).unwrap();
      toast({
        title: 'Categoría eliminada',
        description: `La categoría "${categoriaToDelete.nombre}" fue eliminada exitosamente.`,
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
      onDeleteClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo eliminar la categoría',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const columns = [
    {
      header: 'ID',
      accessor: 'id' as keyof Categoria,
      width: '80px',
    },
    {
      header: 'Nombre',
      accessor: 'nombre' as keyof Categoria,
    },
    {
      header: 'Descripción',
      accessor: 'descripcion' as keyof Categoria,
      cell: (row: Categoria) => row.descripcion || '-',
    },
    {
      header: 'Estado',
      accessor: 'activo' as keyof Categoria,
      width: '100px',
      cell: (row: Categoria) => (
        <Badge colorScheme={row.activo ? 'green' : 'red'}>
          {row.activo ? 'Activo' : 'Inactivo'}
        </Badge>
      ),
    },
    {
      header: 'Acciones',
      accessor: 'id' as keyof Categoria,
      width: '120px',
      cell: (row: Categoria) => (
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
        <Box p={{ base: 4, md: 8 }} borderRadius="lg" borderWidth="1px" borderColor="red.300" bg={bgColor} textAlign="center">
          <Text color="red.400">Error al cargar las categorías</Text>
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
        <HStack justify="space-between" mb={6} flexWrap={{ base: "wrap", md: "nowrap" }} gap={4}>
          <Heading size={{ base: "md", md: "lg" }}>Gestión de Categorías</Heading>
          <Button
            leftIcon={<AddIcon />}
            colorScheme="blue"
            onClick={handleCreate}
            size={{ base: "sm", md: "md" }}
            width={{ base: "full", sm: "auto" }}
          >
            Nueva Categoría
          </Button>
        </HStack>

        <DataTable
          data={categorias || []}
          columns={columns}
        />
      </Box>

      <CategoriaModal
        isOpen={isOpen}
        onClose={onClose}
        categoria={selectedCategoria}
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
              Eliminar Categoría
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar la categoría <strong>{categoriaToDelete?.nombre}</strong>? 
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
