# Fluxo de Manutenção — BoraAli API

## Classificação de Tickets de Manutenção

| Ticket | Tipo | Descrição |
|---|---|---|
| Ticket 1 Corretiva | Corretiva | Erro 500 ao criar pedido quando o evento não possui TicketTypes. A API retorna NullReferenceException em vez de mensagem amigável. |
| Ticket 2 Adaptativa | Adaptativa | Migração do SQLite para PostgreSQL para suportar maior concorrência de escritas em produção. |
| Ticket 3 Perfectiva | Perfectiva | Adicionar cache Redis na rota `GET /api/events` para reduzir latência de consultas repetidas em 80%. |
| Ticket 4 Preventiva | Preventiva | Atualizar pacote `Microsoft.Data.Sqlite` da versão 8.0.0 para 8.0.11 para corrigir CVE de segurança reportado. |
| Ticket 5 Corretiva | Corretiva | Check-in via QR Code permite múltiplas entradas com o mesmo código após race condition no `CheckInOrderAsync`. O método lê o status e depois atualiza sem lock atômico. |
| Ticket 6 Adaptativa | Adaptativa | Adaptar `EmailService` para usar Amazon SES em vez de SMTP Gmail, pois o volume de e-mails transacionais ultrapassou os limites do Gmail. |
| Ticket 7 Perfectiva | Perfectiva | Adicionar endpoint `GET /api/events/recommendations` com algoritmo de recomendação baseado no histórico de compras do usuário. |
| Ticket 8 Preventiva | Preventiva | Refatorar `GenericRepository` para validar `orderBy` contra whitelist de propriedades (já implementado parcialmente na ETAPA 1) e remover interpolação residual de SQL. |
| Ticket 9 Corretiva | Corretiva | Upload de imagem no `CreateEventFormDto` falha silenciosamente quando `WebRootPath` é nulo em ambiente Docker. O fallback usa `Directory.GetCurrentDirectory()` que não tem permissão de escrita. |
| Ticket 10 Adaptativa | Adaptativa | Adicionar suporte a PIX como método de pagamento real integrando com API do Mercado Pago, substituindo a simulação atual de confirmação. |
| Ticket 11 Perfectiva | Perfectiva | Implementar sistema de notificações push (WebSocket/SignalR) para alertar organizadores sobre novas vendas em tempo real no dashboard. |
| Ticket 12 Preventiva | Preventiva | Adicionar health checks para dependências externas (SQLite, SMTP) usando `Microsoft.Extensions.Diagnostics.HealthChecks` e expor em `/healthz`. |

---

## Fluxo de Liberação

O processo de liberação de qualquer alteração no BoraAli segue 4 passos obrigatórios:

### 1. Análise de Impacto

Antes de qualquer código, a equipe avalia o impacto da mudança nas rotas existentes, tabelas afetadas e dependências externas. A análise é documentada no PR com um checklist:
- [ ] Rotas modificadas e seus contratos (request/response)
- [ ] Tabelas alteradas e scripts de migration necessários
- [ ] Testes existentes que precisam ser atualizados
- [ ] Dependências externas impactadas (SMTP, storage de imagens, etc.)

### 2. Teste como Instrumento Cirúrgico

Cada alteração deve ser acompanhada de testes unitários (xUnit + Moq) que cobrem exatamente o comportamento modificado. O padrão AAA (`// Arrange`, `// Act`, `// Assert`) é obrigatório. Testes não podem conter `if`, `switch`, `for` ou `while` no corpo — cada cenário é um método separado com nome `Metodo_Cenario_ResultadoEsperado`.

### 3. Feature Toggle

Funcionalidades novas ou arriscadas são implementadas atrás de feature toggles usando `appsettings.json`:

```json
{
  "FeatureToggles": {
    "EnablePixPayment": false,
    "EnableRedisCache": false,
    "EnablePushNotifications": false
  }
}
```

Isso permite deploy em produção com a feature desligada, ativação gradual por ambiente e rollback instantâneo sem novo deploy.

### 4. Estratégia de Release e Regressão

O pipeline de CI/CD segue o fluxo:
1. **Build + Testes Unitários** (xUnit, 100% pass)
2. **Análise Estática** (`dotnet format`, warnings como erros)
3. **Deploy em Staging** com smoke tests automatizados (chamadas HTTP para rotas críticas)
4. **Regressão automatizada** com script de integração que executa fluxos completos: cadastro → login → criar evento → publicar → comprar ingresso → check-in
5. **Deploy Canário em Produção** (10% do tráfego por 15 minutos)
6. **Deploy Completo** se zero erros no canário; rollback automático se taxa de erro > 1%
