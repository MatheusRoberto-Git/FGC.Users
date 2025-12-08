# 🔐 FGC Users API

Microsserviço de Usuários da plataforma **FIAP Cloud Games (FCG)**.

## 📋 Descrição

Este microsserviço é responsável pelo gerenciamento de usuários, autenticação e autorização na plataforma FCG. Ele fornece tokens JWT que são utilizados pelos demais microsserviços (Games e Payments) para validação de acesso.

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, separando responsabilidades em camadas:

```
┌─────────────────────────────────────────────────────────────┐
│                    FGC.Users.Presentation                    │
│              (Controllers, Models, Program.cs)               │
├─────────────────────────────────────────────────────────────┤
│                    FGC.Users.Application                     │
│                    (DTOs, Use Cases)                         │
├─────────────────────────────────────────────────────────────┤
│                   FGC.Users.Infrastructure                   │
│           (Repositories, DbContext, Services)                │
├─────────────────────────────────────────────────────────────┤
│                      FGC.Users.Domain                        │
│          (Entities, Value Objects, Interfaces)               │
└─────────────────────────────────────────────────────────────┘
```

### Estrutura de Pastas

```
FGC.Users/
├── FGC.Users.Domain/
│   ├── Common/
│   │   ├── Entities/
│   │   └── Events/
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│   ├── Interfaces/
│   └── ValueObjects/
├── FGC.Users.Application/
│   ├── DTOs/
│   └── UseCases/
├── FGC.Users.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Context/
│   ├── Repositories/
│   └── Services/
├── FGC.Users.Presentation/
│   ├── Controllers/
│   ├── Models/
│   │   ├── Requests/
│   │   └── Responses/
│   └── Properties/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
├── Dockerfile
└── README.md
```

---

## 🚀 Endpoints

### 👤 Users Controller (`/api/users`)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/users/register` | Registrar novo usuário | - |
| GET | `/api/users/profile/{id}` | Obter perfil do usuário | User |
| PUT | `/api/users/changePassword/{id}` | Alterar senha | User |

### 🔑 Auth Controller (`/api/auth`)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/auth/login` | Fazer login | - |
| POST | `/api/auth/logout` | Fazer logout | User |
| POST | `/api/auth/validateToken` | Validar token JWT | - |

### 👑 Admin Controller (`/api/admin`)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/admin/create` | Criar usuário admin | Admin |
| PUT | `/api/admin/promote/{userId}` | Promover a admin | Admin |
| PUT | `/api/admin/demote/{adminId}` | Rebaixar admin | Admin |
| PUT | `/api/admin/deactivate/{userId}` | Desativar usuário | Admin |
| PUT | `/api/admin/reactivate/{userId}` | Reativar usuário | Admin |
| GET | `/api/admin/adminLogged` | Info do admin logado | Admin |

---

## 📊 Modelos de Dados

### User Entity

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| Name | string | Nome do usuário |
| Email | Email (VO) | E-mail único |
| Password | Password (VO) | Senha criptografada |
| Role | UserRole | Papel (User/Admin) |
| IsActive | bool | Status ativo/inativo |
| CreatedAt | DateTime | Data de criação |
| LastLoginAt | DateTime? | Último login |

### Enums

```csharp
public enum UserRole
{
    User = 0,
    Admin = 1
}
```

---

## 🔒 Autenticação JWT

Este microsserviço é o **emissor de tokens JWT** para toda a plataforma FCG.

### Fluxo de Autenticação

```
┌──────────┐      POST /api/auth/login       ┌──────────────┐
│  Client  │ ──────────────────────────────► │  Users API   │
│          │ ◄────────────────────────────── │              │
└──────────┘      { token: "eyJ..." }        └──────────────┘
     │
     │  Authorization: Bearer eyJ...
     ▼
┌──────────────┐     ┌──────────────┐
│  Games API   │     │ Payments API │
└──────────────┘     └──────────────┘
```

### Configuração JWT

