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
  useToast,
  FormErrorMessage,
  Select,
  HStack,
  Text,
  Box,
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import { useCreateCategoriaMutation, useUpdateCategoriaMutation, type Categoria } from '../store/api/categoriaApi';

const ICONOS_DISPONIBLES = [
  { value: 'chair', label: '🪑 Silla (chair)' },
  { value: 'couch', label: '🛋️ Sofá (couch)' },
  { value: 'utensils', label: '🍴 Vajilla (utensils)' },
  { value: 'wine-glass', label: '🍷 Copa (wine-glass)' },
  { value: 'coffee', label: '☕ Café (coffee)' },
  { value: 'sparkles', label: '✨ Decoración (sparkles)' },
  { value: 'balloon', label: '🎈 Globo (balloon)' },
  { value: 'party-popper', label: '🎉 Fiesta (party-popper)' },
  { value: 'speaker', label: '🔊 Sonido (speaker)' },
  { value: 'lightbulb', label: '💡 Iluminación (lightbulb)' },
  { value: 'tv', label: '📺 Proyección (tv)' },
  { value: 'monitor', label: '🖥️ Monitor (monitor)' },
  { value: 'music', label: '🎵 Música (music)' },
  { value: 'music-note', label: '🎶 Nota Musical (music-note)' },
  { value: 'camera', label: '📷 Fotografía (camera)' },
  { value: 'game-controller', label: '🎮 Juegos (game-controller)' },
  { value: 'crown', label: '👑 VIP (crown)' },
  { value: 'stage', label: '🎭 Escenario (stage)' },
  { value: 'fan', label: '🌀 Ventilación (fan)' },
  { value: 'shield', label: '🛡️ Seguridad (shield)' },
  { value: 'truck', label: '🚚 Transporte (truck)' },
  { value: 'baby', label: '👶 Infantil (baby)' },
];

interface CategoriaModalProps {
  isOpen: boolean;
  onClose: () => void;
  categoria?: Categoria;
}

export const CategoriaModal = ({ isOpen, onClose, categoria }: CategoriaModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [icono, setIcono] = useState('');
  const [color, setColor] = useState('#3B82F6');
  const [errors, setErrors] = useState({ nombre: '' });

  const [createCategoria, { isLoading: isCreating }] = useCreateCategoriaMutation();
  const [updateCategoria, { isLoading: isUpdating }] = useUpdateCategoriaMutation();

  const isEdit = !!categoria;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (categoria) {
      setNombre(categoria.nombre);
      setDescripcion(categoria.descripcion || '');
      setIcono(categoria.icono || '');
      setColor(categoria.color || '#3B82F6');
    } else {
      setNombre('');
      setDescripcion('');
      setIcono('');
      setColor('#3B82F6');
    }
    setErrors({ nombre: '' });
  }, [categoria, isOpen]);

  const validate = () => {
    const newErrors = { nombre: '' };
    if (!nombre.trim()) {
      newErrors.nombre = 'El nombre es requerido';
    }
    setErrors(newErrors);
    return !newErrors.nombre;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateCategoria({
          id: categoria.id,
          nombre,
          descripcion,
          icono,
          color,
          activo: categoria.activo,
        }).unwrap();
        
        toast({
          title: 'Categoría actualizada',
          description: `La categoría "${nombre}" fue actualizada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createCategoria({
          nombre,
          descripcion,
          icono,
          color,
        }).unwrap();
        
        toast({
          title: 'Categoría creada',
          description: `La categoría "${nombre}" fue creada exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar la categoría',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleClose = () => {
    setNombre('');
    setDescripcion('');
    setIcono('');
    setColor('#3B82F6');
    setErrors({ nombre: '' });
    onClose();
  };

  const bgColor = colorMode === 'dark' ? '#1a2035' : colorMode === 'blue' ? '#192734' : '#ffffff';
  const inputBg = colorMode === 'dark' ? '#242b3d' : colorMode === 'blue' ? '#1e3140' : '#f5f6f8';
  const borderColor = colorMode === 'dark' ? '#2d3548' : colorMode === 'blue' ? '#2a4255' : '#e2e8f0';

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size={{ base: "full", md: "md" }} scrollBehavior="inside">
      <ModalOverlay bg="blackAlpha.300" backdropFilter="blur(10px)" />
      <ModalContent 
        bg={bgColor} 
        borderColor={borderColor} 
        borderWidth="1px"
        mx={{ base: 0, md: 4 }}
        my={{ base: 0, md: "auto" }}
        maxH={{ base: "100vh", md: "90vh" }}
      >
        <form onSubmit={handleSubmit}>
          <ModalHeader fontSize={{ base: "lg", md: "xl" }}>
            {isEdit ? 'Editar Categoría' : 'Nueva Categoría'}
          </ModalHeader>
          <ModalCloseButton />
          
          <ModalBody pb={6}>
            <FormControl isRequired isInvalid={!!errors.nombre} mb={4}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre</FormLabel>
              <Input
                value={nombre}
                onChange={(e) => setNombre(e.target.value)}
                placeholder="Ej: Sillas, Mesas, Vajillas"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
              />
              <FormErrorMessage>{errors.nombre}</FormErrorMessage>
            </FormControl>

            <FormControl>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Descripción</FormLabel>
              <Textarea
                value={descripcion}
                onChange={(e) => setDescripcion(e.target.value)}
                placeholder="Descripción opcional de la categoría"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
                rows={3}
                size={{ base: "sm", md: "md" }}
              />
            </FormControl>

            <FormControl mt={4}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Icono</FormLabel>
              <Select
                value={icono}
                onChange={(e) => setIcono(e.target.value)}
                placeholder="Selecciona un icono"
                bg={inputBg}
                borderColor={borderColor}
                _hover={{ borderColor: 'brand.300' }}
                _focus={{ borderColor: 'brand.400', boxShadow: '0 0 0 1px rgba(107, 163, 245, 0.3)' }}
                size={{ base: "sm", md: "md" }}
              >
                {ICONOS_DISPONIBLES.map((icon) => (
                  <option key={icon.value} value={icon.value}>
                    {icon.label}
                  </option>
                ))}
              </Select>
            </FormControl>

            <FormControl mt={4}>
              <FormLabel fontSize={{ base: "sm", md: "md" }}>Color</FormLabel>
              <HStack spacing={3} flexWrap={{ base: "wrap", md: "nowrap" }}>
                <Input
                  type="color"
                  value={color}
                  onChange={(e) => setColor(e.target.value)}
                  bg={inputBg}
                  borderColor={borderColor}
                  width={{ base: "60px", md: "80px" }}
                  height={{ base: "35px", md: "40px" }}
                  cursor="pointer"
                />
                <Box
                  width={{ base: "35px", md: "40px" }}
                  height={{ base: "35px", md: "40px" }}
                  borderRadius="md"
                  bg={color}
                  border="2px solid"
                  borderColor={borderColor}
                />
                <Text fontSize={{ base: "xs", md: "sm" }} color="gray.500">
                  {color}
                </Text>
              </HStack>
            </FormControl>
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
