'use client';

import {
  Box,
  Heading,
  Button,
  useColorMode,
  Container,
  HStack,
  Spinner,
  Center,
  Text,
  IconButton,
  useToast,
  useDisclosure,
  AlertDialog,
  AlertDialogBody,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogContent,
  AlertDialogOverlay,
  Badge,
  Tooltip,
  Menu,
  MenuButton,
  MenuList,
  MenuItem,
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
  TableContainer,
  Input,
} from '@chakra-ui/react';
import { EditIcon, DeleteIcon, CheckIcon, CloseIcon } from '@chakra-ui/icons';
import { MoreVertical } from 'lucide-react';
import {
  useGetUsuariosQuery,
  useUpdateUsuarioEstadoMutation,
  useDeleteUsuarioMutation,
  type Usuario,
} from '../../store/api/usuarioApi';
import { useState, useRef, useMemo } from 'react';
import { useAppSelector } from '../../store/store';

export default function UsuariosPage() {
  const { colorMode } = useColorMode();
  const { user } = useAppSelector((state) => state.auth);
  const isSuperAdmin = user?.rol === 'SuperAdmin';
  const toast = useToast();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);

  const [usuarioToDelete, setUsuarioToDelete] = useState<Usuario | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  const { data: usuarios, isLoading, error } = useGetUsuariosQuery();
  const [updateEstado, { isLoading: isUpdating }] = useUpdateUsuarioEstadoMutation();
  const [deleteUsuario, { isLoading: isDeleting }] = useDeleteUsuarioMutation();

  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';
  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';

  const handleActivar = async (usuario: Usuario) => {
    try {
      await updateEstado({ id: usuario.id, estado: 'Activo' }).unwrap();
      toast({
        title: 'Usuario activado',
        description: `El usuario "${usuario.nombre_Completo}" fue activado exitosamente.`,
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo activar el usuario',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleDesactivar = async (usuario: Usuario) => {
    try {
      await updateEstado({ id: usuario.id, estado: 'Inactivo' }).unwrap();
      toast({
        title: 'Usuario desactivado',
        description: `El usuario "${usuario.nombre_Completo}" fue desactivado.`,
        status: 'warning',
        duration: 3000,
        isClosable: true,
      });
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo desactivar el usuario',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleDeleteClick = (usuario: Usuario) => {
    setUsuarioToDelete(usuario);
    onDeleteOpen();
  };

  const handleDeleteConfirm = async () => {
    if (!usuarioToDelete) return;

    try {
      await deleteUsuario(usuarioToDelete.id).unwrap();
      toast({
        title: 'Usuario eliminado',
        description: `El usuario "${usuarioToDelete.nombre_Completo}" fue eliminado exitosamente.`,
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
      onDeleteClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'No se pudo eliminar el usuario',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const getEstadoBadge = (estado: string) => {
    const colorScheme = estado === 'Activo' ? 'green' : estado === 'Inactivo' ? 'red' : 'yellow';
    return <Badge colorScheme={colorScheme}>{estado}</Badge>;
  };

  const getRolBadge = (rol?: string) => {
    const colorScheme =
      rol === 'SuperAdmin' ? 'purple' : rol === 'Admin-Proveedor' ? 'blue' : rol === 'Operario-Logística' ? 'cyan' : 'gray';
    return <Badge colorScheme={colorScheme}>{rol || 'Usuario'}</Badge>;
  };

  const filteredUsuarios = useMemo(() => {
    if (!usuarios) return [];
    if (!searchTerm) return usuarios;
    
    const term = searchTerm.toLowerCase();
    return usuarios.filter(u => 
      u.nombre_Completo?.toLowerCase().includes(term) ||
      u.email?.toLowerCase().includes(term) ||
      u.usuario1?.toLowerCase().includes(term) ||
      u.rol?.toLowerCase().includes(term)
    );
  }, [usuarios, searchTerm]);

  const columns = [
    {
      key: 'nombre_Completo',
      label: 'Nombre',
      sortable: true,
    },
    {
      key: 'email',
      label: 'Email',
      sortable: true,
    },
    {
      key: 'usuario1',
      label: 'Usuario',
      sortable: true,
    },
    {
      key: 'rol',
      label: 'Rol',
      sortable: true,
      render: (value: string) => getRolBadge(value),
    },
    {
      key: 'telefono',
      label: 'Teléfono',
      sortable: false,
    },
    {
      key: 'estado',
      label: 'Estado',
      sortable: true,
      render: (value: string) => getEstadoBadge(value),
    },
    {
      key: 'fecha_Creacion',
      label: 'Fecha Registro',
      sortable: true,
      render: (value: string) => new Date(value).toLocaleDateString('es-CO'),
    },
    {
      key: 'ultimo_Acceso',
      label: 'Último Login',
      sortable: true,
      render: (value?: string) => (value ? new Date(value).toLocaleDateString('es-CO') : '-'),
    },
    {
      key: 'actions',
      label: 'Acciones',
      render: (_: any, row: Usuario) => (
        <Menu>
          <MenuButton
            as={IconButton}
            icon={<MoreVertical size={16} />}
            variant="ghost"
            size="sm"
            aria-label="Opciones"
          />
          <MenuList>
            {row.estado === 'Activo' ? (
              <MenuItem icon={<CloseIcon />} onClick={() => handleDesactivar(row)}>
                Desactivar
              </MenuItem>
            ) : (
              <MenuItem icon={<CheckIcon />} onClick={() => handleActivar(row)} color="green.500">
                Activar
              </MenuItem>
            )}
            {isSuperAdmin && (
              <MenuItem icon={<DeleteIcon />} onClick={() => handleDeleteClick(row)} color="red.500">
                Eliminar
              </MenuItem>
            )}
          </MenuList>
        </Menu>
      ),
    },
  ];

  if (isLoading) {
    return (
      <Container maxW="container.2xl" py={8}>
        <Center h="400px">
          <Spinner size="xl" color="blue.500" />
        </Center>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxW="container.2xl" py={8}>
        <Center h="400px">
          <Text color="red.500">Error al cargar usuarios</Text>
        </Center>
      </Container>
    );
  }

  console.log('Usuarios cargados:', usuarios); // Debug

  return (
    <Container maxW="container.2xl" py={8}>
      <Box bg={bgColor} p={6} borderRadius="lg" borderWidth="1px" borderColor={borderColor}>
        <HStack justify="space-between" mb={6}>
          <Heading size="lg">Gestión de Usuarios</Heading>
        </HStack>

        <Input
          placeholder="Buscar usuarios..."
          mb={4}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />

        <TableContainer>
          <Table variant="simple">
            <Thead>
              <Tr>
                <Th>Nombre</Th>
                <Th>Email</Th>
                <Th>Usuario</Th>
                <Th>Rol</Th>
                <Th>Teléfono</Th>
                <Th>Estado</Th>
                <Th>Último Login</Th>
                <Th>Acciones</Th>
              </Tr>
            </Thead>
            <Tbody>
              {filteredUsuarios.map((usuario) => (
                <Tr key={usuario.id}>
                  <Td>{usuario.nombre_Completo}</Td>
                  <Td>{usuario.email}</Td>
                  <Td>{usuario.usuario1}</Td>
                  <Td>{getRolBadge(usuario.rol)}</Td>
                  <Td>{usuario.telefono || '-'}</Td>
                  <Td>{getEstadoBadge(usuario.estado)}</Td>
                  <Td>
                    {usuario.ultimo_Acceso
                      ? new Date(usuario.ultimo_Acceso).toLocaleDateString('es-CO')
                      : '-'}
                  </Td>
                  <Td>
                    <Menu>
                      <MenuButton
                        as={IconButton}
                        icon={<MoreVertical size={16} />}
                        variant="ghost"
                        size="sm"
                        aria-label="Opciones"
                      />
                      <MenuList>
                        {usuario.estado === 'Activo' ? (
                          <MenuItem icon={<CloseIcon />} onClick={() => handleDesactivar(usuario)}>
                            Desactivar
                          </MenuItem>
                        ) : (
                          <MenuItem icon={<CheckIcon />} onClick={() => handleActivar(usuario)} color="green.500">
                            Activar
                          </MenuItem>
                        )}
                        {isSuperAdmin && (
                          <MenuItem icon={<DeleteIcon />} onClick={() => handleDeleteClick(usuario)} color="red.500">
                            Eliminar
                          </MenuItem>
                        )}
                      </MenuList>
                    </Menu>
                  </Td>
                </Tr>
              ))}
            </Tbody>
          </Table>
        </TableContainer>

        {filteredUsuarios.length === 0 && (
          <Center h="200px">
            <Text color="gray.500">No se encontraron usuarios</Text>
          </Center>
        )}
      </Box>

      {/* Dialog de confirmación de eliminación */}
      <AlertDialog isOpen={isDeleteOpen} leastDestructiveRef={cancelRef} onClose={onDeleteClose}>
        <AlertDialogOverlay>
          <AlertDialogContent>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Usuario
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar al usuario <strong>{usuarioToDelete?.nombre_Completo}</strong>? Esta
              acción no se puede deshacer.
            </AlertDialogBody>

            <AlertDialogFooter>
              <Button ref={cancelRef} onClick={onDeleteClose}>
                Cancelar
              </Button>
              <Button colorScheme="red" onClick={handleDeleteConfirm} ml={3} isLoading={isDeleting}>
                Eliminar
              </Button>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialogOverlay>
      </AlertDialog>
    </Container>
  );
}
