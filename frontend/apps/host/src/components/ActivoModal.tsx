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
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import { useCreateActivoMutation, useUpdateActivoMutation, type Activo } from '../store/api/activoApi';
import { useGetCategoriasQuery } from '../store/api/categoriaApi';

interface ActivoModalProps {
  isOpen: boolean;
  onClose: () => void;
  activo?: Activo;
}

export const ActivoModal = ({ isOpen, onClose, activo }: ActivoModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const { data: categorias = [] } = useGetCategoriasQuery();
  
  const [formData, setFormData] = useState({
    categoria_Id: 0,
    codigo_Activo: '',
    nombre: '',
    descripcion: '',
    marca: '',
    modelo: '',
    numero_Serie: '',
    fecha_Adquisicion: '',
    valor_Adquisicion: 0,
    vida_Util_Meses: 0,
    ubicacion_Fisica: '',
    imagen_URL: '',
    observaciones: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createActivo, { isLoading: isCreating }] = useCreateActivoMutation();
  const [updateActivo, { isLoading: isUpdating }] = useUpdateActivoMutation();

  const isEdit = !!activo;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (activo) {
      setFormData({
        categoria_Id: activo.categoria_Id,
        codigo_Activo: activo.codigo_Activo,
        nombre: activo.nombre,
        descripcion: activo.descripcion || '',
        marca: activo.marca || '',
        modelo: activo.modelo || '',
        numero_Serie: activo.numero_Serie || '',
        fecha_Adquisicion: activo.fecha_Adquisicion?.split('T')[0] || '',
        valor_Adquisicion: activo.valor_Adquisicion || 0,
        vida_Util_Meses: activo.vida_Util_Meses || 0,
        ubicacion_Fisica: activo.ubicacion_Fisica || '',
        imagen_URL: activo.imagen_URL || '',
        observaciones: activo.observaciones || '',
      });
    } else {
      setFormData({
        categoria_Id: 0,
        codigo_Activo: '',
        nombre: '',
        descripcion: '',
        marca: '',
        modelo: '',
        numero_Serie: '',
        fecha_Adquisicion: '',
        valor_Adquisicion: 0,
        vida_Util_Meses: 0,
        ubicacion_Fisica: '',
        imagen_URL: '',
        observaciones: '',
      });
    }
    setErrors({});
  }, [activo, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.categoria_Id) newErrors.categoria_Id = 'Debe seleccionar una categoría';
    if (!formData.codigo_Activo.trim()) newErrors.codigo_Activo = 'El código es requerido';
    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es requerido';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateActivo({
          id: activo.id,
          categoria_Id: formData.categoria_Id,
          codigo_Activo: formData.codigo_Activo,
          nombre: formData.nombre,
          descripcion: formData.descripcion || undefined,
          marca: formData.marca || undefined,
          modelo: formData.modelo || undefined,
          numero_Serie: formData.numero_Serie || undefined,
          estado: activo.estado,
          fecha_Adquisicion: formData.fecha_Adquisicion || undefined,
          valor_Adquisicion: formData.valor_Adquisicion || undefined,
          vida_Util_Meses: formData.vida_Util_Meses || undefined,
          ubicacion_Fisica: formData.ubicacion_Fisica || undefined,
          imagen_URL: formData.imagen_URL || undefined,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Activo actualizado',
          description: `El activo ${formData.codigo_Activo} fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createActivo({
          categoria_Id: formData.categoria_Id,
          codigo_Activo: formData.codigo_Activo,
          nombre: formData.nombre,
          descripcion: formData.descripcion || undefined,
          marca: formData.marca || undefined,
          modelo: formData.modelo || undefined,
          numero_Serie: formData.numero_Serie || undefined,
          fecha_Adquisicion: formData.fecha_Adquisicion || undefined,
          valor_Adquisicion: formData.valor_Adquisicion || undefined,
          vida_Util_Meses: formData.vida_Util_Meses || undefined,
          ubicacion_Fisica: formData.ubicacion_Fisica || undefined,
          imagen_URL: formData.imagen_URL || undefined,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Activo creado',
          description: `El activo ${formData.codigo_Activo} fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar el activo',
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
      size={{ base: "full", md: "3xl" }}
      scrollBehavior="inside"
    >
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent 
        bg={bgColor} 
        borderColor={borderColor} 
        borderWidth="1px" 
        maxH={{ base: "100vh", md: "90vh" }} 
        m={{ base: 0, md: 4 }}
        overflow="auto"
      >
        <form onSubmit={handleSubmit}>
          <ModalHeader fontSize={{ base: "lg", md: "xl" }}>
            {isEdit ? 'Editar Activo' : 'Nuevo Activo'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.categoria_Id}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Categoría</FormLabel>
                    <Select
                      value={formData.categoria_Id}
                      onChange={(e) => setFormData({ ...formData, categoria_Id: Number(e.target.value) })}
                      bg={inputBg}
                      borderColor={borderColor}
                      placeholder="Seleccione una categoría"
                      size={{ base: "sm", md: "md" }}
                    >
                      {categorias.map((categoria) => (
                        <option key={categoria.id} value={categoria.id}>
                          {categoria.nombre}
                        </option>
                      ))}
                    </Select>
                    <FormErrorMessage>{errors.categoria_Id}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.codigo_Activo}>
                    <FormLabel>Código del Activo</FormLabel>
                    <Input
                      value={formData.codigo_Activo}
                      onChange={(e) => setFormData({ ...formData, codigo_Activo: e.target.value })}
                      placeholder="ACT-001"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.codigo_Activo}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel>Nombre del Activo</FormLabel>
                <Input
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                  placeholder="Proyector LED 4K"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.nombre}</FormErrorMessage>
              </FormControl>

              <FormControl>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  value={formData.descripcion}
                  onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                  placeholder="Descripción detallada del activo"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
              </FormControl>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Marca</FormLabel>
                    <Input
                      value={formData.marca}
                      onChange={(e) => setFormData({ ...formData, marca: e.target.value })}
                      placeholder="Sony"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Modelo</FormLabel>
                    <Input
                      value={formData.modelo}
                      onChange={(e) => setFormData({ ...formData, modelo: e.target.value })}
                      placeholder="VPL-VW5000ES"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Número de Serie</FormLabel>
                    <Input
                      value={formData.numero_Serie}
                      onChange={(e) => setFormData({ ...formData, numero_Serie: e.target.value })}
                      placeholder="SN123456789"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Fecha de Adquisición</FormLabel>
                    <Input
                      type="date"
                      value={formData.fecha_Adquisicion}
                      onChange={(e) => setFormData({ ...formData, fecha_Adquisicion: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Valor de Adquisición ($)</FormLabel>
                    <NumberInput
                      value={formData.valor_Adquisicion}
                      onChange={(_, val) => setFormData({ ...formData, valor_Adquisicion: val })}
                      min={0}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Vida Útil (meses)</FormLabel>
                    <NumberInput
                      value={formData.vida_Util_Meses}
                      onChange={(_, val) => setFormData({ ...formData, vida_Util_Meses: val })}
                      min={0}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl>
                <FormLabel>Ubicación Física</FormLabel>
                <Input
                  value={formData.ubicacion_Fisica}
                  onChange={(e) => setFormData({ ...formData, ubicacion_Fisica: e.target.value })}
                  placeholder="Bodega Principal - Rack A3"
                  bg={inputBg}
                  borderColor={borderColor}
                />
              </FormControl>

              <FormControl>
                <FormLabel>URL de Imagen</FormLabel>
                <Input
                  value={formData.imagen_URL}
                  onChange={(e) => setFormData({ ...formData, imagen_URL: e.target.value })}
                  placeholder="https://ejemplo.com/imagen.jpg"
                  bg={inputBg}
                  borderColor={borderColor}
                />
              </FormControl>

              <FormControl>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  value={formData.observaciones}
                  onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
                  placeholder="Notas adicionales sobre el activo"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
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
