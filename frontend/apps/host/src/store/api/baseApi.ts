import { createApi, fetchBaseQuery, BaseQueryFn, FetchArgs, FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { API_BASE_URL } from '@eventconnect/shared';
import type { RootState } from '../store';

// Tipo para errores de la API
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode?: number;
}

// Base query con manejo mejorado de errores
const baseQuery = fetchBaseQuery({
  baseUrl: API_BASE_URL,
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.token;
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    headers.set('Content-Type', 'application/json');
    return headers;
  },
});

// Base query con transformación de errores
const baseQueryWithErrorHandling: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError | ApiError> = async (
  args,
  api,
  extraOptions
) => {
  const result = await baseQuery(args, api, extraOptions);

  // Transformar errores de la API a un formato consistente
  if (result.error) {
    const { error, meta } = result;

    // Si es un error de red
    if ('status' in error && error.status === 'FETCH_ERROR') {
      return {
        error: {
          message: 'Error de conexión. Verifica que el servidor esté disponible.',
          statusCode: 0,
        } as ApiError,
      };
    }

    // Si es un error HTTP
    if ('status' in error && 'data' in error) {
      const errorData = error.data as any;
      const status = error.status as number;

      // Intentar extraer mensaje del error
      let message = 'Ha ocurrido un error';
      let errors: Record<string, string[]> | undefined;

      if (typeof errorData === 'string') {
        message = errorData;
      } else if (errorData?.message) {
        message = errorData.message;
      } else if (errorData?.error) {
        message = errorData.error;
      } else if (errorData?.title) {
        message = errorData.title;
      }

      // Extraer errores de validación (ModelState en .NET)
      if (errorData?.errors && typeof errorData.errors === 'object') {
        errors = errorData.errors;
        // Convertir arrays de errores a strings
        const errorMessages: string[] = [];
        Object.keys(errors).forEach((key) => {
          if (Array.isArray(errors![key])) {
            errorMessages.push(...errors![key]);
          }
        });
        if (errorMessages.length > 0) {
          message = errorMessages.join(', ');
        }
      }

      // Mensajes específicos por código de estado
      if (status === 401) {
        message = 'No autorizado. Por favor, inicia sesión nuevamente.';
      } else if (status === 403) {
        message = 'No tienes permisos para realizar esta acción.';
      } else if (status === 404) {
        message = 'El recurso solicitado no fue encontrado.';
      } else if (status === 500) {
        message = 'Error interno del servidor. Por favor, intenta más tarde.';
      }

      return {
        error: {
          message,
          errors,
          statusCode: status,
        } as ApiError,
      };
    }
  }

  return result;
};

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: baseQueryWithErrorHandling,
  tagTypes: [
    'Categoria',
    'Producto',
    'Cliente',
    'Reserva',
    'Activo',
    'Bodega',
    'Lote',
    'Mantenimiento',
    'Usuario',
    'Configuracion',
    'ContenidoLanding',
    'Chat',
    'Mensaje',
    'Pago',
    'Cotizacion',
  ],
  endpoints: () => ({}),
});
