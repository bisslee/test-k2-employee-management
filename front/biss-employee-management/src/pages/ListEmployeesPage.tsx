import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import api from '../shared/api';
import { API_ENDPOINTS, ROUTES } from '../shared/constants';
import { Employee, GetEmployeesRequest, GetEmployeesResponse, EmployeeRole } from '../shared/types';
import toast from 'react-hot-toast';
import 'remixicon/fonts/remixicon.css';

const ListEmployeesPage: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState<GetEmployeesRequest>({
    page: 1,
    pageSize: 10,
  });

  useEffect(() => {
    loadEmployees();
  }, [filters]);

  const loadEmployees = async () => {
    setLoading(true);
    try {
      const response = await api.get<GetEmployeesResponse>(API_ENDPOINTS.EMPLOYEES.BASE, {
        params: filters,
      });

      if (response.data.success && response.data.data?.response) {
        setEmployees(response.data.data.response);
      } else {
        toast.error(response.data.error?.message || 'Erro ao carregar funcionários');
      }
    } catch (error) {
      console.error('Load employees error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (field: keyof GetEmployeesRequest, value: any) => {
    setFilters((prev) => ({ ...prev, [field]: value, page: 1 }));
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Tem certeza que deseja remover este funcionário?')) {
      return;
    }

    try {
      await api.delete(API_ENDPOINTS.EMPLOYEES.BY_ID(id));
      toast.success('Funcionário removido com sucesso!');
      loadEmployees();
    } catch (error) {
      console.error('Delete employee error:', error);
    }
  };

  const getRoleLabel = (role: EmployeeRole): string => {
    const roleMap: Record<EmployeeRole, string> = {
      [EmployeeRole.Director]: 'Diretor',
      [EmployeeRole.Manager]: 'Gerente',
      [EmployeeRole.Analyst]: 'Analista',
      [EmployeeRole.Assistant]: 'Assistente',
    };
    return roleMap[role] || 'Desconhecido';
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
          <p>Nenhum funcionário encontrado</p>
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
                        className="btn-icon btn-danger"
                        onClick={() => handleDelete(employee.id)}
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
    </div>
  );
};

export default ListEmployeesPage;
