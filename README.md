# 🔐 FGC Users API

Microsserviço de Usuários da plataforma **FIAP Cloud Games (FCG)**.

## 📋 Descrição

Este microsserviço é responsável pelo gerenciamento de usuários, autenticação e autorização na plataforma FCG. Ele fornece tokens JWT que são utilizados pelos demais microsserviços (Games e Payments) para validação de acesso.

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, separando responsabilidades em camadas:

```
┌─────────────────────────────────────────────────────────────┐
│                    FGC.Users.Presentation                   │
│              (Controllers, Models, Program.cs)              │
├─────────────────────────────────────────────────────────────┤
│                    FGC.Users.Application                    │
│                    (DTOs, Use Cases)                        │
├─────────────────────────────────────────────────────────────┤
│                   FGC.Users.Infrastructure                  │
│           (Repositories, DbContext, Services)               │
├─────────────────────────────────────────────────────────────┤
│                      FGC.Users.Domain                       │
│          (Entities, Value Objects, Interfaces)              │
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
    "SecretKey": "FGC_SuperSecretKey_2024_MinLength32Chars!",
    "Issuer": "FGC.Users.API",
    "Audience": "FGC.Client",
    "ExpireMinutes": 120
  }
}
```

> ⚠️ **Importante**: A mesma `SecretKey` deve ser configurada nos 3 microsserviços para que os tokens sejam válidos em todos.

---

## 🐳 Docker - Imagem Otimizada (Fase 4)

O Dockerfile foi otimizado na **Fase 4** com as seguintes melhorias:

| Melhoria | Antes | Depois |
|----------|-------|--------|
| **Tamanho da imagem** | ~93 MB | ~64 MB |
| **Imagem base** | aspnet:8.0 | aspnet:8.0-alpine |
| **Usuário** | root | appuser (non-root) |
| **Health check** | Não tinha | Integrado |

### Dockerfile Otimizado

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
# ... build steps

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
RUN addgroup -g 1000 appgroup && adduser -u 1000 -G appgroup -D appuser
USER appuser
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
```

### Build & Run

```bash
# Build
docker build -t fgc-users-api .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="sua_connection_string" \
  -e Jwt__SecretKey="sua_secret_key" \
  fgc-users-api
```

---

## ☸️ Kubernetes (AKS) - Fase 4

Na Fase 4, o microsserviço foi migrado para **Azure Kubernetes Service (AKS)**.

### Recursos Kubernetes

| Recurso | Descrição |
|---------|-----------|
| **Deployment** | Gerencia os pods da aplicação |
| **Service (ClusterIP)** | Exposição interna |
| **Service (LoadBalancer)** | Exposição externa com IP público |
| **HPA** | Auto scaling baseado em CPU (1-5 pods) |
| **ConfigMap** | Configurações não sensíveis |
| **Secret** | Dados sensíveis (connection strings, JWT) |

### HPA (Horizontal Pod Autoscaler)

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: users-api-hpa
  namespace: fgc
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: users-api
  minReplicas: 1
  maxReplicas: 5
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Comandos Úteis

```bash
# Ver pods
kubectl get pods -n fgc

# Ver logs
kubectl logs -n fgc deployment/users-api

# Ver HPA
kubectl get hpa -n fgc

# Escalar manualmente
kubectl scale deployment/users-api --replicas=3 -n fgc
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
| `ApplicationInsights__ConnectionString` | APM monitoring | ⬜ |

---

# 📐 Arquitetura FIAP Cloud Games (FCG) - Fase 4

## 🏛️ Visão Geral da Arquitetura

