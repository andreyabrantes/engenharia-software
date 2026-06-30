# Documento de Arquitetura - BoraAli

## 1. Aspecto 1: Stack Tecnológica (Ferramentas e Tecnologias)

### 1.1 Frontend

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| **Next.js** | 16.2.6 | Framework React com suporte a Server Components, App Router e renderização híbrida (SSR/CSR) |
| **React** | 19 | Biblioteca para construção de interfaces de usuário baseada em componentes |
| **TypeScript** | 5.7.3 | Superset JavaScript com tipagem estática para maior segurança e produtividade |
| **Tailwind CSS** | 4.2 | Framework CSS utility-first para estilização rápida e responsiva |
| **shadcn/ui** | - | Coleção de componentes React reutilizáveis construídos sobre Radix UI |
| **Radix UI** | - | Primitivas de UI acessíveis e sem estilização (accordion, dialog, dropdown, etc.) |
| **Lucide React** | 0.564 | Biblioteca de ícones em SVG |
| **React Hook Form** | 7.54 | Gerenciamento de formulários performático |
| **Zod** | 3.24 | Validação de esquemas TypeScript-first |
| **Sonner** | 1.7 | Sistema de notificações toast |
| **date-fns** | 4.1 | Manipulação de datas |
| **Recharts** | 2.15 | Gráficos e visualizações (planejado para dashboard) |
| **next-themes** | 0.4 | Suporte a temas claro/escuro |
| **Vercel Analytics** | 1.6 | Analytics de performance e audiência |

### 1.2 Backend

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| **.NET 8** | 8.0 | Framework de desenvolvimento cross-platform para API REST |
| **C#** | 12 | Linguagem de programação principal |
| **ASP.NET Core** | 8.0 | Framework web para construção de APIs e middleware |
| **Entity Framework (via Dapper)** | - | Acesso a dados (substituído por Dapper para performance) |

#### Pacotes NuGet

| Pacote | Versão | Finalidade |
|--------|--------|------------|
| **Dapper** | 2.1.28 | Micro-ORM para execução de queries SQL com mapeamento automático |
| **Microsoft.Data.Sqlite** | 8.0 | Provedor SQLite para .NET |
| **BCrypt.Net-Next** | 4.0.3 | Hash seguro de senhas |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 8.0 | Autenticação via tokens JWT |
| **FluentValidation.AspNetCore** | 11.3 | Validação declarativa de requisições |
| **AutoMapper** | 12.0 | Mapeamento objeto-objeto (DTOs ↔ Entidades) |
| **Serilog** | 8.0 | Logging estruturado (console + arquivo) |
| **Swashbuckle (Swagger)** | 6.5 | Documentação interativa da API |
| **MailKit** | 4.16 | Envio de e-mails (SMTP) |
| **QRCoder** | 1.8 | Geração de QR Codes |

### 1.3 Banco de Dados

| Tecnologia | Finalidade |
|------------|------------|
| **SQLite** | Banco de dados relacional embarcado, sem necessidade de servidor externo |
| **SQL** | Linguagem para definição e manipulação dos dados |

### 1.4 Ferramentas de Desenvolvimento

| Ferramenta | Finalidade |
|------------|------------|
| **VS Code** | IDE principal para desenvolvimento frontend e backend |
| **Visual Studio** | IDE alternativa para desenvolvimento .NET |
| **pnpm** | Gerenciador de pacotes JavaScript (rápido e eficiente) |
| **npm** | Gerenciador de pacotes JavaScript alternativo |
| **Git** | Controle de versão |
| **dotnet CLI** | Interface de linha de comando do .NET |

### 1.5 Infraestrutura

| Componente | Tecnologia |
|------------|------------|
| **Hospedagem Frontend** | Vercel (Next.js otimizado) |
| **Hospedagem Backend** | Qualquer servidor Windows/Linux com .NET 8 Runtime |
| **Banco de Dados** | SQLite (arquivo local) |
| **CI/CD** | Planejado (GitHub Actions) |

---

## 2. Aspecto 2: Arquitetura do Produto

### 2.1 Visão Geral da Arquitetura

O BoraAli adota uma **arquitetura de camadas (layered architecture)** com separação clara entre frontend e backend, seguindo os princípios da **Clean Architecture** no backend.

