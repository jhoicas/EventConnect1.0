import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Lote {
  id: number;
  producto_Id: number;
  bodega_Id?: number;
  codigo_Lote: string;
  fecha_Fabricacion?: string;
  fecha_Vencimiento?: string;
  cantidad_Inicial: number;
  cantidad_Actual: number;
  costo_Unitario: number;
  estado: string;
  fecha_Creacion: string;
}

export interface CreateLoteDto {
  producto_Id: number;
  bodega_Id?: number;
  codigo_Lote: string;
  fecha_Fabricacion?: string;
  fecha_Vencimiento?: string;
  cantidad_Inicial: number;
  costo_Unitario: number;
}

export interface UpdateLoteDto {
  id: number;
  producto_Id: number;
  bodega_Id?: number;
  codigo_Lote: string;
  fecha_Fabricacion?: string;
  fecha_Vencimiento?: string;
  cantidad_Inicial: number;
  cantidad_Actual: number;
  costo_Unitario: number;
  estado: string;
}

export const loteApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getLotes: builder.query<Lote[], void>({
      query: () => API_ENDPOINTS.LOTES,
      providesTags: ['Lote'],
    }),
    getLoteById: builder.query<Lote, number>({
      query: (id) => `${API_ENDPOINTS.LOTES}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Lote', id }],
    }),
    getLotesByProducto: builder.query<Lote[], number>({
      query: (productoId) => `${API_ENDPOINTS.LOTES}/producto/${productoId}`,
      providesTags: ['Lote'],
    }),
    getLotesVencidos: builder.query<Lote[], void>({
      query: () => `${API_ENDPOINTS.LOTES}/vencidos`,
      providesTags: ['Lote'],
    }),
    getLotesPorVencer: builder.query<Lote[], number>({
      query: (dias) => `${API_ENDPOINTS.LOTES}/por-vencer/${dias}`,
      providesTags: ['Lote'],
    }),
    createLote: builder.mutation<Lote, CreateLoteDto>({
      query: (body) => ({
        url: API_ENDPOINTS.LOTES,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Lote'],
    }),
    updateLote: builder.mutation<Lote, UpdateLoteDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.LOTES}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Lote', id },
        'Lote',
      ],
    }),
    deleteLote: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.LOTES}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Lote'],
    }),
  }),
});

export const {
  useGetLotesQuery,
  useGetLoteByIdQuery,
  useGetLotesByProductoQuery,
  useGetLotesVencidosQuery,
  useGetLotesPorVencerQuery,
  useCreateLoteMutation,
  useUpdateLoteMutation,
  useDeleteLoteMutation,
} = loteApi;
