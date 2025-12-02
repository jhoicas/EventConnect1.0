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
  useColorMode,
  HStack,
  Text,
} from '@chakra-ui/react';
import {
  useCreateConfiguracionMutation,
  useUpdateConfiguracionMutation,
  type ConfiguracionSistema,
} from '@/store/api/configuracionApi';
import { useEffect, useState } from 'react';

interface ConfiguracionModalProps {
  isOpen: boolean;
  onClose: () => void;
  configuracion?: ConfiguracionSistema;
}

export function ConfiguracionModal({ isOpen, onClose, configuracion }: ConfiguracionModalProps) {
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  const [createConfiguracion, { isLoading: isCreating }] = useCreateConfiguracionMutation();
  const [updateConfiguracion, { isLoading: isUpdating }] = useUpdateConfiguracionMutation();

  const [clave, setClave] = useState('');
  const [valor, setValor] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [tipoDato, setTipoDato] = useState('string');
  const [esGlobal, setEsGlobal] = useState(false);
  const [errors, setErrors] = useState({ clave: '', tipoDato: '' });

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  useEffect(() => {
    if (configuracion) {
      setClave(configuracion.clave);
      setValor(configuracion.valor || '');
      setDescripcion(configuracion.descripcion || '');
      setTipoDato(configuracion.tipo_Dato);
      setEsGlobal(configuracion.es_Global);
    } else {
      setClave('');
      setValor('');
      setDescripcion('');
      setTipoDato('string');
      setEsGlobal(false);
    }
    setErrors({ clave: '', tipoDato: '' });
  }, [configuracion, isOpen]);

  const cardBg = localColorMode === 'dark' ? '#1a2035' : localColorMode === 'blue' ? '#192734' : '#ffffff';

  const validate = () => {
    const newErrors = { clave: '', tipoDato: '' };
    let isValid = true;

    if (!clave.trim()) {
      newErrors.clave = 'La clave es requerida';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    try {
      const payload = {
        clave: clave.trim(),
        valor: valor.trim(),
        descripcion: descripcion.trim(),
        tipo_Dato: tipoDato,
        es_Global: esGlobal,
      };

      if (configuracion) {
        await updateConfiguracion({
          id: configuracion.id,
          body: payload,
        }).unwrap();

        toast({
          title: 'Configuración actualizada',
          description: `La configuración "${clave}" fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createConfiguracion(payload).unwrap();

        toast({
          title: 'Configuración creada',
          description: `La configuración "${clave}" fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }

      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar la configuración',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleClose = () => {
    setClave('');
    setValor('');
    setDescripcion('');
    setTipoDato('string');
    setEsGlobal(false);
    setErrors({ clave: '', tipoDato: '' });
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size="lg">
      <ModalOverlay />
      <ModalContent bg={cardBg}>
        <form onSubmit={handleSubmit}>
          <ModalHeader>
            {configuracion ? 'Editar Configuración' : 'Nueva Configuración'}
          </ModalHeader>
          <ModalCloseButton />

          <ModalBody>
            <VStack spacing={4}>
              <FormControl isInvalid={!!errors.clave} isRequired>
                <FormLabel>Clave</FormLabel>
                <Input
                  value={clave}
                  onChange={(e) => setClave(e.target.value)}
                  placeholder="NOMBRE_CONFIGURACION"
                  isDisabled={!!configuracion}
                />
                <FormErrorMessage>{errors.clave}</FormErrorMessage>
              </FormControl>

              <FormControl isRequired>
                <FormLabel>Tipo de Dato</FormLabel>
                <Select value={tipoDato} onChange={(e) => setTipoDato(e.target.value)}>
                  <option value="string">Texto (string)</option>
                  <option value="int">Número Entero (int)</option>
                  <option value="bool">Booleano (bool)</option>
                  <option value="json">JSON</option>
                </Select>
              </FormControl>

              <FormControl>
                <FormLabel>Valor</FormLabel>
                {tipoDato === 'bool' ? (
                  <Select value={valor} onChange={(e) => setValor(e.target.value)}>
                    <option value="true">Verdadero</option>
                    <option value="false">Falso</option>
                  </Select>
                ) : tipoDato === 'json' ? (
                  <Textarea
                    value={valor}
                    onChange={(e) => setValor(e.target.value)}
                    placeholder='{"key": "value"}'
                    rows={5}
                  />
                ) : (
                  <Input
                    value={valor}
                    onChange={(e) => setValor(e.target.value)}
                    type={tipoDato === 'int' ? 'number' : 'text'}
                    placeholder={
                      tipoDato === 'int'
                        ? '100'
                        : 'Valor de la configuración'
                    }
                  />
                )}
              </FormControl>

              <FormControl>
                <FormLabel>Descripción</FormLabel>
                <Textarea
                  value={descripcion}
                  onChange={(e) => setDescripcion(e.target.value)}
                  placeholder="Describe el propósito de esta configuración"
                  rows={3}
                />
              </FormControl>

              <FormControl display="flex" alignItems="center">
                <FormLabel mb={0}>Configuración Global</FormLabel>
                <Switch
                  isChecked={esGlobal}
                  onChange={(e) => setEsGlobal(e.target.checked)}
                  colorScheme="blue"
                />
              </FormControl>
              {esGlobal && (
                <HStack
                  w="full"
                  p={3}
                  bg="blue.50"
                  borderRadius="md"
                  spacing={2}
                >
                  <Text fontSize="sm" color="blue.700">
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
              isLoading={isCreating || isUpdating}
            >
              {configuracion ? 'Actualizar' : 'Crear'}
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
}
