'use client';

import {
  Box,
  Container,
  Heading,
  HStack,
  VStack,
  Text,
  Button,
  useColorMode,
  Grid,
  GridItem,
  Badge,
  Avatar,
  Divider,
  Spinner,
  Center,
  useDisclosure,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  ModalCloseButton,
  Input,
  Textarea,
  useToast,
} from '@chakra-ui/react';
import { AddIcon, ChatIcon } from '@chakra-ui/icons';
import { useState } from 'react';
import {
  useGetConversacionesQuery,
  useCreateConversacionMutation,
  type Conversacion,
} from '../../store/api/chatApi';
import { ChatWindow } from '../../components/ChatWindow';

export default function ChatPage() {
  const { colorMode } = useColorMode();
  const toast = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [selectedConversacion, setSelectedConversacion] = useState<number | null>(null);
  const [asunto, setAsunto] = useState('');
  const [mensajeInicial, setMensajeInicial] = useState('');

  const { data: conversaciones = [], isLoading } = useGetConversacionesQuery();
  const [createConversacion, { isLoading: isCreating }] = useCreateConversacionMutation();

  const handleCreateConversacion = async () => {
    if (!asunto.trim()) {
      toast({
        title: 'Error',
        description: 'El asunto es requerido',
        status: 'error',
        duration: 3000,
        isClosable: true,
      });
      return;
    }

    try {
      const result = await createConversacion({
        asunto: asunto.trim(),
        mensaje_Inicial: mensajeInicial.trim() || undefined,
      }).unwrap();

      setSelectedConversacion(result.id);
      setAsunto('');
      setMensajeInicial('');
      onClose();

      toast({
        title: 'Conversación creada',
        status: 'success',
        duration: 3000,
        isClosable: true,
      });
    } catch (error) {
      toast({
        title: 'Error al crear conversación',
        status: 'error',
        duration: 3000,
        isClosable: true,
      });
    }
  };

  return (
    <Container maxW="container.xl" py={{ base: 4, md: 8 }}>
      <VStack spacing={{ base: 4, md: 6 }} align="stretch">
        {/* Header */}
        <HStack justify="space-between" flexWrap={{ base: 'wrap', sm: 'nowrap' }} gap={2}>
          <Heading size={{ base: 'md', md: 'lg' }}>
            <ChatIcon mr={2} />
            Chat
          </Heading>
          <Button
            leftIcon={<AddIcon />}
            colorScheme="blue"
            onClick={onOpen}
            size={{ base: 'sm', md: 'md' }}
            width={{ base: 'full', sm: 'auto' }}
          >
            Nueva Conversación
          </Button>
        </HStack>

        {/* Contenido */}
        {isLoading ? (
          <Center h="400px">
            <Spinner size="xl" />
          </Center>
        ) : (
          <Grid
            templateColumns={{ base: '1fr', md: '350px 1fr' }}
            gap={4}
            h="calc(100vh - 250px)"
          >
            {/* Lista de conversaciones */}
            <GridItem
              bg={colorMode === 'light' ? 'white' : 'gray.800'}
              borderRadius="md"
              boxShadow="sm"
              overflowY="auto"
              display={{ base: selectedConversacion ? 'none' : 'block', md: 'block' }}
            >
              <VStack spacing={0} divider={<Divider />} align="stretch">
                {conversaciones.length === 0 ? (
                  <Box p={8} textAlign="center">
                    <Text color="gray.500">No hay conversaciones</Text>
                  </Box>
                ) : (
                  conversaciones.map((conv: Conversacion) => (
                    <Box
                      key={conv.id}
                      p={4}
                      cursor="pointer"
                      bg={
                        selectedConversacion === conv.id
                          ? colorMode === 'light'
                            ? 'blue.50'
                            : 'blue.900'
                          : 'transparent'
                      }
                      _hover={{
                        bg: colorMode === 'light' ? 'gray.50' : 'gray.700',
                      }}
                      onClick={() => setSelectedConversacion(conv.id)}
                    >
                      <HStack spacing={3} align="start">
                        <Avatar size="md" icon={<ChatIcon />} />
                        <VStack align="start" spacing={1} flex={1}>
                          <HStack justify="space-between" w="100%">
                            <Text fontWeight="bold" fontSize="sm" noOfLines={1}>
                              {conv.asunto || 'Sin asunto'}
                            </Text>
                            {conv.mensajes_No_Leidos > 0 && (
                              <Badge colorScheme="red" borderRadius="full">
                                {conv.mensajes_No_Leidos}
                              </Badge>
                            )}
                          </HStack>
                          {conv.ultimo_Mensaje && (
                            <Text fontSize="xs" color="gray.500" noOfLines={2}>
                              {conv.ultimo_Mensaje.contenido}
                            </Text>
                          )}
                          <Text fontSize="xs" color="gray.400">
                            {new Date(conv.fecha_Creacion).toLocaleDateString('es-ES')}
                          </Text>
                        </VStack>
                      </HStack>
                    </Box>
                  ))
                )}
              </VStack>
            </GridItem>

            {/* Ventana de chat */}
            <GridItem
              bg={colorMode === 'light' ? 'white' : 'gray.800'}
              borderRadius="md"
              boxShadow="sm"
              display={{ base: selectedConversacion ? 'block' : 'none', md: 'block' }}
            >
              {selectedConversacion ? (
                <>
                  <Button
                    display={{ base: 'block', md: 'none' }}
                    size="sm"
                    m={2}
                    onClick={() => setSelectedConversacion(null)}
                  >
                    ← Volver
                  </Button>
                  <ChatWindow conversacionId={selectedConversacion} />
                </>
              ) : (
                <Center h="100%">
                  <VStack spacing={3}>
                    <ChatIcon w={12} h={12} color="gray.300" />
                    <Text color="gray.500">Selecciona una conversación</Text>
                  </VStack>
                </Center>
              )}
            </GridItem>
          </Grid>
        )}
      </VStack>

      {/* Modal Nueva Conversación */}
      <Modal isOpen={isOpen} onClose={onClose} size={{ base: 'full', md: 'md' }}>
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Nueva Conversación</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <VStack spacing={4}>
              <Box w="100%">
                <Text mb={2} fontWeight="medium">
                  Asunto *
                </Text>
                <Input
                  value={asunto}
                  onChange={(e) => setAsunto(e.target.value)}
                  placeholder="Ej: Consulta sobre cotización #123"
                />
              </Box>
              <Box w="100%">
                <Text mb={2} fontWeight="medium">
                  Mensaje inicial (opcional)
                </Text>
                <Textarea
                  value={mensajeInicial}
                  onChange={(e) => setMensajeInicial(e.target.value)}
                  placeholder="Escribe tu mensaje..."
                  rows={4}
                />
              </Box>
            </VStack>
          </ModalBody>
          <ModalFooter flexDirection={{ base: 'column', sm: 'row' }} gap={2}>
            <Button
              variant="ghost"
              onClick={onClose}
              width={{ base: 'full', sm: 'auto' }}
            >
              Cancelar
            </Button>
            <Button
              colorScheme="blue"
              onClick={handleCreateConversacion}
              isLoading={isCreating}
              width={{ base: 'full', sm: 'auto' }}
            >
              Crear Conversación
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </Container>
  );
}
