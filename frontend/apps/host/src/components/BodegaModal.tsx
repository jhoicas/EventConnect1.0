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
  NumberInput,
  NumberInputField,
  useToast,
  FormErrorMessage,
  VStack,
  Grid,
  GridItem,
} from '@chakra-ui/react';
import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateBodegaMutation, useUpdateBodegaMutation, type Bodega } from '../store/api/bodegaApi';
import { bodegaSchema, type BodegaFormData } from '../lib/validations/bodegaSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface BodegaModalProps {
  isOpen: boolean;
  onClose: () => void;
  bodega?: Bodega;
}

export const BodegaModal = ({ isOpen, onClose, bodega }: BodegaModalProps) => {
  const toast = useToast();
  const [createBodega, { isLoading: isCreating }] = useCreateBodegaMutation();
  const [updateBodega, { isLoading: isUpdating }] = useUpdateBodegaMutation();

  const isEdit = !!bodega;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
  } = useForm<BodegaFormData>({
    resolver: zodResolver(bodegaSchema),
    defaultValues: {
      codigo_Bodega: '',
      nombre: '',
      direccion: '',
      ciudad: '',
      telefono: '',
      capacidad_M3: 0,
    },
  });

  // Resetear formulario cuando cambia la bodega o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (bodega) {
        reset({
          codigo_Bodega: bodega.codigo_Bodega,
          nombre: bodega.nombre,
          direccion: bodega.direccion || '',
          ciudad: bodega.ciudad || '',
          telefono: bodega.telefono || '',
          capacidad_M3: bodega.capacidad_M3 || 0,
        });
      } else {
        reset({
          codigo_Bodega: '',
          nombre: '',
          direccion: '',
          ciudad: '',
          telefono: '',
          capacidad_M3: 0,
        });
      }
    }
  }, [bodega, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: BodegaFormData) => {
    try {
      if (isEdit) {
        await updateBodega({
          id: bodega.id,
          codigo_Bodega: data.codigo_Bodega,
          nombre: data.nombre,
          direccion: data.direccion || undefined,
          ciudad: data.ciudad || undefined,
          telefono: data.telefono || undefined,
          capacidad_M3: data.capacidad_M3 || undefined,
          estado: bodega.estado,
        }).unwrap();
        
        toast({
          title: 'Bodega actualizada',
          description: `La bodega ${data.nombre} fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createBodega({
          codigo_Bodega: data.codigo_Bodega,
          nombre: data.nombre,
          direccion: data.direccion || undefined,
          ciudad: data.ciudad || undefined,
          telefono: data.telefono || undefined,
          capacidad_M3: data.capacidad_M3 || undefined,
        }).unwrap();
        
        toast({
          title: 'Bodega creada',
          description: `La bodega ${data.nombre} fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar la bodega';
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

  return (
    <Modal 
      isOpen={isOpen} 
      onClose={handleClose} 
      size={{ base: "full", md: "2xl" }}
      scrollBehavior="inside"
    >
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent 
        bg={bgColor} 
        borderColor={borderColor} 
        borderWidth="1px"
        m={{ base: 0, md: 4 }}
        maxH={{ base: "100vh", md: "90vh" }}
      >
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalHeader fontSize={{ base: "lg", md: "xl" }}>
            {isEdit ? 'Editar Bodega' : 'Nueva Bodega'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.codigo_Bodega}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Código</FormLabel>
                    <Input
                      {...register('codigo_Bodega')}
                      placeholder="BOD-001"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.codigo_Bodega?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.nombre}>
                    <FormLabel>Nombre</FormLabel>
                    <Input
                      {...register('nombre')}
                      placeholder="Bodega Principal"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.nombre?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.ciudad}>
                    <FormLabel>Ciudad</FormLabel>
                    <Input
                      {...register('ciudad')}
                      placeholder="Bogotá"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.ciudad?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.telefono}>
                    <FormLabel>Teléfono</FormLabel>
                    <Input
                      {...register('telefono')}
                      placeholder="601 234 5678"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.telefono?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isInvalid={!!errors.direccion}>
                <FormLabel>Dirección</FormLabel>
                <Input
                  {...register('direccion')}
                  placeholder="Calle 123 #45-67, Zona Industrial"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.direccion?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.capacidad_M3}>
                <FormLabel>Capacidad (m³)</FormLabel>
                <Controller
                  name="capacidad_M3"
                  control={control}
                  render={({ field: { onChange, value } }) => (
                    <NumberInput
                      value={value}
                      onChange={(_, val) => onChange(val)}
                      min={0}
                      precision={2}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} placeholder="500.00" />
                    </NumberInput>
                  )}
                />
                <FormErrorMessage>{errors.capacidad_M3?.message}</FormErrorMessage>
              </FormControl>
            </VStack>
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
