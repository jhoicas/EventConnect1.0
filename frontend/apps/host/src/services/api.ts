// API Service para productos
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5555/api';

// Helper para obtener headers con autenticación
const getAuthHeaders = () => {
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };
};

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
  peso_Kg?: number;
  dimensiones?: string;
  observaciones?: string;
  activo: boolean;
}

export interface Empresa {
  id: number;
  razon_Social: string;
  nit: string;
  email: string;
  telefono?: string;
  direccion?: string;
  ciudad?: string;
  pais: string;
  logo_URL?: string;
  estado: string;
}

export interface Categoria {
  id: number;
  empresa_Id: number;
  nombre: string;
  descripcion?: string;
  icono?: string;
  color?: string;
  activo: boolean;
}

// Funciones de API
export const productosAPI = {
  async getAll(): Promise<Producto[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Producto`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar productos: ${response.status} ${response.statusText}`, errorText);
        throw new Error(`Error al cargar productos: ${response.status} ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching productos:', error);
      // Verificar si el servidor está disponible
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
        console.error('Asegúrate de que el backend esté corriendo: cd EventConnect.API && dotnet run');
      }
      return [];
    }
  },

  async getById(id: number): Promise<Producto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/Producto/${id}`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        throw new Error('Error al cargar producto');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching producto:', error);
      return null;
    }
  },

  async getByCategoria(categoriaId: number): Promise<Producto[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Producto/categoria/${categoriaId}`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        throw new Error('Error al cargar productos por categoría');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching productos by categoria:', error);
      return [];
    }
  },

  buscar(productos: Producto[], query: string): Producto[] {
    const searchLower = query.toLowerCase();
    return productos.filter(
      (p) =>
        p.nombre.toLowerCase().includes(searchLower) ||
        (p.descripcion && p.descripcion.toLowerCase().includes(searchLower)) ||
        p.sku.toLowerCase().includes(searchLower)
    );
  },
};

export const empresasAPI = {
  async getAll(): Promise<Empresa[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Empresa`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar empresas: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar empresas');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching empresas:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
        console.error('Asegúrate de que el backend esté corriendo: cd EventConnect.API && dotnet run');
      }
      return [];
    }
  },

  async getById(id: number): Promise<Empresa | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/Empresa/${id}`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar empresa ${id}: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar empresa');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching empresa:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return null;
    }
  },
};

export const categoriasAPI = {
  async getAll(): Promise<Categoria[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Categoria`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar categorías: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar categorías');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching categorias:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return [];
    }
  },

  async getByEmpresa(empresaId: number): Promise<Categoria[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Categoria/empresa/${empresaId}`, {
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar categorías por empresa ${empresaId}: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar categorías por empresa');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching categorias by empresa:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return [];
    }
  },
};

// Interfaces para Reservas
export interface Reserva {
  id: number;
  empresa_Id: number;
  cliente_Id?: number;
  codigo_Reserva: string;
  fecha_Inicio: string;
  fecha_Fin: string;
  fecha_Evento?: string;
  direccion_Entrega?: string;
  ciudad_Entrega?: string;
  observaciones?: string;
  estado: string;
  subtotal: number;
  impuestos: number;
  descuentos: number;
  total: number;
  anticipo_Requerido: number;
  anticipo_Pagado: number;
  metodo_Pago?: string;
  fecha_Creacion: string;
}

export interface DetalleReserva {
  id: number;
  reserva_Id: number;
  producto_Id?: number;
  activo_Id?: number;
  cantidad: number;
  precio_Unitario: number;
  subtotal: number;
  observaciones?: string;
}

// API de Reservas
export const reservasAPI = {
  async getAll(): Promise<Reserva[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Reserva`, {
        headers: getAuthHeaders(),
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar reservas: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar reservas');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching reservas:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return [];
    }
  },

  async getById(id: number): Promise<Reserva | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/Reserva/${id}`, {
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar reserva ${id}: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar reserva');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching reserva:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return null;
    }
  },

  async getByEstado(estado: string): Promise<Reserva[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/Reserva/estado/${estado}`, {
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al cargar reservas por estado ${estado}: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al cargar reservas por estado');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error fetching reservas by estado:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return [];
    }
  },

  async create(reserva: Partial<Reserva>): Promise<Reserva | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/Reserva`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(reserva),
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Error al crear reserva: ${response.status} ${response.statusText}`, errorText);
        throw new Error('Error al crear reserva');
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error creating reserva:', error);
      if (error instanceof TypeError && error.message.includes('fetch')) {
        console.error('⚠️ El servidor backend no está disponible en:', API_BASE_URL);
      }
      return null;
    }
  },
};
