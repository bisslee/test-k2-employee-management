import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import toast from 'react-hot-toast';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

// Create axios instance
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add JWT token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response) {
      const { status, data } = error.response;
      
      // Handle 401 Unauthorized - redirect to login
      if (status === 401) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
        toast.error('Sessão expirada. Por favor, faça login novamente.');
        return Promise.reject(error);
      }

      // Handle API error response
      if (data?.error?.message) {
        toast.error(data.error.message);
      } else if (data?.message) {
        toast.error(data.message);
      } else {
        toast.error('Ocorreu um erro ao processar a requisição.');
      }
    } else if (error.request) {
      toast.error('Não foi possível conectar ao servidor.');
    } else {
      toast.error('Ocorreu um erro inesperado.');
    }
    
    return Promise.reject(error);
  }
);

export default api;
