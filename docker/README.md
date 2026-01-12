# 🐳 Docker Configuration

Configuração Docker para o projeto Employee Management, incluindo SQL Server.

## 📋 Pré-requisitos

- Docker Desktop instalado e rodando
- Docker Compose instalado

## 🚀 Como executar

### 1. Configurar variáveis de ambiente

Copie o arquivo `env.template` para `.env` e ajuste as variáveis conforme necessário:

```bash
cp env.template .env
```

**Nota**: O arquivo `.env` não está versionado no Git por questões de segurança.

**Variáveis disponíveis:**
- `DB_SA_PASSWORD`: Senha do usuário SA do SQL Server (padrão: `YourStrong@Password123`)
- `DB_PORT`: Porta do SQL Server (padrão: `1433`)
- `DB_NAME`: Nome do banco de dados (padrão: `EmployeeManagement`)
- `DB_USER`: Nome do usuário SQL do projeto (padrão: `employee_user`)
- `DB_USER_PASSWORD`: Senha do usuário SQL do projeto (padrão: `Employee@Password123`)

### 2. Subir o SQL Server

```bash
docker-compose up -d
```

### 3. Verificar se está rodando

```bash
docker-compose ps
```

### 4. Parar os containers

```bash
docker-compose down
```

### 5. Parar e remover volumes (⚠️ apaga os dados)

```bash
docker-compose down -v
```

## 📊 Conexão com o banco de dados

### Connection String

```
Server=localhost,1433;Database=EmployeeManagement;User Id=employee_user;Password=Employee@Password123;TrustServerCertificate=True;
```

### Dentro da rede Docker

```
Server=sqlserver,1433;Database=EmployeeManagement;User Id=employee_user;Password=Employee@Password123;TrustServerCertificate=True;
```

### Usando SQL Server Management Studio (SSMS)

- **Server**: `localhost,1433`
- **Authentication**: SQL Server Authentication
- **Login**: `employee_user`
- **Password**: `Employee@Password123`

### Usando Azure Data Studio

- **Server**: `localhost,1433`
- **Authentication type**: SQL Login
- **User name**: `employee_user`
- **Password**: `Employee@Password123`

## 🔧 Configurações

### Porta

A porta padrão é `1433`. Para alterar, modifique a variável `DB_PORT` no arquivo `.env`.

### Volumes

Os dados do SQL Server são persistidos no volume `sqlserver_data`. Para remover completamente os dados:

```bash
docker-compose down -v
```

### Rede

O SQL Server está na rede `biss-employee-network`, que será compartilhada com a API e Frontend quando configurados.

## 📝 Scripts de inicialização

O diretório `init-db/` contém scripts SQL para inicialização do banco de dados:

- `01-create-user.sql`: Cria o banco de dados e usuário SQL do projeto

### Executar script de inicialização

Após o container estar rodando, execute o script SQL manualmente:

**Opção 1: Via docker exec**
```bash
docker exec -i biss-employee-management-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Password123" -C -i /scripts/01-create-user.sql
```

**Opção 2: Via arquivo SQL local**
```bash
docker exec -i biss-employee-management-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Password123" -C < init-db/01-create-user.sql
```

**Nota**: A flag `-C` é necessária para confiar no certificado SSL auto-assinado do SQL Server.

**Opção 3: Usando SQL Server Management Studio ou Azure Data Studio**
Conecte-se ao servidor e execute o conteúdo do arquivo `init-db/01-create-user.sql`

## 🏥 Health Check

O container possui um health check que verifica se o SQL Server está respondendo corretamente. Você pode verificar o status com:

```bash
docker-compose ps
```

## 🔍 Logs

Para visualizar os logs do SQL Server:

```bash
docker-compose logs -f sqlserver
```

## ⚠️ Notas Importantes

1. **Senha do SA**: A senha padrão é `YourStrong@Password123`. **Altere isso em produção!**
2. **TrustServerCertificate**: Necessário para conexões sem certificado SSL válido (desenvolvimento)
3. **Persistência**: Os dados são mantidos mesmo após parar o container, a menos que você use `docker-compose down -v`

## 🐛 Troubleshooting

### Container não inicia

Verifique os logs:
```bash
docker-compose logs sqlserver
```

### Erro de conexão

1. Verifique se o container está rodando: `docker-compose ps`
2. Verifique se a porta não está em uso: `netstat -an | findstr 1433`
3. Verifique as variáveis de ambiente no arquivo `.env`

### Resetar banco de dados

Para resetar completamente o banco de dados:

```bash
docker-compose down -v
docker-compose up -d
```

## 📦 Versões

- **SQL Server**: 2022-latest
- **Docker Compose**: 3.8
