# Padrões de Projeto (Design Patterns) — BoraAli

> **Disciplina:** Programação Orientada a Objetos (POO)
> **Projeto:** BoraAli — Plataforma de Eventos e Ingressos
> **Stack:** Next.js 14 (frontend) + .NET 8 (backend) + SQLite (banco)

---

## 1. Repository Pattern (Generic Repository)

### Motivação

O **Repository Pattern** abstrai a camada de acesso a dados, isolando o código de negócio dos detalhes de infraestrutura (SQL, ORM, conexão). Isso facilita testes unitários, manutenção e troca de tecnologia de banco de dados.

### Implementação no BoraAli

O projeto implementa um **repositório genérico** via interface `IGenericRepository<T>` e a classe concreta `GenericRepository<T>`, que usa **Dapper** (micro-ORM) para executar queries SQL diretamente no SQLite.

#### Interface (`BoraAli.Core/Interfaces/IGenericRepository.cs`)

```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? orderBy = null, bool ascending = true);
}
```

#### Implementação (`BoraAli.Infrastructure/Repositories/GenericRepository.cs`)

A implementação gera SQL dinamicamente via **reflection** para construir INSERT, UPDATE, DELETE e SELECT. Exemplo do método `AddAsync`:

```csharp
public virtual async Task<T> AddAsync(T entity)
{
    var properties = _columnProperties.Value; // Cache de propriedades via Lazy<T>
    var columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
    var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
    var sql = $"INSERT INTO [{_tableName}] ({columns}) VALUES ({parameters}); SELECT last_insert_rowid();";
    // ...
    var id = await _session.Connection.ExecuteScalarAsync<int>(sql, dp);
    // Seta o ID gerado de volta na entidade
    var idProperty = typeof(T).GetProperty("Id");
    idProperty?.SetValue(entity, id);
    return entity;
}
```

**Destaques técnicos da implementação:**
- **`Lazy<List<PropertyInfo>>`** com cache estático: as propriedades de coluna são calculadas uma única vez por tipo `T`, filtrando navigation properties (ICollection, classes) para evitar erros de mapeamento.
- **Pluralização automática** de nomes de tabela: `typeof(T).Name + "s"`, com tratamento especial para `Category → Categories`.
- **SQL parametrizado** via `DynamicParameters` do Dapper, prevenindo SQL Injection.

#### Repositório Especializado (`IEventRepository` / `EventRepository`)

Para entidades que exigem queries complexas (como JOINs para carregar tickets e dados do organizador), o projeto estende o repositório genérico com uma interface especializada:

```csharp
public interface IEventRepository : IGenericRepository<Event>
{
    Task<Event?> GetEventWithDetailsAsync(int id);
    Task<IEnumerable<Event>> GetFeaturedEventsAsync();
    Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId);
    Task<IEnumerable<Event>> GetEventsByCityAsync(string city);
    Task<IEnumerable<Event>> GetEventsByOrganizerAsync(int organizerId);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
}
```

**Por que especializar?** O repositório genérico cobre 90% dos casos (CRUD simples). Para queries com múltiplos JOINs e filtros complexos, o `EventRepository` usa Dapper diretamente, mantendo a separação de responsabilidades sem poluir o genérico.

---

## 2. Unit of Work Pattern

### Motivação

O **Unit of Work** garante atomicidade em operações que envolvem múltiplos repositórios. Ele coordena transações de banco de dados e o ciclo de vida da conexão.

### Implementação no BoraAli

#### Interface (`BoraAli.Core/Interfaces/IUnitOfWork.cs`)

```csharp
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IEventRepository Events { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<TicketType> TicketTypes { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Seat> Seats { get; }
    IGenericRepository<Coupon> Coupons { get; }

    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }

    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

#### Implementação (`BoraAli.Infrastructure/Repositories/UnitOfWork.cs`)

- **Lazy initialization** de repositórios: cada propriedade usa `??=` para instanciar o repositório sob demanda.
- **Gerenciamento de transações**: `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()` delegam para `IDbTransaction` do ADO.NET.
- **Implementa `IDisposable`** para liberar a conexão e transação ao final do ciclo de vida (injetado como **Scoped**).

#### Exemplo Real: Criação de Pedido com Concorrência Segura

No `OrderService`, uma compra com assentos nomeados usa transação explícita:

```csharp
// Inicia transação
await _unitOfWork.BeginTransactionAsync();

