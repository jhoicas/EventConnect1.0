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
  useToast,
  FormErrorMessage,
  VStack,
  Grid,
  GridItem,
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import { useCreateClienteMutation, useUpdateClienteMutation, type Cliente } from '../store/api/clienteApi';

interface ClienteModalProps {
  isOpen: boolean;
  onClose: () => void;
  cliente?: Cliente;
}

export const ClienteModal = ({ isOpen, onClose, cliente }: ClienteModalProps) => {
  const [colorMode, setColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  
  const [formData, setFormData] = useState({
    tipo_Cliente: 'Persona',
    nombre: '',
    documento: '',
    tipo_Documento: 'CC',
    email: '',
    telefono: '',
    direccion: '',
    ciudad: '',
    contacto_Nombre: '',
    contacto_Telefono: '',
    observaciones: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const [createCliente, { isLoading: isCreating }] = useCreateClienteMutation();
  const [updateCliente, { isLoading: isUpdating }] = useUpdateClienteMutation();

  const isEdit = !!cliente;
  const isLoading = isCreating || isUpdating;

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setColorMode(stored);
    }
  }, []);

  useEffect(() => {
    if (cliente) {
      setFormData({
        tipo_Cliente: cliente.tipo_Cliente,
        nombre: cliente.nombre,
        documento: cliente.documento,
        tipo_Documento: cliente.tipo_Documento,
        email: cliente.email || '',
        telefono: cliente.telefono || '',
        direccion: cliente.direccion || '',
        ciudad: cliente.ciudad || '',
        contacto_Nombre: cliente.contacto_Nombre || '',
        contacto_Telefono: cliente.contacto_Telefono || '',
        observaciones: cliente.observaciones || '',
      });
    } else {
      setFormData({
        tipo_Cliente: 'Persona',
        nombre: '',
        documento: '',
        tipo_Documento: 'CC',
        email: '',
        telefono: '',
        direccion: '',
        ciudad: '',
        contacto_Nombre: '',
        contacto_Telefono: '',
        observaciones: '',
      });
    }
    setErrors({});
  }, [cliente, isOpen]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es requerido';
    if (!formData.documento.trim()) newErrors.documento = 'El documento es requerido';
    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      if (isEdit) {
        await updateCliente({
          id: cliente.id,
          tipo_Cliente: formData.tipo_Cliente,
          nombre: formData.nombre,
          documento: formData.documento,
          tipo_Documento: formData.tipo_Documento,
          email: formData.email || undefined,
          telefono: formData.telefono || undefined,
          direccion: formData.direccion || undefined,
          ciudad: formData.ciudad || undefined,
          contacto_Nombre: formData.contacto_Nombre || undefined,
          contacto_Telefono: formData.contacto_Telefono || undefined,
          observaciones: formData.observaciones || undefined,
          rating: cliente.rating,
          estado: cliente.estado,
        }).unwrap();
        
        toast({
          title: 'Cliente actualizado',
          description: `El cliente "${formData.nombre}" fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createCliente({
          tipo_Cliente: formData.tipo_Cliente,
          nombre: formData.nombre,
          documento: formData.documento,
          tipo_Documento: formData.tipo_Documento,
          email: formData.email || undefined,
          telefono: formData.telefono || undefined,
          direccion: formData.direccion || undefined,
          ciudad: formData.ciudad || undefined,
          contacto_Nombre: formData.contacto_Nombre || undefined,
          contacto_Telefono: formData.contacto_Telefono || undefined,
          observaciones: formData.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Cliente creado',
          description: `El cliente "${formData.nombre}" fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al guardar el cliente',
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
            {isEdit ? 'Editar Cliente' : 'Nuevo Cliente'}
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
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Tipo de Cliente</FormLabel>
                    <Select
                      value={formData.tipo_Cliente}
                      size={{ base: "sm", md: "md" }}
                      onChange={(e) => setFormData({ ...formData, tipo_Cliente: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                    >
                      <option value="Persona">Persona Natural</option>
                      <option value="Empresa">Empresa</option>
                    </Select>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Tipo de Documento</FormLabel>
                    <Select
                      value={formData.tipo_Documento}
                      onChange={(e) => setFormData({ ...formData, tipo_Documento: e.target.value })}
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    >
                      <option value="CC">Cédula de Ciudadanía</option>
                      <option value="NIT">NIT</option>
                      <option value="CE">Cédula de Extranjería</option>
                      <option value="Pasaporte">Pasaporte</option>
                    </Select>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre Completo / Razón Social</FormLabel>
                <Input
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                  placeholder={formData.tipo_Cliente === 'Persona' ? 'Ej: Juan Pérez' : 'Ej: Empresa ABC S.A.S.'}
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.nombre}</FormErrorMessage>
              </FormControl>

              <FormControl isRequired isInvalid={!!errors.documento}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Documento de Identidad</FormLabel>
                <Input
                  value={formData.documento}
                  onChange={(e) => setFormData({ ...formData, documento: e.target.value })}
                  placeholder="123456789"
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.documento}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.email}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Email</FormLabel>
                    <Input
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      placeholder="ejemplo@correo.com"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.email}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Teléfono</FormLabel>
                    <Input
                      value={formData.telefono}
                      onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                      placeholder="300 123 4567"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Ciudad</FormLabel>
                    <Input
                      value={formData.ciudad}
                      onChange={(e) => setFormData({ ...formData, ciudad: e.target.value })}
                      placeholder="Bogotá"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Dirección</FormLabel>
                    <Input
                      value={formData.direccion}
                      onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
                      placeholder="Calle 123 #45-67"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              {formData.tipo_Cliente === 'Empresa' && (
                <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                  <GridItem>
                    <FormControl>
                      <FormLabel fontSize={{ base: "sm", md: "md" }}>Contacto - Nombre</FormLabel>
                      <Input
                        value={formData.contacto_Nombre}
                        onChange={(e) => setFormData({ ...formData, contacto_Nombre: e.target.value })}
                        size={{ base: "sm", md: "md" }}
                        placeholder="Nombre del contacto"
                        bg={inputBg}
                        borderColor={borderColor}
                      />
                    </FormControl>
                  </GridItem>

                  <GridItem>
                    <FormControl>
                      <FormLabel fontSize={{ base: "sm", md: "md" }}>Contacto - Teléfono</FormLabel>
                      <Input
                        value={formData.contacto_Telefono}
                        onChange={(e) => setFormData({ ...formData, contacto_Telefono: e.target.value })}
                        placeholder="300 123 4567"
                        bg={inputBg}
                        borderColor={borderColor}
                        size={{ base: "sm", md: "md" }}
                      />
                    </FormControl>
                  </GridItem>
                </Grid>
              )}

              <FormControl>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  value={formData.observaciones}
                  onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
                  placeholder="Notas adicionales sobre el cliente"
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
