'use client';

import { useActionState, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  Box,
  VStack,
  Heading,
  Text,
  useToast,
  Link as ChakraLink,
  useColorMode,
} from '@chakra-ui/react';
import { FormContainer, Input, Button } from '@eventconnect/ui';
import { useLoginMutation } from '../../store/api/authApi';
import { useAppDispatch } from '../../store/store';
import { setCredentials } from '../../store/slices/authSlice';
import { ROUTES } from '@eventconnect/shared';

export default function LoginPage() {
  const router = useRouter();
  const dispatch = useAppDispatch();
  const toast = useToast();
  const { colorMode } = useColorMode();
  const [login, { isLoading }] = useLoginMutation();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({ Username: '', Password: '' });

  const bgColor = colorMode === 'light' ? 'light.bg' : colorMode === 'blue' ? 'blue.bg' : 'dark.bg';
  const mutedColor = colorMode === 'light' ? 'text.light.muted' : colorMode === 'blue' ? 'text.blue.muted' : 'text.dark.muted';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validación simple
    const newErrors = { Username: '', Password: '' };
    if (!username) newErrors.Username = 'El usuario es requerido';
    if (!password) newErrors.Password = 'La contraseña es requerida';

    if (newErrors.Username || newErrors.Password) {
      setErrors(newErrors);
      return;
    }

    try {
      const result = await login({ Username: username, Password: password }).unwrap();

      dispatch(setCredentials({ user: result.usuario, token: result.token }));

      toast({
        title: '¡Bienvenido!',
        description: `Hola ${result.usuario.nombre_Completo}`,
        status: 'success',
        duration: 3000,
      });

      router.push(ROUTES.DASHBOARD);
    } catch (error: any) {
      toast({
        title: 'Error al iniciar sesión',
        description: error?.data?.message || 'Credenciales inválidas',
        status: 'error',
        duration: 5000,
      });
    }
  };

  return (
    <Box
      minH="100vh"
      display="flex"
      alignItems="center"
      justifyContent="center"
      bg={bgColor}
      px={4}
      transition="background-color 0.2s"
    >
      <FormContainer
        title="Iniciar Sesión"
        description="Ingresa tus credenciales para acceder"
        onSubmit={handleSubmit}
        footer={
          <VStack spacing={3} align="stretch">
            <Button type="submit" isLoading={isLoading} width="full">
              Iniciar Sesión
            </Button>
            <Text fontSize="sm" color={mutedColor} textAlign="center">
              ¿No tienes cuenta?{' '}
              <ChakraLink color="brand.300" href="/registro">
                Regístrate aquí
              </ChakraLink>
            </Text>
            <Text fontSize="xs" color={mutedColor} textAlign="center">
              ¿Problemas con tu cuenta?{' '}
              <ChakraLink color="brand.300" href="mailto:admin@eventconnect.com">
                Contacta al administrador
              </ChakraLink>
            </Text>
          </VStack>
        }
      >
        <VStack spacing={4}>
          <Input
            label="Usuario"
            type="text"
            placeholder="Ingresa tu usuario"
            value={username}
            onChange={(e) => {
              setUsername(e.target.value);
              setErrors((prev) => ({ ...prev, Username: '' }));
            }}
            error={errors.Username}
          />
          <Input
            label="Contraseña"
            type="password"
            placeholder=""
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              setErrors((prev) => ({ ...prev, Password: '' }));
            }}
            error={errors.Password}
          />
        </VStack>
      </FormContainer>
    </Box>
  );
}