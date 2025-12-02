'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Text,
  VStack,
  HStack,
  Input,
  InputGroup,
  InputLeftElement,
  IconButton,
  Avatar,
  Flex,
  Badge,
  Divider,
  useColorMode,
  Icon,
  Center,
  Spinner,
} from '@chakra-ui/react';
import { Search, Send, MessageCircle } from 'lucide-react';
import { useAppSelector } from '../../../store/store';

interface ConversacionDTO {
  id: number;
  empresa_Id: number;
  asunto?: string;
  reserva_Id?: number;
  fecha_Creacion: string;
  estado: string;
  ultimo_Mensaje?: MensajeDTO;
  mensajes_No_Leidos: number;
}

interface MensajeDTO {
  id: number;
  conversacion_Id: number;
  emisor_Usuario_Id: number;
  emisor_Nombre: string;
  emisor_Avatar?: string;
  contenido: string;
  leido: boolean;
  fecha_Envio: string;
}

export default function MensajesPage() {
  const { colorMode } = useColorMode();
  const { user } = useAppSelector((state) => state.auth);
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const [selectedChat, setSelectedChat] = useState<number | null>(null);
  const [mensaje, setMensaje] = useState('');
  const [conversaciones, setConversaciones] = useState<ConversacionDTO[]>([]);
  const [mensajes, setMensajes] = useState<MensajeDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  useEffect(() => {
    const fetchConversaciones = async () => {
      try {
        const token = localStorage.getItem('token');
        const response = await fetch('http://localhost:5555/api/Chat/conversaciones', {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        if (response.ok) {
          const data = await response.json();
          setConversaciones(data);
        }
      } catch (error) {
        console.error('Error al cargar conversaciones:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchConversaciones();
  }, []);

  useEffect(() => {
    if (selectedChat) {
      const fetchMensajes = async () => {
        try {
          const token = localStorage.getItem('token');
          const response = await fetch(`http://localhost:5555/api/Chat/mensajes/${selectedChat}`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
          if (response.ok) {
            const data = await response.json();
            setMensajes(data);
          }
        } catch (error) {
          console.error('Error al cargar mensajes:', error);
        }
      };

      fetchMensajes();
    }
  }, [selectedChat]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  if (isLoading) {
    return (
      <Center h="100vh">
        <Spinner size="xl" color="blue.500" />
      </Center>
    );
  }

  return (
    <Box bg={bgColor} minH="100vh">
      <Container maxW="container.xl" py={6}>
        <Flex gap={4} h="calc(100vh - 150px)">
          {/* Lista de Chats */}
          <Box
            w={{ base: '100%', md: '350px' }}
            bg={cardBg}
            borderWidth="1px"
            borderColor={borderColor}
            borderRadius="xl"
            p={4}
          >
            <VStack spacing={4} align="stretch">
              <Text fontSize="xl" fontWeight="bold">
                Mensajes
              </Text>
              <InputGroup>
                <InputLeftElement>
                  <Icon as={Search} color="gray.400" />
                </InputLeftElement>
                <Input placeholder="Buscar conversaciones..." />
              </InputGroup>
              <Divider />
              <VStack spacing={2} align="stretch" overflowY="auto">
                {conversaciones.length === 0 ? (
                  <Center py={10}>
                    <VStack>
                      <Icon as={MessageCircle} boxSize={12} color="gray.400" />
                      <Text color="gray.500">No hay conversaciones</Text>
                    </VStack>
                  </Center>
                ) : (
                  conversaciones.map((conv) => (
                    <Box
                      key={conv.id}
                      p={3}
                      borderRadius="lg"
                      bg={selectedChat === conv.id ? 'blue.50' : 'transparent'}
                      cursor="pointer"
                      _hover={{ bg: 'gray.50' }}
                      onClick={() => setSelectedChat(conv.id)}
                    >
                      <Flex gap={3} align="center">
                        <Avatar 
                          name={conv.ultimo_Mensaje?.emisor_Nombre || 'Usuario'} 
                          src={conv.ultimo_Mensaje?.emisor_Avatar} 
                          size="md" 
                        />
                        <VStack flex={1} align="start" spacing={0}>
                          <Text fontWeight="bold" fontSize="sm">
                            {conv.asunto || `Conversación #${conv.id}`}
                          </Text>
                          <Text fontSize="xs" color="blue.600">
                            {conv.estado}
                          </Text>
                          <Text fontSize="xs" color="gray.500" noOfLines={1}>
                            {conv.ultimo_Mensaje?.contenido || 'Sin mensajes'}
                          </Text>
                        </VStack>
                        <VStack spacing={1}>
                          <Text fontSize="xs" color="gray.400">
                            {conv.ultimo_Mensaje 
                              ? new Date(conv.ultimo_Mensaje.fecha_Envio).toLocaleDateString('es-ES', { 
                                  hour: '2-digit', 
                                  minute: '2-digit' 
                                })
                              : ''
                            }
                          </Text>
                          {conv.mensajes_No_Leidos > 0 && (
                            <Badge colorScheme="red" borderRadius="full">
                              {conv.mensajes_No_Leidos}
                            </Badge>
                          )}
                        </VStack>
                      </Flex>
                    </Box>
                  ))
                )}
              </VStack>
            </VStack>
          </Box>

          {/* Ventana de Chat */}
          <Box
            flex={1}
            bg={cardBg}
            borderWidth="1px"
            borderColor={borderColor}
            borderRadius="xl"
            display={{ base: 'none', md: 'flex' }}
            flexDirection="column"
          >
            {selectedChat ? (
              <>
                {/* Header Chat */}
                <Flex p={4} borderBottomWidth="1px" borderColor={borderColor} align="center" gap={3}>
                  <Avatar 
                    name={conversaciones.find(c => c.id === selectedChat)?.asunto || 'Chat'} 
                    size="sm" 
                  />
                  <VStack align="start" spacing={0} flex={1}>
                    <Text fontWeight="bold">
                      {conversaciones.find(c => c.id === selectedChat)?.asunto || `Conversación #${selectedChat}`}
                    </Text>
                    <Text fontSize="xs" color="blue.600">
                      {conversaciones.find(c => c.id === selectedChat)?.estado || 'Activo'}
                    </Text>
                  </VStack>
                </Flex>

                {/* Mensajes */}
                <VStack flex={1} p={4} spacing={3} overflowY="auto" align="stretch">
                  {mensajes.length === 0 ? (
                    <Center h="100%">
                      <Text color="gray.500">No hay mensajes en esta conversación</Text>
                    </Center>
                  ) : (
                    mensajes.map((msg) => {
                      const esPropio = msg.emisor_Usuario_Id === user?.id;
                      return (
                        <Flex key={msg.id} justify={esPropio ? 'flex-end' : 'flex-start'}>
                          <Box
                            maxW="70%"
                            bg={esPropio ? 'blue.500' : 'gray.100'}
                            color={esPropio ? 'white' : 'black'}
                            p={3}
                            borderRadius="lg"
                          >
                            {!esPropio && (
                              <Text fontSize="xs" fontWeight="bold" mb={1}>
                                {msg.emisor_Nombre}
                              </Text>
                            )}
                            <Text fontSize="sm">{msg.contenido}</Text>
                            <Text fontSize="xs" opacity={0.7} mt={1}>
                              {new Date(msg.fecha_Envio).toLocaleTimeString('es-ES', {
                                hour: '2-digit',
                                minute: '2-digit'
                              })}
                            </Text>
                          </Box>
                        </Flex>
                      );
                    })
                  )}
                </VStack>

                {/* Input Mensaje */}
                <Flex p={4} borderTopWidth="1px" borderColor={borderColor} gap={2}>
                  <Input
                    placeholder="Escribe un mensaje..."
                    value={mensaje}
                    onChange={(e) => setMensaje(e.target.value)}
                    onKeyPress={async (e) => {
                      if (e.key === 'Enter' && mensaje.trim()) {
                        try {
                          const token = localStorage.getItem('token');
                          const response = await fetch('http://localhost:5555/api/Chat/mensajes', {
                            method: 'POST',
                            headers: {
                              'Authorization': `Bearer ${token}`,
                              'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                              conversacion_Id: selectedChat,
                              contenido: mensaje
                            })
                          });
                          if (response.ok) {
                            setMensaje('');
                            // Recargar mensajes
                            const mensajesResponse = await fetch(
                              `http://localhost:5555/api/Chat/mensajes/${selectedChat}`,
                              {
                                headers: { 'Authorization': `Bearer ${token}` }
                              }
                            );
                            if (mensajesResponse.ok) {
                              const data = await mensajesResponse.json();
                              setMensajes(data);
                            }
                          }
                        } catch (error) {
                          console.error('Error al enviar mensaje:', error);
                        }
                      }
                    }}
                  />
                  <IconButton
                    aria-label="Enviar"
                    icon={<Icon as={Send} />}
                    colorScheme="blue"
                    onClick={async () => {
                      if (mensaje.trim()) {
                        try {
                          const token = localStorage.getItem('token');
                          const response = await fetch('http://localhost:5555/api/Chat/mensajes', {
                            method: 'POST',
                            headers: {
                              'Authorization': `Bearer ${token}`,
                              'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                              conversacion_Id: selectedChat,
                              contenido: mensaje
                            })
                          });
                          if (response.ok) {
                            setMensaje('');
                            // Recargar mensajes
                            const mensajesResponse = await fetch(
                              `http://localhost:5555/api/Chat/mensajes/${selectedChat}`,
                              {
                                headers: { 'Authorization': `Bearer ${token}` }
                              }
                            );
                            if (mensajesResponse.ok) {
                              const data = await mensajesResponse.json();
                              setMensajes(data);
                            }
                          }
                        } catch (error) {
                          console.error('Error al enviar mensaje:', error);
                        }
                      }
                    }}
                  />
                </Flex>
              </>
            ) : (
              <Center h="100%">
                <VStack>
                  <Icon as={MessageCircle} boxSize={16} color="gray.300" />
                  <Text color="gray.500">Selecciona una conversación para comenzar</Text>
                </VStack>
              </Center>
            )}
          </Box>
        </Flex>
      </Container>
    </Box>
  );
}
