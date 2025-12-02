import { baseApi } from './baseApi';

export interface Usuario {
  id: number;
  empresa_Id: number;
  rol_Id: number;
  usuario1: string;
  email: string;
  nombre_Completo: string;
  telefono?: string;
  avatar_URL?: string;
  estado: string;
  intentos_Fallidos: number;
  fecha_Creacion: string;
  fecha_Actualizacion: string;
  ultimo_Acceso?: string;
  requiere_Cambio_Password: boolean;
  twoFA_Activo: boolean;
  rol?: string;
  empresa_Nombre?: string;
}

export interface UpdateUsuarioEstadoRequest {
  estado: string;
}

export const usuarioApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getUsuarios: builder.query<Usuario[], void>({
      query: () => '/Usuario',
      providesTags: ['Usuario'],
    }),

    getUsuarioById: builder.query<Usuario, number>({
      query: (id) => `/Usuario/${id}`,
      providesTags: (result, error, id) => [{ type: 'Usuario', id }],
    }),

    updateUsuarioEstado: builder.mutation<void, { id: number; estado: string }>({
      query: ({ id, estado }) => ({
        url: `/Usuario/${id}/estado`,
        method: 'PUT',
        body: { estado },
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Usuario', id }, 'Usuario'],
    }),

    deleteUsuario: builder.mutation<void, number>({
      query: (id) => ({
        url: `/Usuario/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Usuario'],
    }),
  }),
});

export const {
  useGetUsuariosQuery,
  useGetUsuarioByIdQuery,
  useUpdateUsuarioEstadoMutation,
  useDeleteUsuarioMutation,
} = usuarioApi;
