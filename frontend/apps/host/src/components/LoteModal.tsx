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
import { useState, useEffect } from 'react';
import { useCreateLoteMutation, useUpdateLoteMutation, type Lote } from '../store/api/loteApi';
import { useGetProductosQuery } from '../store/api/productoApi';
import { useGetBodegasQuery } from '../store/api/bodegaApi';

interface LoteModalProps {
  isOpen: boolean;
  onClose: () => void;
  lote?: Lote;
}

export const LoteModal = ({ isOpen, onClose, lote }: LoteModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const { data: productos = [] } = useGetProductosQuery();
  const { data: bodegas = [] } = useGetBodegasQuery();
  
  const [formData, setFormData] = useState({
    producto_Id: 0,
    bodega_Id: 0,
    codigo_Lote: '',
    fecha_Fabricacion: '',
    fecha_Vencimiento: '',
    cantidad_Inicial: 0,
    costo_Unitario: 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createLote, { isLoading: isCreating }] = useCreateLoteMutation();
  const [updateLote, { isLoading: isUpdating }] = useUpdateLoteMutation();

  const isEdit = !!lote;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (lote) {
      setFormData({
        producto_Id: lote.producto_Id,
        bodega_Id: lote.bodega_Id || 0,
        codigo_Lote: lote.codigo_Lote,
        fecha_Fabricacion: lote.fecha_Fabricacion?.split('T')[0] || '',
        fecha_Vencimiento: lote.fecha_Vencimiento?.split('T')[0] || '',
        cantidad_Inicial: lote.cantidad_Inicial,
        costo_Unitario: lote.costo_Unitario,
      });
    } else {
      setFormData({
        producto_Id: 0,
        bodega_Id: 0,
        codigo_Lote: '',
        fecha_Fabricacion: '',
        fecha_Vencimiento: '',
        cantidad_Inicial: 0,
        costo_Unitario: 0,
      });
    }
    setErrors({});
  }, [lote, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.producto_Id) newErrors.producto_Id = 'Debe seleccionar un producto';
    if (!formData.codigo_Lote.trim()) newErrors.codigo_Lote = 'El código de lote es requerido';
    if (formData.cantidad_Inicial <= 0) newErrors.cantidad_Inicial = 'La cantidad debe ser mayor a 0';
    if (formData.costo_Unitario <= 0) newErrors.costo_Unitario = 'El costo debe ser mayor a 0';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateLote({
          id: lote.id,
          producto_Id: formData.producto_Id,
          bodega_Id: formData.bodega_Id || undefined,
          codigo_Lote: formData.codigo_Lote,
          fecha_Fabricacion: formData.fecha_Fabricacion || undefined,
          fecha_Vencimiento: formData.fecha_Vencimiento || undefined,
          cantidad_Inicial: formData.cantidad_Inicial,
          cantidad_Actual: lote.cantidad_Actual,
          costo_Unitario: formData.costo_Unitario,
          estado: lote.estado,
        }).unwrap();
        
        toast({
          title: 'Lote actualizado',
          description: `El lote ${formData.codigo_Lote} fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createLote({
          producto_Id: formData.producto_Id,
          bodega_Id: formData.bodega_Id || undefined,
          codigo_Lote: formData.codigo_Lote,
          fecha_Fabricacion: formData.fecha_Fabricacion || undefined,
          fecha_Vencimiento: formData.fecha_Vencimiento || undefined,
          cantidad_Inicial: formData.cantidad_Inicial,
          costo_Unitario: formData.costo_Unitario,
        }).unwrap();
        
        toast({
          title: 'Lote creado',
          description: `El lote ${formData.codigo_Lote} fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar el lote',
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
        <form onSubmit={handleSubmit}>
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
                      value={formData.producto_Id}
                      onChange={(e) => setFormData({ ...formData, producto_Id: Number(e.target.value) })}
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
                    <FormErrorMessage>{errors.producto_Id}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Bodega</FormLabel>
                    <Select
                      value={formData.bodega_Id}
                      onChange={(e) => setFormData({ ...formData, bodega_Id: Number(e.target.value) })}
                      bg={inputBg}
                      borderColor={borderColor}
                      placeholder="Seleccione una bodega"
                    >
                      {bodegas.map((bodega) => (
                        <option key={bodega.id} value={bodega.id}>
                          {bodega.nombre}
                        </option>
                      ))}
                    </Select>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.codigo_Lote}>
                <FormLabel>Código de Lote</FormLabel>
                <Input
                  value={formData.codigo_Lote}
                  onChange={(e) => setFormData({ ...formData, codigo_Lote: e.target.value })}
                  placeholder="LOTE-2024-001"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.codigo_Lote}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha de Fabricación</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Fabricacion}
                      onChange={(e) => setFormData({ ...formData, fecha_Fabricacion: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha de Vencimiento</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Vencimiento}
                      onChange={(e) => setFormData({ ...formData, fecha_Vencimiento: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.cantidad_Inicial}>
                    <FormLabel>Cantidad Inicial</FormLabel>
                    <NumberInput
                      value={formData.cantidad_Inicial}
                      onChange={(_, val) => setFormData({ ...formData, cantidad_Inicial: val })}
                      min={1}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                    <FormErrorMessage>{errors.cantidad_Inicial}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.costo_Unitario}>
                    <FormLabel>Costo Unitario ($)</FormLabel>
                    <NumberInput
                      value={formData.costo_Unitario}
                      onChange={(_, val) => setFormData({ ...formData, costo_Unitario: val })}
                      min={0}
                      precision={2}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                    <FormErrorMessage>{errors.costo_Unitario}</FormErrorMessage>
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
