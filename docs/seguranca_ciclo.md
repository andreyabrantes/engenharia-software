# Segurança no Ciclo de Desenvolvimento — BoraAli API

## 1. Modelagem da Rota de Maior Risco: `POST /api/orders` (Criação de Pedido)

### Ativos Protegidos:
- Dados financeiros dos usuários (valor total do pedido, método de pagamento)
- Disponibilidade de ingressos (quantidade limitada, risco de overselling)
- Integridade das transações (pedido criado ↔ estoque debitado atomicamente)
- Dados pessoais do comprador (nome, e-mail, CPF vinculado ao pedido)

### Vetor de Ataque Provável:
Um atacante autenticado envia múltiplas requisições simultâneas de criação de pedido para o mesmo tipo de ingresso, explorando a janela de race condition entre a verificação de `AvailableQuantity >= @Qty` e o `UPDATE` de débito. Se o timing for preciso, o atacante consegue comprar ingressos que não existem (overselling). O vetor é agravado pela ausência de lock pessimista ou semáforo por evento.

### Falha Arquitetural Potencial:
O controle de concorrência atual usa uma cláusula `WHERE AvailableQuantity >= @Qty` no UPDATE como mecanismo de concorrência otimista, mas a verificação inicial de disponibilidade (`ticketType.AvailableQuantity < itemDto.Quantity`) ocorre em memória com dados carregados antes da transação. Entre a leitura e a escrita, outro pedido pode ter debitado o estoque, e a única proteção é a condição no UPDATE. Se o banco SQLite estiver em modo WAL sem travas adequadas, a condição de corrida ainda pode resultar em AvailableQuantity negativo.

### Controle de Engenharia (Mitigação):
Implementar lock pessimista por evento usando `BEGIN IMMEDIATE TRANSACTION` em vez de `BEGIN TRANSACTION` para garantir que apenas uma transação por evento execute por vez. Adicionalmente, adicionar uma validação pós-UPDATE que verifica se `AvailableQuantity` ficou negativo e, nesse caso, faz rollback com mensagem "Ingressos esgotados durante o processamento". A longo prazo, migrar para PostgreSQL com `SELECT ... FOR UPDATE` para bloqueio de linha.

---

## 2. Barreiras de Segurança no Ciclo de Desenvolvimento

### Gate 1: Validação Estática de Segurança no Commit

**Momento:** Pré-commit e Push.

**O que verifica:**
- Regex scan em busca de padrões de credenciais hardcoded (`Password=`, `SecretKey=`, `ConnectionString=`, chaves API com mais de 20 caracteres alfanuméricos)
- Arquivos `.cs` e `.json` são varridos por `git-secrets` e `detect-secrets`
- Bloqueia o commit se encontrar padrão suspeito, exigindo mover o segredo para variável de ambiente ou `.gitignore`-d secrets

**Ferramenta:** Pre-commit hooks com script bash/powershell + regex customizada.

---

### Gate 2: Análise de Composição de Software (SCA) no CI

**Momento:** Pull Request aberto → GitHub Actions.

**O que verifica:**
- Dependências NuGet contra base de CVEs conhecidas (via `dotnet list package --vulnerable`)
- Licenças de pacotes (bloqueia GPL/AGPL em produção)
- Versões mínimas de pacotes críticos (`Microsoft.Data.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Dapper`, `BCrypt.Net`)

**Ferramenta:** GitHub Actions + `dotnet list package --vulnerable` + OWASP Dependency-Check.

**Ação se falhar:** PR não pode ser mergeado. Comentário automático com o CVE e recomendação de upgrade.

---

### Gate 3: Teste de Penetração Automatizado no Staging

**Momento:** Deploy em staging concluído → antes do deploy canário em produção.

**O que verifica:**
- OWASP ZAP Baseline Scan contra a API de staging (varredura passiva de todas as rotas expostas no Swagger)
- Testes automatizados de segurança:
  - SQL Injection: envia payloads como `' OR '1'='1` em parâmetros de query string e corpo JSON
  - JWT sem assinatura: envia token com `alg: none` e verifica se é rejeitado com 401
  - Rate limiting bypass: envia 50 requisições em 1 segundo para rota de login e verifica se o 6º já retorna 429
  - Mass assignment: envia campos extras no JSON (`isAdmin: true`) para endpoints de criação de usuário

**Ferramenta:** OWASP ZAP (via Docker) + script de testes customizados com `curl` e `jq`.

**Ação se falhar:** Deploy bloqueado. Relatório de vulnerabilidades anexado ao canal #security-alerts. Correção obrigatória antes do próximo deploy.
