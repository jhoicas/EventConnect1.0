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
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
} from '@chakra-ui/react';
import { useRouter } from 'next/navigation';
import { CheckCircle, ArrowLeft, Shield, Lock, Eye, Database } from 'lucide-react';

export default function PoliticaPrivacidadPage() {
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
                <Icon as={Shield} boxSize={16} color="green.500" />
                <Heading size="2xl" textAlign="center">
                  Política de Privacidad
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
                    1. Introducción
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    EventConnect ("nosotros", "nuestro" o "la plataforma") se compromete a proteger su privacidad 
                    y sus datos personales. Esta Política de Privacidad explica cómo recopilamos, usamos, 
                    compartimos y protegemos su información personal de acuerdo con:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      Ley 1581 de 2012 y Decreto 1377 de 2013 (Colombia)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      GDPR - Reglamento (UE) 2016/679 (para usuarios de la UE)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      ISO 27001:2013 - Gestión de Seguridad de la Información
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      ISO 27701:2019 - Sistema de Gestión de Privacidad de la Información
                    </ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    2. Responsable del Tratamiento de Datos
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    <strong>Razón Social:</strong> EventConnect S.A.S.
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>Domicilio:</strong> Colombia
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>Correo de contacto:</strong> privacidad@eventconnect.com
                  </Text>
                  <Text color="gray.600" mb={2}>
                    <strong>Delegado de Protección de Datos:</strong> dpo@eventconnect.com
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    3. Datos Personales que Recopilamos
                  </Heading>
                  <Text color="gray.600" mb={3}>
                    Recopilamos diferentes tipos de información según el tipo de usuario y el uso del servicio:
                  </Text>

                  <Heading size="sm" mb={2}>
                    3.1 Usuarios Persona
                  </Heading>
                  <Table size="sm" variant="simple" mb={4}>
                    <Thead>
                      <Tr>
                        <Th>Tipo de Dato</Th>
                        <Th>Ejemplos</Th>
                        <Th>Finalidad</Th>
                      </Tr>
                    </Thead>
                    <Tbody>
                      <Tr>
                        <Td>Datos de identificación</Td>
                        <Td>Nombre, apellido, documento</Td>
                        <Td>Identificación del usuario</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de contacto</Td>
                        <Td>Email, teléfono</Td>
                        <Td>Comunicación y soporte</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de acceso</Td>
                        <Td>Usuario, contraseña (cifrada)</Td>
                        <Td>Autenticación y seguridad</Td>
                      </Tr>
                    </Tbody>
                  </Table>

                  <Heading size="sm" mb={2}>
                    3.2 Usuarios Empresa
                  </Heading>
                  <Table size="sm" variant="simple" mb={4}>
                    <Thead>
                      <Tr>
                        <Th>Tipo de Dato</Th>
                        <Th>Ejemplos</Th>
                        <Th>Finalidad</Th>
                      </Tr>
                    </Thead>
                    <Tbody>
                      <Tr>
                        <Td>Datos de la empresa</Td>
                        <Td>Razón social, NIT, dirección</Td>
                        <Td>Identificación corporativa</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de contacto</Td>
                        <Td>Email corporativo, teléfono</Td>
                        <Td>Comunicación empresarial</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de facturación</Td>
                        <Td>Información de pago, banco</Td>
                        <Td>Procesamiento de pagos</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos operacionales</Td>
                        <Td>Inventarios, clientes, reservas</Td>
                        <Td>Prestación del servicio</Td>
                      </Tr>
                    </Tbody>
                  </Table>

                  <Heading size="sm" mb={2}>
                    3.3 Datos Recopilados Automáticamente
                  </Heading>
                  <List spacing={2} ml={4}>
                    <ListItem>• Dirección IP y ubicación geográfica aproximada</ListItem>
                    <ListItem>• Tipo de navegador y sistema operativo</ListItem>
                    <ListItem>• Páginas visitadas y tiempo de permanencia</ListItem>
                    <ListItem>• Acciones realizadas en la plataforma</ListItem>
                    <ListItem>• Cookies y tecnologías similares</ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    4. Base Legal y Finalidades del Tratamiento
                  </Heading>
                  <Text color="gray.600" mb={3}>
                    Tratamos sus datos personales con base en las siguientes bases legales:
                  </Text>

                  <VStack align="stretch" spacing={3}>
                    <Box p={4} bg="blue.50" borderRadius="md">
                      <HStack mb={2}>
                        <Icon as={CheckCircle} color="blue.600" />
                        <Text fontWeight="bold" color="blue.700">
                          Consentimiento
                        </Text>
                      </HStack>
                      <Text fontSize="sm" color="blue.600">
                        Al registrarse, usted otorga su consentimiento libre, expreso e informado para el 
                        tratamiento de sus datos personales según esta política.
                      </Text>
                    </Box>

                    <Box p={4} bg="green.50" borderRadius="md">
                      <HStack mb={2}>
                        <Icon as={CheckCircle} color="green.600" />
                        <Text fontWeight="bold" color="green.700">
                          Ejecución del Contrato
                        </Text>
                      </HStack>
                      <Text fontSize="sm" color="green.600">
                        El tratamiento es necesario para la prestación del servicio contratado y el cumplimiento 
                        de nuestros Términos y Condiciones.
                      </Text>
                    </Box>

                    <Box p={4} bg="orange.50" borderRadius="md">
                      <HStack mb={2}>
                        <Icon as={CheckCircle} color="orange.600" />
                        <Text fontWeight="bold" color="orange.700">
                          Obligación Legal
                        </Text>
                      </HStack>
                      <Text fontSize="sm" color="orange.600">
                        Cumplimiento de obligaciones legales, fiscales y contables según la legislación aplicable.
                      </Text>
                    </Box>

                    <Box p={4} bg="purple.50" borderRadius="md">
                      <HStack mb={2}>
                        <Icon as={CheckCircle} color="purple.600" />
                        <Text fontWeight="bold" color="purple.700">
                          Interés Legítimo
                        </Text>
                      </HStack>
                      <Text fontSize="sm" color="purple.600">
                        Mejora del servicio, prevención de fraude, seguridad de la plataforma y comunicaciones 
                        comerciales relacionadas con el servicio.
                      </Text>
                    </Box>
                  </VStack>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    5. Cómo Usamos sus Datos
                  </Heading>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Prestación del servicio:</strong> Crear y gestionar su cuenta, procesar reservas, 
                      gestionar inventarios y proporcionar las funcionalidades de la plataforma
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Comunicación:</strong> Enviar notificaciones sobre su cuenta, responder consultas, 
                      proporcionar soporte técnico
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Facturación:</strong> Procesar pagos, generar facturas, gestionar suscripciones
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Mejora del servicio:</strong> Analizar el uso de la plataforma, identificar 
                      problemas, desarrollar nuevas funcionalidades
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Seguridad:</strong> Prevenir fraudes, detectar y responder a incidentes de seguridad, 
                      proteger nuestros derechos legales
                    </ListItem>
                    <ListItem>
                      <ListIcon as={CheckCircle} color="green.500" />
                      <strong>Cumplimiento legal:</strong> Cumplir con obligaciones legales, regulatorias y 
                      procesos judiciales
                    </ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    6. Compartir Datos con Terceros
                  </Heading>
                  <Text color="gray.600" mb={3}>
                    Podemos compartir sus datos personales en las siguientes circunstancias:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={Lock} color="blue.500" />
                      <strong>Proveedores de servicios:</strong> Hosting, procesamiento de pagos, servicios de 
                      email, análisis de datos (todos bajo acuerdos de confidencialidad y cumplimiento de 
                      normativas de protección de datos)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Lock} color="blue.500" />
                      <strong>Requisitos legales:</strong> Cuando sea requerido por ley, orden judicial o 
                      autoridad competente
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Lock} color="blue.500" />
                      <strong>Transacciones comerciales:</strong> En caso de fusión, adquisición o venta de 
                      activos (con previo aviso)
                    </ListItem>
                  </List>
                  <Text color="gray.600" mt={3} fontSize="sm" fontStyle="italic">
                    <strong>Importante:</strong> Nunca vendemos sus datos personales a terceros para fines de marketing.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    7. Transferencias Internacionales de Datos
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Sus datos pueden ser transferidos y procesados en servidores ubicados fuera de su país de 
                    residencia. En tales casos, garantizamos:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      Cumplimiento con las Cláusulas Contractuales Estándar de la UE
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      Certificaciones de Privacy Shield (cuando aplique)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      Niveles adecuados de protección según ISO 27001 y 27701
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Shield} color="green.500" />
                      Acuerdos de procesamiento de datos con todos los proveedores
                    </ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    8. Seguridad de los Datos
                  </Heading>
                  <Text color="gray.600" mb={3}>
                    Implementamos medidas de seguridad técnicas y organizativas conformes a ISO 27001:2013:
                  </Text>
                  <VStack align="stretch" spacing={2}>
                    <HStack>
                      <Icon as={Lock} color="green.500" />
                      <Text fontSize="sm">
                        <strong>Cifrado:</strong> SSL/TLS para datos en tránsito, AES-256 para datos en reposo
                      </Text>
                    </HStack>
                    <HStack>
                      <Icon as={Lock} color="green.500" />
                      <Text fontSize="sm">
                        <strong>Autenticación:</strong> Contraseñas hasheadas con bcrypt, autenticación de dos factores
                      </Text>
                    </HStack>
                    <HStack>
                      <Icon as={Lock} color="green.500" />
                      <Text fontSize="sm">
                        <strong>Control de acceso:</strong> Principio de mínimo privilegio, registros de auditoría
                      </Text>
                    </HStack>
                    <HStack>
                      <Icon as={Lock} color="green.500" />
                      <Text fontSize="sm">
                        <strong>Respaldos:</strong> Copias de seguridad diarias, cifradas y redundantes
                      </Text>
                    </HStack>
                    <HStack>
                      <Icon as={Lock} color="green.500" />
                      <Text fontSize="sm">
                        <strong>Monitoreo:</strong> Detección de intrusiones, análisis de vulnerabilidades, 
                        respuesta a incidentes
                      </Text>
                    </HStack>
                  </VStack>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    9. Sus Derechos como Titular de Datos
                  </Heading>
                  <Text color="gray.600" mb={3}>
                    Conforme a la Ley 1581 de 2012 y GDPR, usted tiene los siguientes derechos:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de acceso:</strong> Conocer qué datos personales tenemos sobre usted
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de rectificación:</strong> Corregir datos inexactos o incompletos
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de supresión:</strong> Solicitar la eliminación de sus datos (derecho al olvido)
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de oposición:</strong> Oponerse al tratamiento de sus datos
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de portabilidad:</strong> Recibir sus datos en formato estructurado
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho de limitación:</strong> Restringir el tratamiento en ciertas circunstancias
                    </ListItem>
                    <ListItem>
                      <ListIcon as={Eye} color="blue.500" />
                      <strong>Derecho a no ser objeto de decisiones automatizadas:</strong> Incluido el perfilamiento
                    </ListItem>
                  </List>
                  <Text color="gray.600" mt={3}>
                    Para ejercer estos derechos, envíe un correo a: <strong>privacidad@eventconnect.com</strong>
                  </Text>
                  <Text color="gray.600" fontSize="sm" fontStyle="italic" mt={2}>
                    Responderemos su solicitud en un plazo máximo de 15 días hábiles (Colombia) o 30 días (GDPR).
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    10. Retención de Datos
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Conservamos sus datos personales durante el tiempo necesario para:
                  </Text>
                  <Table size="sm" variant="simple">
                    <Thead>
                      <Tr>
                        <Th>Tipo de Dato</Th>
                        <Th>Periodo de Retención</Th>
                      </Tr>
                    </Thead>
                    <Tbody>
                      <Tr>
                        <Td>Datos de cuenta activa</Td>
                        <Td>Mientras la cuenta permanezca activa</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de facturación</Td>
                        <Td>10 años (requisito legal contable)</Td>
                      </Tr>
                      <Tr>
                        <Td>Datos de transacciones</Td>
                        <Td>5 años (requisito legal tributario)</Td>
                      </Tr>
                      <Tr>
                        <Td>Logs de seguridad</Td>
                        <Td>2 años (buenas prácticas ISO 27001)</Td>
                      </Tr>
                      <Tr>
                        <Td>Cuenta eliminada</Td>
                        <Td>30 días (período de gracia para recuperación)</Td>
                      </Tr>
                    </Tbody>
                  </Table>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    11. Cookies y Tecnologías de Seguimiento
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Utilizamos cookies y tecnologías similares para:
                  </Text>
                  <List spacing={2} ml={4} mb={3}>
                    <ListItem>• Mantener su sesión activa (cookies esenciales)</ListItem>
                    <ListItem>• Recordar sus preferencias (tema, idioma)</ListItem>
                    <ListItem>• Analizar el uso de la plataforma (Google Analytics)</ListItem>
                    <ListItem>• Mejorar la seguridad (detección de fraudes)</ListItem>
                  </List>
                  <Text color="gray.600" fontSize="sm">
                    Puede gestionar las cookies desde la configuración de su navegador. Rechazar cookies no 
                    esenciales no afectará la funcionalidad básica del servicio.
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    12. Privacidad de Menores
                  </Heading>
                  <Text color="gray.600">
                    Nuestro servicio no está dirigido a menores de 18 años. No recopilamos intencionalmente 
                    información personal de menores. Si descubrimos que hemos recopilado datos de un menor, 
                    los eliminaremos inmediatamente. Si es padre o tutor y cree que su hijo nos ha proporcionado 
                    datos personales, contáctenos en: privacidad@eventconnect.com
                  </Text>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    13. Notificación de Brechas de Seguridad
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    En caso de una brecha de seguridad que afecte sus datos personales:
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>• Notificaremos a las autoridades competentes en 72 horas (GDPR)</ListItem>
                    <ListItem>• Le informaremos directamente si existe un alto riesgo para sus derechos</ListItem>
                    <ListItem>• Tomaremos medidas inmediatas para mitigar el impacto</ListItem>
                    <ListItem>• Documentaremos el incidente según ISO 27035 (Gestión de Incidentes)</ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    14. Actualizaciones de esta Política
                  </Heading>
                  <Text color="gray.600">
                    Podemos actualizar esta Política de Privacidad ocasionalmente. Le notificaremos cualquier 
                    cambio material mediante:
                  </Text>
                  <List spacing={2} ml={4} mt={2}>
                    <ListItem>• Notificación por email</ListItem>
                    <ListItem>• Aviso en la plataforma</ListItem>
                    <ListItem>• Actualización de la fecha "Última actualización" al inicio de esta política</ListItem>
                  </List>
                </Box>

                <Box>
                  <Heading size="md" mb={3}>
                    15. Contacto y Quejas
                  </Heading>
                  <Text color="gray.600" mb={2}>
                    Para cualquier pregunta sobre esta Política o para ejercer sus derechos:
                  </Text>
                  <List spacing={2} ml={4} mb={3}>
                    <ListItem>• Email: privacidad@eventconnect.com</ListItem>
                    <ListItem>• Delegado de Protección de Datos: dpo@eventconnect.com</ListItem>
                    <ListItem>• Sistema de tickets dentro de la plataforma</ListItem>
                  </List>
                  <Text color="gray.600" mb={2}>
                    <strong>Autoridades de supervisión:</strong>
                  </Text>
                  <List spacing={2} ml={4}>
                    <ListItem>
                      • Colombia: Superintendencia de Industria y Comercio (SIC) - www.sic.gov.co
                    </ListItem>
                    <ListItem>
                      • Unión Europea: Autoridad de protección de datos de su país
                    </ListItem>
                  </List>
                </Box>

                <Box bg="green.50" p={6} borderRadius="lg" borderWidth="1px" borderColor="green.200">
                  <VStack align="start" spacing={3}>
                    <HStack>
                      <Icon as={Shield} color="green.600" />
                      <Heading size="sm" color="green.700">
                        Certificaciones de Privacidad y Seguridad
                      </Heading>
                    </HStack>
                    <Text fontSize="sm" color="green.600">
                      EventConnect cumple con las siguientes normativas internacionales de privacidad y seguridad:
                    </Text>
                    <List spacing={1} fontSize="sm" color="green.600">
                      <ListItem>• ISO 27001:2013 - Sistema de Gestión de Seguridad de la Información</ListItem>
                      <ListItem>• ISO 27002:2013 - Código de Práctica para Controles de Seguridad</ListItem>
                      <ListItem>• ISO 27701:2019 - Sistema de Gestión de Privacidad de la Información</ListItem>
                      <ListItem>• ISO 27018:2019 - Protección de Datos Personales en la Nube</ListItem>
                      <ListItem>• ISO 29100:2011 - Marco de Privacidad</ListItem>
                      <ListItem>• Ley 1581 de 2012 - Régimen General de Protección de Datos (Colombia)</ListItem>
                      <ListItem>• Decreto 1377 de 2013 - Reglamentación Ley 1581 de 2012</ListItem>
                      <ListItem>• GDPR (UE) 2016/679 - Reglamento General de Protección de Datos</ListItem>
                    </List>
                  </VStack>
                </Box>

                <Divider />

                <Box textAlign="center">
                  <Text fontSize="sm" color="gray.500" mb={4}>
                    Su privacidad es nuestra prioridad
                  </Text>
                  <HStack justify="center" spacing={4}>
                    <Button variant="outline" onClick={() => router.back()}>
                      Volver
                    </Button>
                    <Button colorScheme="green" onClick={() => router.push('/terminos')}>
                      Ver Términos y Condiciones
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
