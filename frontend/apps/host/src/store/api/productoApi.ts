import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Producto {
  id: number;
  empresa_Id: number;
  categoria_Id: number;
  sku: string;
  nombre: string;
  descripcion?: string;
  unidad_Medida: string;
  precio_Alquiler_Dia: number;
  cantidad_Stock: number;
  stock_Minimo: number;
  imagen_URL?: string;
  es_Alquilable: boolean;
  es_Vendible: boolean;
  requiere_Mantenimiento: boolean;
  dias_Mantenimiento: number;
  peso_Kg?: number;
  dimensiones?: string;
  observaciones?: string;
  activo: boolean;
  fecha_Creacion: string;
  fecha_Actualizacion: string;
}

export interface CreateProductoDto {
  categoria_Id: number;
  sku: string;
  nombre: string;
  descripcion?: string;
  unidad_Medida?: string;
  precio_Alquiler_Dia: number;
  cantidad_Stock: number;
  stock_Minimo?: number;
  imagen_URL?: string;
  es_Alquilable?: boolean;
  es_Vendible?: boolean;
  peso_Kg?: number;
  dimensiones?: string;
  observaciones?: string;
}

export interface UpdateProductoDto {
  id: number;
  categoria_Id: number;
  sku: string;
  nombre: string;
  descripcion?: string;
  unidad_Medida: string;
  precio_Alquiler_Dia: number;
  cantidad_Stock: number;
  stock_Minimo: number;
  imagen_URL?: string;
  es_Alquilable: boolean;
  es_Vendible: boolean;
  requiere_Mantenimiento: boolean;
  dias_Mantenimiento: number;
  peso_Kg?: number;
  dimensiones?: string;
  observaciones?: string;
  activo: boolean;
}

export const productoApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getProductos: builder.query<Producto[], void>({
      query: () => API_ENDPOINTS.PRODUCTOS,
      providesTags: ['Producto'],
    }),
    getProductoById: builder.query<Producto, number>({
      query: (id) => `${API_ENDPOINTS.PRODUCTOS}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Producto', id }],
    }),
    getProductosStockBajo: builder.query<Producto[], void>({
      query: () => `${API_ENDPOINTS.PRODUCTOS}/stock-bajo`,
      providesTags: ['Producto'],
    }),
    createProducto: builder.mutation<Producto, CreateProductoDto>({
      query: (body) => ({
        url: API_ENDPOINTS.PRODUCTOS,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Producto'],
    }),
    updateProducto: builder.mutation<Producto, UpdateProductoDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.PRODUCTOS}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Producto', id },
        'Producto',
      ],
    }),
    deleteProducto: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.PRODUCTOS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Producto'],
    }),
  }),
});

export const {
  useGetProductosQuery,
  useGetProductoByIdQuery,
  useGetProductosStockBajoQuery,
  useCreateProductoMutation,
  useUpdateProductoMutation,
  useDeleteProductoMutation,
} = productoApi;
