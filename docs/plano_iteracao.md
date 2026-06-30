# Plano de Iteração — BoraAli API

## Objetivo da Iteração:

Implementar a rota de publicação de eventos (`POST /api/events/{id}/publish`) com validações de negócio e o dashboard de resumo de vendas (`GET /api/events/{id}/sales-summary`) com consultas agregadas via Dapper, além de corrigir vulnerabilidades de credenciais hardcoded identificadas na auditoria de segurança.

## Escopo (Backlog Selecionado):

| ID | História/US | Prioridade | Estimativa |
|---|---|---|---|
| US-01 | Como organizador, quero publicar meu evento para que ele fique visível ao público | Alta | 3 pts |
| US-02 | Como organizador, quero ver o resumo de vendas de um evento com taxa de ocupação | Alta | 5 pts |
| US-03 | Como Tech Lead, quero remover credenciais hardcoded do código fonte | Crítica | 2 pts |
| US-04 | Como Tech Lead, quero garantir que todas as queries usem parâmetros Dapper | Crítica | 3 pts |
| US-05 | Como QA, quero testes unitários para os novos endpoints seguindo o padrão AAA | Alta | 3 pts |

**Total:** 16 pontos

## Entregáveis (Evidências):

- Endpoint `POST /api/events/{id}/publish` funcional com 4 validações de negócio
- Endpoint `GET /api/events/{id}/sales-summary` funcional com INNER JOIN e LEFT JOIN
- 3 novos testes unitários em `EventServiceTests` (padrão AAA, sem condicionais)
- Zero credenciais hardcoded no código (verificado por varredura)
- `dotnet build` com 0 warnings e `dotnet test` com 24/24 aprovados

## Risco Principal do Ciclo:

**Risco:** A validação de 24 horas de antecedência para publicação de eventos pode conflitar com eventos criados em lote por scripts de seed, exigindo ajuste manual de datas nos dados de teste. **Mitigação:** Script de seed (`Script0002_SeedData.sql`) atualizado com datas futuras realistas (30 a 90 dias à frente).

## Definição de Pronto (DOD):

- [ ] Código compila com 0 warnings no `dotnet build`
- [ ] 24/24 testes unitários passam sem falhas
- [ ] Testes de integração manual validados no Swagger
- [ ] Nenhuma credencial hardcoded nos arquivos `.cs` e `.json`
- [ ] Documentação SDD atualizada com ADR, dívida técnica, análise arquitetural e plano de iteração
- [ ] PR revisado por pelo menos 1 outro desenvolvedor
- [ ] Feature toggles desnecessários removidos antes do merge

---

## Quadro Kanban da Iteração

O quadro possui 4 colunas com limite de Work In Progress (WIP) explícito. **WIP máximo: 4** itens em progresso simultâneo em todo o quadro.

| Backlog (Fila) | Em Desenvolvimento (WIP ≤ 2) | Em Revisão (WIP ≤ 1) | Concluído |
|---|---|---|---|
| Itens priorizados aguardando capacidade | Desenvolvedor trabalhando ativamente no código | Pull Request aberto, aguardando code review | Merge realizado, deploy em staging |
| US-05 (3 pts) | US-02 (5 pts) ← Desenvolvedor A | US-01 (3 pts) ← Revisor B | US-03 (2 pts) ✅ |
| | US-04 (3 pts) ← Desenvolvedor B | | |

**Regras do quadro:**
- Nenhuma coluna pode exceder seu WIP máximo
- Um item só entra em "Em Revisão" se houver vaga (WIP ≤ 1)
- Itens bloqueados voltam para "Backlog" com tag `[BLOQUEADO]` e motivo documentado
