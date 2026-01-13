import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { ROUTES } from '../shared/constants';
import LoginPage from '../pages/LoginPage';
import CreateEmployeePage from '../pages/CreateEmployeePage';
import ListEmployeesPage from '../pages/ListEmployeesPage';

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path={ROUTES.LOGIN} element={<LoginPage />} />
        <Route
          path={ROUTES.EMPLOYEES}
          element={
            <ProtectedRoute>
              <ListEmployeesPage />
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
        <Route path="/" element={<Navigate to={ROUTES.EMPLOYEES} replace />} />
      </Routes>
    </BrowserRouter>
  );
};
