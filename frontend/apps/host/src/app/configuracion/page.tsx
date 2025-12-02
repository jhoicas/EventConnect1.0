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
  Code,
  ButtonGroup,
  Input,
  InputGroup,
  InputLeftElement,
} from '@chakra-ui/react';
import { AddIcon, EditIcon, DeleteIcon, SearchIcon } from '@chakra-ui/icons';
import { Settings, Globe, Building2, Database } from 'lucide-react';
import { ConfiguracionModal } from '@/components/ConfiguracionModal';
import {
  useGetConfiguracionesQuery,
  useDeleteConfiguracionMutation,
  type ConfiguracionSistema,
} from '@/store/api/configuracionApi';
import { useRef } from 'react';

export default function ConfiguracionPage() {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const toast = useToast();

  const [selectedConfiguracion, setSelectedConfiguracion] = useState<ConfiguracionSistema | undefined>();
  const [configuracionToDelete, setConfiguracionToDelete] = useState<ConfiguracionSistema | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [tipoFilter, setTipoFilter] = useState<string>('Todos');

  const { data: configuraciones = [], isLoading } = useGetConfiguracionesQuery();
  const [deleteConfiguracion] = useDeleteConfiguracionMutation();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const mutedColor = localColorMode === 'light' ? 'text.light.muted' : localColorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';
  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#2d3548' : localColorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  const handleEdit = (configuracion: ConfiguracionSistema) => {
    setSelectedConfiguracion(configuracion);
    onOpen();
  };

  const handleDelete = (configuracion: ConfiguracionSistema) => {
    setConfiguracionToDelete(configuracion);
    onDeleteOpen();
  };

  const confirmDelete = async () => {
    if (configuracionToDelete) {
      try {
        await deleteConfiguracion(configuracionToDelete.id).unwrap();
        toast({
          title: 'Configuración eliminada',
          description: `La configuración "${configuracionToDelete.clave}" fue eliminada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } catch (error: any) {
        toast({
          title: 'Error',
          description: error?.data?.message || 'Ocurrió un error al eliminar la configuración',
          status: 'error',
          duration: 5000,
          isClosable: true,
        });
      }
      onDeleteClose();
      setConfiguracionToDelete(null);
    }
  };

  const handleModalClose = () => {
    setSelectedConfiguracion(undefined);
    onClose();
  };

  const getTipoDatoBadgeColor = (tipo: string) => {
    switch (tipo) {
      case 'string':
        return 'blue';
      case 'int':
        return 'green';
      case 'bool':
        return 'purple';
      case 'json':
        return 'orange';
      default:
        return 'gray';
    }
  };

  const formatValor = (valor?: string, tipoDato?: string) => {
    if (!valor) return 'N/A';
    if (tipoDato === 'json') {
      try {
        const parsed = JSON.parse(valor);
        return JSON.stringify(parsed, null, 2);
      } catch {
        return valor;
      }
    }
    return valor;
  };

  const configuracionesFiltradas = configuraciones
    .filter((c) => {
      const matchesSearch =
        c.clave.toLowerCase().includes(searchTerm.toLowerCase()) ||
        c.descripcion?.toLowerCase().includes(searchTerm.toLowerCase());

      if (tipoFilter === 'Todos') return matchesSearch;
      if (tipoFilter === 'Globales') return matchesSearch && c.es_Global;
      if (tipoFilter === 'Empresa') return matchesSearch && !c.es_Global;

      return matchesSearch;
    });

  const configuracionesGlobales = configuraciones.filter((c) => c.es_Global).length;
  const configuracionesEmpresa = configuraciones.filter((c) => !c.es_Global).length;

  const tipos = ['Todos', 'Globales', 'Empresa'];

  return (
    <VStack spacing={6} align="stretch">
      <Box>
        <Heading size="lg" mb={2} display="flex" alignItems="center" gap={3}>
          <Settings size={28} />
          Configuración del Sistema
        </Heading>
        <Text fontSize="sm" color={mutedColor}>
          Ajustes del sistema y preferencias
        </Text>
      </Box>

      <HStack
        spacing={4}
        p={4}
        bg={cardBg}
        borderWidth="1px"
        borderColor={borderColor}
        borderRadius="lg"
        flexWrap="wrap"
      >
        <HStack flex="1" minW="200px">
          <Database size={20} />
          <Box>
            <Text fontSize="2xl" fontWeight="bold">
              {configuraciones.length}
            </Text>
            <Text fontSize="sm" color={mutedColor}>
              Total
            </Text>
          </Box>
        </HStack>

        <HStack flex="1" minW="200px">
          <Globe size={20} color="#3182ce" />
          <Box>
            <Text fontSize="2xl" fontWeight="bold" color="blue.500">
              {configuracionesGlobales}
            </Text>
            <Text fontSize="sm" color={mutedColor}>
              Globales
            </Text>
          </Box>
        </HStack>

        <HStack flex="1" minW="200px">
          <Building2 size={20} color="#38a169" />
          <Box>
            <Text fontSize="2xl" fontWeight="bold" color="green.500">
              {configuracionesEmpresa}
            </Text>
            <Text fontSize="sm" color={mutedColor}>
              Por Empresa
            </Text>
          </Box>
        </HStack>
      </HStack>

      <HStack justify="space-between" flexWrap="wrap" gap={4}>
        <HStack spacing={4} flex="1">
          <ButtonGroup size="sm" isAttached variant="outline">
            {tipos.map((tipo) => (
              <Button
                key={tipo}
                onClick={() => setTipoFilter(tipo)}
                colorScheme={tipoFilter === tipo ? 'blue' : 'gray'}
                variant={tipoFilter === tipo ? 'solid' : 'outline'}
              >
                {tipo}
              </Button>
            ))}
          </ButtonGroup>

          <InputGroup maxW="300px">
            <InputLeftElement pointerEvents="none">
              <SearchIcon color="gray.300" />
            </InputLeftElement>
            <Input
              placeholder="Buscar configuración..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </InputGroup>
        </HStack>

        <Button
          leftIcon={<AddIcon />}
          colorScheme="blue"
          onClick={onOpen}
          size="md"
        >
          Nueva Configuración
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
              <Th>Clave</Th>
              <Th>Valor</Th>
              <Th>Tipo</Th>
              <Th>Ámbito</Th>
              <Th>Descripción</Th>
              <Th>Última Actualización</Th>
              <Th>Acciones</Th>
            </Tr>
          </Thead>
          <Tbody>
            {isLoading ? (
              <Tr>
                <Td colSpan={7} textAlign="center">
                  Cargando configuraciones...
                </Td>
              </Tr>
            ) : configuracionesFiltradas.length === 0 ? (
              <Tr>
                <Td colSpan={7} textAlign="center">
                  No hay configuraciones {searchTerm && 'que coincidan con la búsqueda'}
                </Td>
              </Tr>
            ) : (
              configuracionesFiltradas.map((configuracion) => (
                <Tr key={configuracion.id}>
                  <Td>
                    <Code fontSize="sm" colorScheme="gray">
                      {configuracion.clave}
                    </Code>
                  </Td>
                  <Td>
                    {configuracion.tipo_Dato === 'json' ? (
                      <Tooltip label={formatValor(configuracion.valor, configuracion.tipo_Dato)}>
                        <Code fontSize="xs" colorScheme="orange" cursor="pointer">
                          {configuracion.valor?.substring(0, 30)}...
                        </Code>
                      </Tooltip>
                    ) : configuracion.tipo_Dato === 'bool' ? (
                      <Badge colorScheme={configuracion.valor === 'true' ? 'green' : 'red'}>
                        {configuracion.valor === 'true' ? 'Verdadero' : 'Falso'}
                      </Badge>
                    ) : (
                      <Text fontSize="sm">{configuracion.valor || 'N/A'}</Text>
                    )}
                  </Td>
                  <Td>
                    <Badge colorScheme={getTipoDatoBadgeColor(configuracion.tipo_Dato)}>
                      {configuracion.tipo_Dato}
                    </Badge>
                  </Td>
                  <Td>
                    {configuracion.es_Global ? (
                      <HStack>
                        <Globe size={14} />
                        <Badge colorScheme="blue">Global</Badge>
                      </HStack>
                    ) : (
                      <HStack>
                        <Building2 size={14} />
                        <Badge colorScheme="green">Empresa</Badge>
                      </HStack>
                    )}
                  </Td>
                  <Td fontSize="sm" maxW="200px" noOfLines={2}>
                    {configuracion.descripcion || 'Sin descripción'}
                  </Td>
                  <Td fontSize="sm">
                    {new Date(configuracion.fecha_Actualizacion).toLocaleDateString('es-CO', {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </Td>
                  <Td>
                    <HStack spacing={2}>
                      <Tooltip label="Editar configuración">
                        <IconButton
                          aria-label="Editar"
                          icon={<EditIcon />}
                          size="sm"
                          colorScheme="blue"
                          variant="ghost"
                          onClick={() => handleEdit(configuracion)}
                        />
                      </Tooltip>
                      <Tooltip label="Eliminar configuración">
                        <IconButton
                          aria-label="Eliminar"
                          icon={<DeleteIcon />}
                          size="sm"
                          colorScheme="red"
                          variant="ghost"
                          onClick={() => handleDelete(configuracion)}
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

      <ConfiguracionModal
        isOpen={isOpen}
        onClose={handleModalClose}
        configuracion={selectedConfiguracion}
      />

      <AlertDialog
        isOpen={isDeleteOpen}
        leastDestructiveRef={cancelRef}
        onClose={onDeleteClose}
      >
        <AlertDialogOverlay>
          <AlertDialogContent bg={cardBg} borderColor={borderColor}>
            <AlertDialogHeader fontSize="lg" fontWeight="bold">
              Eliminar Configuración
            </AlertDialogHeader>

            <AlertDialogBody>
              ¿Estás seguro de eliminar la configuración <Code>{configuracionToDelete?.clave}</Code>? Esta acción no se puede deshacer.
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

