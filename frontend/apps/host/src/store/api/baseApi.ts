import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { API_BASE_URL } from '@eventconnect/shared';
import type { RootState } from '../store';

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: API_BASE_URL,
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
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
