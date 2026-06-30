# Guia de Operação — BoraAli API

## 1. Matriz de Riscos

| Risco | Probabilidade | Impacto | Estratégia | Ação Planejada | Gatilho |
|---|---|---|---|---|---|
| Indisponibilidade do banco SQLite por corrupção de arquivo | Baixa | Alto | Mitigar | Backup automatizado do arquivo `.db` a cada 6 horas para storage externo (S3). Script de restore testado mensalmente em ambiente de staging. | Arquivo BoraAli.db atinge 0 bytes OU taxa de erros 500 com mensagem "database is malformed" excede 10% em 5 minutos |
| Esgotamento dos limites de SMTP do Gmail (500 e-mails/dia) | Média | Médio | Mitigar | Implementar fila de e-mails com retry (Polly) e fallback para Amazon SES se cota do Gmail atingir 80%. Monitorar métrica de envios diários via dashboard. | Contador de e-mails enviados nas últimas 24h atinge 400 envios (80% da cota diária do Gmail) |
| Vazamento de SecretKey JWT via log ou commit acidental | Média | Alto | Evitar | SecretKey armazenada exclusivamente em environment variables / GitHub Secrets. Pré-commit hook valida se arquivos `.cs` e `.json` contêm padrões de chave privada (regex). Scan semanal com `git-secrets`. | Alerta do GitHub Secret Scanning OU detecção do padrão regex de chave JWT em qualquer arquivo do repositório |
| Ataque de força bruta nos endpoints de login e check-in | Média | Médio | Mitigar | Rate limiting já implementado (5 req/min para Login e CheckIn). Adicionar bloqueio progressivo: 5 falhas → 15 min de bloqueio, 10 falhas → 1 hora, 20 falhas → 24 horas. Log de tentativas com IP no Serilog. | Contagem de tentativas de login falhas para o mesmo IP atinge 5 em janela de 1 minuto, acionando alerta no canal #security-alerts do Slack |
| Indisponibilidade do servidor de hospedagem (Docker/VM) | Baixa | Alto | Transferir | Configurar health check endpoint `/healthz` com probe do Kubernetes/Docker Compose para restart automático. Estratégia de deploy Blue/Green com 2 instâncias mínimas. Monitorar uptime via UptimeRobot com alerta SMS. | Health check `/healthz` retorna código diferente de 200 por 3 execuções consecutivas em intervalo de 30 segundos |

---

## 2. Métricas de Fluxo

### Nome da Métrica: Lead Time de Entrega (Issue to Deploy)

**O que Mede:** Tempo total desde a criação de uma issue/US no backlog até o deploy em produção, incluindo desenvolvimento, code review e pipeline de CI/CD.

**Fórmula:** `Lead Time = Data/Hora do Deploy em Produção - Data/Hora da Criação da Issue`

**Fonte de Dados:** GitHub Issues (coluna `created_at`) e GitHub Actions (timestamp do deploy job concluído com sucesso). Extraídos via GitHub API.

**Frequência de Coleta:** A cada deploy concluído (event-driven). Consolidação semanal para relatório de tendências.

**Limites de Saúde:** 
- Saudável: Lead Time ≤ 3 dias
- Alerta: Lead Time entre 3 e 7 dias
- Crítico: Lead Time > 7 dias

**Ação se Violado:** Se Lead Time > 7 dias por 3 deploys consecutivos, agendar reunião de retrospectiva para identificar gargalos (code review lento, testes quebrados, dependências bloqueantes) e propor ajustes no WIP ou na definição de pronto.

---

## 3. Métricas de Qualidade

### Nome da Métrica: Taxa de Sucesso de Deploy

**O que Mede:** Percentual de deploys que chegam a produção sem causar incidentes (erro 500, degradação de latência > 2x baseline, ou rollback).

**Fórmula:** `Taxa de Sucesso = (Deploys sem Incidentes / Total de Deploys) × 100`

**Fonte de Dados:** GitHub Actions deploy logs + métricas de erro do Serilog (contagem de logs de nível `Error` e `Fatal` nos primeiros 15 minutos pós-deploy).

**Frequência de Coleta:** A cada deploy. Dashboard atualizado em tempo real.

**Limites de Saúde:**
- Saudável: Taxa de Sucesso ≥ 95%
- Alerta: Taxa de Sucesso entre 85% e 95%
- Crítico: Taxa de Sucesso < 85%

**Ação se Violado:** Se taxa < 85% na janela dos últimos 10 deploys, ativar feature freeze temporário (apenas hotfixes permitidos), auditar os deploys com falha e exigir aprovação de 2 revisores para o próximo deploy.

---

## 4. SLO — Rota Crítica: Criação de Pedido (`POST /api/orders`)

### SLI (Indicador):
Percentual de requisições `POST /api/orders` que retornam status HTTP 2XX (sucesso) em relação ao total de requisições válidas (excluindo erros 4XX do cliente).

### Fórmula de Coleta:
`SLI = (Requisições com status 2XX / Total de requisições - Requisições com status 4XX) × 100`

### Fonte do Dado:
Métricas extraídas do middleware Serilog + ASP.NET Core, exportadas para Application Insights / Prometheus. Cada requisição registra: timestamp, rota, status code, duração e trace ID.

### Janela de Medição:
30 dias corridos (rolling window). O SLI é recalculado diariamente com base nos últimos 30 dias de dados.

### Alvo (SLO):
**99,9%** das requisições válidas devem retornar sucesso na janela de 30 dias. Isso permite no máximo 0,1% de erro (≈ 43 minutos de indisponibilidade por mês para esta rota).

---

## 5. Error Budget Policy:

A política de Error Budget define ações baseadas no consumo acumulado do orçamento de erro na janela de 30 dias.

### Nível 1: Consumo ≤ 50% do Error Budget
- **Ação:** Operação normal. Nenhuma restrição.
- **Comunicação:** Dashboard verde. Sem alertas.

### Nível 2: Consumo entre 50% e 100% do Error Budget
- **Ação:** Alerta amarelo. Deploys de novas funcionalidades requerem aprovação do Tech Lead. Testes de regressão obrigatórios antes de cada deploy.
- **Comunicação:** Notificação no canal #ops-alerts do Slack. Post-mortem agendado se tendência de consumo acelerar (>10% em 24h).

### Nível 3: Consumo ≥ 100% do Error Budget
- **Ação:** **Feature Freeze** imediato. **Zero novas funcionalidades** são permitidas em produção. Apenas hotfixes de segurança e correções de bugs que queimaram o error budget.
- **Comunicação:** Alerta crítico no Slack + PagerDuty (on-call engineer). Post-mortem obrigatório em até 48 horas.
- **Recuperação:** Error Budget é resetado apenas quando o SLI dos últimos 30 dias volta a ≥ 99,9%. O feature freeze só é levantado após o post-mortem ser concluído e as ações corretivas serem implementadas.
