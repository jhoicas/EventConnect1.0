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
import { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateActivoMutation, useUpdateActivoMutation, type Activo } from '../store/api/activoApi';
import { useGetCategoriasQuery } from '../store/api/categoriaApi';
import { activoSchema, type ActivoFormData } from '../lib/validations/activoSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface ActivoModalProps {
  isOpen: boolean;
  onClose: () => void;
  activo?: Activo;
}

export const ActivoModal = ({ isOpen, onClose, activo }: ActivoModalProps) => {
  const toast = useToast();
  const { data: categorias = [] } = useGetCategoriasQuery();
  
  const [createActivo, { isLoading: isCreating }] = useCreateActivoMutation();
  const [updateActivo, { isLoading: isUpdating }] = useUpdateActivoMutation();

  const isEdit = !!activo;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    reset,
  } = useForm<ActivoFormData>({
    resolver: zodResolver(activoSchema),
    defaultValues: {
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
    },
  });

  // Resetear formulario cuando cambia el activo o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (activo) {
        reset({
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
        reset({
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
    }
  }, [activo, isOpen, reset]);

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

  const onSubmit = async (data: ActivoFormData) => {
    try {
      if (isEdit) {
        await updateActivo({
          id: activo.id,
          categoria_Id: data.categoria_Id,
          codigo_Activo: data.codigo_Activo,
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          marca: data.marca || undefined,
          modelo: data.modelo || undefined,
          numero_Serie: data.numero_Serie || undefined,
          estado: activo.estado,
          fecha_Adquisicion: data.fecha_Adquisicion || undefined,
          valor_Adquisicion: data.valor_Adquisicion || undefined,
          vida_Util_Meses: data.vida_Util_Meses || undefined,
          ubicacion_Fisica: data.ubicacion_Fisica || undefined,
          imagen_URL: data.imagen_URL || undefined,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Activo actualizado',
          description: `El activo ${data.codigo_Activo} fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createActivo({
          categoria_Id: data.categoria_Id,
          codigo_Activo: data.codigo_Activo,
          nombre: data.nombre,
          descripcion: data.descripcion || undefined,
          marca: data.marca || undefined,
          modelo: data.modelo || undefined,
          numero_Serie: data.numero_Serie || undefined,
          fecha_Adquisicion: data.fecha_Adquisicion || undefined,
          valor_Adquisicion: data.valor_Adquisicion || undefined,
          vida_Util_Meses: data.vida_Util_Meses || undefined,
          ubicacion_Fisica: data.ubicacion_Fisica || undefined,
          imagen_URL: data.imagen_URL || undefined,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Activo creado',
          description: `El activo ${data.codigo_Activo} fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar el activo';
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
        <form onSubmit={handleSubmit(onSubmit)}>
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
                      {...register('categoria_Id', { valueAsNumber: true })}
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
                    <FormErrorMessage>{errors.categoria_Id?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isRequired isInvalid={!!errors.codigo_Activo}>
                    <FormLabel>Código del Activo</FormLabel>
                    <Input
                      {...register('codigo_Activo')}
                      placeholder="ACT-001"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.codigo_Activo?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel>Nombre del Activo</FormLabel>
                <Input
                  {...register('nombre')}
                  placeholder="Proyector LED 4K"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.nombre?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.descripcion}>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  {...register('descripcion')}
                  placeholder="Descripción detallada del activo"
                  bg={inputBg}
                  borderColor={borderColor}
                  rows={3}
                />
                <FormErrorMessage>{errors.descripcion?.message}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.marca}>
                    <FormLabel>Marca</FormLabel>
                    <Input
                      {...register('marca')}
                      placeholder="Sony"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.marca?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.modelo}>
                    <FormLabel>Modelo</FormLabel>
                    <Input
                      {...register('modelo')}
                      placeholder="VPL-VW5000ES"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.modelo?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.numero_Serie}>
                    <FormLabel>Número de Serie</FormLabel>
                    <Input
                      {...register('numero_Serie')}
                      placeholder="SN123456789"
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.numero_Serie?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns="repeat(3, 1fr)" gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.fecha_Adquisicion}>
                    <FormLabel>Fecha de Adquisición</FormLabel>
                    <Input
                      type="date"
                      {...register('fecha_Adquisicion')}
                      bg={inputBg}
                      borderColor={borderColor}
                    />
                    <FormErrorMessage>{errors.fecha_Adquisicion?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.valor_Adquisicion}>
                    <FormLabel>Valor de Adquisición ($)</FormLabel>
                    <Controller
                      name="valor_Adquisicion"
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
                    <FormErrorMessage>{errors.valor_Adquisicion?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.vida_Util_Meses}>
                    <FormLabel>Vida Útil (meses)</FormLabel>
                    <Controller
                      name="vida_Util_Meses"
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
                    <FormErrorMessage>{errors.vida_Util_Meses?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isInvalid={!!errors.ubicacion_Fisica}>
                <FormLabel>Ubicación Física</FormLabel>
                <Input
                  {...register('ubicacion_Fisica')}
                  placeholder="Bodega Principal - Rack A3"
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.ubicacion_Fisica?.message}</FormErrorMessage>
              </FormControl>

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

              <FormControl isInvalid={!!errors.observaciones}>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  {...register('observaciones')}
                  placeholder="Notas adicionales sobre el activo"
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
