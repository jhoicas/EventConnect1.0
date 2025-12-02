import { baseApi } from './baseApi';

export interface ContenidoLanding {
  id: number;
  seccion: string;
  titulo: string;
  subtitulo?: string;
  descripcion?: string;
  imagen_URL?: string;
  icono_Nombre?: string;
  orden: number;
  activo: boolean;
  fecha_Actualizacion: string;
}

export interface CreateContenidoDto {
  seccion: string;
  titulo: string;
  subtitulo?: string;
  descripcion?: string;
  imagen_URL?: string;
  icono_Nombre?: string;
  orden: number;
  activo: boolean;
}

export interface UpdateContenidoDto {
  seccion: string;
  titulo: string;
  subtitulo?: string;
  descripcion?: string;
  imagen_URL?: string;
  icono_Nombre?: string;
  orden: number;
  activo: boolean;
}

export const contenidoLandingApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getContenidos: builder.query<ContenidoLanding[], void>({
      query: () => '/ContenidoLanding',
      providesTags: ['ContenidoLanding'],
    }),
    getContenidosActivos: builder.query<ContenidoLanding[], void>({
      query: () => '/ContenidoLanding/activos',
      providesTags: ['ContenidoLanding'],
    }),
    getContenidosBySeccion: builder.query<ContenidoLanding[], string>({
      query: (seccion) => `/ContenidoLanding/seccion/${seccion}`,
      providesTags: ['ContenidoLanding'],
    }),
    getContenidoById: builder.query<ContenidoLanding, number>({
      query: (id) => `/ContenidoLanding/${id}`,
      providesTags: ['ContenidoLanding'],
    }),
    createContenido: builder.mutation<ContenidoLanding, CreateContenidoDto>({
      query: (body) => ({
        url: '/ContenidoLanding',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['ContenidoLanding'],
    }),
    updateContenido: builder.mutation<ContenidoLanding, { id: number; body: UpdateContenidoDto }>({
      query: ({ id, body }) => ({
        url: `/ContenidoLanding/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['ContenidoLanding'],
    }),
    deleteContenido: builder.mutation<void, number>({
      query: (id) => ({
        url: `/ContenidoLanding/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['ContenidoLanding'],
    }),
  }),
  overrideExisting: true,
});

export const {
  useGetContenidosQuery,
  useGetContenidosActivosQuery,
  useGetContenidosBySeccionQuery,
  useGetContenidoByIdQuery,
  useCreateContenidoMutation,
  useUpdateContenidoMutation,
  useDeleteContenidoMutation,
} = contenidoLandingApi;
