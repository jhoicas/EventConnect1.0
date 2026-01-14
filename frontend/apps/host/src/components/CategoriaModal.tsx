'use client';

import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalFooter,
  ModalBody,
  ModalCloseButton,
  Button,
  FormControl,
  FormLabel,
  Input,
  Textarea,
  useToast,
  FormErrorMessage,
  Select,
  HStack,
  Text,
  Box,
} from '@chakra-ui/react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateCategoriaMutation, useUpdateCategoriaMutation, type Categoria } from '../store/api/categoriaApi';
import { categoriaSchema, type CategoriaFormData } from '../lib/validations/categoriaSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

const ICONOS_DISPONIBLES = [
  { value: 'chair', label: '🪑 Silla (chair)' },
  { value: 'couch', label: '🛋️ Sofá (couch)' },
  { value: 'utensils', label: '🍴 Vajilla (utensils)' },
  { value: 'wine-glass', label: '🍷 Copa (wine-glass)' },
  { value: 'coffee', label: '☕ Café (coffee)' },
  { value: 'sparkles', label: '✨ Decoración (sparkles)' },
  { value: 'balloon', label: '🎈 Globo (balloon)' },
  { value: 'party-popper', label: '🎉 Fiesta (party-popper)' },
  { value: 'speaker', label: '🔊 Sonido (speaker)' },
  { value: 'lightbulb', label: '💡 Iluminación (lightbulb)' },
  { value: 'tv', label: '📺 Proyección (tv)' },
  { value: 'monitor', label: '🖥️ Monitor (monitor)' },
  { value: 'music', label: '🎵 Música (music)' },
  { value: 'music-note', label: '🎶 Nota Musical (music-note)' },
  { value: 'camera', label: '📷 Fotografía (camera)' },
  { value: 'game-controller', label: '🎮 Juegos (game-controller)' },
  { value: 'crown', label: '👑 VIP (crown)' },
  { value: 'stage', label: '🎭 Escenario (stage)' },
  { value: 'fan', label: '🌀 Ventilación (fan)' },
  { value: 'shield', label: '🛡️ Seguridad (shield)' },
  { value: 'truck', label: '🚚 Transporte (truck)' },
  { value: 'baby', label: '👶 Infantil (baby)' },
];

interface CategoriaModalProps {
  isOpen: boolean;
  onClose: () => void;
  categoria?: Categoria;
}

export const CategoriaModal = ({ isOpen, onClose, categoria }: CategoriaModalProps) => {
  const toast = useToast();
  const [createCategoria, { isLoading: isCreating }] = useCreateCategoriaMutation();
  const [updateCategoria, { isLoading: isUpdating }] = useUpdateCategoriaMutation();

  const isEdit = !!categoria;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm<CategoriaFormData>({
    resolver: zodResolver(categoriaSchema),
    defaultValues: {
      nombre: '',
      descripcion: '',
      icono: '',
      color: '#3B82F6',
    },
  });

  const color = watch('color');

  // Resetear formulario cuando cambia la categoría o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (categoria) {
        reset({
          nombre: categoria.nombre,
          descripcion: categoria.descripcion || '',
          icono: categoria.icono || '',
          color: categoria.color || '#3B82F6',
        });
      } else {
        reset({
          nombre: '',
          descripcion: '',
          icono: '',
          color: '#3B82F6',
        });
      }
    }
  }, [categoria, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: CategoriaFormData) => {
    try {
      if (isEdit) {
        await updateCategoria({
          id: categoria.id,
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          icono: data.icono || undefined,
          color: data.color || undefined,
          activo: categoria.activo,
        }).unwrap();
        
        toast({
          title: 'Categoría actualizada',
          description: `La categoría "${data.nombre}" fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createCategoria({
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          icono: data.icono || undefined,
          color: data.color || undefined,
        }).unwrap();
        
        toast({
          title: 'Categoría creada',
          description: `La categoría "${data.nombre}" fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar la categoría';
      toast({
        title: 'Error',
        description: errorMessage,
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size={{ base: "full", md: "md" }} scrollBehavior="inside">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent 
        bg={bgColor} 
        borderColor={borderColor} 
        borderWidth="1px"
        mx={{ base: 0, md: 4 }}
        my={{ base: 0, md: "auto" }}
        maxH={{ base: "100vh", md: "90vh" }}
      >
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalHeader fontSize={{ base: "lg", md: "xl" }}>
            {isEdit ? 'Editar Categoría' : 'Nueva Categoría'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <FormControl isRequired isInvalid={!!errors.nombre} mb={4}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre</FormLabel>
              <Input
                {...register('nombre')}
                placeholder="Ej: Sillas, Mesas, Vajillas"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
              />
              <FormErrorMessage>{errors.nombre?.message}</FormErrorMessage>
            </FormControl>

            <FormControl isInvalid={!!errors.descripcion}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Descripción</FormLabel>
              <Textarea
                {...register('descripcion')}
                placeholder="Descripción opcional de la categoría"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
                rows={3}
                size={{ base: "sm", md: "md" }}
              />
              <FormErrorMessage>{errors.descripcion?.message}</FormErrorMessage>
            </FormControl>

            <FormControl mt={4} isInvalid={!!errors.icono}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Icono</FormLabel>
              <Select
                {...register('icono')}
                placeholder="Selecciona un icono"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
                size={{ base: "sm", md: "md" }}
              >
                <option value="">Sin icono</option>
                {ICONOS_DISPONIBLES.map((icon) => (
                  <option key={icon.value} value={icon.value}>
                    {icon.label}
                  </option>
                ))}
              </Select>
              <FormErrorMessage>{errors.icono?.message}</FormErrorMessage>
            </FormControl>

            <FormControl mt={4} isInvalid={!!errors.color}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Color</FormLabel>
              <HStack spacing={3} flexWrap={{ base: "wrap", md: "nowrap" }}>
                <Input
                  type="color"
                  {...register('color')}
                  bg={inputBg}
                  borderColor={borderColor}
                  width={{ base: "60px", md: "80px" }}
                  height={{ base: "35px", md: "40px" }}
                  cursor="pointer"
                />
                <Box
                  width={{ base: "35px", md: "40px" }}
                  height={{ base: "35px", md: "40px" }}
                  borderRadius="md"
                  bg={color}
                  border="2px solid"
                  borderColor={borderColor}
                />
                <Text fontSize={{ base: "xs", md: "sm" }} color="gray.500">
                  {color}
                </Text>
              </HStack>
              <FormErrorMessage>{errors.color?.message}</FormErrorMessage>
            </FormControl>
          </ModalBody>

          <ModalFooter flexDirection={{ base: "column", sm: "row" }} gap={{ base: 2, sm: 0 }}>
            <Button
              variant="ghost"
              mr={{ base: 0, sm: 3 }}
              onClick={handleClose}
              isDisabled={isLoading}
              width={{ base: "full", sm: "auto" }}
              size={{ base: "md", md: "md" }}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              colorScheme="blue"
              isLoading={isLoading}
              loadingText={isEdit ? 'Actualizando...' : 'Creando...'}
              width={{ base: "full", sm: "auto" }}
              size={{ base: "md", md: "md" }}
            >
              {isEdit ? 'Actualizar' : 'Crear'}
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
};
