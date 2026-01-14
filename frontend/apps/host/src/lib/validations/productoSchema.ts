import { z } from 'zod';

// Schema de validación para Producto
export const productoSchema = z.object({
  categoria_Id: z
    .number()
    .min(1, 'Debe seleccionar una categoría'),
  sku: z
    .string()
    .min(1, 'El SKU es requerido')
    .min(3, 'El SKU debe tener al menos 3 caracteres')
    .max(50, 'El SKU no puede exceder 50 caracteres')
    .trim()
    .regex(/^[A-Z0-9-_]+$/i, 'El SKU solo puede contener letras, números, guiones y guiones bajos'),
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
  unidad_Medida: z
    .string()
    .min(1, 'La unidad de medida es requerida')
    .max(50, 'La unidad de medida no puede exceder 50 caracteres'),
  precio_Alquiler_Dia: z
    .number()
    .min(0, 'El precio no puede ser negativo')
    .max(10000000, 'El precio es demasiado alto'),
  cantidad_Stock: z
    .number()
    .int('La cantidad debe ser un número entero')
    .min(0, 'La cantidad no puede ser negativa')
    .max(1000000, 'La cantidad es demasiado alta'),
  stock_Minimo: z
    .number()
    .int('El stock mínimo debe ser un número entero')
    .min(0, 'El stock mínimo no puede ser negativo')
    .max(1000000, 'El stock mínimo es demasiado alto'),
  imagen_URL: z
    .string()
    .url('Debe ser una URL válida')
    .max(500, 'La URL no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  es_Alquilable: z.boolean().default(true),
  es_Vendible: z.boolean().default(false),
  peso_Kg: z
    .number()
    .min(0, 'El peso no puede ser negativo')
    .max(100000, 'El peso es demasiado alto')
    .optional()
    .or(z.number().optional()),
  dimensiones: z
    .string()
    .max(200, 'Las dimensiones no pueden exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  observaciones: z
    .string()
    .max(1000, 'Las observaciones no pueden exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
}).refine(
  (data) => data.stock_Minimo <= data.cantidad_Stock || data.cantidad_Stock === 0,
  {
    message: 'El stock mínimo no puede ser mayor que la cantidad en stock',
    path: ['stock_Minimo'],
  }
);

export type ProductoFormData = z.infer<typeof productoSchema>;
