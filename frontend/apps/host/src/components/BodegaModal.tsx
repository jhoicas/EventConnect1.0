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
import { useState, useEffect } from 'react';
import { useCreateBodegaMutation, useUpdateBodegaMutation, type Bodega } from '../store/api/bodegaApi';

interface BodegaModalProps {
  isOpen: boolean;
  onClose: () => void;
  bodega?: Bodega;
}

export const BodegaModal = ({ isOpen, onClose, bodega }: BodegaModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const [formData, setFormData] = useState({
    codigo_Bodega: '',
    nombre: '',
    direccion: '',
    ciudad: '',
    telefono: '',
    capacidad_M3: 0,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createBodega, { isLoading: isCreating }] = useCreateBodegaMutation();
  const [updateBodega, { isLoading: isUpdating }] = useUpdateBodegaMutation();

  const isEdit = !!bodega;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (bodega) {
      setFormData({
        codigo_Bodega: bodega.codigo_Bodega,
        nombre: bodega.nombre,
        direccion: bodega.direccion || '',
        ciudad: bodega.ciudad || '',
        telefono: bodega.telefono || '',
        capacidad_M3: bodega.capacidad_M3 || 0,
      });
    } else {
      setFormData({
        codigo_Bodega: '',
        nombre: '',
        direccion: '',
        ciudad: '',
        telefono: '',
        capacidad_M3: 0,
      });
    }
    setErrors({});
  }, [bodega, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.codigo_Bodega.trim()) newErrors.codigo_Bodega = 'El código es requerido';
    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es requerido';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateBodega({
          id: bodega.id,
          codigo_Bodega: formData.codigo_Bodega,
          nombre: formData.nombre,
          direccion: formData.direccion || undefined,
          ciudad: formData.ciudad || undefined,
          telefono: formData.telefono || undefined,
          capacidad_M3: formData.capacidad_M3 || undefined,
          estado: bodega.estado,
        }).unwrap();
        
        toast({
          title: 'Bodega actualizada',
          description: `La bodega ${formData.nombre} fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createBodega({
          codigo_Bodega: formData.codigo_Bodega,
          nombre: formData.nombre,
          direccion: formData.direccion || undefined,
          ciudad: formData.ciudad || undefined,
          telefono: formData.telefono || undefined,
          capacidad_M3: formData.capacidad_M3 || undefined,
        }).unwrap();
        
        toast({
          title: 'Bodega creada',
          description: `La bodega ${formData.nombre} fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar la bodega',
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
                      value={formData.codigo_Bodega}
                      onChange={(e) => setFormData({ ...formData, codigo_Bodega: e.target.value })}
                      placeholder="BOD-001"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.codigo_Bodega}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.nombre}>
                    <FormLabel>Nombre</FormLabel>
                    <Input
                      value={formData.nombre}
                      onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                      placeholder="Bodega Principal"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.nombre}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Ciudad</FormLabel>
                    <Input
                      value={formData.ciudad}
                      onChange={(e) => setFormData({ ...formData, ciudad: e.target.value })}
                      placeholder="Bogotá"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Teléfono</FormLabel>
                    <Input
                      value={formData.telefono}
                      onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                      placeholder="601 234 5678"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl>
                <FormLabel>Dirección</FormLabel>
                <Input
                  value={formData.direccion}
                  onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
                  placeholder="Calle 123 #45-67, Zona Industrial"
                  bg={inputBg}
                  borderColor={borderColor}
                />
              </FormControl>

              <FormControl>
                <FormLabel>Capacidad (m³)</FormLabel>
                <NumberInput
                  value={formData.capacidad_M3}
                  onChange={(_, val) => setFormData({ ...formData, capacidad_M3: val })}
                  min={0}
                  precision={2}
                >
                  <NumberInputField bg={inputBg} borderColor={borderColor} placeholder="500.00" />
                </NumberInput>
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