try
{
    // 1. Reserva assentos com UPDATE atômico (só se estiver "Available")
    string reserveSql = "UPDATE Seats SET Status = 'Reserved' WHERE Id = @SeatId AND Status = 'Available'";
    var affectedRows = await _dapper.ExecuteAsync(reserveSql, new { SeatId = item.SeatId.Value });
    if (affectedRows == 0) throw new Exception("Assento já vendido");

    // 2. Cria o pedido
    var createdOrder = await _unitOfWork.Orders.AddAsync(order);

    // 3. Cria OrderItems e baixa estoque
    foreach (var item in orderItems) { /* ... */ }

    // 4. Confirma transação
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(); // Desfaz tudo
    throw;
}
```

**Por que isso é importante?** Sem Unit of Work, uma falha no passo 3 deixaria assentos reservados mas sem pedido (inconsistência). Com transação, ou tudo acontece ou nada acontece (**ACID**).

---

## 3. DTOs (Data Transfer Objects) + AutoMapper

### Motivação

DTOs desacoplam a camada de apresentação (API JSON) da camada de domínio (entidades), evitando:
- Exposição acidental de dados sensíveis (ex: `PasswordHash`)
- Problemas de serialização com navigation properties cíclicas
- Contratos de API que mudam quando o modelo de domínio muda

### Implementação no BoraAli

O projeto define DTOs para entrada e saída no namespace `BoraAli.Api.DTOs`:

| DTO | Direção | Uso |
|-----|---------|-----|
| `CreateEventDto` | Entrada | Criar evento |
| `UpdateEventDto` | Entrada | Atualizar evento |
| `EventDto` | Saída | Listar/detalhar evento |
| `RegisterUserDto` | Entrada | Cadastro |
| `LoginDto` | Entrada | Login |
| `CreateOrderDto` | Entrada | Criar pedido |
| `OrderDto` | Saída | Listar/detalhar pedido |
| `ApiResponseDto<T>` | Saída | Resposta padronizada |

O **AutoMapper** faz a conversão automática entre entidades e DTOs:

#### Perfil de Mapeamento (`BoraAli.Api/Extensions/AutoMapperProfile.cs`)

```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Event: Entity → DTO (com mapeamento de navigation properties)
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.OrganizerName,
                opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.Name : null))
            .ForMember(dest => dest.Tickets,
                opt => opt.MapFrom(src => src.TicketTypes));

        // CreateEventDto → Entity (com valores padrão)
        CreateMap<CreateEventDto, Event>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // UpdateEventDto → Entity (ignora campos null)
        CreateMap<UpdateEventDto, Event>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Order: Entity → DTO (com dados do evento via JOIN)
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.EventTitle,
                opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : null));
    }
}
```

**Destaques:**
- `ForAllMembers(...Condition(...srcMember != null))` no `UpdateEventDto`: apenas campos enviados pelo frontend são atualizados (PATCH semântico via PUT).
- `GenerateOrderCode()`: gera código único no formato `BA-YYYYMMDD-XXXXXXXX`.

### Resposta Padronizada

Toda resposta da API segue o envelope `ApiResponseDto<T>`:

```json
{
  "success": true,
  "message": "Evento criado com sucesso",
  "data": { "id": 1, "title": "Rock in Rio", "..." },
  "errors": null
}
```

Isso permite que o frontend Next.js sempre espere o mesmo formato, independente do endpoint:

```typescript
const data: ApiResponse<ApiCategory[]> = await res.json();
if (data.success) {
  setCategories(data.data);
}
```

---

## 4. Injeção de Dependência (Dependency Injection) no .NET 8

### Motivação

DI é nativa do .NET 8 e promove **baixo acoplamento** e **alta testabilidade**. No BoraAli, ela conecta todas as camadas: Controllers → Services → Repositories → Database.

### Configuração (`BoraAli.Api/Program.cs`)

```csharp
// ===== Database (Singleton - conexão compartilhada) =====
builder.Services.AddSingleton(new DbSession(connectionString));

