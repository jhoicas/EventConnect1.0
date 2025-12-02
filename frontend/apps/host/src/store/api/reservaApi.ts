import { baseApi } from './baseApi';
import { API_ENDPOINTS } from '@eventconnect/shared';

export interface Reserva {
  id: number;
  empresa_Id: number;
  cliente_Id: number;
  codigo_Reserva: string;
  estado: string;
  fecha_Evento: string;
  fecha_Entrega?: string;
  fecha_Devolucion_Programada?: string;
  fecha_Devolucion_Real?: string;
  direccion_Entrega?: string;
  ciudad_Entrega?: string;
  contacto_En_Sitio?: string;
  telefono_Contacto?: string;
  subtotal: number;
  descuento: number;
  total: number;
  fianza: number;
  fianza_Devuelta: boolean;
  metodo_Pago: string;
  estado_Pago: string;
  observaciones?: string;
  creado_Por_Id: number;
  aprobado_Por_Id?: number;
  fecha_Aprobacion?: string;
  fecha_Creacion: string;
  fecha_Actualizacion: string;
  cancelado_Por_Id?: number;
  razon_Cancelacion?: string;
}

export interface CreateReservaDto {
  cliente_Id: number;
  fecha_Evento: string;
  fecha_Entrega?: string;
  fecha_Devolucion_Programada?: string;
  direccion_Entrega?: string;
  ciudad_Entrega?: string;
  contacto_En_Sitio?: string;
  telefono_Contacto?: string;
  subtotal: number;
  descuento?: number;
  total: number;
  fianza?: number;
  metodo_Pago?: string;
  observaciones?: string;
}

export interface UpdateReservaDto {
  id: number;
  cliente_Id: number;
  estado: string;
  fecha_Evento: string;
  fecha_Entrega?: string;
  fecha_Devolucion_Programada?: string;
  fecha_Devolucion_Real?: string;
  direccion_Entrega?: string;
  ciudad_Entrega?: string;
  contacto_En_Sitio?: string;
  telefono_Contacto?: string;
  subtotal: number;
  descuento: number;
  total: number;
  fianza: number;
  fianza_Devuelta: boolean;
  metodo_Pago: string;
  estado_Pago: string;
  observaciones?: string;
}

export const reservaApi = baseApi.injectEndpoints({
  overrideExisting: true,
  endpoints: (builder) => ({
    getReservas: builder.query<Reserva[], void>({
      query: () => API_ENDPOINTS.RESERVAS,
      providesTags: ['Reserva'],
    }),
    getReservaById: builder.query<Reserva, number>({
      query: (id) => `${API_ENDPOINTS.RESERVAS}/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Reserva', id }],
    }),
    getReservasByEstado: builder.query<Reserva[], string>({
      query: (estado) => `${API_ENDPOINTS.RESERVAS}/estado/${estado}`,
      providesTags: ['Reserva'],
    }),
    createReserva: builder.mutation<Reserva, CreateReservaDto>({
      query: (body) => ({
        url: API_ENDPOINTS.RESERVAS,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Reserva'],
    }),
    updateReserva: builder.mutation<Reserva, UpdateReservaDto>({
      query: ({ id, ...body }) => ({
        url: `${API_ENDPOINTS.RESERVAS}/${id}`,
        method: 'PUT',
        body: { ...body, id },
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Reserva', id },
        'Reserva',
      ],
    }),
    deleteReserva: builder.mutation<void, number>({
      query: (id) => ({
        url: `${API_ENDPOINTS.RESERVAS}/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Reserva'],
    }),
  }),
});

export const {
  useGetReservasQuery,
  useGetReservaByIdQuery,
  useGetReservasByEstadoQuery,
  useCreateReservaMutation,
  useUpdateReservaMutation,
  useDeleteReservaMutation,
} = reservaApi;
