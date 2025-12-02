import { baseApi } from './baseApi';
import type {
  Cotizacion,
  CreateCotizacionRequest,
  UpdateCotizacionRequest,
  ConvertirCotizacionRequest,
  ExtenderVencimientoRequest,
  EstadisticasCotizaciones,
} from '@eventconnect/shared';

export const cotizacionApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getCotizaciones: builder.query<Cotizacion[], { incluirVencidas?: boolean }>({
      query: ({ incluirVencidas }) => {
        const params = new URLSearchParams();
        if (incluirVencidas !== undefined) params.append('incluirVencidas', incluirVencidas.toString());
        return `/Cotizacion?${params.toString()}`;
      },
      providesTags: ['Cotizacion'],
    }),

    getCotizacionById: builder.query<Cotizacion, number>({
      query: (id) => `/Cotizacion/${id}`,
      providesTags: (result, error, id) => [{ type: 'Cotizacion', id }],
    }),

    getEstadisticasCotizaciones: builder.query<EstadisticasCotizaciones, void>({
      query: () => '/Cotizacion/estadisticas',
      providesTags: ['Cotizacion'],
    }),

    createCotizacion: builder.mutation<Cotizacion, CreateCotizacionRequest>({
      query: (body) => ({
        url: '/Cotizacion',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Cotizacion', 'Reserva'],
    }),

    updateCotizacion: builder.mutation<Cotizacion, { id: number; body: UpdateCotizacionRequest }>({
      query: ({ id, body }) => ({
        url: `/Cotizacion/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Cotizacion', id }, 'Cotizacion'],
    }),

    convertirCotizacion: builder.mutation<any, ConvertirCotizacionRequest>({
      query: (body) => ({
        url: '/Cotizacion/convertir',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Cotizacion', 'Reserva'],
    }),

    extenderVencimiento: builder.mutation<any, ExtenderVencimientoRequest>({
      query: (body) => ({
        url: '/Cotizacion/extender-vencimiento',
        method: 'POST',
        body,
      }),
      invalidatesTags: (result, error, { cotizacion_Id }) => [
        { type: 'Cotizacion', id: cotizacion_Id },
        'Cotizacion',
      ],
    }),

    deleteCotizacion: builder.mutation<void, number>({
      query: (id) => ({
        url: `/Cotizacion/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Cotizacion'],
    }),
  }),
});

export const {
  useGetCotizacionesQuery,
  useGetCotizacionByIdQuery,
  useGetEstadisticasCotizacionesQuery,
  useCreateCotizacionMutation,
  useUpdateCotizacionMutation,
  useConvertirCotizacionMutation,
  useExtenderVencimientoMutation,
  useDeleteCotizacionMutation,
} = cotizacionApi;
