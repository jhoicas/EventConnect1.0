import { baseApi } from './baseApi';
import type { TransaccionPago, CreateTransaccionPagoRequest, ResumenPagos } from '@eventconnect/shared';

export const pagoApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getTransaccionesByReserva: builder.query<TransaccionPago[], number>({
      query: (reservaId) => `/Pago/reserva/${reservaId}`,
      providesTags: (result, error, reservaId) => [{ type: 'Pago', id: reservaId }],
    }),

    getResumenPagos: builder.query<ResumenPagos, number>({
      query: (reservaId) => `/Pago/resumen/${reservaId}`,
      providesTags: (result, error, reservaId) => [{ type: 'Pago', id: reservaId }],
    }),

    getTransaccionesByEmpresa: builder.query<TransaccionPago[], { fechaInicio?: string; fechaFin?: string }>({
      query: ({ fechaInicio, fechaFin }) => {
        const params = new URLSearchParams();
        if (fechaInicio) params.append('fechaInicio', fechaInicio);
        if (fechaFin) params.append('fechaFin', fechaFin);
        return `/Pago/empresa?${params.toString()}`;
      },
      providesTags: ['Pago'],
    }),

    createTransaccion: builder.mutation<TransaccionPago, CreateTransaccionPagoRequest>({
      query: (body) => ({
        url: '/Pago',
        method: 'POST',
        body,
      }),
      invalidatesTags: (result, error, arg) => [{ type: 'Pago', id: arg.reserva_Id }, 'Reserva'],
    }),

    deleteTransaccion: builder.mutation<void, number>({
      query: (id) => ({
        url: `/Pago/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Pago', 'Reserva'],
    }),
  }),
});

export const {
  useGetTransaccionesByReservaQuery,
  useGetResumenPagosQuery,
  useGetTransaccionesByEmpresaQuery,
  useCreateTransaccionMutation,
  useDeleteTransaccionMutation,
} = pagoApi;
