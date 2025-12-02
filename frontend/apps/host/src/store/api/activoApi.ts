import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Activo {
  id: number;
  empresa_Id: number;
  bodega_Id?: number;
  categoria_Id: number;
  codigo_Activo: string;
  nombre: string;
  descripcion?: string;
  marca?: string;
  modelo?: string;
  numero_Serie?: string;
  estado: string;
  fecha_Adquisicion?: string;
  valor_Adquisicion?: number;
  vida_Util_Meses?: number;
  ubicacion_Fisica?: string;
  qr_Code?: string;
  imagen_URL?: string;
  observaciones?: string;
  fecha_Creacion: string;
  fecha_Actualizacion: string;
}

export interface CreateActivoDto {
  bodega_Id?: number;
  categoria_Id: number;
  codigo_Activo: string;
  nombre: string;
  descripcion?: string;
  marca?: string;
  modelo?: string;
  numero_Serie?: string;
  fecha_Adquisicion?: string;
  valor_Adquisicion?: number;
  vida_Util_Meses?: number;
  ubicacion_Fisica?: string;
  imagen_URL?: string;
  observaciones?: string;
}

export interface UpdateActivoDto {
  id: number;
  bodega_Id?: number;
  categoria_Id: number;
  codigo_Activo: string;
  nombre: string;
  descripcion?: string;
  marca?: string;
  modelo?: string;
  numero_Serie?: string;
  estado: string;
  fecha_Adquisicion?: string;
  valor_Adquisicion?: number;
  vida_Util_Meses?: number;
  ubicacion_Fisica?: string;
  imagen_URL?: string;
  observaciones?: string;
}

export const activoApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getActivos: builder.query<Activo[], void>({
      query: () => API_ENDPOINTS.ACTIVOS,
      providesTags: ['Activo'],
    }),
    getActivoById: builder.query<Activo, number>({
      query: (id) => `${API_ENDPOINTS.ACTIVOS}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Activo', id }],
    }),
    getActivoByCodigo: builder.query<Activo, string>({
      query: (codigo) => `${API_ENDPOINTS.ACTIVOS}/codigo/${codigo}`,
      providesTags: ['Activo'],
    }),
    getActivosByEstado: builder.query<Activo[], string>({
      query: (estado) => `${API_ENDPOINTS.ACTIVOS}/estado/${estado}`,
      providesTags: ['Activo'],
    }),
    createActivo: builder.mutation<Activo, CreateActivoDto>({
      query: (body) => ({
        url: API_ENDPOINTS.ACTIVOS,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Activo'],
    }),
    updateActivo: builder.mutation<Activo, UpdateActivoDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.ACTIVOS}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Activo', id },
        'Activo',
      ],
    }),
    deleteActivo: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.ACTIVOS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Activo'],
    }),
  }),
});

export const {
  useGetActivosQuery,
  useGetActivoByIdQuery,
  useGetActivoByCodigoQuery,
  useGetActivosByEstadoQuery,
  useCreateActivoMutation,
  useUpdateActivoMutation,
  useDeleteActivoMutation,
} = activoApi;
