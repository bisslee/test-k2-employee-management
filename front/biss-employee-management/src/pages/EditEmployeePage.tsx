import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../shared/api';
import { API_ENDPOINTS, ROUTES } from '../shared/constants';
import { EditEmployeeRequest, EmployeeRole, Employee } from '../shared/types';
import toast from 'react-hot-toast';
import * as Dialog from '@radix-ui/react-dialog';
import 'remixicon/fonts/remixicon.css';

const EditEmployeePage: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(false);
  const [loadingEmployee, setLoadingEmployee] = useState(true);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  
  const [formData, setFormData] = useState<EditEmployeeRequest>({
    id: id || '',
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

  // Carregar dados do funcionário
  useEffect(() => {
    const loadEmployee = async () => {
      if (!id) {
        toast.error('ID do funcionário não fornecido');
        navigate(ROUTES.EMPLOYEES);
        return;
      }

      try {
        setLoadingEmployee(true);
        console.log('[EditEmployeePage] Carregando funcionário:', id);

        const response = await api.get<{ success: boolean; data?: { response: Employee }; error?: any }>(
          API_ENDPOINTS.EMPLOYEES.BY_ID(id)
        );

        console.log('[EditEmployeePage] Resposta recebida:', {
          success: response.data.success,
          hasData: !!response.data.data,
          hasError: !!response.data.error,
        });

        if (response.data.success && response.data.data?.response) {
          const employee = response.data.data.response;
          
          // Formatar data de nascimento para o input date (YYYY-MM-DD)
          const birthDateFormatted = employee.birthDate 
            ? new Date(employee.birthDate).toISOString().split('T')[0]
            : '';

          // Preparar telefones - garantir pelo menos 2 campos
          const phoneNumbers = employee.phoneNumbers && employee.phoneNumbers.length > 0
            ? employee.phoneNumbers.map(phone => ({
                id: phone.id || undefined,
                number: phone.number || '',
                type: phone.type || 'Mobile',
              }))
            : [{ id: undefined, number: '', type: 'Mobile' }, { id: undefined, number: '', type: 'Mobile' }];

          // Garantir pelo menos 2 campos de telefone
          while (phoneNumbers.length < 2) {
            phoneNumbers.push({ id: undefined, number: '', type: 'Mobile' });
          }

          setFormData({
            id: employee.id,
            firstName: employee.firstName || '',
            lastName: employee.lastName || '',
            email: employee.email || '',
            document: employee.document || '',
            birthDate: birthDateFormatted,
            role: employee.role || EmployeeRole.Assistant,
            password: '', // Não carregar senha
            phoneNumbers: phoneNumbers,
          });

          console.log('[EditEmployeePage] Funcionário carregado com sucesso:', employee.id);
        } else {
          const errorMessage = response.data.error?.message || 'Erro ao carregar funcionário';
          toast.error(errorMessage);
          navigate(ROUTES.EMPLOYEES);
        }
      } catch (error: any) {
        console.error('[EditEmployeePage] Erro ao carregar funcionário:', {
          error,
          response: error.response?.data,
          status: error.response?.status,
        });

        const errorMessage = error.response?.data?.error?.message 
          || error.response?.data?.message 
          || 'Erro ao carregar funcionário. Tente novamente.';
        toast.error(errorMessage);
        navigate(ROUTES.EMPLOYEES);
      } finally {
        setLoadingEmployee(false);
      }
    };

    loadEmployee();
  }, [id, navigate]);

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
      
      const request: EditEmployeeRequest = {
        ...formData,
        document: normalizedDocument,
        phoneNumbers: formData.phoneNumbers
          .filter((phone) => phone.number.trim() !== '')
          .map(phone => ({
            id: phone.id,
            number: phone.number.trim(),
            type: phone.type || 'Mobile',
          })),
        // Remover password se estiver vazio
        password: formData.password?.trim() || undefined,
      };

      console.log('[EditEmployeePage] Enviando requisição para editar funcionário:', {
        id: request.id,
        email: request.email,
        document: request.document,
        normalizedDocument,
        phoneCount: request.phoneNumbers.length,
        role: request.role,
        hasPassword: !!request.password,
      });

      const response = await api.put(API_ENDPOINTS.EMPLOYEES.BY_ID(request.id), request);

      console.log('[EditEmployeePage] Resposta recebida:', {
        success: response.data.success,
        statusCode: response.status,
        hasData: !!response.data.data,
        hasError: !!response.data.error,
        errorMessage: response.data.error?.message,
      });

      if (response.data.success) {
        console.log('[EditEmployeePage] Funcionário editado com sucesso:', response.data.data?.response?.id);
        toast.success('Funcionário editado com sucesso!');
        setShowSuccessModal(true);
      } else {
        const errorMessage = response.data.error?.message || response.data.error?.detail || response.data.message || 'Erro ao editar funcionário';
        console.warn('[EditEmployeePage] Resposta não foi sucesso:', {
          errorMessage,
          fullResponse: response.data,
        });
        toast.error(errorMessage);
        setLoading(false);
        return;
      }
    } catch (error: any) {
      console.error('[EditEmployeePage] Erro ao editar funcionário:', {
        error,
        response: error.response?.data,
        status: error.response?.status,
        statusText: error.response?.statusText,
        requestData: error.config?.data,
      });

      if (error.response?.data?.error?.message) {
        toast.error(error.response.data.error.message);
      } else if (error.response?.data?.error?.detail) {
        toast.error(error.response.data.error.detail);
      } else if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else if (error.message) {
        toast.error(error.message);
      } else {
        toast.error('Erro ao editar funcionário. Tente novamente.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSuccessModalClose = () => {
    setShowSuccessModal(false);
    navigate(ROUTES.EMPLOYEES);
  };

  if (loadingEmployee) {
    return (
      <div className="page-container">
        <div className="page-header">
          <h1>
            <i className="ri-user-edit-line"></i> Editar Funcionário
          </h1>
        </div>
        <div style={{ textAlign: 'center', padding: '2rem' }}>
          <i className="ri-loader-4-line spin" style={{ fontSize: '2rem' }}></i>
          <p>Carregando dados do funcionário...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>
          <i className="ri-user-edit-line"></i> Editar Funcionário
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
            <i className="ri-lock-line"></i> Nova Senha (deixe em branco para manter a atual)
          </label>
          <input
            type="password"
            id="password"
            name="password"
            value={formData.password}
            onChange={handleInputChange}
            disabled={loading}
            placeholder="Deixe em branco para manter a senha atual"
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
                <i className="ri-loader-4-line spin"></i> Salvando...
              </>
            ) : (
              <>
                <i className="ri-save-line"></i> Salvar Alterações
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
              Funcionário editado com sucesso!
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

export default EditEmployeePage;