```
┌─────────────────────────────────────────────────────────┐
│                    FRONTEND (Next.js)                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │
│  │  Páginas  │  │Componentes│  │   Hooks   │  │  Utilit. │ │
│  │ (App Dir) │  │ (shadcn)  │  │ (useAuth) │  │ (api-typ)│ │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘ │
│       └──────────────┴─────────────┴──────────────┘       │
│                        │ HTTP/JSON                        │
│                   (fetch API)                             │
└────────────────────────┬──────────────────────────────────┘
                         │
┌────────────────────────▼──────────────────────────────────┐
│                    BACKEND (.NET 8)                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              API Layer (Controllers)                  │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │ Events   │  │   Auth   │  │  Orders  │           │  │
│  │  │Controller│  │Controller│  │Controller│           │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘           │  │
│  └───────┼──────────────┼─────────────┼─────────────────┘  │
│          │              │             │                     │
│  ┌───────▼──────────────▼─────────────▼─────────────────┐  │
│  │              Service Layer                            │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │EventSvc  │  │ AuthSvc  │  │OrderSvc  │           │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘           │  │
│  └───────┼──────────────┼─────────────┼─────────────────┘  │
│          │              │             │                     │
│  ┌───────▼──────────────▼─────────────▼─────────────────┐  │
│  │           Repository Layer (Dapper)                   │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │  │
│  │  │EventRepo │  │GenericRepo│  │UnitOfWork│           │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘           │  │
│  └───────┼──────────────┼─────────────┼─────────────────┘  │
│          │              │             │                     │
│  ┌───────▼──────────────▼─────────────▼─────────────────┐  │
│  │              Database (SQLite)                        │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Estrutura de Camadas do Backend

O backend segue uma arquitetura em **3 projetos** dentro de uma solution .NET:

#### 2.2.1 BoraAli.Core (Camada de Domínio)
- **Entidades**: `Event`, `User`, `Order`, `OrderItem`, `TicketType`, `Seat`, `Category`
- **Interfaces**: `IGenericRepository<T>`, `IEventRepository`, `IUnitOfWork`, `IDapperExecutor`
- **Exceções**: `NotFoundException`, `BadRequestException`
- **Dependências**: Nenhuma (camada mais interna)

#### 2.2.2 BoraAli.Infrastructure (Camada de Infraestrutura)
- **Data**: `DbSession` (gerenciamento de conexão SQLite)
- **Repositories**: `GenericRepository<T>`, `EventRepository`, `UnitOfWork`, `DapperExecutor`
- **Middleware**: `ExceptionMiddleware` (tratamento global de erros)
- **Dependências**: `BoraAli.Core`, Dapper, SQLite, BCrypt, Serilog

#### 2.2.3 BoraAli.Api (Camada de Apresentação)
- **Controllers**: `EventsController`, `AuthController`, `OrdersController`
- **Extensions**: `AutoMapperProfile`, `Validators` (FluentValidation)
- **Services**: `EventService`, `AuthService`, `OrderService`
- **Configuração**: `Program.cs` (DI, JWT, CORS, Rate Limiting, Swagger)
- **Dependências**: `BoraAli.Core`, `BoraAli.Infrastructure`

### 2.3 Estrutura do Frontend

```
app/                          # Next.js App Router
├── layout.tsx                # Layout raiz (fontes, metadata)
├── page.tsx                  # Home page (eventos em destaque + grid)
├── login/page.tsx            # Login/Cadastro
├── evento/[id]/page.tsx      # Detalhes do evento + seletor de ingressos
├── checkout/[id]/page.tsx    # Checkout/pagamento
└── criar-evento/page.tsx     # Criação de eventos (organizador)

components/                   # Componentes React
├── header.tsx                # Header com busca, categorias, menu usuário
├── hero-section.tsx          # Carrossel de eventos em destaque
├── events-grid.tsx           # Grid de eventos com filtros
├── event-card.tsx            # Card individual de evento
├── ticket-selector.tsx       # Seletor de tipos/quantidade de ingressos
├── checkout-form.tsx         # Formulário de checkout/pagamento
├── footer.tsx                # Footer
├── theme-provider.tsx        # Provedor de tema (claro/escuro)
└── ui/                       # Componentes shadcn/ui (40+ componentes)

hooks/                        # Custom Hooks
├── use-auth.ts               # Hook de autenticação (login, logout, token)
├── use-mobile.ts             # Hook de detecção mobile
└── use-toast.ts              # Hook de notificações

