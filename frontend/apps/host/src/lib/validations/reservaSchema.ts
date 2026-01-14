import { z } from 'zod';

// Schema de validación para Reserva
export const reservaSchema = z.object({
  cliente_Id: z
    .number()
    .min(1, 'Debe seleccionar un cliente'),
  fecha_Evento: z
    .string()
    .min(1, 'La fecha del evento es requerida'),
  fecha_Entrega: z
    .string()
    .optional()
    .or(z.literal('')),
  fecha_Devolucion_Programada: z
    .string()
    .optional()
    .or(z.literal('')),
  direccion_Entrega: z
    .string()
    .max(500, 'La dirección no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  ciudad_Entrega: z
    .string()
    .max(100, 'La ciudad no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  contacto_En_Sitio: z
    .string()
    .max(200, 'El nombre de contacto no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  telefono_Contacto: z
    .string()
    .max(20, 'El teléfono no puede exceder 20 caracteres')
    .regex(/^[\d\s\-\+\(\)]*$/, 'El teléfono contiene caracteres inválidos')
    .optional()
    .or(z.literal('')),
  subtotal: z
    .number()
    .min(0, 'El subtotal no puede ser negativo')
    .max(100000000, 'El subtotal es demasiado alto'),
  descuento: z
    .number()
    .min(0, 'El descuento no puede ser negativo')
    .max(100000000, 'El descuento es demasiado alto'),
  total: z
    .number()
    .min(0, 'El total no puede ser negativo')
    .max(100000000, 'El total es demasiado alto'),
  fianza: z
    .number()
    .min(0, 'La fianza no puede ser negativa')
    .max(100000000, 'La fianza es demasiado alta')
    .optional()
    .or(z.number().optional()),
  metodo_Pago: z
    .string()
    .min(1, 'Debe seleccionar un método de pago'),
  estado_Pago: z
    .string()
    .min(1, 'Debe seleccionar un estado de pago'),
  observaciones: z
    .string()
    .max(1000, 'Las observaciones no pueden exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
}).refine(
  (data) => data.descuento <= data.subtotal,
  {
    message: 'El descuento no puede ser mayor que el subtotal',
    path: ['descuento'],
  }
);

export type ReservaFormData = z.infer<typeof reservaSchema>;
