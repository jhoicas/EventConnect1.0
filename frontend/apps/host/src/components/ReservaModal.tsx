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
import { useState, useEffect } from 'react';
import { useCreateReservaMutation, useUpdateReservaMutation, type Reserva } from '../store/api/reservaApi';
import { useGetClientesQuery } from '../store/api/clienteApi';

interface ReservaModalProps {
  isOpen: boolean;
  onClose: () => void;
  reserva?: Reserva;
}

export const ReservaModal = ({ isOpen, onClose, reserva }: ReservaModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const { data: clientes = [] } = useGetClientesQuery();
  
  const [formData, setFormData] = useState({
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
    total: 0,
    fianza: 0,
    metodo_Pago: 'Efectivo',
    estado_Pago: 'Pendiente',
    observaciones: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createReserva, { isLoading: isCreating }] = useCreateReservaMutation();
  const [updateReserva, { isLoading: isUpdating }] = useUpdateReservaMutation();

  const isEdit = !!reserva;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (reserva) {
      setFormData({
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
      setFormData({
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
        total: 0,
        fianza: 0,
        metodo_Pago: 'Efectivo',
        estado_Pago: 'Pendiente',
        observaciones: '',
      });
    }
    setErrors({});
  }, [reserva, isOpen]);

  useEffect(() => {
    // Calcular total automáticamente
    const totalCalculado = formData.subtotal - formData.descuento;
    if (totalCalculado !== formData.total) {
      setFormData(prev => ({ ...prev, total: totalCalculado }));
    }
  }, [formData.subtotal, formData.descuento]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.cliente_Id) newErrors.cliente_Id = 'Debe seleccionar un cliente';
    if (!formData.fecha_Evento) newErrors.fecha_Evento = 'La fecha del evento es requerida';
    if (formData.subtotal <= 0) newErrors.subtotal = 'El subtotal debe ser mayor a 0';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateReserva({
          id: reserva.id,
          cliente_Id: formData.cliente_Id,
          estado: reserva.estado,
          fecha_Evento: formData.fecha_Evento,
          fecha_Entrega: formData.fecha_Entrega || undefined,
          fecha_Devolucion_Programada: formData.fecha_Devolucion_Programada || undefined,
          direccion_Entrega: formData.direccion_Entrega || undefined,
          ciudad_Entrega: formData.ciudad_Entrega || undefined,
          contacto_En_Sitio: formData.contacto_En_Sitio || undefined,
          telefono_Contacto: formData.telefono_Contacto || undefined,
          subtotal: formData.subtotal,
          descuento: formData.descuento,
          total: formData.total,
          fianza: formData.fianza,
          fianza_Devuelta: reserva.fianza_Devuelta,
          metodo_Pago: formData.metodo_Pago,
          estado_Pago: formData.estado_Pago,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Reserva actualizada',
          description: `La reserva fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createReserva({
          cliente_Id: formData.cliente_Id,
          fecha_Evento: formData.fecha_Evento,
          fecha_Entrega: formData.fecha_Entrega || undefined,
          fecha_Devolucion_Programada: formData.fecha_Devolucion_Programada || undefined,
          direccion_Entrega: formData.direccion_Entrega || undefined,
          ciudad_Entrega: formData.ciudad_Entrega || undefined,
          contacto_En_Sitio: formData.contacto_En_Sitio || undefined,
          telefono_Contacto: formData.telefono_Contacto || undefined,
          subtotal: formData.subtotal,
          descuento: formData.descuento || 0,
          total: formData.total,
          fianza: formData.fianza || 0,
          metodo_Pago: formData.metodo_Pago,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Reserva creada',
          description: `La reserva fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar la reserva',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleClose = () => {
    onClose();
  };

  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';
  const inputBg = colorMode === 'dark' ? '#242b3d' : colorMode === 'blue' ? '#1e3140' : '#f5f6f8';
  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size="3xl">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent bg={bgColor} borderColor={borderColor} borderWidth="1px" maxH="90vh" overflow="auto">
        <form onSubmit={handleSubmit}>
          <ModalHeader>{isEdit ? 'Editar Reserva' : 'Nueva Reserva'}</ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <FormControl isRequired isInvalid={!!errors.cliente_Id}>
                <FormLabel>Cliente</FormLabel>
                <Select
                  value={formData.cliente_Id}
                  onChange={(e) => setFormData({ ...formData, cliente_Id: Number(e.target.value) })}
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
                <FormErrorMessage>{errors.cliente_Id}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.fecha_Evento}>
                    <FormLabel>Fecha del Evento</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Evento}
                      onChange={(e) => setFormData({ ...formData, fecha_Evento: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Evento}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha de Entrega</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Entrega}
                      onChange={(e) => setFormData({ ...formData, fecha_Entrega: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha Devolución</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Devolucion_Programada}
                      onChange={(e) => setFormData({ ...formData, fecha_Devolucion_Programada: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Ciudad de Entrega</FormLabel>
                    <Input
                      value={formData.ciudad_Entrega}
                      onChange={(e) => setFormData({ ...formData, ciudad_Entrega: e.target.value })}
                      placeholder="Bogotá"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Dirección de Entrega</FormLabel>
                    <Input
                      value={formData.direccion_Entrega}
                      onChange={(e) => setFormData({ ...formData, direccion_Entrega: e.target.value })}
                      placeholder="Calle 123 #45-67"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Contacto en Sitio</FormLabel>
                    <Input
                      value={formData.contacto_En_Sitio}
                      onChange={(e) => setFormData({ ...formData, contacto_En_Sitio: e.target.value })}
                      placeholder="Nombre del contacto"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Teléfono de Contacto</FormLabel>
                    <Input
                      value={formData.telefono_Contacto}
                      onChange={(e) => setFormData({ ...formData, telefono_Contacto: e.target.value })}
                      placeholder="300 123 4567"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(4, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.subtotal}>
                    <FormLabel>Subtotal ($)</FormLabel>
                    <NumberInput
                      value={formData.subtotal}
                      onChange={(_, val) => setFormData({ ...formData, subtotal: val })}
                      min={0}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                    <FormErrorMessage>{errors.subtotal}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Descuento ($)</FormLabel>
                    <NumberInput
                      value={formData.descuento}
                      onChange={(_, val) => setFormData({ ...formData, descuento: val })}
                      min={0}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Total ($)</FormLabel>
                    <NumberInput value={formData.total} isReadOnly>
                      <NumberInputField bg={inputBg} borderColor={borderColor} fontWeight="bold" />
                    </NumberInput>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Fianza ($)</FormLabel>
                    <NumberInput
                      value={formData.fianza}
                      onChange={(_, val) => setFormData({ ...formData, fianza: val })}
                      min={0}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Método de Pago</FormLabel>
                    <Select
                      value={formData.metodo_Pago}
                      onChange={(e) => setFormData({ ...formData, metodo_Pago: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Efectivo">Efectivo</option>
                      <option value="Transferencia">Transferencia</option>
                      <option value="Tarjeta">Tarjeta</option>
                      <option value="Cheque">Cheque</option>
                    </Select>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Estado de Pago</FormLabel>
                    <Select
                      value={formData.estado_Pago}
                      onChange={(e) => setFormData({ ...formData, estado_Pago: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Pendiente">Pendiente</option>
                      <option value="Parcial">Parcial</option>
                      <option value="Pagado">Pagado</option>
                    </Select>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  value={formData.observaciones}
                  onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
                  placeholder="Notas adicionales sobre la reserva"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
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
