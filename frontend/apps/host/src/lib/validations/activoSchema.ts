import { z } from 'zod';

// Schema de validación para Activo
export const activoSchema = z.object({
  categoria_Id: z
    .number()
    .min(1, 'Debe seleccionar una categoría'),
  codigo_Activo: z
    .string()
    .min(1, 'El código del activo es requerido')
    .min(3, 'El código debe tener al menos 3 caracteres')
    .max(50, 'El código no puede exceder 50 caracteres')
    .trim()
    .regex(/^[A-Z0-9-_]+$/i, 'El código solo puede contener letras, números, guiones y guiones bajos'),
  nombre: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(200, 'El nombre no puede exceder 200 caracteres')
    .trim(),
  descripcion: z
    .string()
    .max(1000, 'La descripción no puede exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
  marca: z
    .string()
    .max(100, 'La marca no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  modelo: z
    .string()
    .max(100, 'El modelo no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  numero_Serie: z
    .string()
    .max(100, 'El número de serie no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  fecha_Adquisicion: z
    .string()
    .optional()
    .or(z.literal('')),
  valor_Adquisicion: z
    .number()
    .min(0, 'El valor no puede ser negativo')
    .max(100000000, 'El valor es demasiado alto')
    .optional()
    .or(z.number().optional()),
  vida_Util_Meses: z
    .number()
    .int('La vida útil debe ser un número entero')
    .min(0, 'La vida útil no puede ser negativa')
    .max(600, 'La vida útil no puede exceder 600 meses (50 años)')
    .optional()
    .or(z.number().optional()),
  ubicacion_Fisica: z
    .string()
    .max(200, 'La ubicación no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  imagen_URL: z
    .string()
    .url('Debe ser una URL válida')
    .max(500, 'La URL no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  observaciones: z
    .string()
    .max(1000, 'Las observaciones no pueden exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
});

export type ActivoFormData = z.infer<typeof activoSchema>;
