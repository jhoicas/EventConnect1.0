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
  HStack,
  Switch,
  Grid,
  GridItem,
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import { useCreateProductoMutation, useUpdateProductoMutation, type Producto } from '../store/api/productoApi';
import { useGetCategoriasQuery } from '../store/api/categoriaApi';

interface ProductoModalProps {
  isOpen: boolean;
  onClose: () => void;
  producto?: Producto;
}

export const ProductoModal = ({ isOpen, onClose, producto }: ProductoModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const { data: categorias = [] } = useGetCategoriasQuery();
  
  const [formData, setFormData] = useState({
    categoria_Id: 0,
    sku: '',
    nombre: '',
    descripcion: '',
    unidad_Medida: 'Unidad',
    precio_Alquiler_Dia: 0,
    cantidad_Stock: 0,
    stock_Minimo: 10,
    imagen_URL: '',
    es_Alquilable: true,
    es_Vendible: false,
    peso_Kg: 0,
    dimensiones: '',
    observaciones: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createProducto, { isLoading: isCreating }] = useCreateProductoMutation();
  const [updateProducto, { isLoading: isUpdating }] = useUpdateProductoMutation();

  const isEdit = !!producto;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (producto) {
      setFormData({
        categoria_Id: producto.categoria_Id,
        sku: producto.sku,
        nombre: producto.nombre,
        descripcion: producto.descripcion || '',
        unidad_Medida: producto.unidad_Medida,
        precio_Alquiler_Dia: producto.precio_Alquiler_Dia,
        cantidad_Stock: producto.cantidad_Stock,
        stock_Minimo: producto.stock_Minimo,
        imagen_URL: producto.imagen_URL || '',
        es_Alquilable: producto.es_Alquilable,
        es_Vendible: producto.es_Vendible,
        peso_Kg: producto.peso_Kg || 0,
        dimensiones: producto.dimensiones || '',
        observaciones: producto.observaciones || '',
      });
    } else {
      setFormData({
        categoria_Id: 0,
        sku: '',
        nombre: '',
        descripcion: '',
        unidad_Medida: 'Unidad',
        precio_Alquiler_Dia: 0,
        cantidad_Stock: 0,
        stock_Minimo: 10,
        imagen_URL: '',
        es_Alquilable: true,
        es_Vendible: false,
        peso_Kg: 0,
        dimensiones: '',
        observaciones: '',
      });
    }
    setErrors({});
  }, [producto, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.categoria_Id) newErrors.categoria_Id = 'Debe seleccionar una categoría';
    if (!formData.sku.trim()) newErrors.sku = 'El SKU es requerido';
    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es requerido';
    if (formData.precio_Alquiler_Dia <= 0) newErrors.precio_Alquiler_Dia = 'El precio debe ser mayor a 0';
    if (formData.cantidad_Stock < 0) newErrors.cantidad_Stock = 'El stock no puede ser negativo';
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateProducto({
          id: producto.id,
          categoria_Id: formData.categoria_Id,
          sku: formData.sku,
          nombre: formData.nombre,
          descripcion: formData.descripcion || undefined,
          unidad_Medida: formData.unidad_Medida,
          precio_Alquiler_Dia: formData.precio_Alquiler_Dia,
          cantidad_Stock: formData.cantidad_Stock,
          stock_Minimo: formData.stock_Minimo,
          imagen_URL: formData.imagen_URL || undefined,
          es_Alquilable: formData.es_Alquilable,
          es_Vendible: formData.es_Vendible,
          requiere_Mantenimiento: producto.requiere_Mantenimiento,
          dias_Mantenimiento: producto.dias_Mantenimiento,
          peso_Kg: formData.peso_Kg || undefined,
          dimensiones: formData.dimensiones || undefined,
          observaciones: formData.observaciones || undefined,
          activo: producto.activo,
        }).unwrap();
        
        toast({
          title: 'Producto actualizado',
          description: `El producto "${formData.nombre}" fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createProducto({
          categoria_Id: formData.categoria_Id,
          sku: formData.sku,
          nombre: formData.nombre,
          descripcion: formData.descripcion || undefined,
          unidad_Medida: formData.unidad_Medida,
          precio_Alquiler_Dia: formData.precio_Alquiler_Dia,
          cantidad_Stock: formData.cantidad_Stock,
          stock_Minimo: formData.stock_Minimo,
          imagen_URL: formData.imagen_URL || undefined,
          es_Alquilable: formData.es_Alquilable,
          es_Vendible: formData.es_Vendible,
          peso_Kg: formData.peso_Kg || undefined,
          dimensiones: formData.dimensiones || undefined,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Producto creado',
          description: `El producto "${formData.nombre}" fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar el producto',
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
        maxH={{ base: "100vh", md: "90vh" }}
        m={{ base: 0, md: 4 }}
        overflow="auto"
      >
        <form onSubmit={handleSubmit}>
          <ModalHeader fontSize={{ base: "lg", md: "xl" }}>
            {isEdit ? 'Editar Producto' : 'Nuevo Producto'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <VStack spacing={4}>
              <Grid 
                templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} 
                gap={4} 
                w="full"
              >
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.categoria_Id}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Categoría</FormLabel>
                    <Select
                      value={formData.categoria_Id}
                      onChange={(e) => setFormData({ ...formData, categoria_Id: Number(e.target.value) })}
                      bg={inputBg}
                      size={{ base: "sm", md: "md" }}
                      borderColor={borderColor}
                      placeholder="Seleccione una categoría"
                    >
                      {categorias.map((cat) => (
                        <option key={cat.id} value={cat.id}>
                          {cat.nombre}
                        </option>
                      ))}
                    </Select>
                    <FormErrorMessage>{errors.categoria_Id}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.sku}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>SKU</FormLabel>
                    <Input
                      value={formData.sku}
                      onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
                      placeholder="Ej: SIL-001"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.sku}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre</FormLabel>
                <Input
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                  placeholder="Ej: Silla Tiffany Blanca"
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.nombre}</FormErrorMessage>
              </FormControl>

              <FormControl>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Descripción</FormLabel>
                <Textarea
                  value={formData.descripcion}
                  onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                  placeholder="Descripción detallada del producto"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                  size={{ base: "sm", md: "md" }}
                />
              </FormControl>

              <Grid 
                templateColumns={{ base: "1fr", md: "repeat(3, 1fr)" }} 
                gap={4} 
                w="full"
              >
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.precio_Alquiler_Dia}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Precio por Día ($)</FormLabel>
                    <NumberInput
                      value={formData.precio_Alquiler_Dia}
                      onChange={(_, val) => setFormData({ ...formData, precio_Alquiler_Dia: val })}
                      min={0}
                      size={{ base: "sm", md: "md" }}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                    <FormErrorMessage>{errors.precio_Alquiler_Dia}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.cantidad_Stock}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Stock Actual</FormLabel>
                    <NumberInput
                      value={formData.cantidad_Stock}
                      onChange={(_, val) => setFormData({ ...formData, cantidad_Stock: val })}
                      min={0}
                      size={{ base: "sm", md: "md" }}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                    <FormErrorMessage>{errors.cantidad_Stock}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Stock Mínimo</FormLabel>
                    <NumberInput
                      value={formData.stock_Minimo}
                      onChange={(_, val) => setFormData({ ...formData, stock_Minimo: val })}
                      min={0}
                      size={{ base: "sm", md: "md" }}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel>Unidad de Medida</FormLabel>
                    <Select
                      value={formData.unidad_Medida}
                      onChange={(e) => setFormData({ ...formData, unidad_Medida: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Unidad">Unidad</option>
                      <option value="Metro">Metro</option>
                      <option value="Kit">Kit</option>
                      <option value="Conjunto">Conjunto</option>
                    </Select>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel>Peso (Kg)</FormLabel>
                    <NumberInput
                      value={formData.peso_Kg}
                      onChange={(_, val) => setFormData({ ...formData, peso_Kg: val })}
                      min={0}
                      step={0.1}
                    >
                      <NumberInputField bg={inputBg} borderColor={borderColor} />
                    </NumberInput>
                  </FormControl>
                </GridItem>
              </Grid>

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

              <HStack w="full" spacing={8}>
                <FormControl display="flex" alignItems="center">
                  <FormLabel mb="0">Alquilable</FormLabel>
                  <Switch
                    isChecked={formData.es_Alquilable}
                    onChange={(e) => setFormData({ ...formData, es_Alquilable: e.target.checked })}
                  />
                </FormControl>

                <FormControl display="flex" alignItems="center">
                  <FormLabel mb="0">Vendible</FormLabel>
                  <Switch
                    isChecked={formData.es_Vendible}
                    onChange={(e) => setFormData({ ...formData, es_Vendible: e.target.checked })}
                  />
                </FormControl>
              </HStack>
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
