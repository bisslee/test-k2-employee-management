# 🧑‍💼 Employee Management API

API REST para gerenciamento de funcionários desenvolvida com **.NET 8**, aplicando **Clean Architecture**, **CQRS**, **JWT Authentication**, **Entity Framework Core**.

## 📋 Índice

- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar Localmente](#como-executar-localmente)
- [Configuração](#configuração)
- [Endpoints](#endpoints)
- [Autenticação](#autenticação)
- [Testes](#testes)

## 🏗️ Arquitetura

O projeto segue **Clean Architecture**, organizado em camadas:

```
Biss.EmployeeManagement
│
├── Api              # Controllers, Authentication, Swagger, IoC
├── Application      # CQRS, Commands, Queries, Handlers
├── Domain           # Entities, Business Rules, Specifications
├── Infrastructure   # EF Core, Repositories, Seed
├── CrossCutting     # Dependency Injection, Health Checks
└── Tests            # Unit Tests (xUnit)
```

### Padrões Aplicados

- ✅ **Clean Architecture**
- ✅ **CQRS** com MediatR
- ✅ **Repository Pattern**
- ✅ **Specification Pattern**
- ✅ **JWT Authentication**
- ✅ **Domain-Driven Design** (DDD - light approach)

## 🛠️ Tecnologias

### Framework e Runtime
- **.NET 8.0**
- **ASP.NET Core Web API**

### Banco de Dados
- **SQL Server 2022**
- **Entity Framework Core 8.0.6**

### Autenticação e Segurança
- **JWT** (Microsoft.AspNetCore.Authentication.JwtBearer 8.0.6)
- **BCrypt.Net-Next** 4.0.3 (hash de senhas)

### Outras Bibliotecas
- **MediatR** 12.3.0 (CQRS)
- **AutoMapper** 13.0.1 (mapeamento de objetos)
- **FluentValidation** 11.9.0 (validação de entrada)
- **Swashbuckle.AspNetCore** 6.6.2 (Swagger/OpenAPI)
- **Biss.MultiSinkLogger** 1.0.1 (logging estruturado)
- **Serilog** 4.3.0 (logging)

## 📁 Estrutura do Projeto

```
Biss.EmployeeManagement.Api/
├── Controllers/          # Controllers da API
│   ├── AuthController.cs
│   └── EmployeeController.cs
├── Extensions/           # Extensões de configuração
│   ├── ConfigureServicesExtension.cs
│   ├── ConfigureMiddlewaresExtension.cs
│   ├── LoggingExtension.cs
│   └── MigrationExtension.cs
├── Helper/               # Helpers e utilitários
│   └── BaseControllerHandle.cs
├── Middleware/           # Middlewares customizados
├── Properties/
├── Resources/            # Recursos de localização
├── appsettings.json      # Configurações
├── appsettings.Development.json
├── Program.cs            # Ponto de entrada
└── Dockerfile            # Dockerfile para containerização
```

## 📋 Pré-requisitos

- **.NET 8 SDK** instalado
- **SQL Server** ou **SQL Server LocalDB**
- **Visual Studio 2022** ou **VS Code** (opcional)

## 🚀 Como Executar Localmente

### 1. Configurar Connection String

Edite o arquivo `appsettings.Development.json` e configure a connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=EmployeeManagement;User Id=employee_user;Password=Employee@123;TrustServerCertificate=True;"
  }
}
```

### 2. Aplicar Migrations

As migrations são aplicadas automaticamente na inicialização. Se precisar aplicar manualmente:

```bash
cd Biss.EmployeeManagement.Api
dotnet ef database update --project ..\Biss.EmployeeManagement.Infrastructure
```

### 3. Executar a API

```bash
cd Biss.EmployeeManagement.Api
dotnet run
```

A API estará disponível em:
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger

## ⚙️ Configuração

### appsettings.json

Principais configurações:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${connectionStringDefault}"
  },
  "Security": {
    "JwtSettings": {
      "SecretKey": "${jwtSecretKey}",
      "Issuer": "Biss.EmployeeManagement",
      "Audience": "Biss.EmployeeManagement.Users",
      "ExpirationMinutes": 60
    }
  },
  "LoggerManagerSettings": {
    "MinimumLevel": "Information",
    "Sinks": [...]
  }
}
```

### Variáveis de Ambiente

Você pode usar variáveis de ambiente para sobrescrever configurações:

- `ConnectionStrings__DefaultConnection`
- `Security__JwtSettings__SecretKey`
- `ASPNETCORE_ENVIRONMENT`

## 📡 Endpoints

### Autenticação

- `POST /api/v1/auth/login` - Realizar login e obter token JWT

### Funcionários (requer autenticação)

- `GET /api/v1/employees` - Listar funcionários (com paginação e filtros)
- `GET /api/v1/employees/{id}` - Obter funcionário por ID
- `POST /api/v1/employees` - Criar novo funcionário
- `PUT /api/v1/employees/{id}` - Atualizar funcionário
- `DELETE /api/v1/employees/{id}` - Remover funcionário (soft delete)

### Health Check

- `GET /health` - Verificar saúde da aplicação

## 🔐 Autenticação

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@employee.com",
  "password": "admin@123"
}
```

**Resposta:**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "response": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiresAt": "2024-01-14T10:00:00Z",
      "employee": {
        "id": "...",
        "firstName": "Admin",
        "lastName": "Master",
        "email": "admin@employee.com",
        "role": "Director"
      }
    }
  }
}
```

### Usar Token

Inclua o token no header `Authorization`:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Usuário Master (Seed)

Após a inicialização, um usuário master é criado automaticamente:

- **Email**: `admin@employee.com`
- **Senha**: `admin@123`
- **Role**: `Director`

## 🧪 Testes

### Executar Testes

```bash
cd Biss.EmployeeManagement
dotnet test
```

### Estrutura de Testes

```
Biss.EmployeeManagement.Tests/
├── Api/                    # Testes de Controllers
├── Application/            # Testes de Handlers
│   ├── Commands/
│   └── Queries/
└── Infrastructure/         # Testes de Repositories
```

## 📝 Logging

O projeto utiliza **Biss.MultiSinkLogger** para logging estruturado com suporte a:

- **Console** (desenvolvimento)
- **File** (arquivos de log)
- **SQL Server** (opcional)
- **Slack** (opcional)

Logs são salvos em `logs/` por padrão.

## 🔄 Migrations

### Criar Nova Migration

```bash
cd Biss.EmployeeManagement.Api
dotnet ef migrations add NomeDaMigration --project ..\Biss.EmployeeManagement.Infrastructure
```

### Aplicar Migrations

As migrations são aplicadas automaticamente na inicialização. Para aplicar manualmente:

```bash
dotnet ef database update --project ..\Biss.EmployeeManagement.Infrastructure
```

## 🐳 Docker

Para executar via Docker, consulte o README em `../../docker/README.md`.

## 📚 Documentação Adicional

- **Swagger**: Disponível em `/swagger` quando a API está rodando
- **Health Check**: Disponível em `/health`

## 🔧 Troubleshooting

### Erro de conexão com banco

1. Verifique se o SQL Server está rodando
2. Verifique a connection string em `appsettings.Development.json`
3. Verifique se as migrations foram aplicadas

### Token JWT inválido

1. Verifique se o `SecretKey` está configurado corretamente
2. Verifique se o token não expirou (padrão: 60 minutos)

### Migration não aplica

1. Verifique se o banco de dados existe
2. Verifique se o usuário tem permissões adequadas
3. Verifique os logs da aplicação

## 📦 Versões

- **.NET**: 8.0
- **Entity Framework Core**: 8.0.6
- **SQL Server**: 2022 (ou compatível)
