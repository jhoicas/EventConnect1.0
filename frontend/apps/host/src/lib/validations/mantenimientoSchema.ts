import { z } from 'zod';

// Schema de validación para Mantenimiento
export const mantenimientoSchema = z.object({
  activo_Id: z
    .number()
    .min(1, 'Debe seleccionar un activo'),
  tipo_Mantenimiento: z
    .string()
    .min(1, 'El tipo de mantenimiento es requerido')
    .max(100, 'El tipo no puede exceder 100 caracteres'),
  fecha_Programada: z
    .string()
    .optional()
    .or(z.literal('')),
  fecha_Realizada: z
    .string()
    .optional()
    .or(z.literal('')),
  descripcion: z
    .string()
    .max(1000, 'La descripción no puede exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
  responsable_Id: z
    .number()
    .int('Debe ser un número entero')
    .min(1, 'Debe seleccionar un responsable válido')
    .optional(),
  proveedor_Servicio: z
    .string()
    .max(200, 'El proveedor no puede exceder 200 caracteres')
    .optional()
    .or(z.literal('')),
  costo: z
    .number()
    .min(0, 'El costo no puede ser negativo')
    .max(10000000, 'El costo es demasiado alto')
    .optional(),
  observaciones: z
    .string()
    .max(1000, 'Las observaciones no pueden exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
});

export type MantenimientoFormData = z.infer<typeof mantenimientoSchema>;