// ===== Repositories (Scoped - um por requisição HTTP) =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDapperExecutor, DapperExecutor>();

// ===== Services (Scoped) =====
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();

// ===== AutoMapper (Singleton) =====
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ===== FluentValidation (Transient) =====
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddTransient<CreateEventValidator>();
builder.Services.AddTransient<RegisterUserValidator>();
// ...

// ===== JWT Authentication =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... */ });

// ===== Authorization Policies =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrganizadorOnly",
        policy => policy.RequireRole("Organizador", "Admin"));
    options.AddPolicy("ClienteOnly",
        policy => policy.RequireRole("Cliente", "Admin"));
});

// ===== Rate Limiting =====
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Global", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("Login", config =>
    {
        config.PermitLimit = 5;  // 5 tentativas/min
        config.Window = TimeSpan.FromMinutes(1);
    });
});
```

### Ciclo de Vida dos Serviços

| Tipo | Exemplo | Justificativa |
|------|---------|---------------|
| **Singleton** | `DbSession` | Uma única conexão SQLite compartilhada |
| **Scoped** | `UnitOfWork`, `EventService`, `OrderService` | Um por requisição HTTP — transações e repositórios alinhados ao ciclo da request |
| **Transient** | Validators (FluentValidation) | Leves, sem estado, descartáveis |

### Injeção por Construtor

Todos os serviços recebem suas dependências via construtor:

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly IDapperExecutor _dapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<OrderService> logger, IDapperExecutor dapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _dapper = dapper;
    }
}
```

**Por que isso é "ouro para a disciplina de POO"?** A injeção por construtor torna explícitas todas as dependências de uma classe, facilitando testes unitários com mocks e respeitando o **Princípio da Inversão de Dependência (SOLID — DIP)**.

---

## 5. Padrões Complementares

### 5.1. FluentValidation (Validation Pattern)

Validações de entrada são isoladas em classes validator, mantendo os controllers limpos:

```csharp
public class CreateEventValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty();
        // ...
    }
}
```

### 5.2. Middleware Pattern (Exception Handling)

O `ExceptionMiddleware` captura exceções não tratadas e retorna respostas JSON padronizadas, em vez de stack traces HTML:

```csharp
app.UseMiddleware<ExceptionMiddleware>(); // Primeiro middleware do pipeline
```

### 5.3. Rate Limiting Pattern

Proteção contra abusos com políticas configuráveis por endpoint:
- **Global**: 100 req/min
- **Login**: 5 req/min (anti brute-force)
- **Orders**: 10 req/min
- **Upload**: 20 req/min

### 5.4. Token-Based Authentication (JWT)

Autenticação stateless via JWT com roles (Admin, Organizador, Cliente) e políticas de autorização (`OrganizadorOnly`, `ClienteOnly`).

---

## Resumo Visual da Arquitetura

```
┌─────────────────────────────────────────────────────────┐
│                    FRONTEND (Next.js)                    │
│  app/login  app/criar-evento  app/evento  app/checkin   │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTP (JSON) + JWT Bearer Token
┌──────────────────────▼──────────────────────────────────┐
│                 .NET 8 API (Backend)                     │
│                                                         │
│  Controllers ──── Services ──── Repositories ──── DB    │
│  (HTTP)          (Business)    (Data Access)    (SQLite)│
│                                                         │
│  Cross-cutting:                                          │
│  ├── AutoMapper (Entity ↔ DTO)                          │
│  ├── FluentValidation (Input Validation)                 │
│  ├── JWT Auth (Authentication + Authorization)           │
│  ├── Rate Limiting (Anti-abuse)                          │
│  ├── Serilog (Structured Logging)                       │
│  └── ExceptionMiddleware (Global Error Handling)         │
└─────────────────────────────────────────────────────────┘
```
