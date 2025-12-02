import { baseApi } from './baseApi';

export interface Conversacion {
  id: number;
  empresa_Id: number;
  asunto?: string;
  reserva_Id?: number;
  fecha_Creacion: string;
  estado: string;
  ultimo_Mensaje?: Mensaje;
  mensajes_No_Leidos: number;
}

export interface Mensaje {
  id: number;
  conversacion_Id: number;
  emisor_Usuario_Id: number;
  emisor_Nombre: string;
  emisor_Avatar?: string;
  contenido: string;
  leido: boolean;
  fecha_Envio: string;
}

export interface CreateConversacionDto {
  asunto?: string;
  reserva_Id?: number;
  mensaje_Inicial?: string;
}

export interface SendMensajeDto {
  conversacion_Id: number;
  contenido: string;
}

export const chatApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getConversaciones: builder.query<Conversacion[], void>({
      query: () => '/Chat/conversaciones',
      providesTags: ['Chat'],
    }),
    getConversacion: builder.query<Conversacion, number>({
      query: (id) => `/Chat/conversaciones/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Chat', id }],
    }),
    getMensajes: builder.query<Mensaje[], number>({
      query: (conversacionId) => `/Chat/mensajes/${conversacionId}`,
      providesTags: (_result, _error, conversacionId) => [
        { type: 'Mensaje', id: conversacionId },
      ],
    }),
    createConversacion: builder.mutation<{ id: number }, CreateConversacionDto>({
      query: (body) => ({
        url: '/Chat/conversaciones',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Chat'],
    }),
    sendMensaje: builder.mutation<{ id: number }, SendMensajeDto>({
      query: (body) => ({
        url: '/Chat/mensajes',
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, arg) => [
        { type: 'Mensaje', id: arg.conversacion_Id },
        'Chat',
      ],
    }),
    marcarLeidos: builder.mutation<void, number>({
      query: (conversacion_Id) => ({
        url: '/Chat/mensajes/marcar-leidos',
        method: 'POST',
        body: { conversacion_Id },
      }),
      invalidatesTags: (_result, _error, conversacionId) => [
        { type: 'Mensaje', id: conversacionId },
        'Chat',
      ],
    }),
    cerrarConversacion: builder.mutation<void, number>({
      query: (id) => ({
        url: `/Chat/conversaciones/${id}/cerrar`,
        method: 'PUT',
      }),
      invalidatesTags: ['Chat'],
    }),
  }),
});

export const {
  useGetConversacionesQuery,
  useGetConversacionQuery,
  useGetMensajesQuery,
  useCreateConversacionMutation,
  useSendMensajeMutation,
  useMarcarLeidosMutation,
  useCerrarConversacionMutation,
} = chatApi;
