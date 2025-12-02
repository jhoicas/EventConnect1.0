'use client';

import {
  Box,
  Button,
  Card,
  CardBody,
  CardHeader,
  Divider,
  Flex,
  Grid,
  Heading,
  Icon,
  IconButton,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Progress,
  Select,
  Stack,
  Text,
  useDisclosure,
  useToast,
  Badge,
  Input,
  FormControl,
  FormLabel,
  Textarea,
} from '@chakra-ui/react';
import { FiDollarSign, FiPlus, FiTrash2, FiFileText, FiCheckCircle } from 'react-icons/fi';
import { useState } from 'react';
import { useGetResumenPagosQuery, useCreateTransaccionMutation, useDeleteTransaccionMutation } from '@/store/api/pagoApi';

interface GestionPagosProps {
  reservaId: number;
  totalReserva: number;
}

export default function GestionPagos({ reservaId, totalReserva }: GestionPagosProps) {
  const toast = useToast();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const { data: resumen, isLoading } = useGetResumenPagosQuery(reservaId);
  const [createTransaccion, { isLoading: isCreating }] = useCreateTransaccionMutation();
  const [deleteTransaccion] = useDeleteTransaccionMutation();

  const [formData, setFormData] = useState({
    monto: '',
    tipo: 'Pago',
    metodo: 'Efectivo',
    referencia_Externa: '',
    comprobante_URL: '',
  });

  const handleSubmit = async () => {
    if (!formData.monto || parseFloat(formData.monto) <= 0) {
      toast({
        title: 'Error',
        description: 'Ingrese un monto válido',
        status: 'error',
        duration: 3000,
      });
      return;
    }

    try {
      await createTransaccion({
        reserva_Id: reservaId,
        monto: parseFloat(formData.monto),
        tipo: formData.tipo,
        metodo: formData.metodo,
        referencia_Externa: formData.referencia_Externa || undefined,
        comprobante_URL: formData.comprobante_URL || undefined,
      }).unwrap();

      toast({
        title: 'Éxito',
        description: 'Transacción registrada correctamente',
        status: 'success',
        duration: 3000,
      });

      setFormData({
        monto: '',
        tipo: 'Pago',
        metodo: 'Efectivo',
        referencia_Externa: '',
        comprobante_URL: '',
      });
      onClose();
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Error al registrar transacción',
        status: 'error',
        duration: 4000,
      });
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('¿Está seguro de eliminar esta transacción?')) return;

    try {
      await deleteTransaccion(id).unwrap();
      toast({
        title: 'Éxito',
        description: 'Transacción eliminada correctamente',
        status: 'success',
        duration: 3000,
      });
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.data?.message || 'Error al eliminar transacción',
        status: 'error',
        duration: 4000,
      });
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(value);
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleString('es-CO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (isLoading) {
    return (
      <Card>
        <CardBody>
          <Text>Cargando información de pagos...</Text>
        </CardBody>
      </Card>
    );
  }

  const porcentajePagado = resumen?.porcentaje_Pagado || 0;
  const totalPagado = resumen?.total_Pagado || 0;
  const saldoPendiente = resumen?.saldo_Pendiente || totalReserva;

  return (
    <>
      <Card>
        <CardHeader>
          <Flex justify="space-between" align="center" flexWrap="wrap" gap={2}>
            <Heading size={{ base: 'sm', md: 'md' }}>
              <Icon as={FiDollarSign} mr={2} />
              Gestión de Pagos
            </Heading>
            <Button
              leftIcon={<FiPlus />}
              colorScheme="blue"
              onClick={onOpen}
              size={{ base: 'sm', md: 'md' }}
            >
              Registrar Pago
            </Button>
          </Flex>
        </CardHeader>
        <CardBody>
          <Stack spacing={6}>
            {/* Resumen de Pagos */}
            <Box>
              <Grid templateColumns={{ base: '1fr', md: 'repeat(3, 1fr)' }} gap={4} mb={4}>
                <Card bg="blue.50">
                  <CardBody>
                    <Text fontSize="sm" color="gray.600">Total Reserva</Text>
                    <Text fontSize={{ base: 'lg', md: '2xl' }} fontWeight="bold" color="blue.600">
                      {formatCurrency(resumen?.total_Reserva || totalReserva)}
                    </Text>
                  </CardBody>
                </Card>
                <Card bg="green.50">
                  <CardBody>
                    <Text fontSize="sm" color="gray.600">Total Pagado</Text>
                    <Text fontSize={{ base: 'lg', md: '2xl' }} fontWeight="bold" color="green.600">
                      {formatCurrency(totalPagado)}
                    </Text>
                  </CardBody>
                </Card>
                <Card bg="orange.50">
                  <CardBody>
                    <Text fontSize="sm" color="gray.600">Saldo Pendiente</Text>
                    <Text fontSize={{ base: 'lg', md: '2xl' }} fontWeight="bold" color="orange.600">
                      {formatCurrency(saldoPendiente)}
                    </Text>
                  </CardBody>
                </Card>
              </Grid>

              <Box>
                <Flex justify="space-between" mb={2}>
                  <Text fontSize="sm" fontWeight="medium">Progreso de Pago</Text>
                  <Text fontSize="sm" fontWeight="bold" color="blue.600">
                    {porcentajePagado.toFixed(1)}%
                  </Text>
                </Flex>
                <Progress
                  value={porcentajePagado}
                  colorScheme={porcentajePagado >= 100 ? 'green' : 'blue'}
                  size="lg"
                  borderRadius="md"
                />
              </Box>
            </Box>

            <Divider />

            {/* Historial de Transacciones */}
            <Box>
              <Heading size="sm" mb={4}>Historial de Transacciones</Heading>
              {!resumen?.transacciones || resumen.transacciones.length === 0 ? (
                <Text color="gray.500" textAlign="center" py={8}>
                  No hay transacciones registradas
                </Text>
              ) : (
                <Stack spacing={3}>
                  {resumen.transacciones.map((transaccion) => (
                    <Card key={transaccion.id} variant="outline">
                      <CardBody>
                        <Grid templateColumns={{ base: '1fr', md: 'repeat(2, 1fr)' }} gap={4}>
                          <Box>
                            <Flex align="center" mb={2}>
                              <Badge
                                colorScheme={
                                  transaccion.tipo === 'Pago' ? 'green' :
                                  transaccion.tipo === 'Devolucion_Fianza' ? 'blue' : 'orange'
                                }
                                mr={2}
                              >
                                {transaccion.tipo}
                              </Badge>
                              <Text fontSize="sm" color="gray.600">{transaccion.metodo}</Text>
                            </Flex>
                            <Text fontSize={{ base: 'xl', md: '2xl' }} fontWeight="bold">
                              {formatCurrency(transaccion.monto)}
                            </Text>
                            <Text fontSize="xs" color="gray.500">
                              {formatDate(transaccion.fecha_Transaccion)}
                            </Text>
                            {transaccion.registrado_Por_Nombre && (
                              <Text fontSize="xs" color="gray.600" mt={1}>
                                Registrado por: {transaccion.registrado_Por_Nombre}
                              </Text>
                            )}
                          </Box>

                          <Box>
                            {transaccion.referencia_Externa && (
                              <Text fontSize="sm" mb={1}>
                                <strong>Referencia:</strong> {transaccion.referencia_Externa}
                              </Text>
                            )}
                            {transaccion.comprobante_URL && (
                              <Button
                                as="a"
                                href={transaccion.comprobante_URL}
                                target="_blank"
                                size="sm"
                                leftIcon={<FiFileText />}
                                variant="ghost"
                                mb={2}
                              >
                                Ver Comprobante
                              </Button>
                            )}
                            <IconButton
                              aria-label="Eliminar"
                              icon={<FiTrash2 />}
                              colorScheme="red"
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDelete(transaccion.id)}
                            />
                          </Box>
                        </Grid>
                      </CardBody>
                    </Card>
                  ))}
                </Stack>
              )}
            </Box>
          </Stack>
        </CardBody>
      </Card>

      {/* Modal Registrar Pago */}
      <Modal isOpen={isOpen} onClose={onClose} size={{ base: 'full', md: 'xl' }}>
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Registrar Transacción</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <Stack spacing={4}>
              <FormControl isRequired>
                <FormLabel>Tipo</FormLabel>
                <Select
                  value={formData.tipo}
                  onChange={(e) => setFormData({ ...formData, tipo: e.target.value })}
                >
                  <option value="Pago">Pago</option>
                  <option value="Devolucion_Fianza">Devolución de Fianza</option>
                  <option value="Reembolso">Reembolso</option>
                </Select>
              </FormControl>

              <FormControl isRequired>
                <FormLabel>Monto</FormLabel>
                <Input
                  type="number"
                  placeholder="0.00"
                  value={formData.monto}
                  onChange={(e) => setFormData({ ...formData, monto: e.target.value })}
                />
                <Text fontSize="xs" color="gray.600" mt={1}>
                  Saldo pendiente: {formatCurrency(saldoPendiente)}
                </Text>
              </FormControl>

              <FormControl isRequired>
                <FormLabel>Método de Pago</FormLabel>
                <Select
                  value={formData.metodo}
                  onChange={(e) => setFormData({ ...formData, metodo: e.target.value })}
                >
                  <option value="Efectivo">Efectivo</option>
                  <option value="Tarjeta">Tarjeta</option>
                  <option value="Transferencia">Transferencia</option>
                  <option value="Nequi">Nequi</option>
                  <option value="Daviplata">Daviplata</option>
                  <option value="PayU">PayU</option>
                  <option value="Stripe">Stripe</option>
                </Select>
              </FormControl>

              <FormControl>
                <FormLabel>Referencia Externa (Opcional)</FormLabel>
                <Input
                  placeholder="ID de transacción, número de recibo, etc."
                  value={formData.referencia_Externa}
                  onChange={(e) => setFormData({ ...formData, referencia_Externa: e.target.value })}
                />
              </FormControl>

              <FormControl>
                <FormLabel>URL del Comprobante (Opcional)</FormLabel>
                <Input
                  placeholder="https://..."
                  value={formData.comprobante_URL}
                  onChange={(e) => setFormData({ ...formData, comprobante_URL: e.target.value })}
                />
              </FormControl>
            </Stack>
          </ModalBody>
          <ModalFooter>
            <Button variant="ghost" mr={3} onClick={onClose}>
              Cancelar
            </Button>
            <Button
              colorScheme="blue"
              onClick={handleSubmit}
              isLoading={isCreating}
              leftIcon={<FiCheckCircle />}
            >
              Registrar
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
}
