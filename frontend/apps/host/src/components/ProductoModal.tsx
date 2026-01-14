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
import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateProductoMutation, useUpdateProductoMutation, type Producto } from '../store/api/productoApi';
import { useGetCategoriasQuery } from '../store/api/categoriaApi';
import { productoSchema, type ProductoFormData } from '../lib/validations/productoSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface ProductoModalProps {
  isOpen: boolean;
  onClose: () => void;
  producto?: Producto;
}

export const ProductoModal = ({ isOpen, onClose, producto }: ProductoModalProps) => {
  const toast = useToast();
  const { data: categorias = [] } = useGetCategoriasQuery();
  
  const [createProducto, { isLoading: isCreating }] = useCreateProductoMutation();
  const [updateProducto, { isLoading: isUpdating }] = useUpdateProductoMutation();

  const isEdit = !!producto;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
  } = useForm<ProductoFormData>({
    resolver: zodResolver(productoSchema),
    defaultValues: {
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
    },
  });

  // Resetear formulario cuando cambia el producto o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (producto) {
        reset({
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
        reset({
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
    }
  }, [producto, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: ProductoFormData) => {
    try {
      if (isEdit) {
        await updateProducto({
          id: producto.id,
          categoria_Id: data.categoria_Id,
          sku: data.sku,
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          unidad_Medida: data.unidad_Medida,
          precio_Alquiler_Dia: data.precio_Alquiler_Dia,
          cantidad_Stock: data.cantidad_Stock,
          stock_Minimo: data.stock_Minimo,
          imagen_URL: data.imagen_URL || undefined,
          es_Alquilable: data.es_Alquilable,
          es_Vendible: data.es_Vendible,
          requiere_Mantenimiento: producto.requiere_Mantenimiento,
          dias_Mantenimiento: producto.dias_Mantenimiento,
          peso_Kg: data.peso_Kg || undefined,
          dimensiones: data.dimensiones || undefined,
          observaciones: data.observaciones || undefined,
          activo: producto.activo,
        }).unwrap();
        
        toast({
          title: 'Producto actualizado',
          description: `El producto "${data.nombre}" fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createProducto({
          categoria_Id: data.categoria_Id,
          sku: data.sku,
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          unidad_Medida: data.unidad_Medida,
          precio_Alquiler_Dia: data.precio_Alquiler_Dia,
          cantidad_Stock: data.cantidad_Stock,
          stock_Minimo: data.stock_Minimo,
          imagen_URL: data.imagen_URL || undefined,
          es_Alquilable: data.es_Alquilable,
          es_Vendible: data.es_Vendible,
          peso_Kg: data.peso_Kg || undefined,
          dimensiones: data.dimensiones || undefined,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Producto creado',
          description: `El producto "${data.nombre}" fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar el producto';
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
        maxH={{ base: "100vh", md: "90vh" }}
        m={{ base: 0, md: 4 }}
        overflow="auto"
      >
        <form onSubmit={handleSubmit(onSubmit)}>
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
                      {...register('categoria_Id', { valueAsNumber: true })}
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
                    <FormErrorMessage>{errors.categoria_Id?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.sku}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>SKU</FormLabel>
                    <Input
                      {...register('sku')}
                      placeholder="Ej: SIL-001"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.sku?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre</FormLabel>
                <Input
                  {...register('nombre')}
                  placeholder="Ej: Silla Tiffany Blanca"
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.nombre?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.descripcion}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Descripción</FormLabel>
                <Textarea
                  {...register('descripcion')}
                  placeholder="Descripción detallada del producto"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.descripcion?.message}</FormErrorMessage>
              </FormControl>

              <Grid 
                templateColumns={{ base: "1fr", md: "repeat(3, 1fr)" }} 
                gap={4} 
                w="full"
              >
                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.precio_Alquiler_Dia}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Precio por Día ($)</FormLabel>
                    <Controller
                      name="precio_Alquiler_Dia"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                          size={{ base: "sm", md: "md" }}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.precio_Alquiler_Dia?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.cantidad_Stock}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Stock Actual</FormLabel>
                    <Controller
                      name="cantidad_Stock"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                          size={{ base: "sm", md: "md" }}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.cantidad_Stock?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.stock_Minimo}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Stock Mínimo</FormLabel>
                    <Controller
                      name="stock_Minimo"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                          size={{ base: "sm", md: "md" }}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.stock_Minimo?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(2, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.unidad_Medida}>
                    <FormLabel>Unidad de Medida</FormLabel>
                    <Select
                      {...register('unidad_Medida')}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Unidad">Unidad</option>
                      <option value="Metro">Metro</option>
                      <option value="Kit">Kit</option>
                      <option value="Conjunto">Conjunto</option>
                    </Select>
                    <FormErrorMessage>{errors.unidad_Medida?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.peso_Kg}>
                    <FormLabel>Peso (Kg)</FormLabel>
                    <Controller
                      name="peso_Kg"
                      control={control}
                      render={({ field: { onChange, value } }) => (
                        <NumberInput
                          value={value}
                          onChange={(_, val) => onChange(val)}
                          min={0}
                          step={0.1}
                        >
                          <NumberInputField bg={inputBg} borderColor={borderColor} />
                        </NumberInput>
                      )}
                    />
                    <FormErrorMessage>{errors.peso_Kg?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isInvalid={!!errors.imagen_URL}>
                <FormLabel>URL de Imagen</FormLabel>
                <Input
                  {...register('imagen_URL')}
                  placeholder="https://ejemplo.com/imagen.jpg"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.imagen_URL?.message}</FormErrorMessage>
              </FormControl>

              <HStack w="full" spacing={8}>
                <FormControl display="flex" alignItems="center">
                  <FormLabel mb="0">Alquilable</FormLabel>
                  <Switch {...register('es_Alquilable')} />
                </FormControl>

                <FormControl display="flex" alignItems="center">
                  <FormLabel mb="0">Vendible</FormLabel>
                  <Switch {...register('es_Vendible')} />
                </FormControl>
              </HStack>

              <FormControl isInvalid={!!errors.dimensiones}>
                <FormLabel>Dimensiones</FormLabel>
                <Input
                  {...register('dimensiones')}
                  placeholder="Ej: 50x60x80 cm"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.dimensiones?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.observaciones}>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  {...register('observaciones')}
                  placeholder="Notas adicionales sobre el producto"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
                <FormErrorMessage>{errors.observaciones?.message}</FormErrorMessage>
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
