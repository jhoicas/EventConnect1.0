import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Bodega {
  id: number;
  empresa_Id: number;
  codigo_Bodega: string;
  nombre: string;
  direccion?: string;
  ciudad?: string;
  telefono?: string;
  responsable_Id?: number;
  capacidad_M3?: number;
  estado: string;
  fecha_Creacion: string;
  fecha_Actualizacion: string;
}

export interface CreateBodegaDto {
  codigo_Bodega: string;
  nombre: string;
  direccion?: string;
  ciudad?: string;
  telefono?: string;
  responsable_Id?: number;
  capacidad_M3?: number;
}

export interface UpdateBodegaDto {
  id: number;
  codigo_Bodega: string;
  nombre: string;
  direccion?: string;
  ciudad?: string;
  telefono?: string;
  responsable_Id?: number;
  capacidad_M3?: number;
  estado: string;
}

export const bodegaApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getBodegas: builder.query<Bodega[], void>({
      query: () => API_ENDPOINTS.BODEGAS,
      providesTags: ['Bodega'],
    }),
    getBodegaById: builder.query<Bodega, number>({
      query: (id) => `${API_ENDPOINTS.BODEGAS}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Bodega', id }],
    }),
    createBodega: builder.mutation<Bodega, CreateBodegaDto>({
      query: (body) => ({
        url: API_ENDPOINTS.BODEGAS,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Bodega'],
    }),
    updateBodega: builder.mutation<Bodega, UpdateBodegaDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.BODEGAS}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Bodega', id },
        'Bodega',
      ],
    }),
    deleteBodega: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.BODEGAS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Bodega'],
    }),
  }),
});

export const {
  useGetBodegasQuery,
  useGetBodegaByIdQuery,
  useCreateBodegaMutation,
  useUpdateBodegaMutation,
  useDeleteBodegaMutation,
} = bodegaApi;
