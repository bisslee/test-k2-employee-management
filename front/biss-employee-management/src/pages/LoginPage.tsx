import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import api from '../shared/api';
import { API_ENDPOINTS, ROUTES } from '../shared/constants';
import { LoginRequest, LoginResponse } from '../shared/types';
import toast from 'react-hot-toast';
import 'remixicon/fonts/remixicon.css';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const request: LoginRequest = { email, password };
      const response = await api.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, request);

      if (response.data.success && response.data.data?.response) {
        const { token, employee } = response.data.data.response;
        
        if (token && employee) {
          login(token, employee);
          toast.success('Login realizado com sucesso!');
          navigate(ROUTES.EMPLOYEES);
        }
      } else {
        toast.error(response.data.error?.message || 'Erro ao realizar login');
      }
    } catch (error) {
      // Error is handled by axios interceptor
      console.error('Login error:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <i className="ri-user-line login-icon"></i>
          <h1>Employee Management</h1>
          <p>Faça login para continuar</p>
        </div>
        
        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="email">
              <i className="ri-mail-line"></i> Email
            </label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="seu@email.com"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">
              <i className="ri-lock-line"></i> Senha
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="••••••••"
              disabled={loading}
            />
          </div>

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <>
                <i className="ri-loader-4-line spin"></i> Entrando...
              </>
            ) : (
              <>
                <i className="ri-login-box-line"></i> Entrar
              </>
            )}
          </button>
        </form>

        <div className="login-footer">
          <p>Usuário master: admin@employee.com</p>
          <p>Senha: admin@123</p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
