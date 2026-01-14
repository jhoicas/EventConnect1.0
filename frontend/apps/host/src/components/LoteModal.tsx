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
  Select,
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
import { useCreateLoteMutation, useUpdateLoteMutation, type Lote } from '../store/api/loteApi';
import { useGetProductosQuery } from '../store/api/productoApi';
import { useGetBodegasQuery } from '../store/api/bodegaApi';
import { loteSchema, type LoteFormData } from '../lib/validations/loteSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface LoteModalProps {
  isOpen: boolean;
  onClose: () => void;
  lote?: Lote;
}

export const LoteModal = ({ isOpen, onClose, lote }: LoteModalProps) => {
  const toast = useToast();
  const { data: productos = [] } = useGetProductosQuery();
  const { data: bodegas = [] } = useGetBodegasQuery();
  
  const [createLote, { isLoading: isCreating }] = useCreateLoteMutation();
  const [updateLote, { isLoading: isUpdating }] = useUpdateLoteMutation();

  const isEdit = !!lote;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
  } = useForm<LoteFormData>({
    resolver: zodResolver(loteSchema),
    defaultValues: {
      producto_Id: 0,
      bodega_Id: 0,
      codigo_Lote: '',
      fecha_Fabricacion: '',
      fecha_Vencimiento: '',
      cantidad_Inicial: 0,
      costo_Unitario: 0,
    },
  });

  // Resetear formulario cuando cambia el lote o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (lote) {
        reset({
          producto_Id: lote.producto_Id,
          bodega_Id: lote.bodega_Id || 0,
          codigo_Lote: lote.codigo_Lote,
          fecha_Fabricacion: lote.fecha_Fabricacion?.split('T')[0] || '',
          fecha_Vencimiento: lote.fecha_Vencimiento?.split('T')[0] || '',
          cantidad_Inicial: lote.cantidad_Inicial,
          costo_Unitario: lote.costo_Unitario,
        });
      } else {
        reset({
          producto_Id: 0,
          bodega_Id: 0,
          codigo_Lote: '',
          fecha_Fabricacion: '',
          fecha_Vencimiento: '',
          cantidad_Inicial: 0,
          costo_Unitario: 0,
        });
      }
    }
  }, [lote, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: LoteFormData) => {
    try {
      if (isEdit) {
        await updateLote({
          id: lote.id,
          producto_Id: data.producto_Id,
          bodega_Id: data.bodega_Id || undefined,
          codigo_Lote: data.codigo_Lote,
          fecha_Fabricacion: data.fecha_Fabricacion || undefined,
          fecha_Vencimiento: data.fecha_Vencimiento || undefined,
          cantidad_Inicial: data.cantidad_Inicial,
          cantidad_Actual: lote.cantidad_Actual,
          costo_Unitario: data.costo_Unitario,
          estado: lote.estado,
        }).unwrap();
        
        toast({
          title: 'Lote actualizado',
          description: `El lote ${data.codigo_Lote} fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createLote({
          producto_Id: data.producto_Id,
          bodega_Id: data.bodega_Id || undefined,
          codigo_Lote: data.codigo_Lote,
          fecha_Fabricacion: data.fecha_Fabricacion || undefined,
          fecha_Vencimiento: data.fecha_Vencimiento || undefined,
          cantidad_Inicial: data.cantidad_Inicial,
          costo_Unitario: data.costo_Unitario,
        }).unwrap();
        
        toast({
          title: 'Lote creado',
          description: `El lote ${data.codigo_Lote} fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar el lote';
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
            {isEdit ? 'Editar Lote' : 'Nuevo Lote'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.producto_Id}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Producto</FormLabel>
                    <Select
                      {...register('producto_Id', { valueAsNumber: true })}
                      bg={inputBg}
                      borderColor={borderColor}
                      placeholder="Seleccione un producto"
                      size={{ base: "sm", md: "md" }}
                    >
                      {productos.map((producto) => (
                        <option key={producto.id} value={producto.id}>
                          {producto.nombre} ({producto.sku})
                        </option>
                      ))}
                    </Select>
                    <FormErrorMessage>{errors.producto_Id?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.bodega_Id}>
                    <FormLabel>Bodega</FormLabel>
                    <Select
                      {...register('bodega_Id', { valueAsNumber: true })}
                      bg={inputBg}
                      borderColor={borderColor}
                      placeholder="Seleccione una bodega"
                    >
                      <option value={0}>Sin bodega</option>
                      {bodegas.map((bodega) => (
                        <option key={bodega.id} value={bodega.id}>
                          {bodega.nombre}
                        </option>
                      ))}
                    </Select>
                    <FormErrorMessage>{errors.bodega_Id?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.codigo_Lote}>
                <FormLabel>Código de Lote</FormLabel>
                <Input
                  {...register('codigo_Lote')}
                  placeholder="LOTE-2024-001"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.codigo_Lote?.message}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Fabricacion}>
                    <FormLabel>Fecha de Fabricación</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Fabricacion')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Fabricacion?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Vencimiento}>
                    <FormLabel>Fecha de Vencimiento</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Vencimiento')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Vencimiento?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.cantidad_Inicial}>
                    <FormLabel>Cantidad Inicial</FormLabel>
                    <Controller
                      name="cantidad_Inicial"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={1}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.cantidad_Inicial?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.costo_Unitario}>
                    <FormLabel>Costo Unitario ($)</FormLabel>
                    <Controller
                      name="costo_Unitario"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                          precision={2}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.costo_Unitario?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>
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
