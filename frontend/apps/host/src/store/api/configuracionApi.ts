import { baseApi } from './baseApi';

export interface ConfiguracionSistema {
  id: number;
  empresa_Id?: number;
  clave: string;
  valor?: string;
  descripcion?: string;
  tipo_Dato: string;
  es_Global: boolean;
  fecha_Actualizacion: string;
}

export interface CreateConfiguracionDto {
  empresa_Id?: number;
  clave: string;
  valor?: string;
  descripcion?: string;
  tipo_Dato: string;
  es_Global: boolean;
}

export interface UpdateConfiguracionDto {
  empresa_Id?: number;
  clave: string;
  valor?: string;
  descripcion?: string;
  tipo_Dato: string;
  es_Global: boolean;
}

export const configuracionApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getConfiguraciones: builder.query<ConfiguracionSistema[], void>({
      query: () => '/ConfiguracionSistema',
      providesTags: ['Configuracion'],
    }),
    getConfiguracionById: builder.query<ConfiguracionSistema, number>({
      query: (id) => `/ConfiguracionSistema/${id}`,
      providesTags: ['Configuracion'],
    }),
    getConfiguracionByClave: builder.query<ConfiguracionSistema, string>({
      query: (clave) => `/ConfiguracionSistema/clave/${clave}`,
      providesTags: ['Configuracion'],
    }),
    getConfiguracionesGlobales: builder.query<ConfiguracionSistema[], void>({
      query: () => '/ConfiguracionSistema/globales',
      providesTags: ['Configuracion'],
    }),
    createConfiguracion: builder.mutation<ConfiguracionSistema, CreateConfiguracionDto>({
      query: (body) => ({
        url: '/ConfiguracionSistema',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Configuracion'],
    }),
    updateConfiguracion: builder.mutation<ConfiguracionSistema, { id: number; body: UpdateConfiguracionDto }>({
      query: ({ id, body }) => ({
        url: `/ConfiguracionSistema/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: ['Configuracion'],
    }),
    deleteConfiguracion: builder.mutation<void, number>({
      query: (id) => ({
        url: `/ConfiguracionSistema/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Configuracion'],
    }),
  }),
  overrideExisting: true,
});

export const {
  useGetConfiguracionesQuery,
  useGetConfiguracionByIdQuery,
  useGetConfiguracionByClaveQuery,
  useGetConfiguracionesGlobalesQuery,
  useCreateConfiguracionMutation,
  useUpdateConfiguracionMutation,
  useDeleteConfiguracionMutation,
} = configuracionApi;
