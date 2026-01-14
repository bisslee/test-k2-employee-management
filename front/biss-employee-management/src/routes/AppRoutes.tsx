import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { ROUTES } from '../shared/constants';
import LoginPage from '../pages/LoginPage';
import CreateEmployeePage from '../pages/CreateEmployeePage';
import EditEmployeePage from '../pages/EditEmployeePage';
import ListEmployeesPage from '../pages/ListEmployeesPage';

// Componente para recarregar lista ao navegar
const ListEmployeesPageWithReload = () => {
  const location = useLocation();
  
  useEffect(() => {
    // Forçar reload se vier da página de criação
    if (location.state?.fromCreate) {
      window.location.reload();
    }
  }, [location]);

  return <ListEmployeesPage />;
};

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path={ROUTES.LOGIN} element={<LoginPage />} />
        <Route
          path={ROUTES.EMPLOYEES}
          element={
            <ProtectedRoute>
              <ListEmployeesPageWithReload />
            </ProtectedRoute>
          }
        />
        <Route
          path={ROUTES.CREATE_EMPLOYEE}
          element={
            <ProtectedRoute>
              <CreateEmployeePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/employees/edit/:id"
          element={
            <ProtectedRoute>
              <EditEmployeePage />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to={ROUTES.EMPLOYEES} replace />} />
      </Routes>
    </BrowserRouter>
  );
};
