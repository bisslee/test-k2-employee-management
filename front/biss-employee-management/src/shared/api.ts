import axios, { AxiosInstance, InternalAxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import toast from 'react-hot-toast';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

// Helper para logar no console apenas em desenvolvimento
const log = (level: 'info' | 'warn' | 'error', message: string, data?: any) => {
  if (import.meta.env.DEV) {
    const timestamp = new Date().toISOString();
    const logMessage = `[API ${timestamp}] ${message}`;
    
    switch (level) {
      case 'info':
        console.log(logMessage, data || '');
        break;
      case 'warn':
        console.warn(logMessage, data || '');
        break;
      case 'error':
        console.error(logMessage, data || '');
        break;
    }
  }
};

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
      log('info', `[REQUEST] ${config.method?.toUpperCase()} ${config.url}`, {
        hasToken: !!token,
        tokenLength: token.length,
        params: config.params,
        data: config.data,
      });
    } else {
      log('info', `[REQUEST] ${config.method?.toUpperCase()} ${config.url}`, {
        hasToken: false,
        params: config.params,
        data: config.data,
      });
    }
    return config;
  },
  (error) => {
    log('error', '[REQUEST ERROR]', error);
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response: AxiosResponse) => {
    log('info', `[RESPONSE] ${response.config.method?.toUpperCase()} ${response.config.url}`, {
      status: response.status,
      statusText: response.statusText,
      success: response.data?.success,
      hasData: !!response.data?.data,
      hasError: !!response.data?.error,
      errorMessage: response.data?.error?.message,
    });
    return response;
  },
  (error: AxiosError) => {
    if (error.response) {
      const { status, data, config } = error.response;
      
      log('error', `[RESPONSE ERROR] ${config?.method?.toUpperCase()} ${config?.url}`, {
        status,
        statusText: error.response.statusText,
        data: data,
        errorMessage: (data as any)?.error?.message || (data as any)?.message,
        errorDetail: (data as any)?.error?.detail,
      });
      
      // Handle 401 Unauthorized - redirect to login
      if (status === 401) {
        log('warn', '[AUTH] Token inválido ou expirado, redirecionando para login');
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
        toast.error('Sessão expirada. Por favor, faça login novamente.');
        return Promise.reject(error);
      }

      // Para erros 400 e 500, a mensagem já será tratada no componente
      // Não mostrar toast aqui para evitar duplicação
      // Apenas rejeitar a promise para que o componente possa tratar
    } else if (error.request) {
      log('error', '[NETWORK ERROR] Requisição não chegou ao servidor', {
        url: error.config?.url,
        method: error.config?.method,
        message: error.message,
      });
      toast.error('Não foi possível conectar ao servidor.');
    } else {
      log('error', '[UNEXPECTED ERROR]', {
        message: error.message,
        config: error.config,
      });
      toast.error('Ocorreu um erro inesperado.');
    }
    
    return Promise.reject(error);
  }
);

export default api;
