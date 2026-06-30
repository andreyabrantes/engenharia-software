# Análise de Arquitetura — BoraAli API

## 1. Cenários com Padrão Arquitetural e Trade-offs

### Cenário 1: Escolha do Padrão Repository + Unit of Work para Persistência

O projeto BoraAli utiliza o padrão **Repository Pattern** combinado com **Unit of Work** para abstrair o acesso a dados com Dapper sobre SQLite. Cada entidade possui um repositório genérico (`GenericRepository<T>`) que encapsula as queries SQL, enquanto o `UnitOfWork` gerencia a conexão e transações.

**Trade-off:** Performance de escrita com SQL bruto vs. segurança de tipo em tempo de compilação.
**Positivo:** Queries otimizadas manualmente, alta performance com Dapper, controle total sobre o SQL gerado e transações atômicas gerenciadas pelo Unit of Work.
**Negativo:** Ausência de type-safety nas queries SQL (strings mágicas), risco de erros de mapeamento entre colunas e propriedades que só são detectados em runtime, e duplicação de lógica de projeção de dados.

### Cenário 2: Minimal API com Controladores Traditionais (Sistema Híbrido)

O projeto adota controllers tradicionais (`[ApiController]`) do ASP.NET Core em vez de Minimal API pura com `app.MapGet()`. Isso foi combinado com o ecossistema Swagger, FluentValidation e JWT Authentication.

**Trade-off:** Estrutura previsível e documentável vs. minimalismo e performance de cold start.
**Positivo:** Fácil integração com Swagger, suporte nativo a Model Binding, FluentValidation e atributos de autorização. Estrutura familiar para times que já conhecem ASP.NET Core MVC.
**Negativo:** Mais verboso e com overhead de infraestrutura maior que Minimal APIs puras; cada controller depende de herança e atributos que dificultam testes isolados sem framework.

### Cenário 3: SQLite com DbUp para Migrations Versionadas

Em vez de um ORM completo com migrations automáticas (Entity Framework), o projeto usa **DbUp** para executar scripts SQL versionados e SQLite como banco relacional embutido.

**Trade-off:** Simplicidade operacional e portabilidade vs. escalabilidade e recursos avançados de banco.
**Positivo:** Zero dependência de servidor de banco externo, migrations previsíveis e auditáveis (arquivos `.sql`), ideal para desenvolvimento local e MVPs. O DbUp garante execução idempotente dos scripts.
**Negativo:** Sem suporte a migrations reversíveis (rollback), concorrência limitada do SQLite, e funcionalidades ausentes como views materializadas, procedures e replicação. Não adequado para ambientes com alta concorrência de escrita.

---

## 2. Análise de Violações Arquiteturais no Código

Trecho analisado: `GenericRepository<T>` (`BoraAli.Infrastructure/Repositories/GenericRepository.cs`)

### Violação 1

**Problema:** O repositório genérico traduz `Expression<Func<T, bool>>` para LINQ-to-Objects carregando todos os registros em memória.
**Evidência:** O método `FindAsync(Expression<Func<T, bool>> predicate)` executa `await GetAllAsync()` e depois aplica `.AsQueryable().Where(predicate).ToList()`. Não há tradução para SQL.
**Impacto:** Consultas que deveriam ser filtradas no banco carregam tabelas inteiras em memória, causando degradação de performance proporcional ao crescimento dos dados e potencial `OutOfMemoryException` em produção.
**Ação Recomendada:** Implementar um visitor de Expression Tree para converter predicates em cláusulas WHERE SQL com parâmetros Dapper, ou restringir `FindAsync` a cenários com coleções pequenas e documentar a limitação.

### Violação 2

**Problema:** Acoplamento forte entre `DapperExecutor` e `UnitOfWork` via dependência concreta da conexão e transação.
**Evidência:** `DapperExecutor` recebe `IUnitOfWork` por injeção e acessa diretamente `_unitOfWork.Connection` e `_unitOfWork.Transaction`. O construtor assume que a conexão já está aberta.
**Impacto:** Impossível testar `DapperExecutor` sem mockar todo o `IUnitOfWork`. Se o `UnitOfWork` mudar o gerenciamento de conexão, o executor quebra silenciosamente. Viola o Princípio de Inversão de Dependência (DIP).
**Ação Recomendada:** `DapperExecutor` deve receber `IDbConnection` e `IDbTransaction?` diretamente via injeção com ciclo de vida Scoped, removendo o acoplamento com Unit of Work.

### Violação 3

**Problema:** Lógica de pluralização de nomes de tabela frágil e ad-hoc no construtor do repositório.
**Evidência:** `_tableName = typeof(T).Name + "s"` com substituições manuais para casos especiais (`"ys"` → `"ies"`, `"Userss"` → `"Users"`). Não cobre exceções como "Person" → "People", "Child" → "Children".
**Impacto:** Adicionar uma nova entidade com nome irregular causa `SQLiteException` em runtime (tabela não encontrada). O bug só aparece quando o repositório é usado pela primeira vez.
**Ação Recomendada:** Usar um atributo customizado `[Table("TableName")]` na entidade ou uma biblioteca de pluralização como `Humanizer`. Alternativamente, implementar um dicionário explícito de mapeamento `Dictionary<Type, string>`.

### Violação 4

**Problema:** Mixagem de responsabilidades no `EventService`: regras de negócio, queries Dapper, orquestração de transações e logging no mesmo serviço.
**Evidência:** `GetOrganizerStatsAsync` contém 5 queries SQL inline com lógica de agregação (SUM, COUNT, GROUP BY), formatação de dados e tratamento de nulos com coalesce, tudo em um único método de ~75 linhas.
**Impacto:** O serviço acumula múltiplas razões para mudar (negócio, queries, formatação). Testar o método requer mockar Dapper + UnitOfWork simultaneamente. Dificulta reuso das queries agregadas.
**Ação Recomendada:** Extrair queries analíticas para uma classe `OrganizerAnalyticsQueries` dedicada com métodos como `GetRevenueByTicketTypeAsync()`, seguindo o padrão Query Object. O `EventService` deve orquestrar chamadas, não escrever SQL.

### Violação 5

**Problema:** Gerenciamento manual de transações espalhado pelos serviços (não centralizado).
**Evidência:** `OrderService.CreateOrderWithQuantityAsync`, `CancelOrderAsync` e `RefundOrderAsync` repetem o mesmo padrão try-catch com `BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`. Cada método duplica ~15 linhas de boilerplate transacional.
**Impacto:** Risco de inconsistência: se um novo desenvolvedor esquecer o rollback no catch, transações ficam abertas. Código duplicado dificulta alterações no mecanismo transacional (ex: mudar para `TransactionScope`).
**Ação Recomendada:** Criar um método helper `ExecuteInTransactionAsync(Func<Task> action)` no `UnitOfWork` que encapsula Begin/Commit/Rollback, ou usar um `TransactionMiddleware` via `IAsyncActionFilter`.
