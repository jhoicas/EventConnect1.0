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
  Select,
  NumberInput,
  NumberInputField,
  useToast,
  FormErrorMessage,
  VStack,
  Grid,
  GridItem,
  Switch,
  HStack,
} from '@chakra-ui/react';
import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateReservaMutation, useUpdateReservaMutation, type Reserva } from '../store/api/reservaApi';
import { useGetClientesQuery } from '../store/api/clienteApi';
import { reservaSchema, type ReservaFormData } from '../lib/validations/reservaSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface ReservaModalProps {
  isOpen: boolean;
  onClose: () => void;
  reserva?: Reserva;
}

export const ReservaModal = ({ isOpen, onClose, reserva }: ReservaModalProps) => {
  const toast = useToast();
  const { data: clientes = [] } = useGetClientesQuery();
  
  const [createReserva, { isLoading: isCreating }] = useCreateReservaMutation();
  const [updateReserva, { isLoading: isUpdating }] = useUpdateReservaMutation();

  const isEdit = !!reserva;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<ReservaFormData>({
    resolver: zodResolver(reservaSchema),
    defaultValues: {
      cliente_Id: 0,
      fecha_Evento: '',
      fecha_Entrega: '',
      fecha_Devolucion_Programada: '',
      direccion_Entrega: '',
      ciudad_Entrega: '',
      contacto_En_Sitio: '',
      telefono_Contacto: '',
      subtotal: 0,
      descuento: 0,
      total: 0 as number,
      fianza: 0,
      metodo_Pago: 'Efectivo',
      estado_Pago: 'Pendiente',
      observaciones: '',
    },
  });

  const subtotal = watch('subtotal');
  const descuento = watch('descuento');

  // Calcular total automáticamente cuando cambian subtotal o descuento
  useEffect(() => {
    const totalCalculado = subtotal - descuento;
    setValue('total', totalCalculado >= 0 ? totalCalculado : 0, { shouldValidate: true });
  }, [subtotal, descuento, setValue]);

  // Resetear formulario cuando cambia la reserva o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (reserva) {
        reset({
          cliente_Id: reserva.cliente_Id,
          fecha_Evento: reserva.fecha_Evento.split('T')[0],
          fecha_Entrega: reserva.fecha_Entrega?.split('T')[0] || '',
          fecha_Devolucion_Programada: reserva.fecha_Devolucion_Programada?.split('T')[0] || '',
          direccion_Entrega: reserva.direccion_Entrega || '',
          ciudad_Entrega: reserva.ciudad_Entrega || '',
          contacto_En_Sitio: reserva.contacto_En_Sitio || '',
          telefono_Contacto: reserva.telefono_Contacto || '',
          subtotal: reserva.subtotal,
          descuento: reserva.descuento,
          total: reserva.total,
          fianza: reserva.fianza,
          metodo_Pago: reserva.metodo_Pago,
          estado_Pago: reserva.estado_Pago,
          observaciones: reserva.observaciones || '',
        });
      } else {
        reset({
          cliente_Id: 0,
          fecha_Evento: '',
          fecha_Entrega: '',
          fecha_Devolucion_Programada: '',
          direccion_Entrega: '',
          ciudad_Entrega: '',
          contacto_En_Sitio: '',
          telefono_Contacto: '',
          subtotal: 0,
          descuento: 0,
          total: 0 as number,
          fianza: 0,
          metodo_Pago: 'Efectivo',
          estado_Pago: 'Pendiente',
          observaciones: '',
        });
      }
    }
  }, [reserva, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: ReservaFormData) => {
    // Calcular total antes de enviar
    const totalCalculado = data.subtotal - data.descuento;
    const totalFinal = totalCalculado >= 0 ? totalCalculado : 0;
    const dataConTotal = {
      ...data,
      total: totalFinal,
    };

    try {
      if (isEdit) {
        await updateReserva({
          id: reserva.id,
          cliente_Id: dataConTotal.cliente_Id,
          estado: reserva.estado,
          fecha_Evento: dataConTotal.fecha_Evento,
          fecha_Entrega: dataConTotal.fecha_Entrega || undefined,
          fecha_Devolucion_Programada: dataConTotal.fecha_Devolucion_Programada || undefined,
          direccion_Entrega: dataConTotal.direccion_Entrega || undefined,
          ciudad_Entrega: dataConTotal.ciudad_Entrega || undefined,
          contacto_En_Sitio: dataConTotal.contacto_En_Sitio || undefined,
          telefono_Contacto: dataConTotal.telefono_Contacto || undefined,
          subtotal: dataConTotal.subtotal,
          descuento: dataConTotal.descuento,
          total: dataConTotal.total,
          fianza: dataConTotal.fianza || 0,
          fianza_Devuelta: reserva.fianza_Devuelta,
          metodo_Pago: dataConTotal.metodo_Pago,
          estado_Pago: dataConTotal.estado_Pago,
          observaciones: dataConTotal.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Reserva actualizada',
          description: 'La reserva fue actualizada exitosamente.',
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createReserva({
          cliente_Id: dataConTotal.cliente_Id,
          fecha_Evento: dataConTotal.fecha_Evento,
          fecha_Entrega: dataConTotal.fecha_Entrega || undefined,
          fecha_Devolucion_Programada: dataConTotal.fecha_Devolucion_Programada || undefined,
          direccion_Entrega: dataConTotal.direccion_Entrega || undefined,
          ciudad_Entrega: dataConTotal.ciudad_Entrega || undefined,
          contacto_En_Sitio: dataConTotal.contacto_En_Sitio || undefined,
          telefono_Contacto: dataConTotal.telefono_Contacto || undefined,
          subtotal: dataConTotal.subtotal,
          descuento: dataConTotal.descuento || 0,
          total: dataConTotal.total,
          fianza: dataConTotal.fianza || 0,
          metodo_Pago: dataConTotal.metodo_Pago,
          observaciones: dataConTotal.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Reserva creada',
          description: 'La reserva fue creada exitosamente.',
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar la reserva';
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
    <Modal isOpen={isOpen} onClose={handleClose} size="3xl">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent bg={bgColor} borderColor={borderColor} borderWidth="1px" maxH="90vh" overflow="auto">
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalHeader>{isEdit ? 'Editar Reserva' : 'Nueva Reserva'}</ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <FormControl isRequired isInvalid={!!errors.cliente_Id}>
                <FormLabel>Cliente</FormLabel>
                <Select
                  {...register('cliente_Id', { valueAsNumber: true })}
                  bg={inputBg}
                  borderColor={borderColor}
                  placeholder="Seleccione un cliente"
                >
                  {clientes.map((cliente) => (
                    <option key={cliente.id} value={cliente.id}>
                      {cliente.nombre} - {cliente.documento}
                    </option>
                  ))}
                </Select>
                <FormErrorMessage>{errors.cliente_Id?.message}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.fecha_Evento}>
                    <FormLabel>Fecha del Evento</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Evento')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Evento?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Entrega}>
                    <FormLabel>Fecha de Entrega</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Entrega')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Entrega?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Devolucion_Programada}>
                    <FormLabel>Fecha Devolución</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Devolucion_Programada')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Devolucion_Programada?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.ciudad_Entrega}>
                    <FormLabel>Ciudad de Entrega</FormLabel>
                    <Input
                      {...register('ciudad_Entrega')}
                      placeholder="Bogotá"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.ciudad_Entrega?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.direccion_Entrega}>
                    <FormLabel>Dirección de Entrega</FormLabel>
                    <Input
                      {...register('direccion_Entrega')}
                      placeholder="Calle 123 #45-67"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.direccion_Entrega?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.contacto_En_Sitio}>
                    <FormLabel>Contacto en Sitio</FormLabel>
                    <Input
                      {...register('contacto_En_Sitio')}
                      placeholder="Nombre del contacto"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.contacto_En_Sitio?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.telefono_Contacto}>
                    <FormLabel>Teléfono de Contacto</FormLabel>
                    <Input
                      {...register('telefono_Contacto')}
                      placeholder="300 123 4567"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.telefono_Contacto?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(4, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.subtotal}>
                    <FormLabel>Subtotal ($)</FormLabel>
                    <Controller
                      name="subtotal"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.subtotal?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.descuento}>
                    <FormLabel>Descuento ($)</FormLabel>
                    <Controller
                      name="descuento"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.descuento?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.total}>
                    <FormLabel>Total ($)</FormLabel>
                    <Controller
                      name="total"
                      control={control}
                      render={({ field: { value } }) => (
                        <NumberInput value={value} isReadOnly>
                          <NumberInputField bg={inputBg} borderColor={borderColor} fontWeight="bold" />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.total?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.fianza}>
                    <FormLabel>Fianza ($)</FormLabel>
                    <Controller
                      name="fianza"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.fianza?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.metodo_Pago}>
                    <FormLabel>Método de Pago</FormLabel>
                    <Select
                      {...register('metodo_Pago')}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Efectivo">Efectivo</option>
                      <option value="Transferencia">Transferencia</option>
                      <option value="Tarjeta">Tarjeta</option>
                      <option value="Cheque">Cheque</option>
                    </Select>
                    <FormErrorMessage>{errors.metodo_Pago?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.estado_Pago}>
                    <FormLabel>Estado de Pago</FormLabel>
                    <Select
                      {...register('estado_Pago')}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Pendiente">Pendiente</option>
                      <option value="Parcial">Parcial</option>
                      <option value="Pagado">Pagado</option>
                    </Select>
                    <FormErrorMessage>{errors.estado_Pago?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isInvalid={!!errors.observaciones}>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  {...register('observaciones')}
                  placeholder="Notas adicionales sobre la reserva"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
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