```json
{
  "Jwt": {
    "SecretKey": "FGC_SuperSecretKey_2024_FIAP_TechChallenge_MinimumLengthRequired32Chars",
    "Issuer": "FGC.Users.API",
    "Audience": "FGC.Client",
    "ExpireMinutes": 120
  }
}
```

> ⚠️ **Importante**: A mesma `SecretKey` deve ser configurada nos 3 microsserviços para que os tokens sejam válidos em todos.

---

## 🔧 Configuração Local

### Pré-requisitos

- .NET 8.0 SDK
- SQL Server (local ou Azure)
- Visual Studio 2022 ou VS Code

### Executar

```bash
cd FGC.Users.Presentation
dotnet restore
dotnet run
```

A API estará disponível em: `http://localhost:5001`

### Migrations

```bash
# Criar migration
dotnet ef migrations add InitialCreate -p FGC.Users.Infrastructure -s FGC.Users.Presentation

# Aplicar migration
dotnet ef database update -p FGC.Users.Infrastructure -s FGC.Users.Presentation
```

---

## 🐳 Docker

### Build

```bash
docker build -t fgc-users-api .
```

### Run

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="sua_connection_string" \
  -e Jwt__SecretKey="sua_secret_key" \
  fgc-users-api
```

---

## 📦 Variáveis de Ambiente

| Variável | Descrição | Obrigatório |
|----------|-----------|-------------|
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server | ✅ |
| `Jwt__SecretKey` | Chave secreta do JWT (min 32 chars) | ✅ |
| `Jwt__Issuer` | Emissor do token | ✅ |
| `Jwt__Audience` | Audiência do token | ✅ |
| `Jwt__ExpireMinutes` | Tempo de expiração em minutos | ✅ |
| `ASPNETCORE_ENVIRONMENT` | Ambiente (Development/Production) | ✅ |

---

## 🔗 Comunicação entre Microsserviços

```
                    ┌─────────────────────┐
                    │    FGC Users API    │
                    │   (Autenticação)    │
                    │  :8080 / :5001      │
                    └─────────┬───────────┘
                              │
                    Gera Token JWT
                              │
           ┌──────────────────┼──────────────────┐
           │                  │                  │
           ▼                  ▼                  ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  FGC Games API  │ │FGC Payments API │ │  Outros Clients │
│   (Catálogo)    │ │  (Transações)   │ │   (Frontend)    │
│  :8080 / :5002  │ │  :8080 / :5003  │ │                 │
└─────────────────┘ └─────────────────┘ └─────────────────┘
```

### Este microsserviço:
- ✅ **Emite** tokens JWT para autenticação
- ✅ **Valida** credenciais de usuários
- ✅ **Gerencia** perfis e permissões

### Dependências:
- 🔵 **Azure SQL Database** - Armazenamento de dados
- 🔵 **Azure Container Instance** - Hospedagem

---

## 🚀 CI/CD

### Pipeline CI (Pull Requests)

```yaml
- Checkout do código
- Setup .NET 8.0
- Restore de dependências
- Build da solução
- Execução de testes
```

### Pipeline CD (Push para master)

```yaml
- Checkout do código
- Build e testes
- Login no Azure
- Build da imagem Docker
- Push para Azure Container Registry
- Deploy no Azure Container Instance
- Health check
```

---

## 📍 URLs de Produção

| Ambiente | URL |
|----------|-----|
| **Swagger** | http://fgc-users-api.eastus2.azurecontainer.io:8080 |
| **Health Check** | http://fgc-users-api.eastus2.azurecontainer.io:8080/health |
| **Info** | http://fgc-users-api.eastus2.azurecontainer.io:8080/info |

---

## 🧪 Exemplos de Uso

### Registrar Usuário

```bash
curl -X POST http://localhost:5001/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@email.com",
    "password": "Senha@123456"
  }'
```

### Login

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@email.com",
    "password": "Senha@123456"
  }'
```

### Resposta do Login

```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "aade087c-6e81-4b4d-92cf-77663eac0600",
    "name": "João Silva",
    "email": "joao@email.com",
    "role": "User"
  }
}
```

