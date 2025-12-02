'use client';

import { useEffect, useState } from 'react';
import {
  Box,
  Container,
  Heading,
  Text,
  VStack,
  useColorMode,
  List,
  ListItem,
  ListIcon,
  Divider,
  HStack,
  Icon,
  Button,
} from '@chakra-ui/react';
import { useRouter } from 'next/navigation';
import { CheckCircle, ArrowLeft, FileText, Shield } from 'lucide-react';

export default function TerminosPage() {
  const router = useRouter();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  return (
    <Box bg={bgColor} minH="100vh" py={10}>
      <Container maxW="container.lg">
        <VStack spacing={8} align="stretch">
          {/* Header */}
          <HStack spacing={4} cursor="pointer" onClick={() => router.back()}>
            <Icon as={ArrowLeft} />
            <Text>Volver</Text>
          </HStack>

          {/* Content Card */}
          <Box bg={cardBg} p={10} borderRadius="2xl" borderWidth="1px" borderColor={borderColor}>
            <VStack spacing={8} align="stretch">
              {/* Título */}
              <VStack spacing={4} align="center">
                <Icon as={FileText} boxSize={16} color="blue.500" />
                <Heading size="2xl" textAlign="center">
                  Términos y Condiciones
                </Heading>
                <Text fontSize="sm" color="gray.500">
                  Última actualización: 25 de noviembre de 2025
                </Text>
              </VStack>

              <Divider />

              {/* Contenido */}
              <VStack spacing={6} align="stretch">
                <Box>
                  <Heading size="md" mb={3}>
                    1. Aceptación de los Términos
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Al acceder y utilizar EventConnect, usted acepta estar sujeto a estos Términos y Condiciones, 
                    todas las leyes y regulaciones aplicables, y acepta que es responsable del cumplimiento de 
                    todas las leyes locales aplicables.
                  </Text>
                  <Text color="gray.600" fontSize="sm" fontStyle="italic">
                    <strong>Cumplimiento normativo:</strong> ISO 27001:2013 (Gestión de Seguridad de la Información)
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    2. Descripción del Servicio
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    EventConnect es una plataforma de software como servicio (SaaS) diseñada para la gestión 
                    integral de empresas de alquiler de equipos y organización de eventos. Los servicios incluyen:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Gestión de inventarios y productos
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Control de reservas y clientes
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Sistema Integrado de Gestión de Inventarios (SIGI)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Gestión de activos fijos y mantenimientos
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Reportes y análisis de datos
                    </ListItem>
                  </List>
                  <Text color="gray.600" fontSize="sm" fontStyle="italic" mt={2}>
                    <strong>Cumplimiento normativo:</strong> ISO 9001:2015 (Gestión de Calidad)
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    3. Registro y Cuentas de Usuario
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    <strong>3.1 Tipos de Usuario:</strong>
                  </Text>
                  <List spacing={2} ml={4} mb={3}>
                    <ListItem>
                      • <strong>Persona:</strong> Usuario individual con acceso inmediato tras el registro
                    </ListItem>
                    <ListItem>
                      • <strong>Empresa:</strong> Organización que requiere aprobación administrativa previa
                    </ListItem>
                  </List>
                  <Text color="gray.600" mb={2}>
                    <strong>3.2 Responsabilidades del Usuario:</strong>
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="blue.500" />
                      Proporcionar información precisa y actualizada
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="blue.500" />
                      Mantener la confidencialidad de sus credenciales
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="blue.500" />
                      Notificar inmediatamente cualquier uso no autorizado de su cuenta
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="blue.500" />
                      Cumplir con todas las leyes aplicables al usar el servicio
                    </ListItem>
                  </List>
                  <Text color="gray.600" fontSize="sm" fontStyle="italic" mt={2}>
                    <strong>Cumplimiento normativo:</strong> ISO 27002:2013 (Controles de Seguridad)
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    4. Planes y Facturación
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    <strong>4.1 Plan Básico (Gratuito):</strong>
                  </Text>
                  <Text color="gray.600" mb={3} ml={4}>
                    Incluye funcionalidades básicas de gestión de productos, categorías, clientes y reservas. 
                    Disponible de forma gratuita para siempre.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>4.2 Plan SIGI ($49 USD/mes):</strong>
                  </Text>
                  <Text color="gray.600" mb={3} ml={4}>
                    Incluye todas las funcionalidades del Plan Básico más el Sistema Integrado de Gestión de 
                    Inventarios con activos fijos, bodegas, lotes, mantenimientos y depreciación automática.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>4.3 Facturación:</strong>
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>• Los planes de pago se facturan mensualmente por adelantado</ListItem>
                    <ListItem>• Los pagos se procesan el mismo día cada mes</ListItem>
                    <ListItem>• Puede cancelar su suscripción en cualquier momento</ListItem>
                    <ListItem>• No se realizan reembolsos por períodos parciales</ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    5. Propiedad Intelectual
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    EventConnect y su contenido original, características y funcionalidad son y seguirán siendo 
                    propiedad exclusiva de EventConnect y sus licenciantes. El servicio está protegido por derechos 
                    de autor, marcas registradas y otras leyes.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>Sus Datos:</strong> Usted conserva todos los derechos sobre los datos que ingresa en 
                    la plataforma. Otorga a EventConnect una licencia para usar, almacenar y procesar dichos datos 
                    únicamente con el propósito de proporcionar el servicio.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    6. Protección de Datos y Privacidad
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Nos comprometemos a proteger su información personal de acuerdo con nuestra{' '}
                    <Text as="span" color="blue.500" cursor="pointer" onClick={() => router.push('/politica-privacidad')}>
                      Política de Privacidad
                    </Text>{' '}
                    y las siguientes normativas:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      ISO 27001:2013 - Sistema de Gestión de Seguridad de la Información
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      ISO 27701:2019 - Gestión de la Privacidad de la Información
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      Ley 1581 de 2012 (Colombia) - Protección de Datos Personales
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      GDPR - Reglamento General de Protección de Datos (aplicable a usuarios de la UE)
                    </ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    7. Disponibilidad del Servicio
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Nos esforzamos por mantener el servicio disponible 24/7, pero no garantizamos que el servicio 
                    estará siempre disponible, ininterrumpido o libre de errores. Podemos:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>• Realizar mantenimiento programado con previo aviso</ListItem>
                    <ListItem>• Suspender temporalmente el servicio por razones técnicas</ListItem>
                    <ListItem>• Actualizar o modificar el servicio sin previo aviso</ListItem>
                  </List>
                  <Text color="gray.600" fontSize="sm" fontStyle="italic" mt={2}>
                    <strong>Compromiso de disponibilidad:</strong> ISO 20000-1:2018 (Gestión de Servicios de TI)
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    8. Limitación de Responsabilidad
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    En ningún caso EventConnect, sus directores, empleados o agentes serán responsables de daños 
                    indirectos, incidentales, especiales, consecuentes o punitivos, incluida la pérdida de 
                    beneficios, datos, uso u otras pérdidas intangibles, resultantes de:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>• Su acceso o uso (o incapacidad de acceso o uso) del servicio</ListItem>
                    <ListItem>• Cualquier conducta o contenido de terceros en el servicio</ListItem>
                    <ListItem>• Cualquier contenido obtenido del servicio</ListItem>
                    <ListItem>• Acceso no autorizado, uso o alteración de sus transmisiones o contenido</ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    9. Terminación
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Podemos terminar o suspender su cuenta inmediatamente, sin previo aviso o responsabilidad, 
                    por cualquier motivo, incluyendo sin limitación si incumple estos Términos.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    Al finalizar, su derecho a usar el servicio cesará inmediatamente. Si desea terminar su cuenta, 
                    puede simplemente dejar de usar el servicio o contactar al soporte.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    10. Ley Aplicable y Jurisdicción
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Estos Términos se regirán e interpretarán de acuerdo con las leyes de la República de Colombia, 
                    sin tener en cuenta sus disposiciones sobre conflictos de leyes.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    Cualquier disputa relacionada con estos términos estará sujeta a la jurisdicción exclusiva de 
                    los tribunales de Colombia.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    11. Modificaciones
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Nos reservamos el derecho, a nuestra sola discreción, de modificar o reemplazar estos Términos 
                    en cualquier momento. Si una revisión es material, intentaremos proporcionar un aviso de al 
                    menos 30 días antes de que entren en vigencia los nuevos términos.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    Al continuar accediendo o usando nuestro servicio después de que esas revisiones entren en 
                    vigencia, acepta estar sujeto a los términos revisados.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    12. Contacto
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Si tiene alguna pregunta sobre estos Términos y Condiciones, puede contactarnos:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>• Por email: legal@eventconnect.com</ListItem>
                    <ListItem>• Por email de soporte: soporte@eventconnect.com</ListItem>
                    <ListItem>• A través de nuestro sistema de tickets en la plataforma</ListItem>
                  </List>
                </Box>

                <Box bg="blue.50" p={6} borderRadius="lg" borderWidth="1px" borderColor="blue.200">
                  <VStack align="start" spacing={3}>
                    <HStack>
                      <Icon as={Shield} color="blue.600" />
                      <Heading size="sm" color="blue.700">
                        Certificaciones y Cumplimiento Normativo
                      </Heading>
                    </HStack>
                    <Text fontSize="sm" color="blue.600">
                      EventConnect se compromete a cumplir con los más altos estándares internacionales:
                    </Text>
                    <List spacing={1} fontSize="sm" color="blue.600">
                      <ListItem>• ISO 27001:2013 - Seguridad de la Información</ListItem>
                      <ListItem>• ISO 27002:2013 - Controles de Seguridad</ListItem>
                      <ListItem>• ISO 27701:2019 - Gestión de Privacidad</ListItem>
                      <ListItem>• ISO 9001:2015 - Gestión de Calidad</ListItem>
                      <ListItem>• ISO 20000-1:2018 - Gestión de Servicios de TI</ListItem>
                      <ListItem>• Ley 1581 de 2012 - Protección de Datos Personales (Colombia)</ListItem>
                      <ListItem>• GDPR - Reglamento General de Protección de Datos (UE)</ListItem>
                    </List>
                  </VStack>
                </Box>

                <Divider />

                <Box textAlign="center">
                  <Text fontSize="sm" color="gray.500" mb={4}>
                    Al usar EventConnect, usted acepta estos Términos y Condiciones
                  </Text>
                  <HStack justify="center" spacing={4}>
                    <Button variant="outline" onClick={() => router.back()}>
                      Volver
                    </Button>
                    <Button colorScheme="blue" onClick={() => router.push('/politica-privacidad')}>
                      Ver Política de Privacidad
                    </Button>
                  </HStack>
                </Box>
              </VStack>
            </VStack>
          </Box>
        </VStack>
      </Container>
    </Box>
  );
}
