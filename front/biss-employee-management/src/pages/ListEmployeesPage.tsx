import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import api from '../shared/api';
import { API_ENDPOINTS, ROUTES } from '../shared/constants';
import { Employee, GetEmployeesRequest, GetEmployeesResponse, EmployeeRole } from '../shared/types';
import toast from 'react-hot-toast';
import * as Dialog from '@radix-ui/react-dialog';
import 'remixicon/fonts/remixicon.css';

const ListEmployeesPage: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<GetEmployeesRequest & { fullName?: string; firstName?: string; lastName?: string }>({
    page: 1,
    pageSize: 10,
  });
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [employeeToDelete, setEmployeeToDelete] = useState<{ id: string; name: string } | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [viewDialogOpen, setViewDialogOpen] = useState(false);
  const [employeeToView, setEmployeeToView] = useState<Employee | null>(null);
  const [loadingEmployeeDetails, setLoadingEmployeeDetails] = useState(false);

  useEffect(() => {
    // Recarregar sempre que a página for acessada (incluindo ao voltar do create)
    loadEmployees();
  }, [filters, location.pathname]);

  const loadEmployees = async () => {
    setLoading(true);
    try {
      // Preparar parâmetros para a API (remover fullName e enviar firstName/lastName)
      const apiParams: any = {
        page: filters.page,
        pageSize: filters.pageSize,
      };
      
      if (filters.firstName) apiParams.firstName = filters.firstName;
      if (filters.lastName) apiParams.lastName = filters.lastName;
      if (filters.email) apiParams.email = filters.email;
      if (filters.role !== undefined && filters.role !== null) apiParams.role = filters.role;
      
      console.log('[ListEmployeesPage] Carregando funcionários com filtros:', { filters, apiParams });
      
      const response = await api.get<GetEmployeesResponse>(API_ENDPOINTS.EMPLOYEES.BASE, {
        params: apiParams,
      });

      console.log('[ListEmployeesPage] Resposta recebida:', {
        success: response.data.success,
        statusCode: response.status,
        employeeCount: response.data.data?.response?.length || 0,
        hasError: !!response.data.error,
        errorMessage: response.data.error?.message,
        totalCount: response.headers['x-total-count'],
      });

      if (response.data.success) {
        const employeesList = response.data.data?.response || [];
        console.log('[ListEmployeesPage] Funcionários carregados:', employeesList.length);
        setEmployees(employeesList);
        // Não mostrar erro se a lista estiver vazia - isso é normal quando não há resultados
      } else {
        console.warn('[ListEmployeesPage] Resposta não foi sucesso:', response.data);
        // Só mostrar toast de erro se realmente houver um erro de servidor
        if (response.status !== 200 && response.status !== 206 && response.status !== 204) {
          toast.error(response.data.error?.message || 'Erro ao carregar funcionários');
        }
        setEmployees([]);
      }
    } catch (error: any) {
      console.error('[ListEmployeesPage] Erro ao carregar funcionários:', {
        error,
        response: error.response?.data,
        status: error.response?.status,
        statusText: error.response?.statusText,
        filters,
      });
      
      // Se for erro 404 ou 204 (No Content), apenas mostrar lista vazia
      if (error.response?.status === 404 || error.response?.status === 204) {
        setEmployees([]);
      } else {
        // Só mostrar toast para erros reais do servidor
        toast.error('Erro ao carregar funcionários. Tente novamente.');
        setEmployees([]);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (field: keyof GetEmployeesRequest, value: any) => {
    if (field === 'fullName') {
      const nameValue = (value as string).trim();
      
      // Se tiver espaço, dividir em FirstName e LastName
      // Se não tiver espaço, buscar em ambos os campos (pode ser parte do primeiro ou último nome)
      if (nameValue.includes(' ')) {
        const nameParts = nameValue.split(/\s+/);
        const firstName = nameParts[0] || '';
        const lastName = nameParts.slice(1).join(' ') || '';
        
        setFilters((prev) => ({ 
          ...prev, 
          firstName: firstName || undefined,
          lastName: lastName || undefined,
          fullName: value, // Manter para o input
          page: 1 
        }));
      } else {
        // Sem espaço: buscar o termo tanto no FirstName quanto no LastName
        // Vamos usar firstName para buscar e o backend precisa aceitar busca parcial
        setFilters((prev) => ({ 
          ...prev, 
          firstName: nameValue || undefined,
          lastName: nameValue || undefined, // Buscar o mesmo termo em ambos
          fullName: value, // Manter para o input
          page: 1 
        }));
      }
    } else {
      setFilters((prev) => ({ ...prev, [field]: value, page: 1 }));
    }
  };

  const handleDeleteClick = (employee: Employee) => {
    setEmployeeToDelete({
      id: employee.id,
      name: `${employee.firstName} ${employee.lastName}`,
    });
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!employeeToDelete) return;

    setDeleting(true);
    console.log('[ListEmployeesPage] Iniciando remoção do funcionário:', employeeToDelete.id);

    try {
      const response = await api.delete(API_ENDPOINTS.EMPLOYEES.BY_ID(employeeToDelete.id));
      
      console.log('[ListEmployeesPage] Resposta de remoção recebida:', {
        status: response.status,
        success: response.data?.success,
        hasError: !!response.data?.error,
        errorMessage: response.data?.error?.message,
        fullResponse: response.data,
      });

      // Considerar sucesso se:
      // 1. Status 200 ou 204 (No Content)
      // 2. Ou se response.data.success é true
      const isSuccess = response.status === 200 || response.status === 204 || response.data?.success === true;

      if (isSuccess) {
        toast.success(`Funcionário "${employeeToDelete.name}" removido com sucesso!`);
        setDeleteDialogOpen(false);
        setEmployeeToDelete(null);
        // Forçar recarregamento da lista
        await loadEmployees();
      } else {
        const errorMessage = response.data?.error?.message || 'Erro ao remover funcionário';
        toast.error(errorMessage);
      }
    } catch (error: any) {
      console.error('[ListEmployeesPage] Erro ao remover funcionário:', {
        error,
        employeeId: employeeToDelete.id,
        response: error.response?.data,
        status: error.response?.status,
        statusText: error.response?.statusText,
      });

      // Se o status for 200 ou 204, considerar sucesso mesmo com erro no catch
      if (error.response?.status === 200 || error.response?.status === 204) {
        toast.success(`Funcionário "${employeeToDelete.name}" removido com sucesso!`);
        setDeleteDialogOpen(false);
        setEmployeeToDelete(null);
        await loadEmployees();
      } else {
        const errorMessage = error.response?.data?.error?.message 
          || error.response?.data?.message 
          || 'Erro ao remover funcionário. Tente novamente.';
        toast.error(errorMessage);
      }
    } finally {
      setDeleting(false);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteDialogOpen(false);
    setEmployeeToDelete(null);
  };

  const handleViewClick = async (employee: Employee) => {
    setEmployeeToView(employee);
    setViewDialogOpen(true);
    setLoadingEmployeeDetails(true);

    try {
      // Carregar detalhes completos do funcionário
      const response = await api.get<{ success: boolean; data?: { response: Employee }; error?: any }>(
        API_ENDPOINTS.EMPLOYEES.BY_ID(employee.id)
      );

      if (response.data.success && response.data.data?.response) {
        setEmployeeToView(response.data.data.response);
      }
    } catch (error: any) {
      console.error('[ListEmployeesPage] Erro ao carregar detalhes do funcionário:', error);
      toast.error('Erro ao carregar detalhes do funcionário');
    } finally {
      setLoadingEmployeeDetails(false);
    }
  };

  const handleViewClose = () => {
    setViewDialogOpen(false);
    setEmployeeToView(null);
  };

  const getRoleLabel = (role: EmployeeRole | number | string): string => {
    // Se for string, tentar converter para número ou mapear diretamente
    if (typeof role === 'string') {
      // Se já for uma string legível, retornar
      const roleLower = role.toLowerCase();
      if (roleLower === 'director' || roleLower === 'diretor') return 'Diretor';
      if (roleLower === 'manager' || roleLower === 'gerente') return 'Gerente';
      if (roleLower === 'analyst' || roleLower === 'analista') return 'Analista';
      if (roleLower === 'assistant' || roleLower === 'assistente') return 'Assistente';
      
      // Tentar converter string numérica para número
      const roleNum = parseInt(role, 10);
      if (!isNaN(roleNum)) {
        const roleMap: Record<number, string> = {
          [EmployeeRole.Director]: 'Diretor',
          [EmployeeRole.Manager]: 'Gerente',
          [EmployeeRole.Analyst]: 'Analista',
          [EmployeeRole.Assistant]: 'Assistente',
        };
        return roleMap[roleNum] || 'Desconhecido';
      }
      return 'Desconhecido';
    }
    
    // Se for número, mapear diretamente
    const roleMap: Record<number, string> = {
      [EmployeeRole.Director]: 'Diretor',
      [EmployeeRole.Manager]: 'Gerente',
      [EmployeeRole.Analyst]: 'Analista',
      [EmployeeRole.Assistant]: 'Assistente',
    };
    return roleMap[role as number] || 'Desconhecido';
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>
          <i className="ri-team-line"></i> Funcionários
        </h1>
        <div className="header-actions">
          <button
            className="btn-primary"
            onClick={() => navigate(ROUTES.CREATE_EMPLOYEE)}
          >
            <i className="ri-user-add-line"></i> Novo Funcionário
          </button>
          <div className="user-menu">
            <span>
              <i className="ri-user-line"></i> {user?.firstName} {user?.lastName}
            </span>
            <button className="btn-secondary" onClick={logout}>
              <i className="ri-logout-box-line"></i> Sair
            </button>
          </div>
        </div>
      </div>

      <div className="filters-container">
        <div className="filter-group">
          <input
            type="text"
            placeholder="Buscar por nome..."
            value={filters.fullName || ''}
            onChange={(e) => handleFilterChange('fullName', e.target.value)}
            className="filter-input"
          />
          <input
            type="email"
            placeholder="Buscar por email..."
            value={filters.email || ''}
            onChange={(e) => handleFilterChange('email', e.target.value)}
            className="filter-input"
          />
          <select
            value={filters.role || ''}
            onChange={(e) => handleFilterChange('role', e.target.value ? Number(e.target.value) : undefined)}
            className="filter-select"
          >
            <option value="">Todos os cargos</option>
            <option value={EmployeeRole.Director}>Diretor</option>
            <option value={EmployeeRole.Manager}>Gerente</option>
            <option value={EmployeeRole.Analyst}>Analista</option>
            <option value={EmployeeRole.Assistant}>Assistente</option>
          </select>
        </div>
      </div>

      {loading ? (
        <div className="loading-container">
          <i className="ri-loader-4-line spin"></i>
          <p>Carregando funcionários...</p>
        </div>
      ) : employees.length === 0 ? (
        <div className="empty-state">
          <i className="ri-inbox-line"></i>
          <p>
            {(filters.firstName || filters.lastName || filters.email || (filters.role !== undefined && filters.role !== null))
              ? 'Nenhum funcionário encontrado com os filtros aplicados'
              : 'Nenhum funcionário encontrado'}
          </p>
          {(filters.firstName || filters.lastName || filters.email || (filters.role !== undefined && filters.role !== null)) && (
            <button
              className="btn-secondary"
              onClick={() => setFilters({ page: 1, pageSize: 10 })}
              style={{ marginTop: '1rem' }}
            >
              Limpar Filtros
            </button>
          )}
        </div>
      ) : (
        <div className="table-container">
          <table className="employees-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Email</th>
                <th>Documento</th>
                <th>Cargo</th>
                <th>Telefones</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {employees.map((employee) => (
                <tr key={employee.id}>
                  <td>
                    {employee.firstName} {employee.lastName}
                  </td>
                  <td>{employee.email}</td>
                  <td>{employee.document}</td>
                  <td>{getRoleLabel(employee.role)}</td>
                  <td>
                    {employee.phoneNumbers && employee.phoneNumbers.length > 0 ? (
                      <div className="phone-numbers">
                        {employee.phoneNumbers.map((phone, index) => (
                          <span key={index} className="phone-badge">
                            <i className="ri-phone-line"></i> {phone.number}
                          </span>
                        ))}
                      </div>
                    ) : (
                      <span className="no-data">-</span>
                    )}
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon btn-primary"
                        onClick={() => handleViewClick(employee)}
                        title="Visualizar"
                      >
                        <i className="ri-eye-line"></i>
                      </button>
                      <button
                        className="btn-icon btn-primary"
                        onClick={() => navigate(ROUTES.EDIT_EMPLOYEE(employee.id))}
                        title="Editar"
                      >
                        <i className="ri-edit-line"></i>
                      </button>
                      <button
                        className="btn-icon btn-danger"
                        onClick={() => handleDeleteClick(employee)}
                        title="Remover"
                      >
                        <i className="ri-delete-bin-line"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Dialog de confirmação de exclusão */}
      <Dialog.Root open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <Dialog.Portal>
          <Dialog.Overlay className="dialog-overlay" />
          <Dialog.Content className="dialog-content">
            <Dialog.Title className="dialog-title">
              <i className="ri-error-warning-line warning-icon"></i>
              Confirmar Exclusão
            </Dialog.Title>
            <Dialog.Description className="dialog-description">
              Tem certeza que deseja remover o funcionário <strong>{employeeToDelete?.name}</strong>?
              <br />
              <span style={{ fontSize: '0.9rem', color: '#666', marginTop: '0.5rem', display: 'block' }}>
                Esta ação não pode ser desfeita.
              </span>
            </Dialog.Description>
            <div className="dialog-actions">
              <button
                className="btn-secondary"
                onClick={handleDeleteCancel}
                disabled={deleting}
              >
                Cancelar
              </button>
              <button
                className="btn-danger"
                onClick={handleDeleteConfirm}
                disabled={deleting}
              >
                {deleting ? (
                  <>
                    <i className="ri-loader-4-line spin"></i> Removendo...
                  </>
                ) : (
                  <>
                    <i className="ri-delete-bin-line"></i> Remover
                  </>
                )}
              </button>
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>

      {/* Dialog de visualização de detalhes */}
      <Dialog.Root open={viewDialogOpen} onOpenChange={setViewDialogOpen}>
        <Dialog.Portal>
          <Dialog.Overlay className="dialog-overlay" />
          <Dialog.Content className="dialog-content" style={{ maxWidth: '600px' }}>
            <Dialog.Title className="dialog-title">
              <i className="ri-user-line"></i>
              Detalhes do Funcionário
            </Dialog.Title>
            <Dialog.Description className="dialog-description">
              {loadingEmployeeDetails ? (
                <div style={{ textAlign: 'center', padding: '2rem' }}>
                  <i className="ri-loader-4-line spin" style={{ fontSize: '2rem' }}></i>
                  <p>Carregando detalhes...</p>
                </div>
              ) : employeeToView ? (
                <div className="employee-details">
                  <div className="detail-row">
                    <strong>Nome Completo:</strong>
                    <span>{employeeToView.firstName} {employeeToView.lastName}</span>
                  </div>
                  <div className="detail-row">
                    <strong>Email:</strong>
                    <span>{employeeToView.email}</span>
                  </div>
                  <div className="detail-row">
                    <strong>Documento:</strong>
                    <span>{employeeToView.document}</span>
                  </div>
                  <div className="detail-row">
                    <strong>Data de Nascimento:</strong>
                    <span>
                      {employeeToView.birthDate 
                        ? new Date(employeeToView.birthDate).toLocaleDateString('pt-BR')
                        : '-'}
                    </span>
                  </div>
                  <div className="detail-row">
                    <strong>Cargo:</strong>
                    <span>{getRoleLabel(employeeToView.role)}</span>
                  </div>
                  {employeeToView.phoneNumbers && employeeToView.phoneNumbers.length > 0 ? (
                    <div className="detail-row">
                      <strong>Telefones:</strong>
                      <div className="phone-list">
                        {employeeToView.phoneNumbers.map((phone, index) => (
                          <div key={index} className="phone-item">
                            <i className="ri-phone-line"></i>
                            <span>{phone.number}</span>
                            {phone.type && <span className="phone-type">({phone.type})</span>}
                          </div>
                        ))}
                      </div>
                    </div>
                  ) : (
                    <div className="detail-row">
                      <strong>Telefones:</strong>
                      <span className="no-data">-</span>
                    </div>
                  )}
                  {employeeToView.createdAt && (
                    <div className="detail-row">
                      <strong>Data de Criação:</strong>
                      <span>
                        {new Date(employeeToView.createdAt).toLocaleDateString('pt-BR', {
                          day: '2-digit',
                          month: '2-digit',
                          year: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })}
                      </span>
                    </div>
                  )}
                </div>
              ) : (
                <p>Nenhum dado disponível</p>
              )}
            </Dialog.Description>
            <div className="dialog-actions">
              <button
                className="btn-primary"
                onClick={handleViewClose}
              >
                Fechar
              </button>
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>
    </div>
  );
};

export default ListEmployeesPage;
