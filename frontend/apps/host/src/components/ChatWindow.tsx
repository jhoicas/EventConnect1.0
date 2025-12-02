'use client';

import {
  Box,
  VStack,
  HStack,
  Text,
  Input,
  IconButton,
  Avatar,
  Badge,
  Divider,
  Spinner,
  Center,
  useColorMode,
  Flex,
  Icon,
} from '@chakra-ui/react';
import { CheckIcon } from '@chakra-ui/icons';
import { Send } from 'lucide-react';
import { useState, useEffect, useRef } from 'react';
import {
  useGetMensajesQuery,
  useSendMensajeMutation,
  useMarcarLeidosMutation,
  type Mensaje,
} from '@/store/api/chatApi';
import { useAppSelector } from '@/store/store';

interface ChatWindowProps {
  conversacionId: number;
}

export function ChatWindow({ conversacionId }: ChatWindowProps) {
  const { colorMode } = useColorMode();
  const { user } = useAppSelector((state) => state.auth);
  const [mensaje, setMensaje] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const { data: mensajes = [], isLoading } = useGetMensajesQuery(conversacionId, {
    pollingInterval: 5000, // Polling cada 5 segundos
  });

  const [sendMensaje, { isLoading: isSending }] = useSendMensajeMutation();
  const [marcarLeidos] = useMarcarLeidosMutation();

  // Scroll al último mensaje
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [mensajes]);

  // Marcar como leídos cuando se monta el componente
  useEffect(() => {
    if (conversacionId && user) {
      marcarLeidos(conversacionId);
    }
  }, [conversacionId, user, marcarLeidos]);

  const handleSend = async () => {
    if (!mensaje.trim() || isSending) return;

    try {
      await sendMensaje({
        conversacion_Id: conversacionId,
        contenido: mensaje.trim(),
      }).unwrap();
      setMensaje('');
    } catch (error) {
      console.error('Error al enviar mensaje:', error);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  if (isLoading) {
    return (
      <Center h="400px">
        <Spinner size="xl" />
      </Center>
    );
  }

  return (
    <VStack spacing={0} h="100%" w="100%">
      {/* Mensajes */}
      <Box
        flex={1}
        w="100%"
        overflowY="auto"
        p={4}
        bg={colorMode === 'light' ? 'gray.50' : 'gray.800'}
      >
        <VStack spacing={3} align="stretch">
          {mensajes.map((msg: Mensaje) => {
            const isMine = msg.emisor_Usuario_Id === user?.id;
            return (
              <Flex key={msg.id} justify={isMine ? 'flex-end' : 'flex-start'}>
                <HStack
                  spacing={2}
                  maxW="70%"
                  flexDirection={isMine ? 'row-reverse' : 'row'}
                >
                  <Avatar
                    size="sm"
                    name={msg.emisor_Nombre}
                    src={msg.emisor_Avatar}
                  />
                  <Box>
                    <Box
                      bg={isMine ? 'blue.500' : colorMode === 'light' ? 'white' : 'gray.700'}
                      color={isMine ? 'white' : 'inherit'}
                      px={4}
                      py={2}
                      borderRadius="lg"
                      boxShadow="sm"
                    >
                      {!isMine && (
                        <Text fontSize="xs" fontWeight="bold" mb={1}>
                          {msg.emisor_Nombre}
                        </Text>
                      )}
                      <Text fontSize="sm" whiteSpace="pre-wrap">
                        {msg.contenido}
                      </Text>
                    </Box>
                    <HStack justify={isMine ? 'flex-end' : 'flex-start'} mt={1} spacing={1}>
                      <Text fontSize="xs" color="gray.500">
                        {new Date(msg.fecha_Envio).toLocaleTimeString('es-ES', {
                          hour: '2-digit',
                          minute: '2-digit',
                        })}
                      </Text>
                      {isMine && msg.leido && (
                        <CheckIcon w={3} h={3} color="blue.500" />
                      )}
                    </HStack>
                  </Box>
                </HStack>
              </Flex>
            );
          })}
          <div ref={messagesEndRef} />
        </VStack>
      </Box>

      <Divider />

      {/* Input de mensaje */}
      <HStack w="100%" p={4} spacing={2}>
        <Input
          value={mensaje}
          onChange={(e) => setMensaje(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Escribe un mensaje..."
          size="md"
          disabled={isSending}
        />
        <IconButton
          aria-label="Enviar mensaje"
          icon={<Icon as={Send} />}
          colorScheme="blue"
          onClick={handleSend}
          isLoading={isSending}
          disabled={!mensaje.trim()}
        />
      </HStack>
    </VStack>
  );
}
