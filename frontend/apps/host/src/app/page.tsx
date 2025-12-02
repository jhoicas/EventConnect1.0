'use client';

import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Container,
  Heading,
  Text,
  VStack,
  HStack,
  SimpleGrid,
  Icon,
  Image,
  Flex,
  useColorMode,
  Badge,
  Stack,
} from '@chakra-ui/react';
import { useRouter } from 'next/navigation';
import {
  Calendar,
  Package,
  Database,
  Users,
  BarChart3,
  Building2,
  Award,
  Zap,
  Rocket,
  Crown,
  ArrowRight,
  CheckCircle,
  Sparkles,
} from 'lucide-react';
import { useGetContenidosActivosQuery } from '../store/api/contenidoLandingApi';
import { ROUTES } from '@eventconnect/shared';

const iconMap: Record<string, any> = {
  Calendar,
  Package,
  Database,
  Users,
  BarChart3,
  Building2,
  Award,
  Zap,
  Rocket,
  Crown,
  Sparkles,
  CheckCircle,
};

export default function LandingPage() {
  const router = useRouter();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const { data: contenidos = [], isLoading } = useGetContenidosActivosQuery();

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#ffffff';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#f7fafc';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  const heroContent = contenidos.find((c) => c.seccion === 'hero');
  const servicios = contenidos.filter((c) => c.seccion === 'servicios');
  const nosotros = contenidos.find((c) => c.seccion === 'nosotros');
  const planes = contenidos.filter((c) => c.seccion === 'planes');

  if (isLoading) {
    return (
      <Flex minH="100vh" align="center" justify="center" bg={bgColor}>
        <Text>Cargando...</Text>
      </Flex>
    );
  }

  return (
    <Box bg={bgColor} minH="100vh">
      {/* Navbar */}
      <Box
        position="sticky"
        top={0}
        bg={cardBg}
        borderBottom="1px"
        borderColor={borderColor}
        zIndex={10}
        backdropFilter="blur(10px)"
        bgColor={cardBg + 'ee'}
      >
        <Container maxW="container.xl" py={4}>
          <Flex justify="space-between" align="center">
            <HStack spacing={2}>
              <Icon as={Sparkles} boxSize={8} color="blue.500" />
              <Heading size="md" bgGradient="linear(to-r, blue.400, purple.500)" bgClip="text">
                EventConnect
              </Heading>
            </HStack>
            <HStack spacing={4}>
              <Button variant="ghost" onClick={() => router.push(ROUTES.LOGIN)}>
                Iniciar Sesión
              </Button>
              <Button colorScheme="blue" onClick={() => router.push('/registro')}>
                Registrarse Gratis
              </Button>
            </HStack>
          </Flex>
        </Container>
      </Box>

      {/* Hero Section */}
      <Box
        position="relative"
        overflow="hidden"
        py={20}
        bgGradient="linear(to-br, blue.500, purple.600)"
      >
        <Container maxW="container.xl">
          <SimpleGrid columns={{ base: 1, md: 2 }} spacing={10} alignItems="center">
            <VStack align="start" spacing={6}>
              <Badge colorScheme="whiteAlpha" fontSize="md" px={4} py={2} borderRadius="full">
                🚀 Plataforma Todo-en-Uno
              </Badge>
              <Heading size="2xl" color="white" lineHeight="1.2">
                {heroContent?.titulo || 'Gestiona tu Negocio de Eventos con EventConnect'}
              </Heading>
              <Text fontSize="xl" color="whiteAlpha.900">
                {heroContent?.subtitulo || 'La plataforma completa para empresas de alquiler y gestión de eventos'}
              </Text>
              <Text fontSize="md" color="whiteAlpha.800">
                {heroContent?.descripcion}
              </Text>
              <HStack spacing={4}>
                <Button
                  size="lg"
                  colorScheme="whiteAlpha"
                  rightIcon={<ArrowRight />}
                  onClick={() => router.push('/registro')}
                >
                  Comenzar Gratis
                </Button>
                <Button size="lg" variant="outline" colorScheme="whiteAlpha" onClick={() => router.push(ROUTES.LOGIN)}>
                  Ver Demo
                </Button>
              </HStack>
            </VStack>
            <Box>
              <Image
                src={heroContent?.imagen_URL || 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30'}
                alt="Hero"
                borderRadius="2xl"
                boxShadow="2xl"
                fallbackSrc="https://via.placeholder.com/600x400?text=EventConnect"
              />
            </Box>
          </SimpleGrid>
        </Container>
      </Box>

      {/* Servicios */}
      <Container maxW="container.xl" py={20}>
        <VStack spacing={12}>
          <VStack spacing={4} textAlign="center">
            <Heading size="xl">Nuestros Servicios</Heading>
            <Text fontSize="lg" color="gray.600" maxW="2xl">
              Todo lo que necesitas para gestionar tu negocio de eventos en un solo lugar
            </Text>
          </VStack>

          <SimpleGrid columns={{ base: 1, md: 2, lg: 3 }} spacing={8} w="full">
            {servicios.map((servicio) => {
              const IconComponent = servicio.icono_Nombre ? iconMap[servicio.icono_Nombre] : Package;
              return (
                <Box
                  key={servicio.id}
                  bg={cardBg}
                  p={8}
                  borderRadius="xl"
                  borderWidth="1px"
                  borderColor={borderColor}
                  transition="all 0.3s"
                  _hover={{ transform: 'translateY(-8px)', shadow: '2xl' }}
                >
                  <VStack align="start" spacing={4}>
                    <Icon as={IconComponent} boxSize={12} color="blue.500" />
                    <Heading size="md">{servicio.titulo}</Heading>
                    <Text fontSize="sm" color="gray.600" fontWeight="semibold">
                      {servicio.subtitulo}
                    </Text>
                    <Text color="gray.500">{servicio.descripcion}</Text>
                    {servicio.imagen_URL && (
                      <Image
                        src={servicio.imagen_URL}
                        alt={servicio.titulo}
                        borderRadius="md"
                        w="full"
                        h="150px"
                        objectFit="cover"
                        fallbackSrc="https://via.placeholder.com/400x150"
                      />
                    )}
                  </VStack>
                </Box>
              );
            })}
          </SimpleGrid>
        </VStack>
      </Container>

      {/* Nosotros */}
      {nosotros && (
        <Box bg={cardBg} py={20}>
          <Container maxW="container.xl">
            <SimpleGrid columns={{ base: 1, md: 2 }} spacing={10} alignItems="center">
              <Image
                src={nosotros.imagen_URL || 'https://images.unsplash.com/photo-1511578314322-379afb476865'}
                alt={nosotros.titulo}
                borderRadius="2xl"
                boxShadow="xl"
                fallbackSrc="https://via.placeholder.com/600x400"
              />
              <VStack align="start" spacing={6}>
                <Icon as={iconMap[nosotros.icono_Nombre || 'Award']} boxSize={12} color="blue.500" />
                <Heading size="xl">{nosotros.titulo}</Heading>
                <Text fontSize="lg" fontWeight="semibold" color="blue.600">
                  {nosotros.subtitulo}
                </Text>
                <Text fontSize="md" color="gray.600">
                  {nosotros.descripcion}
                </Text>
                <HStack spacing={4}>
                  <Icon as={CheckCircle} color="green.500" />
                  <Text>Más de 100 empresas confían en nosotros</Text>
                </HStack>
                <HStack spacing={4}>
                  <Icon as={CheckCircle} color="green.500" />
                  <Text>Soporte técnico 24/7</Text>
                </HStack>
                <HStack spacing={4}>
                  <Icon as={CheckCircle} color="green.500" />
                  <Text>Actualizaciones constantes</Text>
                </HStack>
              </VStack>
            </SimpleGrid>
          </Container>
        </Box>
      )}

      {/* Planes */}
      <Container maxW="container.xl" py={20}>
        <VStack spacing={12}>
          <VStack spacing={4} textAlign="center">
            <Heading size="xl">Elige tu Plan</Heading>
            <Text fontSize="lg" color="gray.600" maxW="2xl">
              Comienza gratis y escala cuando tu negocio crezca
            </Text>
          </VStack>

          <SimpleGrid columns={{ base: 1, md: 3 }} spacing={8} w="full">
            {planes.map((plan, idx) => {
              const IconComponent = plan.icono_Nombre ? iconMap[plan.icono_Nombre] : Zap;
              const isPopular = idx === 1;
              return (
                <Box
                  key={plan.id}
                  bg={cardBg}
                  p={8}
                  borderRadius="xl"
                  borderWidth="2px"
                  borderColor={isPopular ? 'blue.500' : borderColor}
                  position="relative"
                  transition="all 0.3s"
                  _hover={{ transform: 'translateY(-8px)', shadow: '2xl' }}
                >
                  {isPopular && (
                    <Badge
                      position="absolute"
                      top={-3}
                      right={4}
                      colorScheme="blue"
                      fontSize="sm"
                      px={4}
                      py={1}
                      borderRadius="full"
                    >
                      Más Popular
                    </Badge>
                  )}
                  <VStack spacing={6} align="start">
                    <Icon as={IconComponent} boxSize={12} color="blue.500" />
                    <Heading size="lg">{plan.titulo}</Heading>
                    <Heading size="2xl" color="blue.600">
                      {plan.subtitulo}
                    </Heading>
                    <Text color="gray.600">{plan.descripcion}</Text>
                    <Button
                      w="full"
                      size="lg"
                      colorScheme={isPopular ? 'blue' : 'gray'}
                      variant={isPopular ? 'solid' : 'outline'}
                      onClick={() => router.push('/registro')}
                    >
                      {idx === 0 ? 'Comenzar Gratis' : 'Contactar Ventas'}
                    </Button>
                  </VStack>
                </Box>
              );
            })}
          </SimpleGrid>
        </VStack>
      </Container>

      {/* CTA Final */}
      <Box bg="blue.500" py={20}>
        <Container maxW="container.xl">
          <VStack spacing={8} textAlign="center">
            <Heading size="2xl" color="white">
              ¿Listo para Transformar tu Negocio?
            </Heading>
            <Text fontSize="xl" color="whiteAlpha.900" maxW="2xl">
              Únete a cientos de empresas que ya usan EventConnect para gestionar sus eventos
            </Text>
            <HStack spacing={4}>
              <Button
                size="lg"
                colorScheme="whiteAlpha"
                rightIcon={<ArrowRight />}
                onClick={() => router.push('/registro')}
              >
                Registrarse Ahora
              </Button>
              <Button size="lg" variant="outline" colorScheme="whiteAlpha" onClick={() => router.push(ROUTES.LOGIN)}>
                Iniciar Sesión
              </Button>
            </HStack>
          </VStack>
        </Container>
      </Box>

      {/* Footer */}
      <Box bg={cardBg} borderTop="1px" borderColor={borderColor} py={8}>
        <Container maxW="container.xl">
          <Flex justify="space-between" align="center" flexWrap="wrap" gap={4}>
            <HStack spacing={2}>
              <Icon as={Sparkles} boxSize={6} color="blue.500" />
              <Text fontWeight="bold">EventConnect</Text>
            </HStack>
            <Text fontSize="sm" color="gray.500">
              © 2025 EventConnect. Todos los derechos reservados.
            </Text>
            <HStack spacing={6}>
              <Text
                as="a"
                href="/terminos"
                target="_blank"
                fontSize="sm"
                color="gray.500"
                cursor="pointer"
                _hover={{ color: 'blue.500' }}
              >
                Términos
              </Text>
              <Text
                as="a"
                href="/politica-privacidad"
                target="_blank"
                fontSize="sm"
                color="gray.500"
                cursor="pointer"
                _hover={{ color: 'blue.500' }}
              >
                Privacidad
              </Text>
              <Text
                as="a"
                href="mailto:soporte@eventconnect.com"
                fontSize="sm"
                color="gray.500"
                cursor="pointer"
                _hover={{ color: 'blue.500' }}
              >
                Contacto
              </Text>
            </HStack>
          </Flex>
        </Container>
      </Box>
    </Box>
  );
}

