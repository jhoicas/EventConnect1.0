'use client';

import React, { Component, ErrorInfo, ReactNode } from 'react';
import {
  Box,
  Button,
  Container,
  Heading,
  Text,
  VStack,
  Alert,
  AlertIcon,
  AlertTitle,
  AlertDescription,
  Code,
} from '@chakra-ui/react';
import { AlertCircle, RefreshCw, Home } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { ROUTES } from '@eventconnect/shared';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return {
      hasError: true,
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error capturado por ErrorBoundary:', error, errorInfo);
    this.setState({
      error,
      errorInfo,
    });

    // Aquí podrías enviar el error a un servicio de monitoreo como Sentry
    // if (process.env.NEXT_PUBLIC_SENTRY_DSN) {
    //   Sentry.captureException(error, { contexts: { react: { componentStack: errorInfo.componentStack } } });
    // }
  }

  handleReset = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
    });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return <ErrorFallback error={this.state.error} errorInfo={this.state.errorInfo} onReset={this.handleReset} />;
    }

    return this.props.children;
  }
}

interface ErrorFallbackProps {
  error: Error | null;
  errorInfo: ErrorInfo | null;
  onReset: () => void;
}

function ErrorFallback({ error, errorInfo, onReset }: ErrorFallbackProps) {
  const router = useRouter();
  const isDevelopment = process.env.NODE_ENV === 'development';

  return (
    <Box minH="100vh" bg="gray.50" py={12}>
      <Container maxW="container.md">
        <VStack spacing={6} align="stretch">
          <Alert status="error" borderRadius="lg">
            <AlertIcon as={AlertCircle} boxSize={6} />
            <Box flex="1">
              <AlertTitle fontSize="lg">Algo salió mal</AlertTitle>
              <AlertDescription>
                Ha ocurrido un error inesperado. Por favor, intenta recargar la página.
              </AlertDescription>
            </Box>
          </Alert>

          <Box bg="white" p={6} borderRadius="lg" boxShadow="sm">
            <VStack spacing={4} align="stretch">
              <Heading size="md" color="red.600">
                Detalles del Error
              </Heading>

              {error && (
                <Box>
                  <Text fontWeight="bold" mb={2} color="gray.700">
                    Mensaje:
                  </Text>
                  <Code p={2} borderRadius="md" display="block" whiteSpace="pre-wrap" colorScheme="red">
                    {error.message}
                  </Code>
                </Box>
              )}

              {isDevelopment && errorInfo && (
                <Box>
                  <Text fontWeight="bold" mb={2} color="gray.700">
                    Stack Trace:
                  </Text>
                  <Code
                    p={3}
                    borderRadius="md"
                    display="block"
                    whiteSpace="pre-wrap"
                    fontSize="xs"
                    maxH="300px"
                    overflowY="auto"
                    colorScheme="gray"
                  >
                    {errorInfo.componentStack}
                  </Code>
                </Box>
              )}

              <VStack spacing={3} pt={4}>
                <Button
                  leftIcon={<RefreshCw size={18} />}
                  colorScheme="blue"
                  onClick={onReset}
                  size="md"
                  width="full"
                >
                  Reintentar
                </Button>
                <Button
                  leftIcon={<Home size={18} />}
                  variant="outline"
                  onClick={() => router.push(ROUTES.DASHBOARD)}
                  size="md"
                  width="full"
                >
                  Ir al Inicio
                </Button>
              </VStack>

              {!isDevelopment && (
                <Box mt={4} p={4} bg="blue.50" borderRadius="md">
                  <Text fontSize="sm" color="blue.800">
                    Si el problema persiste, por favor contacta al soporte técnico.
                  </Text>
                </Box>
              )}
            </VStack>
          </Box>
        </VStack>
      </Container>
    </Box>
  );
}
