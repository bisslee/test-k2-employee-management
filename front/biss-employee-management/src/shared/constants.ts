export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/v1/auth/login',
  },
  EMPLOYEES: {
    BASE: '/api/v1/employee',
    BY_ID: (id: string) => `/api/v1/employee/${id}`,
  },
} as const;

export const STORAGE_KEYS = {
  TOKEN: 'token',
  USER: 'user',
} as const;

export const ROUTES = {
  LOGIN: '/login',
  EMPLOYEES: '/employees',
  CREATE_EMPLOYEE: '/employees/create',
} as const;
