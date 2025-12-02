import { baseApi } from './baseApi';
import {
  LoginRequest,
  LoginResponse,
  API_ENDPOINTS,
} from '@eventconnect/shared';

export interface RegisterPersonaRequest {
  Usuario: string;
  Email: string;
  Password: string;
  Nombre_Completo: string;
  Telefono?: string;
  Empresa_Id: number;
  Rol_Id: number;
}

export interface RegisterEmpresaRequest {
  Usuario: string;
  Email: string;
  Password: string;
  Nombre_Completo: string;
  Telefono?: string;
  Empresa_Id: number;
  Rol_Id: number;
  // Datos adicionales de empresa
  Razon_Social?: string;
  NIT?: string;
  Direccion?: string;
  Ciudad?: string;
  Requiere_SIGI?: boolean;
}

export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: API_ENDPOINTS.LOGIN,
        method: 'POST',
        body: credentials,
      }),
    }),
    register: builder.mutation<LoginResponse, RegisterPersonaRequest | RegisterEmpresaRequest>({
      query: (userData) => ({
        url: '/Auth/register',
        method: 'POST',
        body: userData,
      }),
    }),
  }),
});

export const { useLoginMutation, useRegisterMutation } = authApi;
