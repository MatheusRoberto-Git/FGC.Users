# FGC Users API

Microsserviço de Usuários da plataforma FIAP Cloud Games (FCG).

## 📋 Descrição

Este microsserviço é responsável pelo gerenciamento de usuários, autenticação e autorização na plataforma FCG.

## 🏗️ Arquitetura

```
FGC.Users.Domain/          → Entidades, Value Objects, Eventos, Interfaces
FGC.Users.Application/     → DTOs, Use Cases
FGC.Users.Infrastructure/  → Repositórios, DbContext, Serviços
FGC.Users.Presentation/    → Controllers, Models, Program.cs
```

## 🚀 Endpoints

### Users
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/users/register` | Registrar novo usuário |
| GET | `/api/users/profile/{id}` | Obter perfil do usuário |
| PUT | `/api/users/changePassword/{id}` | Alterar senha |

### Auth
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/login` | Realizar login |
| POST | `/api/auth/logout` | Realizar logout |
| POST | `/api/auth/validateToken` | Validar token JWT |

### Admin (requer role Admin)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/admin/create` | Criar novo admin |
| PUT | `/api/admin/promote` | Promover usuário a admin |
| PUT | `/api/admin/demote/{adminId}` | Despromover admin |
| PUT | `/api/admin/deactivate/{userId}` | Desativar usuário |
| PUT | `/api/admin/reactivate/{userId}` | Reativar usuário |
| GET | `/api/admin/adminLogged` | Info do admin logado |

## 🔧 Configuração Local

### Pré-requisitos
- .NET 8.0 SDK
- SQL Server (local ou Azure)

### Executar
```bash
cd FGC.Users.Presentation
dotnet restore
dotnet run
```

### Migrations
```bash
dotnet ef migrations add InitialCreate -p FGC.Users.Infrastructure -s FGC.Users.Presentation
dotnet ef database update -p FGC.Users.Infrastructure -s FGC.Users.Presentation
```

## 🐳 Docker

```bash
docker build -t fgc-users-api .
docker run -p 8080:8080 fgc-users-api
```

## 📦 Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server |
| `Jwt__SecretKey` | Chave secreta do JWT (min 32 chars) |
| `Jwt__Issuer` | Emissor do token |
| `Jwt__Audience` | Audiência do token |
| `Jwt__ExpireMinutes` | Tempo de expiração em minutos |

## 🔗 Integração

Este microsserviço se comunica com:
- **FGC Games API** → Validação de usuários
- **FGC Payments API** → Autorização de pagamentos

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET