# 🎨 Frontend - Employee Management

Frontend React com TypeScript, Vite, Axios e React Router DOM para o sistema de gerenciamento de funcionários.

## 📋 Tecnologias

- **React 18** - Biblioteca JavaScript para construção de interfaces
- **TypeScript** - Superset do JavaScript com tipagem estática
- **Vite** - Build tool e dev server rápido
- **React Router DOM** - Roteamento para aplicações React
- **Axios** - Cliente HTTP para requisições à API
- **React Hot Toast** - Biblioteca de notificações toast
- **Remix Icon** - Biblioteca de ícones
- **Radix UI** - Componentes acessíveis (Dialog, Select, Label)

## 🚀 Como executar localmente

### Pré-requisitos

- Node.js 20+ instalado
- npm ou yarn instalado

### 1. Instalar dependências

```bash
npm install
```

### 2. Configurar variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
VITE_API_BASE_URL=http://localhost:8080
```

**Nota**: O arquivo `.env` não está versionado no Git. Use `.env.example` como referência.

### 3. Executar em modo desenvolvimento

```bash
npm run dev
```

O frontend estará disponível em: http://localhost:5173

### 4. Build para produção

```bash
npm run build
```

Os arquivos de produção serão gerados na pasta `dist/`.

### 5. Preview da build de produção

```bash
npm run preview
```

## 📁 Estrutura de Pastas

```
src/
├── auth/              # Contexto e hooks de autenticação
│   └── AuthContext.tsx
├── pages/             # Páginas da aplicação
│   ├── LoginPage.tsx
│   ├── CreateEmployeePage.tsx
│   └── ListEmployeesPage.tsx
├── routes/            # Configuração de rotas
│   ├── AppRoutes.tsx
│   └── ProtectedRoute.tsx
├── shared/            # Código compartilhado
│   ├── api.ts         # Instância do Axios configurada
│   ├── types.ts       # Tipos TypeScript
│   └── constants.ts   # Constantes da aplicação
├── styles/            # Estilos globais
│   └── global.css
└── main.tsx           # Ponto de entrada da aplicação
```

## 🔐 Autenticação

A aplicação utiliza JWT (JSON Web Tokens) para autenticação:

1. **Login**: O usuário faz login através da página `/login`
2. **Armazenamento**: O token JWT é armazenado no `localStorage`
3. **Rotas Protegidas**: Rotas protegidas verificam a autenticação antes de permitir acesso
4. **Interceptors**: O Axios adiciona automaticamente o token JWT em todas as requisições

### Credenciais do Usuário Master

- **Email**: `admin@employee.com`
- **Senha**: `admin@123`

## 📄 Páginas

### Login (`/login`)

Página de autenticação onde o usuário faz login no sistema.

### Lista de Funcionários (`/employees`)

Página principal que lista todos os funcionários cadastrados, com:
- Filtros por nome, email e cargo
- Tabela responsiva
- Ações para remover funcionários
- Botão para criar novo funcionário

### Criar Funcionário (`/employees/create`)

Formulário para criação de novo funcionário com os seguintes campos:
- Primeiro Nome
- Último Nome
- Email
- Documento (CPF/CNPJ)
- Data de Nascimento
- Cargo (Diretor, Gerente, Analista, Assistente)
- Senha
- Dois telefones (com tipo: Celular, Residencial, Trabalho)

## 🔧 Configuração

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `VITE_API_BASE_URL` | URL base da API | `http://localhost:8080` |

### Porta

A porta padrão do servidor de desenvolvimento é `5173`. Para alterar, modifique o arquivo `vite.config.ts`:

```typescript
export default defineConfig({
  server: {
    port: 5173, // Altere aqui
  },
});
```

## 🐳 Docker

### Build da imagem

```bash
docker build -t biss-employee-management-frontend .
```

### Executar container

```bash
docker run -p 5173:5173 \
  -e VITE_API_BASE_URL=http://localhost:8080 \
  biss-employee-management-frontend
```

### Usando Docker Compose

O frontend está configurado no `docker-compose.yml` na pasta `docker/`. Para subir todos os serviços:

```bash
cd docker
docker-compose up -d --build
```

## 📦 Scripts Disponíveis

| Script | Descrição |
|--------|-----------|
| `npm run dev` | Inicia o servidor de desenvolvimento |
| `npm run build` | Cria a build de produção |
| `npm run preview` | Preview da build de produção |

## 🎨 Estilos

Os estilos estão centralizados em `src/styles/global.css` e utilizam CSS Variables para facilitar a customização:

```css
:root {
  --primary-color: #2563eb;
  --secondary-color: #6b7280;
  --success-color: #10b981;
  --danger-color: #ef4444;
  /* ... */
}
```

## 🔌 Integração com API

A aplicação consome os seguintes endpoints:

- `POST /api/v1/auth/login` - Autenticação
- `GET /api/v1/employee` - Listar funcionários
- `GET /api/v1/employee/{id}` - Obter funcionário por ID
- `POST /api/v1/employee` - Criar funcionário
- `PUT /api/v1/employee/{id}` - Atualizar funcionário
- `DELETE /api/v1/employee/{id}` - Remover funcionário

### Tratamento de Erros

O Axios está configurado com interceptors que:
- Adicionam automaticamente o token JWT nas requisições
- Tratam erros 401 (não autorizado) redirecionando para login
- Exibem mensagens de erro usando React Hot Toast

## 🐛 Troubleshooting

### Erro de conexão com a API

1. Verifique se a API está rodando em `http://localhost:8080`
2. Verifique a variável `VITE_API_BASE_URL` no arquivo `.env`
3. Verifique o console do navegador para mais detalhes

### Erro de autenticação

1. Verifique se o token JWT está sendo armazenado no `localStorage`
2. Verifique se o token não expirou
3. Faça logout e login novamente

### Build falha

1. Limpe o cache: `rm -rf node_modules dist`
2. Reinstale as dependências: `npm install`
3. Tente fazer o build novamente: `npm run build`

## 📝 Notas Importantes

1. **Variáveis de Ambiente**: Variáveis que começam com `VITE_` são expostas ao código do frontend
2. **CORS**: Certifique-se de que a API está configurada para aceitar requisições do frontend
3. **HTTPS**: Em produção, use HTTPS para proteger o token JWT
4. **Token Expiration**: O token JWT expira após 60 minutos (configurável na API)

## 📦 Versões

- **React**: 18.x
- **TypeScript**: 5.9.x
- **Vite**: 7.x
- **React Router DOM**: 7.x
- **Axios**: 1.x
- **React Hot Toast**: 2.x
- **Remix Icon**: 4.x

## 🔗 Links Úteis

- [Documentação do React](https://react.dev/)
- [Documentação do Vite](https://vite.dev/)
- [Documentação do React Router](https://reactrouter.com/)
- [Documentação do Axios](https://axios-http.com/)
- [Remix Icon](https://remixicon.com/)
