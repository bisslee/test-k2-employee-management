-- Script para criar usuário SQL e banco de dados
-- Este script será executado automaticamente quando o container for criado pela primeira vez

USE master;
GO

-- Criar banco de dados se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EmployeeManagement')
BEGIN
    CREATE DATABASE EmployeeManagement;
    PRINT 'Database EmployeeManagement created successfully.';
END
ELSE
BEGIN
    PRINT 'Database EmployeeManagement already exists.';
END
GO

-- Usar o banco de dados
USE EmployeeManagement;
GO

-- Criar login se não existir
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'employee_user')
BEGIN
    CREATE LOGIN employee_user WITH PASSWORD = 'Employee@Password123';
    PRINT 'Login employee_user created successfully.';
END
ELSE
BEGIN
    PRINT 'Login employee_user already exists.';
END
GO

-- Criar usuário no banco de dados
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'employee_user')
BEGIN
    CREATE USER employee_user FOR LOGIN employee_user;
    PRINT 'User employee_user created in database.';
END
ELSE
BEGIN
    PRINT 'User employee_user already exists in database.';
END
GO

-- Conceder permissões ao usuário
ALTER ROLE db_owner ADD MEMBER employee_user;
GO

PRINT 'Database setup completed successfully.';
GO
