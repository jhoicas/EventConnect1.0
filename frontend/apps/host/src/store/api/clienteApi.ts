import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Cliente {
  id: number;
  empresa_Id: number;
  usuario_Id?: number | null;
  empresa_Nombre?: string; // Para SuperAdmin
  tipo_Cliente: string;
  nombre: string;
  documento: string;
  tipo_Documento: string;
  email?: string;
  telefono?: string;
  direccion?: string;
  ciudad?: string;
  contacto_Nombre?: string;
  contacto_Telefono?: string;
  observaciones?: string;
  rating: number;
  total_Alquileres: number;
  total_Danos_Reportados: number;
  estado: string;
  fecha_Registro: string;
  fecha_Actualizacion: string;
}

export interface CreateClienteDto {
  tipo_Cliente?: string;
  nombre: string;
  documento: string;
  tipo_Documento?: string;
  email?: string;
  telefono?: string;
  direccion?: string;
  ciudad?: string;
  contacto_Nombre?: string;
  contacto_Telefono?: string;
  observaciones?: string;
}

export interface UpdateClienteDto {
  id: number;
  tipo_Cliente: string;
  nombre: string;
  documento: string;
  tipo_Documento: string;
  email?: string;
  telefono?: string;
  direccion?: string;
  ciudad?: string;
  contacto_Nombre?: string;
  contacto_Telefono?: string;
  observaciones?: string;
  rating: number;
  estado: string;
}

export const clienteApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getClientes: builder.query<Cliente[], void>({
      query: () => API_ENDPOINTS.CLIENTES,
      providesTags: ['Cliente'],
    }),
    getClienteById: builder.query<Cliente, number>({
      query: (id) => `${API_ENDPOINTS.CLIENTES}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Cliente', id }],
    }),
    createCliente: builder.mutation<Cliente, CreateClienteDto>({
      query: (body) => ({
        url: API_ENDPOINTS.CLIENTES,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Cliente'],
    }),
    updateCliente: builder.mutation<Cliente, UpdateClienteDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.CLIENTES}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Cliente', id },
        'Cliente',
      ],
    }),
    deleteCliente: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.CLIENTES}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Cliente'],
    }),
  }),
});

export const {
  useGetClientesQuery,
  useGetClienteByIdQuery,
  useCreateClienteMutation,
  useUpdateClienteMutation,
  useDeleteClienteMutation,
} = clienteApi;
