# ADR 001: Escolha do Micro-ORM (Dapper) para Acesso a Dados

**Status: Aceito**

**Data:** 2024-01-15

**Stakeholders:** Equipe de Desenvolvimento BoraAli, Tech Lead, Arquiteto de Software

---

## Contexto

O projeto BoraAli é uma plataforma de venda de ingressos para eventos que precisa de uma API rápida e leve. Durante a fase inicial de arquitetura, a equipe avaliou duas abordagens para acesso a dados:

1. **Entity Framework Core (EF Core)** — ORM completo com Change Tracker, migrations automáticas, navegação lazy/eager loading e suporte a LINQ to SQL.
2. **Dapper** — Micro-ORM que executa SQL bruto com mapeamento leve de objetos, sem Change Tracker ou abstrações pesadas.

O banco de dados escolhido foi **SQLite** para simplificar o desenvolvimento local e reduzir dependências de infraestrutura externa. As principais operações incluem consultas de eventos com múltiplos JOINs (eventos, categorias, tipos de ingresso, organizadores), criação transacional de pedidos com controle de concorrência e dashboards analíticos com agregações (SUM, COUNT, GROUP BY).

Os critérios de decisão foram: performance de consulta, controle sobre o SQL gerado, facilidade de manutenção, curva de aprendizado para novos desenvolvedores e compatibilidade com SQLite.

---

## Decisão

**Escolhemos Dapper como Micro-ORM para todas as operações de acesso a dados**, complementado por:

- **DbUp** para execução de migrations SQL versionadas (substitui migrations automáticas do EF Core).
- **Padrão Repository + Unit of Work** para encapsular queries e gerenciar transações.
- **IDapperExecutor** como abstração sobre os métodos `QueryAsync<T>`, `QuerySingleOrDefaultAsync<T>` e `ExecuteAsync` do Dapper.

Para as consultas que exigem múltiplos JOINs (ex: evento com categoria, organizador e tipos de ingresso), utilizamos o mapeamento multi-result do Dapper (`QueryAsync<T1, T2, T3, TReturn>`) com dicionários de agregação manual para evitar duplicação de registros (problema do "cartesian product").

Rotas críticas como criação de pedidos usam queries SQL parametrizadas com `UPDATE TicketTypes SET AvailableQuantity = AvailableQuantity - @Qty WHERE ... AND AvailableQuantity >= @Qty` para garantir atomicidade e controle de concorrência otimista.

---

## Consequências

### Prós:

- **Performance superior em leitura:** Dapper é 3-5x mais rápido que EF Core em benchmarks com múltiplos JOINs, pois não mantém Change Tracker nem cache de identidade.
- **Controle total sobre o SQL:** A equipe escreve e otimiza queries manualmente, eliminando surpresas com SQL gerado (problema comum do EF Core com queries complexas).
- **Curva de aprendizado baixa:** Desenvolvedores com conhecimento SQL são produtivos imediatamente, sem precisar dominar LINQ to Entities ou convenções de navegação.
- **Menor consumo de memória:** Sem tracking de entidades, o footprint de memória é significativamente menor, importante para ambientes com recursos limitados.

### Contras:

- **Produtividade reduzida em CRUD simples:** Operações básicas exigem escrita manual de SQL (`INSERT INTO`, `UPDATE`, `DELETE`), enquanto o EF Core gera automaticamente. O `GenericRepository` mitiga isso parcialmente com reflexão.
- **Ausência de type-safety em queries:** Erros de digitação em nomes de colunas ou parâmetros (`@Parametro`) só são detectados em runtime, não em tempo de compilação. Testes de integração são essenciais para mitigar.
- **Mapeamento manual de relações:** JOINs complexos exigem lógica de agrupamento manual (dicionários), propensa a erros. O EF Core faria isso automaticamente com `.Include()`.
- **Sem migrations reversíveis:** DbUp não suporta rollback nativo. Cada migration precisa de um script de reversão manual, aumentando o risco em deploys.
- **Duplicação de queries similares:** Consultas como "evento com detalhes" aparecem em `EventRepository` e `OrderService` com pequenas variações, sem um mecanismo centralizado de reuso.
