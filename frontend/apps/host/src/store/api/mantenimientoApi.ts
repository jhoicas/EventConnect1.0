import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Mantenimiento {
  id: number;
  activo_Id: number;
  tipo_Mantenimiento: string;
  fecha_Programada?: string;
  fecha_Realizada?: string;
  descripcion?: string;
  responsable_Id?: number;
  proveedor_Servicio?: string;
  costo?: number;
  estado: string;
  observaciones?: string;
  fecha_Creacion: string;
}

export interface CreateMantenimientoDto {
  activo_Id: number;
  tipo_Mantenimiento: string;
  fecha_Programada?: string;
  descripcion?: string;
  responsable_Id?: number;
  proveedor_Servicio?: string;
  costo?: number;
  observaciones?: string;
}

export interface UpdateMantenimientoDto {
  id: number;
  activo_Id: number;
  tipo_Mantenimiento: string;
  fecha_Programada?: string;
  fecha_Realizada?: string;
  descripcion?: string;
  responsable_Id?: number;
  proveedor_Servicio?: string;
  costo?: number;
  estado: string;
  observaciones?: string;
}

export const mantenimientoApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getMantenimientos: builder.query<Mantenimiento[], void>({
      query: () => API_ENDPOINTS.MANTENIMIENTOS,
      providesTags: ['Mantenimiento'],
    }),
    getMantenimientoById: builder.query<Mantenimiento, number>({
      query: (id) => `${API_ENDPOINTS.MANTENIMIENTOS}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Mantenimiento', id }],
    }),
    getMantenimientosByActivo: builder.query<Mantenimiento[], number>({
      query: (activoId) => `${API_ENDPOINTS.MANTENIMIENTOS}/activo/${activoId}`,
      providesTags: ['Mantenimiento'],
    }),
    getMantenimientosPendientes: builder.query<Mantenimiento[], void>({
      query: () => `${API_ENDPOINTS.MANTENIMIENTOS}/pendientes`,
      providesTags: ['Mantenimiento'],
    }),
    getMantenimientosVencidos: builder.query<Mantenimiento[], void>({
      query: () => `${API_ENDPOINTS.MANTENIMIENTOS}/vencidos`,
      providesTags: ['Mantenimiento'],
    }),
    createMantenimiento: builder.mutation<Mantenimiento, CreateMantenimientoDto>({
      query: (body) => ({
        url: API_ENDPOINTS.MANTENIMIENTOS,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Mantenimiento'],
    }),
    updateMantenimiento: builder.mutation<Mantenimiento, UpdateMantenimientoDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.MANTENIMIENTOS}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Mantenimiento', id },
        'Mantenimiento',
      ],
    }),
    deleteMantenimiento: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.MANTENIMIENTOS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Mantenimiento'],
    }),
  }),
});

export const {
  useGetMantenimientosQuery,
  useGetMantenimientoByIdQuery,
  useGetMantenimientosByActivoQuery,
  useGetMantenimientosPendientesQuery,
  useGetMantenimientosVencidosQuery,
  useCreateMantenimientoMutation,
  useUpdateMantenimientoMutation,
  useDeleteMantenimientoMutation,
} = mantenimientoApi;
