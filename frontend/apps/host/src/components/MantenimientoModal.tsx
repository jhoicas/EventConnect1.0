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
import { useState, useEffect } from 'react';
import { useCreateMantenimientoMutation, useUpdateMantenimientoMutation, type Mantenimiento } from '../store/api/mantenimientoApi';
import { useGetActivosQuery } from '../store/api/activoApi';

interface MantenimientoModalProps {
  isOpen: boolean;
  onClose: () => void;
  mantenimiento?: Mantenimiento;
}

export const MantenimientoModal = ({ isOpen, onClose, mantenimiento }: MantenimientoModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const { data: activos = [] } = useGetActivosQuery();
  
  const [formData, setFormData] = useState({
    activo_Id: 0,
    tipo_Mantenimiento: 'Preventivo',
    fecha_Programada: '',
    fecha_Realizada: '',
    descripcion: '',
    proveedor_Servicio: '',
    costo: 0,
    observaciones: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createMantenimiento, { isLoading: isCreating }] = useCreateMantenimientoMutation();
  const [updateMantenimiento, { isLoading: isUpdating }] = useUpdateMantenimientoMutation();

  const isEdit = !!mantenimiento;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (mantenimiento) {
      setFormData({
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
      setFormData({
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
    setErrors({});
  }, [mantenimiento, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.activo_Id) newErrors.activo_Id = 'Debe seleccionar un activo';
    if (!formData.tipo_Mantenimiento) newErrors.tipo_Mantenimiento = 'Debe seleccionar un tipo';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateMantenimiento({
          id: mantenimiento.id,
          activo_Id: formData.activo_Id,
          tipo_Mantenimiento: formData.tipo_Mantenimiento,
          fecha_Programada: formData.fecha_Programada || undefined,
          fecha_Realizada: formData.fecha_Realizada || undefined,
          descripcion: formData.descripcion || undefined,
          proveedor_Servicio: formData.proveedor_Servicio || undefined,
          costo: formData.costo || undefined,
          estado: mantenimiento.estado,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Mantenimiento actualizado',
          description: `El mantenimiento fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createMantenimiento({
          activo_Id: formData.activo_Id,
          tipo_Mantenimiento: formData.tipo_Mantenimiento,
          fecha_Programada: formData.fecha_Programada || undefined,
          descripcion: formData.descripcion || undefined,
          proveedor_Servicio: formData.proveedor_Servicio || undefined,
          costo: formData.costo || undefined,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Mantenimiento creado',
          description: `El mantenimiento fue programado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar el mantenimiento',
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
    <Modal isOpen={isOpen} onClose={handleClose} size="2xl">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent bg={bgColor} borderColor={borderColor} borderWidth="1px">
        <form onSubmit={handleSubmit}>
          <ModalHeader>{isEdit ? 'Editar Mantenimiento' : 'Nuevo Mantenimiento'}</ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.activo_Id}>
                    <FormLabel>Activo</FormLabel>
                    <Select
                      value={formData.activo_Id}
                      onChange={(e) => setFormData({ ...formData, activo_Id: Number(e.target.value) })}
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
                    <FormErrorMessage>{errors.activo_Id}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.tipo_Mantenimiento}>
                    <FormLabel>Tipo de Mantenimiento</FormLabel>
                    <Select
                      value={formData.tipo_Mantenimiento}
                      onChange={(e) => setFormData({ ...formData, tipo_Mantenimiento: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Preventivo">Preventivo</option>
                      <option value="Correctivo">Correctivo</option>
                      <option value="Predictivo">Predictivo</option>
                    </Select>
                    <FormErrorMessage>{errors.tipo_Mantenimiento}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha Programada</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Programada}
                      onChange={(e) => setFormData({ ...formData, fecha_Programada: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                {isEdit && (
                  <GridItem>
                    <FormControl>
                      <FormLabel>Fecha Realizada</FormLabel>
                      <Input
                        type="date"
                        value={formData.fecha_Realizada}
                        onChange={(e) => setFormData({ ...formData, fecha_Realizada: e.target.value })}
                        bg={inputBg}
                        borderColor={borderColor}
                      />
                    </FormControl>
                  </GridItem>
                )}
              </Grid>

              <FormControl>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  value={formData.descripcion}
                  onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                  placeholder="Detalle del mantenimiento a realizar"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
              </FormControl>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Proveedor de Servicio</FormLabel>
                    <Input
                      value={formData.proveedor_Servicio}
                      onChange={(e) => setFormData({ ...formData, proveedor_Servicio: e.target.value })}
                      placeholder="Empresa o técnico"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Costo ($)</FormLabel>
                    <NumberInput
                      value={formData.costo}
                      onChange={(_, val) => setFormData({ ...formData, costo: val })}
                      min={0}
                      precision={2}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  value={formData.observaciones}
                  onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
                  placeholder="Notas adicionales"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={2}
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