lib/                          # Utilitários e tipos
├── api-types.ts              # Tipos TypeScript para API
├── mock-data.ts              # Dados mockados para desenvolvimento
└── utils.ts                  # Funções utilitárias (cn, formatação)
```

### 2.4 Padrões Arquiteturais

#### 2.4.1 Repository Pattern
Abstração do acesso a dados através de interfaces genéricas:
```csharp
// IGenericRepository<T> - Operações CRUD genéricas
Task<T?> GetByIdAsync(int id);
Task<IEnumerable<T>> GetAllAsync();
Task<T> AddAsync(T entity);
Task UpdateAsync(T entity);
Task DeleteAsync(T entity);
Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(...);
```

#### 2.4.2 Unit of Work Pattern
Gerencia transações e coordena múltiplos repositórios:
```csharp
// IUnitOfWork
IGenericRepository<User> Users { get; }
IEventRepository Events { get; }
IGenericRepository<Order> Orders { get; }
Task<int> CompleteAsync();
Task BeginTransactionAsync();
Task CommitTransactionAsync();
Task RollbackTransactionAsync();
```

#### 2.4.3 Dapper para Consultas Otimizadas
Para consultas complexas que exigem JOINs e agregações, o Dapper é usado diretamente:
```csharp
// IDapperExecutor
Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
```

#### 2.4.4 Autenticação JWT
- Tokens JWT com claims de Role (Admin, Cliente, Organizador)
- Políticas de autorização: `ClienteOnly`, `OrganizadorOnly`, `ClienteOrOrganizador`
- Senhas hash com BCrypt (11 rounds de salt)

#### 2.4.5 Rate Limiting
Proteção contra abusos com políticas de janela fixa:
- **Global**: 100 requisições/minuto por IP
- **Login**: 5 tentativas/minuto por IP
- **Orders**: 10 pedidos/minuto por IP
- **Upload**: 20 uploads/minuto por IP

### 2.5 Modelo de Dados (Entidade-Relacionamento)

#### 2.5.1 Diagrama de Classes UML

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DIAGRAMA DE CLASSES - BoraAli                   │
│                                                                              │
│  ┌──────────────────────────────┐                                            │
│  │           User               │                                            │
│  ├──────────────────────────────┤                                            │
│  │ - Id: int                    │                                            │
│  │ - Name: string               │                                            │
│  │ - Email: string              │                                            │
│  │ - Cpf: string?               │                                            │
│  │ - PasswordHash: string       │                                            │
│  │ - Phone: string?             │                                            │
│  │ - AvatarUrl: string?         │                                            │
│  │ - Role: string               │  «enum»                                    │
│  │ - IsActive: bool             │  User, Admin, Organizer                    │
│  │ - CreatedAt: DateTime        │                                            │
│  │ - UpdatedAt: DateTime?       │                                            │
│  ├──────────────────────────────┤                                            │
│  │ + Events: ICollection<Event> │ 1───N                                      │
│  │ + Orders: ICollection<Order> │ 1───N                                      │
│  └──────────┬───────────────────┘                                            │
│             │ 1                                                              │
│             │ (OrganizerId)                                                  │
│             ▼                                                                │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                              Event                                    │   │
│  ├──────────────────────────────────────────────────────────────────────┤   │
│  │ - Id: int                                                             │   │
│  │ - Title: string                                                       │   │
│  │ - Description: string                                                 │   │
│  │ - FullDescription: string?                                            │   │
│  │ - EventDate: DateTime                                                 │   │
│  │ - Time: string                                                        │   │
│  │ - Location: string                                                    │   │
│  │ - Address: string                                                     │   │
│  │ - City: string                                                        │   │
│  │ - Cep, Street, Neighborhood, State, AddressNumber: string?            │   │
│  │ - ImageUrl: string?                                                   │   │
│  │ - IsFeatured: bool                                                    │   │
│  │ - Status: string                    «enum» Draft, Published, ...       │   │
│  │ - CategoryId: int                                                     │   │
│  │ - OrganizerId: int                                                    │   │
│  │ - CreatedAt, UpdatedAt, PublishedAt: DateTime?                        │   │
│  ├──────────────────────────────────────────────────────────────────────┤   │
│  │ + Category: Category?                          N───1                  │   │
│  │ + Organizer: User?                              N───1                  │   │
│  │ + TicketTypes: ICollection<TicketType>          1───N                  │   │
│  │ + Orders: ICollection<Order>                    1───N                  │   │
│  └──────┬────────────────────────────────────────────────────────────────┘   │
│         │ 1                                                                   │
│         │ (EventId)                                                           │
│         ├──────────────────────┐                                              │
│         ▼                      ▼                                              │
│  ┌─────────────────┐  ┌─────────────────┐                                    │
│  │   TicketType    │  │      Seat       │                                    │
│  ├─────────────────┤  ├─────────────────┤                                    │
│  │ - Id: int       │  │ - Id: int       │                                    │
│  │ - EventId: int  │  │ - EventId: int  │                                    │
│  │ - Name: string  │  │ - TicketTypeId  │                                    │
│  │ - Price: decimal│  │   : int?        │                                    │
│  │ - TotalQuantity │  │ - Row: string   │                                    │
│  │   : int         │  │ - Number: string│                                    │
│  │ - AvailableQty  │  │ - Section: str? │                                    │
│  │   : int         │  │ - Price: decimal│                                    │
│  │ - Description   │  │ - Status: string│  «enum» Available, Sold, ...       │
│  │   : string?     │  ├─────────────────┤                                    │
│  │ - SaleStartDate │  │ + Event: Event? │                                    │
│  │ - SaleEndDate   │  │ + TicketType    │                                    │
│  │   : DateTime?   │  │   : TicketType? │                                    │
│  │ - IsActive: bool│  └─────────────────┘                                    │
│  │ - CreatedAt     │                                                         │
│  ├─────────────────┤                                                         │
│  │ + Event: Event? │                                                         │
│  │ + OrderItems    │                                                         │
│  │   : ICollection │                                                         │
│  │   <OrderItem>   │                                                         │
│  └────────┬────────┘                                                         │
│           │ 1                                                                │
│           │ (TicketTypeId)                                                   │
│           ▼                                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │                           Order                                  │        │
│  ├─────────────────────────────────────────────────────────────────┤        │
│  │ - Id: int                                                        │        │
│  │ - UserId: int                                                    │        │
│  │ - EventId: int                                                   │        │
│  │ - OrderCode: string                                              │        │
│  │ - TotalAmount: decimal                                           │        │
│  │ - Status: string              «enum» Pending, Confirmed, ...      │        │
│  │ - PaymentMethod: string?                                          │        │
│  │ - PaymentId: string?                                              │        │
│  │ - CreatedAt, ConfirmedAt, UpdatedAt: DateTime?                    │        │
│  ├─────────────────────────────────────────────────────────────────┤        │
│  │ + User: User?                              N───1                  │        │
│  │ + Event: Event?                            N───1                  │        │
│  │ + OrderItems: ICollection<OrderItem>       1───N                  │        │
│  └──────────┬───────────────────────────────────────────────────────┘        │
│             │ 1                                                               │
│             │ (OrderId)                                                       │
│             ▼                                                                 │
│  ┌──────────────────────────────────────────────────────────────────┐        │
│  │                         OrderItem                                │        │
│  ├──────────────────────────────────────────────────────────────────┤        │
│  │ - Id: int                                                        │        │
│  │ - OrderId: int                                                   │        │
│  │ - TicketTypeId: int                                              │        │
│  │ - SeatId: int?                                                   │        │
│  │ - Quantity: int                                                  │        │
│  │ - UnitPrice: decimal                                             │        │
│  │ - Subtotal: decimal                                              │        │
│  ├──────────────────────────────────────────────────────────────────┤        │
│  │ + Order: Order?                              N───1                │        │
│  │ + TicketType: TicketType?                    N───1                │        │
│  │ + Seat: Seat?                                N───1                │        │
│  └──────────────────────────────────────────────────────────────────┘        │
│                                                                              │
│  ┌──────────────────────┐                                                    │
│  │      Category        │                                                    │
│  ├──────────────────────┤                                                    │
│  │ - Id: int            │                                                    │
│  │ - Name: string       │                                                    │
│  │ - Slug: string       │                                                    │
│  │ - Icon: string?      │                                                    │
│  │ - IsActive: bool     │                                                    │
│  │ - CreatedAt: DateTime│                                                    │
│  ├──────────────────────┤                                                    │
│  │ + Events: ICollection│ 1───N                                              │
│  │   <Event>            │                                                    │
│  └──────────────────────┘                                                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 2.5.2 Modelo Relacional (Entidade-Relacionamento)

```
┌───────────┐     ┌───────────┐     ┌───────────┐
│  Users    │1───N│  Events   │N───1│ Categories│
└───────────┘     └───────────┘     └───────────┘
     │                  │                  │
     │                  │                  │
     │                  │                  │
     │           ┌──────┴──────┐           │
     │           │             │           │
     │     ┌───────────┐ ┌───────────┐    │
     │     │TicketTypes│ │   Seats   │    │
     │     └─────┬─────┘ └─────┬─────┘    │
     │           │             │          │
     │           │             │          │
