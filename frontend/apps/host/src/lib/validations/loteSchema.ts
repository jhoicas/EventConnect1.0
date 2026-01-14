import { z } from 'zod';

// Schema de validación para Lote
export const loteSchema = z.object({
  producto_Id: z
    .number()
    .min(1, 'Debe seleccionar un producto'),
  bodega_Id: z
    .number()
    .int('Debe ser un número entero')
    .min(0, 'El ID de bodega no puede ser negativo')
    .optional(),
  codigo_Lote: z
    .string()
    .min(1, 'El código de lote es requerido')
    .min(3, 'El código debe tener al menos 3 caracteres')
    .max(50, 'El código no puede exceder 50 caracteres')
    .trim()
    .regex(/^[A-Z0-9-_]+$/i, 'El código solo puede contener letras, números, guiones y guiones bajos'),
  fecha_Fabricacion: z
    .string()
    .optional()
    .or(z.literal('')),
  fecha_Vencimiento: z
    .string()
    .optional()
    .or(z.literal('')),
  cantidad_Inicial: z
    .number()
    .int('La cantidad debe ser un número entero')
    .min(1, 'La cantidad inicial debe ser mayor a 0')
    .max(1000000, 'La cantidad es demasiado alta'),
  costo_Unitario: z
    .number()
    .min(0, 'El costo no puede ser negativo')
    .max(10000000, 'El costo es demasiado alto'),
});

export type LoteFormData = z.infer<typeof loteSchema>;