```
┌───────────────────────────────────────────────────────────────────────────────────────────────┐
│                                           CLIENTES                                            │
│                                                                                               │
│   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐                   │
│   │   Web App    │   │  Mobile App  │   │   Swagger    │   │   Postman    │                   │
│   └──────┬───────┘   └──────┬───────┘   └──────┬───────┘   └──────┬───────┘                   │
│          └──────────────────┴──────────────────┴──────────────────┴──────────────────--┐      │
└────────────────────────────────────────────────────────────────────────────────────────┼──────┘
                                                                                         │
                                                                                         ▼
┌───────────────────────────────────────────────────────────────────────────────────────────────┐
│                               AZURE CLOUD INFRASTRUCTURE                                      │
│                                                                                               │
│   ┌───────────────────────────────────────────────────────────────────────────────────────┐   │
│   │                         AZURE KUBERNETES SERVICE (AKS)                                │   │
│   │                             fgc-aks-cluster                                           │   │
│   │                                                                                       │   │
│   │   ┌──────────────────────┐   ┌──────────────────────┐   ┌────────────────────-──┐     │   │
│   │   │  🔐 FGC Users API    │   │  🎮 FGC Games API    │   │  💳 FGC Payments API │     │   │
│   │   │        (Pod)         │   │        (Pod)         │   │        (Pod)          │     │   │
│   │   │  HPA: 1–5 pods       │   │  HPA: 1–5 pods       │   │  HPA: 1–5 pods        │     │   │
│   │   │  CPU: 70%            │   │  CPU: 70%            │   │  CPU: 70%             │     │   │
│   │   │                      │   │                      │   │                       │     │   │
│   │   │  📍 LoadBalancer     │   │  📍 ClusterIP        │   │  📍 LoadBalancer     │     │   │
│   │   │  68.220.143.16       │   │  (interno)           │   │  128.85.227.213       │     │   │
│   │   └──────────┬───────────┘   └──────────┬───────────┘   └──────────┬─────────── ┘     │   │
│   └──────────────┼──────────────────────────┼──────────────────────────┼────────────---───┘   │
│                  │                          │                          │                      │
│                  └──────────────────────────┴──────────────────────────┘                      │
│                                             │                                                 │
│                          ┌────────────────--┴────────────────┐                                │
│                          ▼                                 ▼                                  │
│   ┌──────────────────────────────────────┐   ┌────────────────────────────────────────┐       │
│   │           AZURE SQL DATABASE          │   │            AZURE SERVICE BUS                  │
│   │  📍 fgc-sql-server.database.windows.net│ │  📍 fgc-servicebus                    │       │
│   │  📁 fgc-database                      │   │  📬 Queue: payment-notifications             │
│   └──────────────────────────────────────┘   └────────────────────────────────────────┘       │
│                                                                                               │
│   ┌─────────────────────────────────-─────┐   ┌───────────────────────────────────────┐       │
│   │        AZURE CONTAINER REGISTRY       │   │     APPLICATION INSIGHTS (APM)        │       │
│   │  🐳 fgcregistry.azurecr.io            │   │  📊 Métricas de performance          │       │
│   │  • fgc-users-api:latest               │   │  📈 Logs e traces distribuídos        │      │
│   │  • fgc-games-api:latest               │   │  🔍 Monitoramento em tempo real       │      │
│   │  • fgc-payments-api:latest            │   │                                       │       │
│   └─────────────────────────────────────-─┘   └───────────────────────────────────────┘       │
└───────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Comunicação Assíncrona (Service Bus)

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                         FLUXO DE MENSAGERIA ASSÍNCRONA                                  │
│                                                                                         │
│    ┌──────────┐      POST /api/payments/{id}/process      ┌──────────────────┐          │
│    │  Client  │ ─────────────────────────────────────────►│  Payments API    │          │
│    └──────────┘                                           └────────┬─────────┘          │
│                                                                    │                    │
│                                                                    │ Publica mensagem   │
│                                                                    ▼                    │
│                                                           ┌──────────────────┐          │
│                                                           │  Azure Service   │          │
│                                                           │      Bus         │          │
│                                                           │  Queue:          │          │
│                                                           │  payment-        │          │
│                                                           │  notifications   │          │
│                                                           └────────┬─────────┘          │
│                                                                    │                    │
│                                                                    │ Consome mensagem   │
│                                                                    ▼                    │
│                                                           ┌──────────────────┐          │
│                                                           │  Azure Function  │          │
│                                                           │   (Consumer)     │          │
│                                                           │  • Notificações  │          │
│                                                           │  • Webhooks      │          │
│                                                           └──────────────────┘          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### Exemplo de Mensagem

```json
{
  "PaymentId": "52453aa9-a78e-47ed-a831-9fa1326a6a00",
  "UserId": "541d9aca-0619-47d4-bc6d-cf64ba74f4fe",
  "GameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 99.90,
  "Status": "Completed",
  "ProcessedAt": "2026-01-08T22:43:37.5861524Z"
}
```

---

## 🔧 Pipeline CI/CD

```
┌──────────────────────────────────────────────────────────────────────────────────────-───┐
│                                    CI/CD PIPELINE                                        │
│                                                                                          │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│    │                              GITHUB REPOSITORIES                                 │  │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │  │
│    │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                  │  │
│    │   └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                  │  │
│    └────────────┼────────────────────┼────────────────────┼───────────────────────────┘  │
│                 └────────────────────┼────────────────────┘                              │
│                                      ▼                                                   │
│    ┌──────────────────────────────────────────────────────────────────────────────────-┐ │
│    │                            GITHUB ACTIONS                                         │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────-┐    │ │
│    │   │  CI (Pull Requests)                                                      │    │ │
│    │   │  📥 Checkout ──► 🔧 Setup .NET ──► 📦 Restore ──► 🏗️ Build ──► 🧪 Test │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────-┘    │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────-┐    │ │
│    │   │  CD (Push to master)                                                     │    │ │
│    │   │  📥 Checkout ──► 🏗️ Build ──► 🧪 Test ──► 🔐 Azure Login                │    │ │
│    │   │       │                                         │                        │    │ │
│    │   │       ▼                                         ▼                        │    │ │
│    │   │  🐳 Docker Build ──► 📤 Push ACR ──► 🚀 Deploy ──► 🏥 Health Check      │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────-┘    │ │
│    └──────────────────────────────────────────────────────────────────────────────-────┘ │
└────────────────────────────────────────────────────────────────────────────────────-─────┘
```

---

## 📊 Recursos Azure - Fase 4

| Recurso | Nome | Tipo | Região |
|---------|------|------|--------|
| Resource Group | `rg-fgc-api` | Resource Group | East US 2 |
| AKS Cluster | `fgc-aks-cluster` | Azure Kubernetes Service | East US 2 |
| SQL Server | `fgc-sql-server` | Azure SQL Server | East US 2 |
| Database | `fgc-database` | Azure SQL Database | East US 2 |
| Container Registry | `fgcregistry` | Azure Container Registry | East US 2 |
| Service Bus | `fgc-servicebus` | Azure Service Bus | East US 2 |
| App Insights | `fgc-appinsights` | Application Insights | East US 2 |

---

## 🌐 URLs de Produção (Kubernetes)

| Microsserviço | URL | Swagger |
|---------------|-----|---------|
| **Users API** | http://68.220.143.16 | ✅ |
| **Games API** | Interno (ClusterIP) | - |
| **Payments API** | http://128.85.227.213 | ✅ |

---

## 🧪 Exemplos de Uso

### Registrar Usuário

```bash
curl -X POST http://68.220.143.16/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@email.com",
    "password": "Senha@123456"
  }'