┌────┴────┐     │             │          │
│ Orders  │─────┴──────┬──────┘          │
└────┬────┘            │                 │
     │                 │                 │
     │           ┌─────┴──────┐          │
     │           │ OrderItems │          │
     │           └────────────┘          │
     │                                   │
     └───────────────────────────────────┘
```

#### 2.5.3 Relacionamentos

| Entidade Origem | Relacionamento | Entidade Destino | Tipo | Descrição |
|:---------------:|:--------------:|:----------------:|:----:|-----------|
| **User** | 1 → N | **Event** | Agregação | Um organizador pode criar vários eventos |
| **User** | 1 → N | **Order** | Agregação | Um cliente pode fazer vários pedidos |
| **Category** | 1 → N | **Event** | Classificação | Uma categoria pode conter vários eventos |
| **Event** | 1 → N | **TicketType** | Composição | Um evento pode ter vários tipos de ingresso |
| **Event** | 1 → N | **Seat** | Composição | Um evento pode ter vários assentos |
| **Event** | 1 → N | **Order** | Agregação | Um evento pode ter vários pedidos |
| **Order** | 1 → N | **OrderItem** | Composição | Um pedido pode ter vários itens |
| **TicketType** | 1 → N | **OrderItem** | Referência | Um tipo de ingresso pode estar em vários itens |
| **Seat** | 1 → 1 | **OrderItem** | Referência | Um assento pode estar em no máximo um item |

### 2.6 Fluxo de Dados - Compra de Ingresso

```
1. Usuário navega → Home Page (GET /api/events/featured, GET /api/events)
2. Usuário seleciona evento → Página do Evento (GET /api/events/{id})
3. Usuário escolhe ingressos → TicketSelector (client-side state)
4. Usuário vai para checkout → Checkout Page (GET /api/events/{id})
5. Usuário confirma compra → POST /api/orders (JWT autenticado)
6. Backend processa:
   a. Valida dados (FluentValidation)
   b. Inicia transação (UnitOfWork.BeginTransactionAsync)
   c. Cria pedido (Orders)
   d. Cria itens do pedido (OrderItems)
   e. Atualiza disponibilidade (TicketTypes.AvailableQuantity)
   f. Confirma transação (UnitOfWork.CommitTransactionAsync)
