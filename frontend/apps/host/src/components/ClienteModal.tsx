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
import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useCreateClienteMutation, useUpdateClienteMutation, type Cliente } from '../store/api/clienteApi';
import { clienteSchema, type ClienteFormData } from '../lib/validations/clienteSchema';

interface ClienteModalProps {
  isOpen: boolean;
  onClose: () => void;
  cliente?: Cliente;
}

export const ClienteModal = ({ isOpen, onClose, cliente }: ClienteModalProps) => {
  const toast = useToast();
  const [createCliente, { isLoading: isCreating }] = useCreateClienteMutation();
  const [updateCliente, { isLoading: isUpdating }] = useUpdateClienteMutation();

  const isEdit = !!cliente;
  const isLoading = isCreating || isUpdating;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm<ClienteFormData>({
    resolver: zodResolver(clienteSchema),
    defaultValues: {
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
    },
  });

  const tipoCliente = watch('tipo_Cliente');

  // Resetear formulario cuando cambia el cliente o se abre/cierra el modal
  useEffect(() => {
    if (isOpen) {
      if (cliente) {
        reset({
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
        reset({
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
    }
  }, [cliente, isOpen, reset]);

  const onSubmit = async (data: ClienteFormData) => {
    try {
      if (isEdit) {
        await updateCliente({
          id: cliente.id,
          tipo_Cliente: data.tipo_Cliente,
          nombre: data.nombre,
          documento: data.documento,
          tipo_Documento: data.tipo_Documento,
          email: data.email || undefined,
          telefono: data.telefono || undefined,
          direccion: data.direccion || undefined,
          ciudad: data.ciudad || undefined,
          contacto_Nombre: data.contacto_Nombre || undefined,
          contacto_Telefono: data.contacto_Telefono || undefined,
          observaciones: data.observaciones || undefined,
          rating: cliente.rating,
          estado: cliente.estado,
        }).unwrap();
        
        toast({
          title: 'Cliente actualizado',
          description: `El cliente "${data.nombre}" fue actualizado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      } else {
        await createCliente({
          tipo_Cliente: data.tipo_Cliente,
          nombre: data.nombre,
          documento: data.documento,
          tipo_Documento: data.tipo_Documento,
          email: data.email || undefined,
          telefono: data.telefono || undefined,
          direccion: data.direccion || undefined,
          ciudad: data.ciudad || undefined,
          contacto_Nombre: data.contacto_Nombre || undefined,
          contacto_Telefono: data.contacto_Telefono || undefined,
          observaciones: data.observaciones || undefined,
        }).unwrap();
        
        toast({
          title: 'Cliente creado',
          description: `El cliente "${data.nombre}" fue creado exitosamente.`,
          status: 'success',
          duration: 3000,
          isClosable: true,
        });
      }
      
      handleClose();
    } catch (error: any) {
      const errorMessage = error?.data?.message || error?.message || 'Ocurrió un error al guardar el cliente';
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

  // Obtener color mode y colores del hook personalizado
  const { bgColor, inputBg, borderColor } = useColorModeLocal();

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
                      {...register('tipo_Cliente')}
                      size={{ base: "sm", md: "md" }}
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
                      {...register('tipo_Documento')}
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    >
                      <option value="CC">Cédula de Ciudadanía</option>
                      <option value="NIT">NIT</option>
                      <option value="CE">Cédula de Extranjería</option>
                      <option value="PAS">Pasaporte</option>
                    </Select>
                  </FormControl>
                </GridItem>
              </Grid>

              <FormControl isRequired isInvalid={!!errors.nombre}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Nombre Completo / Razón Social</FormLabel>
                <Input
                  {...register('nombre')}
                  placeholder={tipoCliente === 'Persona' ? 'Ej: Juan Pérez' : 'Ej: Empresa ABC S.A.S.'}
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.nombre?.message}</FormErrorMessage>
              </FormControl>

              <FormControl isRequired isInvalid={!!errors.documento}>
                <FormLabel fontSize={{ base: "sm", md: "md" }}>Documento de Identidad</FormLabel>
                <Input
                  {...register('documento')}
                  placeholder="123456789"
                  bg={inputBg}
                  borderColor={borderColor}
                  size={{ base: "sm", md: "md" }}
                />
                <FormErrorMessage>{errors.documento?.message}</FormErrorMessage>
              </FormControl>

              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl isInvalid={!!errors.email}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Email</FormLabel>
                    <Input
                      type="email"
                      {...register('email')}
                      placeholder="ejemplo@correo.com"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.email?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>

                <GridItem>
                  <FormControl isInvalid={!!errors.telefono}>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Teléfono</FormLabel>
                    <Input
                      {...register('telefono')}
                      placeholder="300 123 4567"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                    <FormErrorMessage>{errors.telefono?.message}</FormErrorMessage>
                  </FormControl>
                </GridItem>
              </Grid>

              <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                <GridItem>
                  <FormControl>
                    <FormLabel fontSize={{ base: "sm", md: "md" }}>Ciudad</FormLabel>
                    <Input
                      {...register('ciudad')}
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
                      {...register('direccion')}
                      placeholder="Calle 123 #45-67"
                      bg={inputBg}
                      borderColor={borderColor}
                      size={{ base: "sm", md: "md" }}
                    />
                  </FormControl>
                </GridItem>
              </Grid>

              {tipoCliente === 'Empresa' && (
                <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)" }} gap={4} w="full">
                  <GridItem>
                    <FormControl>
                      <FormLabel fontSize={{ base: "sm", md: "md" }}>Contacto - Nombre</FormLabel>
                      <Input
                        {...register('contacto_Nombre')}
                        size={{ base: "sm", md: "md" }}
                        placeholder="Nombre del contacto"
                        bg={inputBg}
                        borderColor={borderColor}
                      />
                    </FormControl>
                  </GridItem>

                  <GridItem>
                    <FormControl isInvalid={!!errors.contacto_Telefono}>
                      <FormLabel fontSize={{ base: "sm", md: "md" }}>Contacto - Teléfono</FormLabel>
                      <Input
                        {...register('contacto_Telefono')}
                        placeholder="300 123 4567"
                        bg={inputBg}
                        borderColor={borderColor}
                        size={{ base: "sm", md: "md" }}
                      />
                      <FormErrorMessage>{errors.contacto_Telefono?.message}</FormErrorMessage>
                    </FormControl>
                  </GridItem>
                </Grid>
              )}

              <FormControl>
                <FormLabel>Observaciones</FormLabel>
                <Textarea
                  {...register('observaciones')}
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
