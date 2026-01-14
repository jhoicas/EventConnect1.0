'use client';

import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalCloseButton,
  ModalBody,
  ModalFooter,
  Button,
  FormControl,
  FormLabel,
  Input,
  Textarea,
  Select,
  Switch,
  VStack,
  useToast,
  FormErrorMessage,
  HStack,
  Text,
} from '@chakra-ui/react';
import {
  useCreateConfiguracionMutation,
  useUpdateConfiguracionMutation,
  type ConfiguracionSistema,
} from '@/store/api/configuracionApi';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { configuracionSchema, type ConfiguracionFormData } from '../lib/validations/configuracionSchema';
import { useColorModeLocal } from '../hooks/useColorModeLocal';

interface ConfiguracionModalProps {
  isOpen: boolean;
  onClose: () => void;
  configuracion?: ConfiguracionSistema;
}

export function ConfiguracionModal({ isOpen, onClose, configuracion }: ConfiguracionModalProps) {
  const toast = useToast();
  const [createConfiguracion, { isLoading: isCreating }] = useCreateConfiguracionMutation();
  const [updateConfiguracion, { isLoading: isUpdating }] = useUpdateConfiguracionMutation();

  const isEdit = !!configuracion;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<ConfiguracionFormData>({
    resolver: zodResolver(configuracionSchema),
    defaultValues: {
      clave: '',
      tipo_Dato: 'string',
      valor: '',
      descripcion: '',
      es_Global: false,
    },
  });

  const tipoDato = watch('tipo_Dato');
  const esGlobal = watch('es_Global');

  // Obtener color mode y colores del hook personalizado
  const { colorMode, bgColor: cardBg, inputBg, borderColor } = useColorModeLocal();

  useEffect(() => {
    if (isOpen) {
      if (configuracion) {
        reset({
          clave: configuracion.clave,
          valor: configuracion.valor || '',
          descripcion: configuracion.descripcion || '',
          tipo_Dato: configuracion.tipo_Dato,
          es_Global: configuracion.es_Global,
        });
      } else {
        reset({
          clave: '',
          valor: '',
          descripcion: '',
          tipo_Dato: 'string',
          es_Global: false,
        });
      }
    }
  }, [configuracion, isOpen, reset]);

  const onSubmit = async (data: ConfiguracionFormData) => {
    try {
      const payload = {
        clave: data.clave.trim(),
        valor: data.valor?.trim() || undefined,
        descripcion: data.descripcion?.trim() || undefined,
        tipo_Dato: data.tipo_Dato,
        es_Global: data.es_Global,
      };

      if (isEdit) {
        await updateConfiguracion({
          id: configuracion.id,
          body: payload,
        }).unwrap();

        toast({
          title: 'Configuración actualizada',
          description: `La configuración "${data.clave}" fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createConfiguracion(payload).unwrap();

        toast({
          title: 'Configuración creada',
          description: `La configuración "${data.clave}" fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }

      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar la configuración';
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
    <Modal isOpen={isOpen} onClose={handleClose} size="lg">
      <ModalOverlay />
      <ModalContent bg={cardBg}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalHeader>
            {configuracion ? 'Editar Configuración' : 'Nueva Configuración'}
          </ModalHeader>
          <ModalCloseButton />

          <ModalBody>
            <VStack spacing={4}>
              <FormControl isInvalid={!!errors.clave} isRequired>
                <FormLabel>Clave</FormLabel>
                <Input
                  {...register('clave')}
                  placeholder="NOMBRE_CONFIGURACION"
                  isDisabled={isEdit}
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.clave?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isRequired isInvalid={!!errors.tipo_Dato}>
                <FormLabel>Tipo de Dato</FormLabel>
                <Select
                  {...register('tipo_Dato')}
                  bg={inputBg}
                  borderColor={borderColor}
                >
                  <option value="string">Texto (string)</option>
                  <option value="int">Número Entero (int)</option>
                  <option value="bool">Booleano (bool)</option>
                  <option value="json">JSON</option>
                </Select>
                <FormErrorMessage>{errors.tipo_Dato?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.valor}>
                <FormLabel>Valor</FormLabel>
                {tipoDato === 'bool' ? (
                  <Select
                    {...register('valor')}
                    bg={inputBg}
                    borderColor={borderColor}
                  >
                    <option value="true">Verdadero</option>
                    <option value="false">Falso</option>
                  </Select>
                ) : tipoDato === 'json' ? (
                  <Textarea
                    {...register('valor')}
                    placeholder='{"key": "value"}'
                    rows={5}
                    bg={inputBg}
                    borderColor={borderColor}
                  />
                ) : (
                  <Input
                    {...register('valor')}
                    type={tipoDato === 'int' ? 'number' : 'text'}
                    placeholder={
                      tipoDato === 'int'
                        ? '100'
                        : 'Valor de la configuración'
                    }
                    bg={inputBg}
                    borderColor={borderColor}
                  />
                )}
                <FormErrorMessage>{errors.valor?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isInvalid={!!errors.descripcion}>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  {...register('descripcion')}
                  placeholder="Describe el propósito de esta configuración"
                  rows={3}
                  bg={inputBg}
                  borderColor={borderColor}
                />
                <FormErrorMessage>{errors.descripcion?.message}</FormErrorMessage>
              </FormControl>

              <FormControl display="flex" alignItems="center">
                <FormLabel mb={0}>Configuración Global</FormLabel>
                <Switch
                  {...register('es_Global')}
                  isChecked={esGlobal}
                  onChange={(e) => setValue('es_Global', e.target.checked, { shouldValidate: true })}
                  colorScheme="blue"
                />
              </FormControl>
              {esGlobal && (
                <HStack
                  w="full"
                  p={3}
                  bg={colorMode === 'dark' ? 'blue.900' : colorMode === 'blue' ? 'blue.800' : 'blue.50'}
                  borderRadius="md"
                  spacing={2}
                >
                  <Text fontSize="sm" color={colorMode === 'dark' || colorMode === 'blue' ? 'blue.200' : 'blue.700'}>
                    ⚠️ Las configuraciones globales aplican a todas las empresas
                  </Text>
                </HStack>
              )}
            </VStack>
          </ModalBody>

          <ModalFooter>
            <Button variant="ghost" mr={3} onClick={handleClose}>
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
}
