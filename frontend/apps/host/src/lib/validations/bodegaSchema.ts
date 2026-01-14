import { z } from 'zod';

// Schema de validación para Bodega
export const bodegaSchema = z.object({
  codigo_Bodega: z
    .string()
    .min(3, 'El código debe tener al menos 3 caracteres')
    .max(50, 'El código no puede exceder 50 caracteres')
    .trim()
    .regex(/^[A-Z0-9-_]+$/i, 'El código solo puede contener letras, números, guiones y guiones bajos'),
  nombre: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(200, 'El nombre no puede exceder 200 caracteres')
    .trim(),
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
  telefono: z
    .string()
    .max(20, 'El teléfono no puede exceder 20 caracteres')
    .regex(/^[\d\s\-\+\(\)]*$/, 'El teléfono contiene caracteres inválidos')
    .optional()
    .or(z.literal('')),
  responsable_Id: z
    .number()
    .int('Debe ser un número entero')
    .min(1, 'Debe seleccionar un responsable válido')
    .optional(),
  capacidad_M3: z
    .number()
    .min(0, 'La capacidad no puede ser negativa')
    .max(1000000, 'La capacidad es demasiado alta')
    .optional(),
});

export type BodegaFormData = z.infer<typeof bodegaSchema>;
