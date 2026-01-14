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
  Textarea,
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
import { useCreateMantenimientoMutation, useUpdateMantenimientoMutation, type Mantenimiento } from '../store/api/mantenimientoApi';
import { useGetActivosQuery } from '../store/api/activoApi';
import { mantenimientoSchema, type MantenimientoFormData } from '../lib/validations/mantenimientoSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface MantenimientoModalProps {
  isOpen: boolean;
  onClose: () => void;
  mantenimiento?: Mantenimiento;
}

export const MantenimientoModal = ({ isOpen, onClose, mantenimiento }: MantenimientoModalProps) => {
  const toast = useToast();
  const { data: activos = [] } = useGetActivosQuery();
  
  const [createMantenimiento, { isLoading: isCreating }] = useCreateMantenimientoMutation();
  const [updateMantenimiento, { isLoading: isUpdating }] = useUpdateMantenimientoMutation();

  const isEdit = !!mantenimiento;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
  } = useForm<MantenimientoFormData>({
    resolver: zodResolver(mantenimientoSchema),
    defaultValues: {
      activo_Id: 0,
      tipo_Mantenimiento: 'Preventivo',
      fecha_Programada: '',
      fecha_Realizada: '',
      descripcion: '',
      proveedor_Servicio: '',
      costo: 0,
      observaciones: '',
    },
  });

  // Resetear formulario cuando cambia el mantenimiento o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (mantenimiento) {
        reset({
          activo_Id: mantenimiento.activo_Id,
          tipo_Mantenimiento: mantenimiento.tipo_Mantenimiento,
          fecha_Programada: mantenimiento.fecha_Programada?.split('T')[0] || '',
          fecha_Realizada: mantenimiento.fecha_Realizada?.split('T')[0] || '',
          descripcion: mantenimiento.descripcion || '',
          proveedor_Servicio: mantenimiento.proveedor_Servicio || '',
          costo: mantenimiento.costo || 0,
          observaciones: mantenimiento.observaciones || '',
        });
      } else {
        reset({
          activo_Id: 0,
          tipo_Mantenimiento: 'Preventivo',
          fecha_Programada: '',
          fecha_Realizada: '',
          descripcion: '',
          proveedor_Servicio: '',
          costo: 0,
          observaciones: '',
        });
      }
    }
  }, [mantenimiento, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: MantenimientoFormData) => {
    try {
      if (isEdit) {
        await updateMantenimiento({
          id: mantenimiento.id,
          activo_Id: data.activo_Id,
          tipo_Mantenimiento: data.tipo_Mantenimiento,
          fecha_Programada: data.fecha_Programada || undefined,
          fecha_Realizada: data.fecha_Realizada || undefined,
          descripcion: data.descripcion || undefined,
          proveedor_Servicio: data.proveedor_Servicio || undefined,
          costo: data.costo || undefined,
          estado: mantenimiento.estado,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Mantenimiento actualizado',
          description: 'El mantenimiento fue actualizado exitosamente.',
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createMantenimiento({
          activo_Id: data.activo_Id,
          tipo_Mantenimiento: data.tipo_Mantenimiento,
          fecha_Programada: data.fecha_Programada || undefined,
          descripcion: data.descripcion || undefined,
          proveedor_Servicio: data.proveedor_Servicio || undefined,
          costo: data.costo || undefined,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Mantenimiento creado',
          description: 'El mantenimiento fue programado exitosamente.',
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar el mantenimiento';
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
    <Modal isOpen={isOpen} onClose={handleClose} size="2xl">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent bg={bgColor} borderColor={borderColor} borderWidth="1px">
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalHeader>{isEdit ? 'Editar Mantenimiento' : 'Nuevo Mantenimiento'}</ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.activo_Id}>
                    <FormLabel>Activo</FormLabel>
                    <Select
                      {...register('activo_Id', { valueAsNumber: true })}
                      bg={inputBg}
                      borderColor={borderColor}
                      placeholder="Seleccione un activo"
                    >
                      {activos.map((activo) => (
                        <option key={activo.id} value={activo.id}>
                          {activo.nombre} ({activo.codigo_Activo})
                        </option>
                      ))}
                    </Select>
                    <FormErrorMessage>{errors.activo_Id?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.tipo_Mantenimiento}>
                    <FormLabel>Tipo de Mantenimiento</FormLabel>
                    <Select
                      {...register('tipo_Mantenimiento')}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Preventivo">Preventivo</option>
                      <option value="Correctivo">Correctivo</option>
                      <option value="Predictivo">Predictivo</option>
                    </Select>
                    <FormErrorMessage>{errors.tipo_Mantenimiento?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Programada}>
                    <FormLabel>Fecha Programada</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Programada')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Programada?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                {isEdit && (
                  <GridItem>
                    <FormControl isInvalid={!!errors.fecha_Realizada}>
                      <FormLabel>Fecha Realizada</FormLabel>
                      <Input
                        type="date"
                        {...register('fecha_Realizada')}
                        bg={inputBg}
                        borderColor={borderColor}
                      />
                      <FormErrorMessage>{errors.fecha_Realizada?.message}</FormErrorMessage>
                    </FormControl>
                  </GridItem>
                )}
              </Grid>

              <FormControl isInvalid={!!errors.descripcion}>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  {...register('descripcion')}
                  placeholder="Detalle del mantenimiento a realizar"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
                <FormErrorMessage>{errors.descripcion?.message}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.proveedor_Servicio}>
                    <FormLabel>Proveedor de Servicio</FormLabel>
                    <Input
                      {...register('proveedor_Servicio')}
                      placeholder="Empresa o técnico"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.proveedor_Servicio?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.costo}>
                    <FormLabel>Costo ($)</FormLabel>
                    <Controller
                      name="costo"
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
                    <FormErrorMessage>{errors.costo?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isInvalid={!!errors.observaciones}>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  {...register('observaciones')}
                  placeholder="Notas adicionales"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={2}
                />
                <FormErrorMessage>{errors.observaciones?.message}</FormErrorMessage>
              </FormControl>
            </VStack>
          </ModalBody>

          <ModalFooter>
            <Button
              variant="ghost"
              mr={3}
              onClick={handleClose}
              isDisabled={isLoading}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              colorScheme="blue"
              isLoading={isLoading}
              loadingText={isEdit ? 'Actualizando...' : 'Creando...'}
            >
              {isEdit ? 'Actualizar' : 'Crear'}
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
};
