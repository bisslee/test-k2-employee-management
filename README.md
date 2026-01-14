# 🧑‍💼 Employee Management System

Sistema completo de gerenciamento de funcionários desenvolvido com **.NET 8** (Backend) e **React + TypeScript** (Frontend), aplicando **Clean Architecture**, **CQRS**, **JWT Authentication**, **Entity Framework Core**, e **Docker**.

---

## 📋 Visão Geral

Este projeto é uma aplicação full-stack para gerenciamento de funcionários de uma empresa fictícia, implementando boas práticas de desenvolvimento, arquitetura limpa e padrões de design modernos.

### 🎯 Funcionalidades Principais

- ✅ Autenticação JWT
- ✅ CRUD completo de funcionários
- ✅ Gerenciamento de telefones (múltiplos por funcionário)
- ✅ Sistema de roles hierárquico (Director, Manager, Analyst, Assistant)
- ✅ Paginação e filtros
- ✅ Interface web responsiva
- ✅ Testes unitários abrangentes

---

## 🏗️ Arquitetura

O projeto segue **Clean Architecture**, organizado em camadas:

```
EmployeeManagement
│
├── Backend (.NET 8)
│   ├── Api              # Controllers, Authentication, Swagger, IoC
│   ├── Application      # CQRS, Commands, Queries, Handlers
│   ├── Domain           # Entities, Business Rules, Specifications
│   ├── Infrastructure   # EF Core, Repositories, Migrations
│   └── Tests            # Unit tests (54 testes passando)
│
└── Frontend (React + TypeScript)
    ├── auth              # Context e hooks de autenticação
    ├── pages             # Páginas (Login, List, Create)
    ├── routes            # Roteamento e rotas protegidas
    ├── shared            # Instância axios, tipos, constantes
    └── styles            # Estilos globais
```

### Padrões Aplicados

- ✅ Clean Architecture
- ✅ CQRS com MediatR
- ✅ Repository Pattern
- ✅ Specifications Pattern
- ✅ JWT Authentication
- ✅ Domain-Driven Design (DDD - light approach)
- ✅ Dependency Injection
- ✅ FluentValidation

---

## 🚀 Início Rápido

### Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose instalado
- (Opcional) .NET 8 SDK para desenvolvimento local
- (Opcional) Node.js 20+ para desenvolvimento local do frontend

### Executar com Docker (Recomendado)

1. **Clone o repositório**

```bash
git clone <repository-url>
cd repo
```

2. **Configure as variáveis de ambiente**

```bash
cd docker
cp env.template .env
# Edite o .env conforme necessário
```

3. **Suba todos os serviços**

```bash
docker-compose up -d --build
```

4. **Acesse a aplicação**

- **Frontend**: http://localhost:5173
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/health

### Credenciais do Usuário Master

Após a inicialização, um usuário master é criado automaticamente:

```
Email: admin@employee.com
Senha: admin@123
Role: Director
```

---

## 📦 Tecnologias Utilizadas

### Backend

- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - Framework web
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **MediatR** - CQRS pattern
- **FluentValidation** - Validação de entrada
- **AutoMapper** - Mapeamento de objetos
- **BCrypt.Net** - Hash de senhas
- **JWT Bearer** - Autenticação
- **Biss.MultiSinkLogger** - Logging estruturado
- **xUnit** - Framework de testes
- **Moq** - Mocking para testes
- **FluentAssertions** - Assertions fluentes
- **Bogus** - Geração de dados fake

### Frontend

- **React 18** - Biblioteca JavaScript
- **TypeScript** - Tipagem estática
- **Vite** - Build tool e dev server
- **React Router DOM** - Roteamento
- **Axios** - Cliente HTTP
- **React Hot Toast** - Notificações
- **Remix Icon** - Ícones
- **Radix UI** - Componentes acessíveis
- **Nginx** - Servidor web (produção)

### DevOps

- **Docker** - Containerização
- **Docker Compose** - Orquestração
- **SQL Server 2022** - Banco de dados containerizado

---

## 📁 Estrutura do Projeto

```
repo/
├── back/                          # Backend
│   └── Biss.EmployeeManagement/
│       └── Biss.EmployeeManagement/
│           ├── src/               # Código fonte
│           └── test/              # Testes
│
├── front/                         # Frontend
│   └── biss-employee-management/
│       ├── src/                   # Código fonte
│       ├── Dockerfile              # Build do frontend
│       └── nginx.conf             # Configuração nginx
│
├── docker/                        # Configuração Docker
│   ├── docker-compose.yml         # Orquestração de serviços
│   ├── env.template               # Template de variáveis
│   ├── init-db/                   # Scripts de inicialização
│   └── README.md                  # Documentação Docker
│
└── README.md                      # Este arquivo
```

---

## 🔐 Autenticação

A aplicação utiliza **JWT (JSON Web Tokens)** para autenticação:

1. O usuário faz login através do endpoint `/api/v1/auth/login`
2. O sistema retorna um token JWT válido
3. O token é armazenado no `localStorage` (frontend)
4. Todas as requisições subsequentes incluem o token no header `Authorization: Bearer <token>`
5. O token expira após 60 minutos (configurável)

