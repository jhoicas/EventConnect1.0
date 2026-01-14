import { z } from 'zod';

// Schema de validación para Configuración
export const configuracionSchema = z.object({
  clave: z
    .string()
    .min(1, 'La clave es requerida')
    .min(3, 'La clave debe tener al menos 3 caracteres')
    .max(100, 'La clave no puede exceder 100 caracteres')
    .regex(/^[A-Za-z_][A-Za-z0-9_]*$/, 'La clave solo puede contener letras, números y guiones bajos, y debe empezar con letra o guion bajo')
    .trim(),
  tipo_Dato: z
    .enum(['string', 'int', 'bool', 'json'], {
      required_error: 'El tipo de dato es requerido',
      invalid_type_error: 'Tipo de dato inválido',
    }),
  valor: z
    .string()
    .optional()
    .or(z.literal('')),
  descripcion: z
    .string()
    .max(500, 'La descripción no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  es_Global: z.boolean().default(false),
});

export type ConfiguracionFormData = z.infer<typeof configuracionSchema>;