7. Retorna confirmação → Frontend exibe toast de sucesso
```

### 2.7 Segurança

| Aspecto | Implementação |
|---------|--------------|
| **Autenticação** | JWT Bearer Token com expiração configurável (8h padrão) |
| **Senhas** | BCrypt com salt de 11 rounds |
| **Autorização** | Role-based: Admin, Cliente, Organizador |
| **CORS** | Restrito a origens configuradas (localhost:3000, etc.) |
| **Rate Limiting** | 100 req/min global, 5 req/min login |
| **Validação** | FluentValidation em todas as requisições |
| **Logging** | Serilog com rotação diária de arquivos |
| **Exception Handling** | Middleware global com respostas padronizadas |

### 2.8 API Endpoints

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|:------------:|
| GET | `/api/events` | Listar eventos (paginado) | ❌ |
| GET | `/api/events/featured` | Eventos em destaque | ❌ |
| GET | `/api/events/categories` | Listar categorias | ❌ |
| GET | `/api/events/{id}` | Detalhes do evento | ❌ |
| POST | `/api/events` | Criar evento | ✅ Organizador |
| PUT | `/api/events/{id}` | Atualizar evento | ✅ Organizador |
| DELETE | `/api/events/{id}` | Remover evento | ✅ Organizador |
| POST | `/api/auth/register` | Registrar usuário | ❌ |
| POST | `/api/auth/login` | Login | ❌ |
| POST | `/api/orders` | Criar pedido | ✅ Cliente |
| GET | `/api/orders/my` | Meus pedidos | ✅ Cliente |
| POST | `/api/upload/image` | Upload de imagem | ✅ Organizador |
| GET | `/health` | Health check | ❌ |