---

# 📐 Arquitetura FIAP Cloud Games (FCG) - Fase 3

## 🏛️ Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                      CLIENTES                                            │
│                                                                                          │
│    ┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐      │
│    │   Web App    │     │  Mobile App  │     │   Swagger    │     │   Postman    │      │
│    └──────┬───────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘      │
│           │                    │                    │                    │               │
└───────────┼────────────────────┼────────────────────┼────────────────────┼───────────────┘
            │                    │                    │                    │
            └────────────────────┴────────────────────┴────────────────────┘
                                          │
                                          ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD INFRASTRUCTURE                                  │
│                                                                                          │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           AZURE CONTAINER INSTANCES                                │  │
│  │                                                                                    │  │
│  │   ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐       │  │
│  │   │  🔐 FGC Users API   │  │  🎮 FGC Games API   │  │  💳 FGC Payments API│       │  │
│  │   │                     │  │                     │  │                     │       │  │
│  │   │  ┌───────────────┐  │  │  ┌───────────────┐  │  │  ┌───────────────┐  │       │  │
│  │   │  │ Presentation  │  │  │  │ Presentation  │  │  │  │ Presentation  │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │ Application   │  │  │  │ Application   │  │  │  │ Application   │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │Infrastructure │  │  │  │Infrastructure │  │  │  │Infrastructure │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │    Domain     │  │  │  │    Domain     │  │  │  │    Domain     │  │       │  │
│  │   │  └───────────────┘  │  │  └───────────────┘  │  │  └───────────────┘  │       │  │
│  │   │                     │  │                     │  │                     │       │  │
│  │   │  📍 :8080           │  │  📍 :8080           │  │  📍 :8080           │       │  │
│  │   │  fgc-users-api      │  │  fgc-games-api      │  │  fgc-payments-api   │       │  │
│  │   └──────────┬──────────┘  └──────────┬──────────┘  └──────────┬──────────┘       │  │
│  │              │                        │                        │                  │  │
│  └──────────────┼────────────────────────┼────────────────────────┼──────────────────┘  │
│                 │                        │                        │                     │
│                 └────────────────────────┼────────────────────────┘                     │
│                                          │                                              │
│                                          ▼                                              │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                              AZURE SQL DATABASE                                    │  │
│  │                                                                                    │  │
│  │   ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐               │  │
│  │   │  📋 Users       │    │  📋 Games       │    │  📋 Payments    │               │  │
│  │   │                 │    │                 │    │                 │               │  │
│  │   │  - Id           │    │  - Id           │    │  - Id           │               │  │
│  │   │  - Name         │    │  - Title        │    │  - UserId       │               │  │
│  │   │  - Email        │    │  - Description  │    │  - GameId       │               │  │
│  │   │  - Password     │    │  - Price        │    │  - Amount       │               │  │
│  │   │  - Role         │    │  - Category     │    │  - Status       │               │  │
│  │   │  - IsActive     │    │  - Developer    │    │  - Method       │               │  │
│  │   │  - CreatedAt    │    │  - Publisher    │    │  - TransactionId│               │  │
│  │   │  - LastLoginAt  │    │  - ReleaseDate  │    │  - CreatedAt    │               │  │
│  │   │                 │    │  - IsActive     │    │  - ProcessedAt  │               │  │
│  │   │                 │    │  - Rating       │    │  - CompletedAt  │               │  │
│  │   │                 │    │  - TotalSales   │    │  - FailureReason│               │  │
│  │   └─────────────────┘    └─────────────────┘    └─────────────────┘               │  │
│  │                                                                                    │  │
│  │   📍 fgc-sql-server.database.windows.net                                          │  │
│  │   📁 fgc-database                                                                 │  │
│  └───────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                          │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                          AZURE CONTAINER REGISTRY                                  │  │
│  │                                                                                    │  │
│  │   🐳 fgcregistry.azurecr.io                                                       │  │
│  │                                                                                    │  │
│  │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                   │  │
│  │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                   │  │
│  │   │    :latest      │  │    :latest      │  │    :latest      │                   │  │
│  │   └─────────────────┘  └─────────────────┘  └─────────────────┘                   │  │
│  └───────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Comunicação entre Microsserviços

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                              FLUXO DE AUTENTICAÇÃO JWT                                   │
│                                                                                          │
│    ┌──────────┐         POST /api/auth/login              ┌──────────────────┐          │
│    │          │ ─────────────────────────────────────────►│                  │          │
│    │  Client  │         { email, password }               │  FGC Users API   │          │
│    │          │◄───────────────────────────────────────── │                  │          │
│    └────┬─────┘         { token: "eyJ..." }               └──────────────────┘          │
│         │                                                                                │
│         │                                                                                │
│         │  Authorization: Bearer eyJ...                                                  │
│         │                                                                                │
│         ├─────────────────────────────────────────────────────────────────┐              │
│         │                                                                 │              │
│         ▼                                                                 ▼              │
│    ┌──────────────────┐                                    ┌──────────────────┐         │
│    │                  │         Valida JWT                 │                  │         │
│    │  FGC Games API   │         (mesma SecretKey)          │FGC Payments API  │         │
│    │                  │                                    │                  │         │
│    └──────────────────┘                                    └──────────────────┘         │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🛒 Fluxo de Compra de Jogo

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                               FLUXO DE COMPRA DE JOGO                                    │
│                                                                                          │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 1️⃣ POST /api/auth/login                                                        │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │  FGC Users API   │  ──►  Valida credenciais                                        │
│    │                  │  ──►  Retorna JWT Token                                         │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { token }                                                                  │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 2️⃣ GET /api/games (com Bearer Token)                                           │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │  FGC Games API   │  ──►  Valida JWT                                                │
│    │                  │  ──►  Retorna lista de jogos                                    │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { games[] }                                                                │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │  ──►  Usuário escolhe um jogo                                           │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 3️⃣ POST /api/payments (com Bearer Token)                                       │
│         │    { userId, gameId, amount, paymentMethod }                                   │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │FGC Payments API  │  ──►  Valida JWT                                                │
│    │                  │  ──►  Cria pagamento (status: Pending)                          │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { payment: { id, status: "Pending" } }                                     │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 4️⃣ POST /api/payments/{id}/process                                             │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │FGC Payments API  │  ──►  Processa pagamento                                        │
│    │                  │  ──►  Atualiza status (Completed/Failed)                        │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { payment: { status: "Completed" } }                                       │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │  ──►  ✅ Compra finalizada!                                              │
│    └──────────┘                                                                          │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Pipeline CI/CD

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                                    CI/CD PIPELINE                                        │
│                                                                                          │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                              GITHUB REPOSITORIES                                  │ │
│    │                                                                                   │ │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │ │
│    │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                  │ │
│    │   └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                  │ │
│    │            │                    │                    │                           │ │
│    └────────────┼────────────────────┼────────────────────┼───────────────────────────┘ │
│                 │                    │                    │                             │
│                 └────────────────────┼────────────────────┘                             │
│                                      │                                                  │
│                                      ▼                                                  │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                            GITHUB ACTIONS                                         │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────┐    │ │
│    │   │                        CI (Pull Requests)                                │    │ │
│    │   │                                                                          │    │ │
│    │   │   📥 Checkout  ──►  🔧 Setup .NET  ──►  📦 Restore  ──►  🏗️ Build  ──►  🧪 Test │ │
│    │   └─────────────────────────────────────────────────────────────────────────┘    │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────┐    │ │
│    │   │                        CD (Push to master)                               │    │ │
│    │   │                                                                          │    │ │
│    │   │   📥 Checkout  ──►  🏗️ Build  ──►  🧪 Test  ──►  🔐 Azure Login          │    │ │
│    │   │        │                                              │                  │    │ │
│    │   │        ▼                                              ▼                  │    │ │
│    │   │   🐳 Docker Build  ──►  📤 Push ACR  ──►  🚀 Deploy ACI  ──►  🏥 Health  │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────┘    │ │
│    └──────────────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                                  │
│                                      ▼                                                  │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                              AZURE RESOURCES                                      │ │
│    │                                                                                   │ │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │ │
│    │   │ Container       │  │ Container       │  │   SQL Server    │                  │ │
│    │   │ Registry (ACR)  │  │ Instances (ACI) │  │   Database      │                  │ │
│    │   └─────────────────┘  └─────────────────┘  └─────────────────┘                  │ │
│    └──────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Recursos Azure