```

### Login

```bash
curl -X POST http://68.220.143.16/api/auth/login \
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

## 🔗 Comunicação entre Microsserviços

```
                    ┌─────────────────────┐
                    │    FGC Users API    │
                    │   (Autenticação)    │
                    │  68.220.143.16      │
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
│  ClusterIP      │ │ 128.85.227.213  │ │                 │
└─────────────────┘ └────────┬────────┘ └─────────────────┘
                             │
                             ▼
                   ┌─────────────────┐
                   │  Service Bus    │
                   │  (Mensageria)   │
                   └─────────────────┘
```

### Este microsserviço:
- ✅ **Emite** tokens JWT para autenticação
- ✅ **Valida** credenciais de usuários
- ✅ **Gerencia** perfis e permissões

### Dependências:
- 🔵 **Azure SQL Database** - Armazenamento de dados
- 🔵 **Azure Kubernetes Service** - Orquestração de containers
- 🔵 **Application Insights** - Monitoramento APM

---

## 📋 Checklist da Fase 4

| Requisito | Status |
|-----------|--------|
| ✅ Dockerfiles otimizados (Alpine, non-root) | Implementado |
| ✅ Cluster Kubernetes (AKS) | Implementado |
| ✅ Deployments e Services | Implementado |
| ✅ HPA (Auto Scaling 1-5 pods, CPU 70%) | Implementado |
| ✅ Comunicação Assíncrona (Azure Service Bus) | Implementado |
| ✅ APM (Application Insights) | Implementado |

---

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET

**Tech Challenge - Fase 4**