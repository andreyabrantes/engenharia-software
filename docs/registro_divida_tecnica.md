# Registro de Dívida Técnica — BoraAli API

| ID da Dívida | Descrição Técnica | Freq. Alteração | Risco | Esforço | Decisão |
|---|---|---|---|---|---|
| DT-001 | `GenericRepository.FindAsync()` carrega tabelas inteiras em memória em vez de traduzir predicates para SQL. Toda filtragem ocorre via LINQ-to-Objects após `GetAllAsync()`. | Alto | Alto | Médio | Prioridade 1 (Imediato) |
| DT-002 | Fallback de `SecretKey` JWT removido do `AuthService.cs` mas sem validação em startup. Se a variável de ambiente não for configurada, a API inicia e falha apenas na primeira requisição autenticada. | Baixo | Alto | Baixo | Prioridade 1 (Imediato) |
| DT-003 | Gerenciamento de transações duplicado em 4 métodos do `OrderService` (criação, cancelamento, reembolso, confirmação de pagamento). Padrão try-catch com Begin/Commit/Rollback repetido ~15 linhas por método. | Alto | Médio | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-004 | Scripts SQL de migration (`Script0001` a `Script0004`) não possuem versões de rollback. Em caso de falha de deploy, a reversão é manual e propensa a erro humano. | Baixo | Alto | Médio | Prioridade 2 (Próxima Sprint) |
| DT-005 | `EventService.GetAllEventsAsync()` carrega todos os eventos em memória e pagina via LINQ `Skip().Take()`, sem utilizar `LIMIT/OFFSET` no banco. A paginação no banco existe em `GetPagedAsync` mas não é utilizada. | Alto | Médio | Médio | Prioridade 2 (Próxima Sprint) |
| DT-006 | Validação de CPF no `RegisterUserValidator` não cobre todos os casos de CPFs inválidos conhecidos (ex: não valida dígitos iguais com máscaras não padronizadas). A validação atual cobre apenas CPFs sem pontuação. | Baixo | Baixo | Baixo | Prioridade 3 (Aceitar/Ignorar) |