| Recurso | Nome | Tipo | Região |
|---------|------|------|--------|
| Resource Group | `rg-fgc-api` | Resource Group | East US 2 |
| SQL Server | `fgc-sql-server` | Azure SQL Server | East US 2 |
| Database | `fgc-database` | Azure SQL Database | East US 2 |
| Container Registry | `fgcregistry` | Azure Container Registry | East US 2 |
| Container Instance | `fgc-users-container` | Azure Container Instance | East US 2 |
| Container Instance | `fgc-games-container` | Azure Container Instance | East US 2 |
| Container Instance | `fgc-payments-container` | Azure Container Instance | East US 2 |

---

## 🌐 URLs de Produção

| Microsserviço | URL | Swagger |
|---------------|-----|---------|
| **Users API** | http://fgc-users-api.eastus2.azurecontainer.io:8080 | ✅ |
| **Games API** | http://fgc-games-api.eastus2.azurecontainer.io:8080 | ✅ |
| **Payments API** | http://fgc-payments-api.eastus2.azurecontainer.io:8080 | ✅ |

---

## 🔐 Segurança

### JWT Token Flow

```
┌────────────────────────────────────────────────────────────────┐
│                        JWT TOKEN                                │
│                                                                 │
│  Header:     { "alg": "HS256", "typ": "JWT" }                  │
│                                                                 │
│  Payload:    {                                                  │
│                "sub": "user-id",                               │
│                "email": "user@email.com",                      │
│                "role": "Admin",                                │
│                "exp": 1702044800                               │
│              }                                                  │
│                                                                 │
│  Signature:  HMACSHA256(                                       │
│                base64UrlEncode(header) + "." +                 │
│                base64UrlEncode(payload),                       │
│                secret_key                                       │
│              )                                                  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

### Mesma Secret Key em todos os microsserviços:

```
FGC_SuperSecretKey_2024_FIAP_TechChallenge_MinimumLengthRequired32Chars
```

---

## 📁 Estrutura dos Repositórios

```
GitHub Organization/User
│
├── 📁 fgc-users-api/
│   ├── 📁 .github/workflows/
│   │   ├── ci.yml
│   │   └── cd.yml
│   ├── 📁 FGC.Users.Domain/
│   ├── 📁 FGC.Users.Application/
│   ├── 📁 FGC.Users.Infrastructure/
│   ├── 📁 FGC.Users.Presentation/
│   ├── 🐳 Dockerfile
│   ├── 📄 FGC.Users.sln
│   └── 📖 README.md
│
├── 📁 fgc-games-api/
│   ├── 📁 .github/workflows/
│   │   ├── ci.yml
│   │   └── cd.yml
│   ├── 📁 FGC.Games.Domain/
│   ├── 📁 FGC.Games.Application/
│   ├── 📁 FGC.Games.Infrastructure/
│   ├── 📁 FGC.Games.Presentation/
│   ├── 🐳 Dockerfile
│   ├── 📄 FGC.Games.sln
│   └── 📖 README.md
│
└── 📁 fgc-payments-api/
    ├── 📁 .github/workflows/
    │   ├── ci.yml
    │   └── cd.yml
    ├── 📁 FGC.Payments.Domain/
    ├── 📁 FGC.Payments.Application/
    ├── 📁 FGC.Payments.Infrastructure/
    ├── 📁 FGC.Payments.Presentation/
    ├── 🐳 Dockerfile
    ├── 📄 FGC.Payments.sln
    └── 📖 README.md
```

---

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET

**Tech Challenge - Fase 3**