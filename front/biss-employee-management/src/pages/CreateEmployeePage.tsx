import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../shared/api';
import { API_ENDPOINTS, ROUTES } from '../shared/constants';
import { AddEmployeeRequest, EmployeeRole } from '../shared/types';
import toast from 'react-hot-toast';
import * as Dialog from '@radix-ui/react-dialog';
import 'remixicon/fonts/remixicon.css';

const CreateEmployeePage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  
  const [formData, setFormData] = useState<AddEmployeeRequest>({
    firstName: '',
    lastName: '',
    email: '',
    document: '',
    birthDate: '',
    role: EmployeeRole.Assistant,
    password: '',
    phoneNumbers: [
      { number: '', type: 'Mobile' },
      { number: '', type: 'Mobile' },
    ],
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handlePhoneChange = (index: number, field: 'number' | 'type', value: string) => {
    setFormData((prev) => {
      const newPhones = [...prev.phoneNumbers];
      newPhones[index] = { ...newPhones[index], [field]: value };
      return { ...prev, phoneNumbers: newPhones };
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      // Normalizar documento removendo formatação antes de enviar
      const normalizedDocument = formData.document.replace(/\D/g, '');
      
      const request: AddEmployeeRequest = {
        ...formData,
        document: normalizedDocument,
        phoneNumbers: formData.phoneNumbers.filter((phone) => phone.number.trim() !== ''),
      };

      console.log('[CreateEmployeePage] Enviando requisição para criar funcionário:', {
        email: request.email,
        document: request.document,
        normalizedDocument,
        phoneCount: request.phoneNumbers.length,
        role: request.role,
      });

      const response = await api.post(API_ENDPOINTS.EMPLOYEES.BASE, request);

      console.log('[CreateEmployeePage] Resposta recebida:', {
        success: response.data.success,
        statusCode: response.status,
        hasData: !!response.data.data,
        hasError: !!response.data.error,
        errorMessage: response.data.error?.message,
      });

      if (response.data.success) {
        console.log('[CreateEmployeePage] Funcionário criado com sucesso:', response.data.data?.response?.id);
        toast.success('Funcionário criado com sucesso!');
        setShowSuccessModal(true);
        // A navegação será feita no modal de sucesso
      } else {
        // Se não foi sucesso, mostrar erro e não fechar o formulário
        const errorMessage = response.data.error?.message || response.data.error?.detail || response.data.message || 'Erro ao criar funcionário';
        console.warn('[CreateEmployeePage] Resposta não foi sucesso:', {
          errorMessage,
          fullResponse: response.data,
        });
        toast.error(errorMessage);
        setLoading(false);
        return; // Não navegar nem mostrar modal de sucesso
      }
    } catch (error: any) {
      console.error('[CreateEmployeePage] Erro ao criar funcionário:', {
        error,
        response: error.response?.data,
        status: error.response?.status,
        statusText: error.response?.statusText,
        requestData: error.config?.data,
      });

      // Verificar se é um erro do axios com resposta da API
      if (error.response?.data?.error?.message) {
        toast.error(error.response.data.error.message);
      } else if (error.response?.data?.error?.detail) {
        // Pode ter detalhes adicionais no error.detail
        toast.error(error.response.data.error.detail);
      } else if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else if (error.message) {
        toast.error(error.message);
      } else {
        toast.error('Erro ao criar funcionário. Tente novamente.');
      }
      // Não navegar em caso de erro - manter o usuário no formulário
    } finally {
      setLoading(false);
    }
  };

  const handleSuccessModalClose = () => {
    setShowSuccessModal(false);
    // Navegar para a página de login conforme especificação
    navigate(ROUTES.LOGIN);
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>
          <i className="ri-user-add-line"></i> Criar Funcionário
        </h1>
        <button
          className="btn-secondary"
          onClick={() => navigate(ROUTES.EMPLOYEES)}
        >
          <i className="ri-arrow-left-line"></i> Voltar
        </button>
      </div>

      <form onSubmit={handleSubmit} className="form-container">
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="firstName">
              <i className="ri-user-line"></i> Primeiro Nome *
            </label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="lastName">
              <i className="ri-user-line"></i> Último Nome *
            </label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="email">
              <i className="ri-mail-line"></i> Email *
            </label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="document">
              <i className="ri-id-card-line"></i> Documento (CPF/CNPJ) *
            </label>
            <input
              type="text"
              id="document"
              name="document"
              value={formData.document}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="birthDate">
              <i className="ri-calendar-line"></i> Data de Nascimento *
            </label>
            <input
              type="date"
              id="birthDate"
              name="birthDate"
              value={formData.birthDate}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="role">
              <i className="ri-briefcase-line"></i> Cargo *
            </label>
            <select
              id="role"
              name="role"
              value={formData.role}
              onChange={handleInputChange}
              required
              disabled={loading}
            >
              <option value={EmployeeRole.Director}>Diretor</option>
              <option value={EmployeeRole.Manager}>Gerente</option>
              <option value={EmployeeRole.Analyst}>Analista</option>
              <option value={EmployeeRole.Assistant}>Assistente</option>
            </select>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="password">
            <i className="ri-lock-line"></i> Senha *
          </label>
          <input
            type="password"
            id="password"
            name="password"
            value={formData.password}
            onChange={handleInputChange}
            required
            disabled={loading}
          />
        </div>

        <div className="form-section">
          <h3>
            <i className="ri-phone-line"></i> Telefones
          </h3>
          {formData.phoneNumbers.map((phone, index) => (
            <div key={index} className="form-row">
              <div className="form-group">
                <label htmlFor={`phone-${index}`}>
                  Telefone {index + 1}
                </label>
                <input
                  type="tel"
                  id={`phone-${index}`}
                  value={phone.number}
                  onChange={(e) => handlePhoneChange(index, 'number', e.target.value)}
                  placeholder="(00) 00000-0000"
                  disabled={loading}
                />
              </div>
              <div className="form-group">
                <label htmlFor={`phone-type-${index}`}>Tipo</label>
                <select
                  id={`phone-type-${index}`}
                  value={phone.type || 'Mobile'}
                  onChange={(e) => handlePhoneChange(index, 'type', e.target.value)}
                  disabled={loading}
                >
                  <option value="Mobile">Celular</option>
                  <option value="Home">Residencial</option>
                  <option value="Work">Trabalho</option>
                </select>
              </div>
            </div>
          ))}
        </div>

        <div className="form-actions">
          <button
            type="button"
            className="btn-secondary"
            onClick={() => navigate(ROUTES.EMPLOYEES)}
            disabled={loading}
          >
            Cancelar
          </button>
          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? (
              <>
                <i className="ri-loader-4-line spin"></i> Criando...
              </>
            ) : (
              <>
                <i className="ri-save-line"></i> Criar Funcionário
              </>
            )}
          </button>
        </div>
      </form>

      <Dialog.Root open={showSuccessModal} onOpenChange={setShowSuccessModal}>
        <Dialog.Portal>
          <Dialog.Overlay className="dialog-overlay" />
          <Dialog.Content className="dialog-content">
            <Dialog.Title className="dialog-title">
              <i className="ri-checkbox-circle-line success-icon"></i>
              Sucesso!
            </Dialog.Title>
            <Dialog.Description className="dialog-description">
              Funcionário criado com sucesso!
            </Dialog.Description>
            <div className="dialog-actions">
              <button
                className="btn-primary"
                onClick={handleSuccessModalClose}
              >
                OK
              </button>
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>
    </div>
  );
};

export default CreateEmployeePage;