### Endpoints de Autenticação

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@employee.com",
  "password": "admin@123"
}
```

---

## 📡 Endpoints da API

### Autenticação

- `POST /api/v1/auth/login` - Realizar login

### Funcionários

- `GET /api/v1/employee` - Listar funcionários (com paginação e filtros)
- `GET /api/v1/employee/{id}` - Obter funcionário por ID
- `POST /api/v1/employee` - Criar funcionário
- `PUT /api/v1/employee/{id}` - Atualizar funcionário
- `DELETE /api/v1/employee/{id}` - Remover funcionário (soft delete)

**Nota**: Todos os endpoints de funcionários requerem autenticação JWT (exceto login).

---

## 🧪 Testes

O projeto possui **54 testes unitários** implementados, todos passando:

- ✅ Testes de Handlers (AddEmployee, ChangeEmployee, RemoveEmployee, GetEmployees, GetEmployeeByKey, Login)
- ✅ Testes de Specifications (EmailMustBeUnique, DocumentMustBeUnique, MustExist)
- ✅ Testes de Controllers (EmployeeController)
- ✅ Testes de Repositories (ReadRepository, WriteRepository)

### Executar Testes

```bash
cd back/Biss.EmployeeManagement/Biss.EmployeeManagement
dotnet test
```

---

## 🐳 Docker

### Serviços

O `docker-compose.yml` define três serviços:

1. **sqlserver** - SQL Server 2022 (porta 1433)
2. **api** - API .NET 8 (porta 8080)
3. **frontend** - Frontend React (porta 5173)

### Variáveis de Ambiente

Consulte `docker/env.template` para todas as variáveis disponíveis.

### Comandos Úteis

```bash
# Subir todos os serviços
docker-compose up -d --build

# Ver logs
docker-compose logs -f

# Parar serviços
docker-compose down

# Parar e remover volumes (⚠️ apaga dados)
docker-compose down -v

# Rebuild específico
docker-compose up -d --build api
```

Para mais detalhes, consulte [docker/README.md](docker/README.md).

---

## 💻 Desenvolvimento Local

### Backend

1. **Configurar banco de dados**

   - Use o Docker para subir o SQL Server (veja seção Docker)
   - Ou configure uma instância local do SQL Server

2. **Configurar connection string**

   Edite `appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=EmployeeManagement;..."
     }
   }
   ```

3. **Executar migrations**

   ```bash
   cd back/Biss.EmployeeManagement/Biss.EmployeeManagement/src/Biss.EmployeeManagement.Api
   dotnet ef database update --project ../Biss.EmployeeManagement.Infrastructure
   ```

4. **Executar a API**

   ```bash
   dotnet run
   ```

   A API estará disponível em: http://localhost:8080

### Frontend

1. **Instalar dependências**

   ```bash
   cd front/biss-employee-management
   npm install
   ```

2. **Configurar variáveis de ambiente**

   Crie um arquivo `.env`:

   ```env
   VITE_API_BASE_URL=http://localhost:8080
   ```

3. **Executar em desenvolvimento**

   ```bash
   npm run dev
   ```

   O frontend estará disponível em: http://localhost:5173

Para mais detalhes, consulte:
- [Backend README](back/Biss.EmployeeManagement/Biss.EmployeeManagement/src/Biss.EmployeeManagement.Api/README.md)
- [Frontend README](front/biss-employee-management/README.md)

---

## 📊 Estrutura de Dados

### Employee (Funcionário)

- `Id` (Guid) - Identificador único
- `FirstName` (string) - Primeiro nome
- `LastName` (string) - Último nome
- `Email` (string) - Email (único)
- `Document` (string) - CPF/CNPJ (único)
- `BirthDate` (DateTime) - Data de nascimento
- `Role` (EmployeeRole) - Cargo (Director, Manager, Analyst, Assistant)
- `PasswordHash` (string) - Hash da senha (BCrypt)
- `PhoneNumbers` (List<PhoneNumber>) - Lista de telefones

### PhoneNumber (Telefone)

- `Id` (Guid) - Identificador único
- `Number` (string) - Número do telefone
- `Type` (PhoneNumberType) - Tipo (Mobile, Home, Work)
- `EmployeeId` (Guid) - Referência ao funcionário

---

## 🔒 Segurança

- ✅ Senhas hasheadas com BCrypt
- ✅ JWT tokens com expiração
- ✅ Validação de entrada com FluentValidation
- ✅ Regras de negócio no Domain (Specifications)
- ✅ Soft delete (não remove dados permanentemente)
- ✅ Headers de segurança (CORS, XSS Protection, etc.)

---

## 📝 Documentação Adicional

- [Docker README](docker/README.md) - Configuração e uso do Docker
- [API README](back/Biss.EmployeeManagement/Biss.EmployeeManagement/src/Biss.EmployeeManagement.Api/README.md) - Documentação da API
- [Frontend README](front/biss-employee-management/README.md) - Documentação do Frontend

---

## 🐛 Troubleshooting

### Problemas Comuns

**Erro de conexão com o banco de dados**
- Verifique se o SQL Server está rodando
- Verifique a connection string
- Verifique se a porta não está em uso

**Erro de autenticação**
- Verifique se o token JWT não expirou
- Faça logout e login novamente
- Verifique se o JWT_SECRET_KEY está configurado

**Frontend não conecta com a API**
- Verifique a variável `VITE_API_BASE_URL`
- Verifique se a API está rodando
- Verifique CORS na API

**Docker não inicia**
- Verifique se o Docker Desktop está rodando
- Verifique os logs: `docker-compose logs`
- Verifique se as portas não estão em uso

---

## 📦 Versões

- **.NET**: 8.0
- **SQL Server**: 2022-latest
- **React**: 18.x
- **TypeScript**: 5.9.x
- **Vite**: 7.x
- **Node.js**: 20+
- **Docker Compose**: 3.8

---

## 📄 Licença

Este projeto foi desenvolvido como teste técnico.

---

## 👥 Contribuindo

Este é um projeto de teste técnico. Para questões ou sugestões, abra uma issue no repositório.

---

**Desenvolvido seguindo os princípios de Clean Architecture e boas práticas de desenvolvimento**
