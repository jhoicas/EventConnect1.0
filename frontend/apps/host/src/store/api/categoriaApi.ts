import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Categoria {
  id: number;
  nombre: string;
  descripcion?: string;
  icono?: string;
  color?: string;
  fecha_Creacion: string;
  activo: boolean;
}

export interface CreateCategoriaDto {
  nombre: string;
  descripcion?: string;
  icono?: string;
  color?: string;
}

export interface UpdateCategoriaDto {
  id: number;
  nombre: string;
  descripcion?: string;
  icono?: string;
  color?: string;
  activo: boolean;
}

export const categoriaApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getCategorias: builder.query<Categoria[], void>({
      query: () => API_ENDPOINTS.CATEGORIAS,
      providesTags: ['Categoria'],
    }),
    getCategoriaById: builder.query<Categoria, number>({
      query: (id) => `${API_ENDPOINTS.CATEGORIAS}/${id}`,
      providesTags: (result, error, id) => [{ type: 'Categoria', id }],
    }),
    createCategoria: builder.mutation<Categoria, CreateCategoriaDto>({
      query: (data) => ({
        url: API_ENDPOINTS.CATEGORIAS,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Categoria'],
    }),
    updateCategoria: builder.mutation<Categoria, UpdateCategoriaDto>({
      query: ({ id, ...data }) => ({
        url: `${API_ENDPOINTS.CATEGORIAS}/${id}`,
        method: 'PUT',
        body: { ...data, id },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'Categoria', id },
        'Categoria',
      ],
    }),
    deleteCategoria: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.CATEGORIAS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Categoria'],
    }),
  }),
});

export const {
  useGetCategoriasQuery,
  useGetCategoriaByIdQuery,
  useCreateCategoriaMutation,
  useUpdateCategoriaMutation,
  useDeleteCategoriaMutation,
} = categoriaApi;
