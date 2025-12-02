'use client';

import { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Container,
  Heading,
  Text,
  VStack,
  HStack,
  FormControl,
  FormLabel,
  Input,
  Select,
  useColorMode,
  useToast,
  FormErrorMessage,
  Icon,
  SimpleGrid,
  Radio,
  RadioGroup,
  Stack,
  Divider,
  Checkbox,
  Link,
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
} from '@chakra-ui/react';
import { useRouter } from 'next/navigation';
import { Sparkles, User, Building2, CheckCircle, ArrowLeft } from 'lucide-react';
import { ROUTES } from '@eventconnect/shared';
import { useRegisterMutation } from '@/store/api/authApi';
import { useAppDispatch } from '@/store/store';
import { setCredentials } from '@/store/slices/authSlice';

export default function RegistroPage() {
  const router = useRouter();
  const dispatch = useAppDispatch();
  const { colorMode } = useColorMode();
  const [localColorMode, setLocalColorMode] = useState<'light' | 'dark' | 'blue'>('light');
  const toast = useToast();
  const [register, { isLoading }] = useRegisterMutation();

  const [tipoUsuario, setTipoUsuario] = useState<'persona' | 'empresa'>('persona');
  const [step, setStep] = useState(1);
  const [showSuccess, setShowSuccess] = useState(false);

  // Formulario Persona
  const [nombre, setNombre] = useState('');
  const [apellido, setApellido] = useState('');
  const [email, setEmail] = useState('');
  const [telefono, setTelefono] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [documento, setDocumento] = useState('');
  const [tipoDocumento, setTipoDocumento] = useState('CC');

  // Formulario Empresa
  const [nombreEmpresa, setNombreEmpresa] = useState('');
  const [nit, setNit] = useState('');
  const [direccion, setDireccion] = useState('');
  const [ciudad, setCiudad] = useState('');
  const [moduloSIGI, setModuloSIGI] = useState(false);

  const [aceptaTerminos, setAceptaTerminos] = useState(false);
  const [errors, setErrors] = useState<any>({});

  useEffect(() => {
    const stored = localStorage.getItem('chakra-ui-color-mode');
    if (stored === 'light' || stored === 'dark' || stored === 'blue') {
      setLocalColorMode(stored);
    }
  }, [colorMode]);

  const bgColor = localColorMode === 'dark' ? '#0d1117' : localColorMode === 'blue' ? '#0a1929' : '#f7fafc';
  const cardBg = localColorMode === 'dark' ? '#161b22' : localColorMode === 'blue' ? '#0d1b2a' : '#ffffff';
  const borderColor = localColorMode === 'dark' ? '#30363d' : localColorMode === 'blue' ? '#1e3a5f' : '#e2e8f0';

  const validate = () => {
    const newErrors: any = {};

    if (!email) newErrors.email = 'El email es requerido';
    if (!password) newErrors.password = 'La contraseña es requerida';
    if (password.length < 6) newErrors.password = 'Mínimo 6 caracteres';
    if (password !== confirmPassword) newErrors.confirmPassword = 'Las contraseñas no coinciden';
    if (!aceptaTerminos) newErrors.terminos = 'Debes aceptar los términos';

    if (tipoUsuario === 'persona') {
      if (!nombre) newErrors.nombre = 'El nombre es requerido';
      if (!apellido) newErrors.apellido = 'El apellido es requerido';
      if (!documento) newErrors.documento = 'El documento es requerido';
    } else {
      if (!nombreEmpresa) newErrors.nombreEmpresa = 'El nombre de empresa es requerido';
      if (!nit) newErrors.nit = 'El NIT es requerido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) return;

    try {
      // Preparar datos según el tipo de usuario
      const nombreCompleto = tipoUsuario === 'persona' 
        ? `${nombre} ${apellido}` 
        : nombreEmpresa;

      // Usar endpoint específico para clientes (register-cliente)
      const clienteData = {
        Email: email,
        Password: password,
        Nombre_Completo: nombreCompleto,
        Telefono: telefono || undefined,
        Empresa_Id: 1, // ID por defecto
        Tipo_Cliente: tipoUsuario === 'persona' ? 'Persona' : 'Empresa',
        Documento: tipoUsuario === 'persona' ? documento : nit,
        Tipo_Documento: tipoUsuario === 'persona' ? tipoDocumento : 'NIT',
        Direccion: direccion || undefined,
        Ciudad: ciudad || undefined,
      };

      // Llamar al endpoint de registro de cliente
      const response = await fetch('http://localhost:5555/api/Auth/register-cliente', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(clienteData),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Error al registrar');
      }

      const result = await response.json();

      // Si es Persona: login automático (tiene token)
      // Si es Empresa: mostrar pantalla de aprobación (sin token)
      if (result.token) {
        // Login automático para personas
        dispatch(setCredentials({
          user: result.usuario,
          token: result.token,
        }));

        toast({
          title: 'Registro exitoso',
          description: 'Bienvenido a EventConnect',
          status: 'success',
          duration: 3000,
          isClosable: true,
        });

        // Redirigir a la vista de cliente
        router.push('/cliente/explorar');
      } else {
        // Empresa: mostrar mensaje de aprobación pendiente
        toast({
          title: 'Registro exitoso',
          description: result.message || 'Tu cuenta será activada por un administrador',
          status: 'success',
          duration: 5000,
          isClosable: true,
        });

        // Mostrar pantalla de éxito
        setShowSuccess(true);
      }
    } catch (error: any) {
      console.error('Error en registro:', error);
      toast({
        title: 'Error',
        description: error?.data?.message || 'Ocurrió un error al registrar. Por favor intenta nuevamente.',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  if (showSuccess) {
    return (
      <Box bg={bgColor} minH="100vh" py={20}>
        <Container maxW="container.md">
          <VStack spacing={8} bg={cardBg} p={10} borderRadius="2xl" borderWidth="1px" borderColor={borderColor}>
            <Icon as={CheckCircle} boxSize={20} color="green.500" />
            <Heading size="xl" textAlign="center">
              ¡Registro Exitoso!
            </Heading>
            <Text fontSize="lg" textAlign="center" color="gray.600">
              Tu cuenta ha sido creada correctamente.
            </Text>
            <Alert status="info" borderRadius="lg">
              <AlertIcon />
              <Box>
                <AlertTitle>Cuenta Pendiente de Activación</AlertTitle>
                <AlertDescription>
                  Un administrador debe activar tu cuenta antes de que puedas iniciar sesión.
                  Te notificaremos por email cuando tu cuenta sea aprobada.
                  Este proceso puede tomar de 1 a 2 días hábiles.
                </AlertDescription>
              </Box>
            </Alert>
            <Button size="lg" colorScheme="blue" onClick={() => router.push('/')}>
              Volver al Inicio
            </Button>
          </VStack>
        </Container>
      </Box>
    );
  }

  return (
    <Box bg={bgColor} minH="100vh" py={10}>
      <Container maxW="container.lg">
        <VStack spacing={8}>
          {/* Header */}
          <HStack w="full" justify="space-between">
            <HStack spacing={2} cursor="pointer" onClick={() => router.push('/')}>
              <Icon as={ArrowLeft} />
              <Text>Volver</Text>
            </HStack>
            <HStack spacing={2}>
              <Icon as={Sparkles} boxSize={8} color="blue.500" />
              <Heading size="md" bgGradient="linear(to-r, blue.400, purple.500)" bgClip="text">
                EventConnect
              </Heading>
            </HStack>
            <Text fontSize="sm" color="gray.500">
              ¿Ya tienes cuenta?{' '}
              <Link color="blue.500" onClick={() => router.push(ROUTES.LOGIN)}>
                Inicia sesión
              </Link>
            </Text>
          </HStack>

          {/* Form Card */}
          <Box bg={cardBg} p={10} borderRadius="2xl" borderWidth="1px" borderColor={borderColor} w="full">
            <VStack spacing={8}>
              <VStack spacing={2} textAlign="center">
                <Heading size="xl">Crear Cuenta</Heading>
                <Text color="gray.600">Únete a EventConnect y comienza a gestionar tu negocio</Text>
              </VStack>

              {/* Tipo de Usuario */}
              {step === 1 && (
                <VStack spacing={6} w="full">
                  <FormControl>
                    <FormLabel>¿Cómo quieres registrarte?</FormLabel>
                    <RadioGroup value={tipoUsuario} onChange={(val: any) => setTipoUsuario(val)}>
                      <Stack spacing={4}>
                        <Box
                          p={4}
                          borderWidth="2px"
                          borderColor={tipoUsuario === 'persona' ? 'blue.500' : borderColor}
                          borderRadius="lg"
                          cursor="pointer"
                          onClick={() => setTipoUsuario('persona')}
                          transition="all 0.2s"
                        >
                          <HStack justify="space-between">
                            <HStack spacing={4}>
                              <Icon as={User} boxSize={6} color="blue.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontWeight="bold">Persona</Text>
                                <Text fontSize="sm" color="gray.600">
                                  Para usuarios individuales
                                </Text>
                              </VStack>
                            </HStack>
                            <Radio value="persona" />
                          </HStack>
                        </Box>

                        <Box
                          p={4}
                          borderWidth="2px"
                          borderColor={tipoUsuario === 'empresa' ? 'blue.500' : borderColor}
                          borderRadius="lg"
                          cursor="pointer"
                          onClick={() => setTipoUsuario('empresa')}
                          transition="all 0.2s"
                        >
                          <HStack justify="space-between">
                            <HStack spacing={4}>
                              <Icon as={Building2} boxSize={6} color="purple.500" />
                              <VStack align="start" spacing={0}>
                                <Text fontWeight="bold">Empresa</Text>
                                <Text fontSize="sm" color="gray.600">
                                  Para negocios de eventos y alquiler
                                </Text>
                              </VStack>
                            </HStack>
                            <Radio value="empresa" />
                          </HStack>
                        </Box>
                      </Stack>
                    </RadioGroup>
                  </FormControl>

                  {tipoUsuario === 'empresa' && (
                    <Alert status="info" borderRadius="lg">
                      <AlertIcon />
                      <Box>
                        <AlertTitle>Aprobación Requerida</AlertTitle>
                        <AlertDescription fontSize="sm">
                          Las cuentas de empresa requieren aprobación del administrador antes de poder acceder.
                        </AlertDescription>
                      </Box>
                    </Alert>
                  )}

                  <Button size="lg" colorScheme="blue" w="full" onClick={() => setStep(2)}>
                    Continuar
                  </Button>
                </VStack>
              )}

              {/* Formulario */}
              {step === 2 && (
                <VStack spacing={6} w="full">
                  <Heading size="md">
                    {tipoUsuario === 'persona' ? 'Información Personal' : 'Información de la Empresa'}
                  </Heading>

                  {tipoUsuario === 'persona' ? (
                    <SimpleGrid columns={{ base: 1, md: 2 }} spacing={4} w="full">
                      <FormControl isInvalid={!!errors.nombre} isRequired>
                        <FormLabel>Nombre</FormLabel>
                        <Input value={nombre} onChange={(e) => setNombre(e.target.value)} placeholder="Juan" />
                        <FormErrorMessage>{errors.nombre}</FormErrorMessage>
                      </FormControl>

                      <FormControl isInvalid={!!errors.apellido} isRequired>
                        <FormLabel>Apellido</FormLabel>
                        <Input value={apellido} onChange={(e) => setApellido(e.target.value)} placeholder="Pérez" />
                        <FormErrorMessage>{errors.apellido}</FormErrorMessage>
                      </FormControl>

                      <FormControl isInvalid={!!errors.email} isRequired>
                        <FormLabel>Email</FormLabel>
                        <Input
                          type="email"
                          value={email}
                          onChange={(e) => setEmail(e.target.value)}
                          placeholder="juan@example.com"
                        />
                        <FormErrorMessage>{errors.email}</FormErrorMessage>
                      </FormControl>

                      <FormControl>
                        <FormLabel>Teléfono</FormLabel>
                        <Input value={telefono} onChange={(e) => setTelefono(e.target.value)} placeholder="+57 300 123 4567" />
                      </FormControl>

                      <FormControl isInvalid={!!errors.tipoDocumento}>
                        <FormLabel>Tipo de Documento</FormLabel>
                        <Select value={tipoDocumento} onChange={(e) => setTipoDocumento(e.target.value)}>
                          <option value="CC">Cédula de Ciudadanía</option>
                          <option value="CE">Cédula de Extranjería</option>
                          <option value="Pasaporte">Pasaporte</option>
                        </Select>
                      </FormControl>

                      <FormControl isInvalid={!!errors.documento} isRequired>
                        <FormLabel>Número de Documento</FormLabel>
                        <Input
                          value={documento}
                          onChange={(e) => setDocumento(e.target.value)}
                          placeholder="1234567890"
                        />
                        <FormErrorMessage>{errors.documento}</FormErrorMessage>
                      </FormControl>

                      <FormControl isInvalid={!!errors.password} isRequired>
                        <FormLabel>Contraseña</FormLabel>
                        <Input
                          type="password"
                          value={password}
                          onChange={(e) => setPassword(e.target.value)}
                          placeholder="Mínimo 6 caracteres"
                        />
                        <FormErrorMessage>{errors.password}</FormErrorMessage>
                      </FormControl>

                      <FormControl isInvalid={!!errors.confirmPassword} isRequired>
                        <FormLabel>Confirmar Contraseña</FormLabel>
                        <Input
                          type="password"
                          value={confirmPassword}
                          onChange={(e) => setConfirmPassword(e.target.value)}
                          placeholder="Repite la contraseña"
                        />
                        <FormErrorMessage>{errors.confirmPassword}</FormErrorMessage>
                      </FormControl>
                    </SimpleGrid>
                  ) : (
                    <VStack spacing={4} w="full">
                      <SimpleGrid columns={{ base: 1, md: 2 }} spacing={4} w="full">
                        <FormControl isInvalid={!!errors.nombreEmpresa} isRequired>
                          <FormLabel>Nombre de la Empresa</FormLabel>
                          <Input
                            value={nombreEmpresa}
                            onChange={(e) => setNombreEmpresa(e.target.value)}
                            placeholder="Eventos Mágicos S.A.S"
                          />
                          <FormErrorMessage>{errors.nombreEmpresa}</FormErrorMessage>
                        </FormControl>

                        <FormControl isInvalid={!!errors.nit} isRequired>
                          <FormLabel>NIT</FormLabel>
                          <Input value={nit} onChange={(e) => setNit(e.target.value)} placeholder="900123456-7" />
                          <FormErrorMessage>{errors.nit}</FormErrorMessage>
                        </FormControl>

                        <FormControl isInvalid={!!errors.email} isRequired>
                          <FormLabel>Email Corporativo</FormLabel>
                          <Input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="info@empresa.com"
                          />
                          <FormErrorMessage>{errors.email}</FormErrorMessage>
                        </FormControl>

                        <FormControl>
                          <FormLabel>Teléfono</FormLabel>
                          <Input value={telefono} onChange={(e) => setTelefono(e.target.value)} placeholder="+57 1 234 5678" />
                        </FormControl>

                        <FormControl>
                          <FormLabel>Ciudad</FormLabel>
                          <Input value={ciudad} onChange={(e) => setCiudad(e.target.value)} placeholder="Bogotá" />
                        </FormControl>

                        <FormControl>
                          <FormLabel>Dirección</FormLabel>
                          <Input value={direccion} onChange={(e) => setDireccion(e.target.value)} placeholder="Calle 123 #45-67" />
                        </FormControl>

                        <FormControl isInvalid={!!errors.password} isRequired>
                          <FormLabel>Contraseña</FormLabel>
                          <Input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="Mínimo 6 caracteres"
                          />
                          <FormErrorMessage>{errors.password}</FormErrorMessage>
                        </FormControl>

                        <FormControl isInvalid={!!errors.confirmPassword} isRequired>
                          <FormLabel>Confirmar Contraseña</FormLabel>
                          <Input
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                            placeholder="Repite la contraseña"
                          />
                          <FormErrorMessage>{errors.confirmPassword}</FormErrorMessage>
                        </FormControl>
                      </SimpleGrid>

                      <Divider />

                      <Box w="full" p={4} bg="blue.50" borderRadius="lg" borderWidth="1px" borderColor="blue.200">
                        <VStack align="start" spacing={3}>
                          <Heading size="sm" color="blue.700">
                            ¿Necesitas el Módulo SIGI?
                          </Heading>
                          <Text fontSize="sm" color="blue.600">
                            Sistema Integrado de Gestión de Inventarios: activos fijos, bodegas, depreciación,
                            mantenimientos y más.
                          </Text>
                          <Checkbox
                            isChecked={moduloSIGI}
                            onChange={(e) => setModuloSIGI(e.target.checked)}
                            colorScheme="blue"
                          >
                            <Text fontSize="sm" fontWeight="bold">
                              Sí, solicitar módulo SIGI ($49 USD/mes)
                            </Text>
                          </Checkbox>
                        </VStack>
                      </Box>
                    </VStack>
                  )}

                  <FormControl isInvalid={!!errors.terminos}>
                    <Checkbox
                      isChecked={aceptaTerminos}
                      onChange={(e) => setAceptaTerminos(e.target.checked)}
                      colorScheme="blue"
                    >
                      <Text fontSize="sm">
                        Acepto los{' '}
                        <Link color="blue.500" href="/terminos" target="_blank">
                          Términos y Condiciones
                        </Link>{' '}
                        y la{' '}
                        <Link color="blue.500" href="/politica-privacidad" target="_blank">
                          Política de Privacidad
                        </Link>
                      </Text>
                    </Checkbox>
                    <FormErrorMessage>{errors.terminos}</FormErrorMessage>
                  </FormControl>

                  <HStack w="full" spacing={4}>
                    <Button size="lg" variant="outline" onClick={() => setStep(1)} w="full">
                      Atrás
                    </Button>
                    <Button size="lg" colorScheme="blue" onClick={handleSubmit} isLoading={isLoading} w="full">
                      {tipoUsuario === 'empresa' ? 'Enviar Solicitud' : 'Crear Cuenta'}
                    </Button>
                  </HStack>
                </VStack>
              )}
            </VStack>
          </Box>
        </VStack>
      </Container>
    </Box>
  );
}
