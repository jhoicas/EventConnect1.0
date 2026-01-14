import { z } from 'zod';

// Schema de validación para Categoria
export const categoriaSchema = z.object({
  nombre: z
    .string()
    .min(1, 'El nombre es requerido')
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres')
    .trim(),
  descripcion: z
    .string()
    .max(500, 'La descripción no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  icono: z
    .string()
    .max(100, 'El nombre del icono no puede exceder 100 caracteres')
    .optional()
    .or(z.literal('')),
  color: z
    .string()
    .regex(/^#[0-9A-F]{6}$/i, 'El color debe ser un código hexadecimal válido (ej: #FF5733)')
    .optional()
    .or(z.literal('')),
});

export type CategoriaFormData = z.infer<typeof categoriaSchema>;
