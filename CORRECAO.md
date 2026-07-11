# Correção AV2 — engenharia-software (BoraAli)

**Grupo:** Andrey, Gustavo, Cristiano, Nathan, Lucas, Julia

| # | Item de Avaliação | Nota | Justificativa |
|---|-------------------|:----:|---------------|
| 01 | Padrão AAA nos Testes | 0,5 | `EventServiceTests.cs`, `AuthServiceTests.cs` e `OrderServiceTests.cs` com `// Arrange`, `// Act`, `// Assert`; Moq para dependências |
| 02 | Nomenclatura e Independência | 0,5 | `GetEventByIdAsync_WithExistingId_ReturnsEventDto` segue `Metodo_Cenario_ResultadoEsperado`; zero condicionais |
| 03 | Padrões Arquiteturais | 0,5 | 3 cenários com `Positivo:`/`Negativo:` em `/docs/analise_arquitetura.md` |
| 04 | Violações Arquiteturais | 0,5 | 5 violações com `**Problema:**`, `**Evidência:**`, `**Impacto:**`, `**Ação Recomendada:**` |
| 05 | ADR | 0,5 | `/docs/adrs/001-escolha-do-micro-orm.md` com Contexto, Decisão, Consequências, Status: Aceito, Prós/Contras |
| 06 | Dívida Técnica | 0,5 | Tabela com 6+ dívidas e colunas: ID, Descrição Técnica, Freq. Alteração, Risco, Esforço, Decisão |
| 07 | Priorização Dívida | 0,5 | P1, P2 e P3 presentes |
| 08 | Classificação Manutenção | 0,5 | 12 tickets classificados como Corretiva, Adaptativa, Perfectiva, Preventiva (Swanson) |
| 09 | Pipeline de Liberação | 0,5 | 4 passos: Análise de Impacto, Teste Cirúrgico, Feature Toggle, Estratégia de Release |
| 10 | Plano de Iteração | 0,5 | Objetivo, Escopo, Entregáveis, Risco Principal, DoD preenchidos |
| 11 | Quadro Kanban e WIP | 0,5 | 4+ colunas + WIP ≤ 6 integrantes |
| 12 | Matriz de Riscos | 0,5 | 5+ riscos com Probabilidade, Impacto, Estratégia, Ação Planejada |
| 13 | Gatilhos de Risco | 0,5 | Todos gatilhos com ≥20 caracteres |
| 14 | Métrica DORA | 0,5 | Ficha com 7 campos (Nome, O que Mede, Fórmula, Fonte, Frequência, Limites, Ação se Violado) |
| 15 | Métrica de Qualidade | 0,5 | Segunda métrica com 7 campos |
| 16 | SLO | 0,5 | SLI, Fórmula de Coleta, Fonte, Janela, Alvo definidos |
| 17 | Error Budget Policy | 0,5 | 3 níveis graduados; Nível 3 com Feature Freeze/congelamento |
| 18 | Segurança SSDF | 0,5 | Nenhuma credencial hardcoded nos 39 `.cs` |
| 19 | Threat Model e Gates | 0,5 | Ativos, Vetor, Falha, Mitigação + 3 Gates |
| 20 | Topologia Times e DoD | 0,5 | 4 tipos Team Topologies + `release_checklist_final.md` com 7 `[x]` |

**Nota Final: 10,0 / 10,0**
