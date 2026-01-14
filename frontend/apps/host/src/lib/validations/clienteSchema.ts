import { z } from 'zod';

// Schema de validación para Cliente
export const clienteSchema = z.object({
  tipo_Cliente: z.enum(['Persona', 'Empresa'], {
    errorMap: () => ({ message: 'Debe seleccionar un tipo de cliente válido' }),
  }),
  nombre: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(200, 'El nombre no puede exceder 200 caracteres')
    .trim(),
  documento: z
    .string()
    .min(1, 'El documento es requerido')
    .min(5, 'El documento debe tener al menos 5 caracteres')
    .max(50, 'El documento no puede exceder 50 caracteres')
    .trim(),
  tipo_Documento: z.enum(['CC', 'NIT', 'CE', 'PAS'], {
    errorMap: () => ({ message: 'Debe seleccionar un tipo de documento válido' }),
  }),
  email: z
    .string()
    .email('Email inválido')
    .max(200, 'El email no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  telefono: z
    .string()
    .max(20, 'El teléfono no puede exceder 20 caracteres')
    .regex(/^[\d\s\-\+\(\)]*$/, 'El teléfono contiene caracteres inválidos')
    .optional()
    .or(z.literal('')),
  direccion: z
    .string()
    .max(500, 'La dirección no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  ciudad: z
    .string()
    .max(100, 'La ciudad no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  contacto_Nombre: z
    .string()
    .max(200, 'El nombre de contacto no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  contacto_Telefono: z
    .string()
    .max(20, 'El teléfono de contacto no puede exceder 20 caracteres')
    .regex(/^[\d\s\-\+\(\)]*$/, 'El teléfono contiene caracteres inválidos')
    .optional()
    .or(z.literal('')),
  observaciones: z
    .string()
    .max(1000, 'Las observaciones no pueden exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
});

export type ClienteFormData = z.infer<typeof clienteSchema>;
