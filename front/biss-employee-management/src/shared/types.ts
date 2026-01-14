// Types for API responses and requests

export interface ApiResponse<T> {
  success: boolean;
  data?: ApiDataResponse<T>;
  metadata?: ApiMetaDataResponse;
  error?: ApiErrorResponse;
  statusCode: number;
}

export interface ApiDataResponse<T> {
  response: T;
}

export interface ApiMetaDataResponse {
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ApiErrorResponse {
  code: string;
  message: string;
}

// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse extends ApiResponse<LoginData> {}

export interface LoginData {
  token: string;
  refreshToken?: string;
  expiresAt: string;
  employee?: EmployeeInfo;
}

export interface EmployeeInfo {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

// Employee types
export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  document: string;
  birthDate: string;
  role: EmployeeRole;
  phoneNumbers?: PhoneNumber[];
  createdAt?: string;
  updatedAt?: string;
}

export enum EmployeeRole {
  Director = 1,
  Manager = 2,
  Analyst = 3,
  Assistant = 4,
}

export interface PhoneNumber {
  id?: string;
  number: string;
  type?: PhoneNumberType;
}

export enum PhoneNumberType {
  Mobile = "Mobile",
  Home = "Home",
  Work = "Work",
}

export interface AddEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  document: string;
  birthDate: string;
  role: EmployeeRole;
  password: string;
  phoneNumbers: PhoneNumberRequest[];
}

export interface PhoneNumberRequest {
  number: string;
  type?: string;
}

export interface GetEmployeesRequest {
  fullName?: string;
  email?: string;
  document?: string;
  role?: EmployeeRole;
  status?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface GetEmployeesResponse extends ApiResponse<Employee[]> {}

export interface EditEmployeeRequest {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  document: string;
  birthDate: string;
  role: EmployeeRole;
  password?: string; // Opcional na edição
  phoneNumbers: EditPhoneNumberRequest[];
}

export interface EditPhoneNumberRequest {
  id?: string;
  number: string;
  type?: string;
}
