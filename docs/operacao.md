# Operação — TicketPrime

## Matriz de Riscos

| Risco | Probabilidade | Impacto | Ação | Gatilho |
|---|---|---|---|---|
| Race condition em reserva simultânea de assento | Alta | Alto | Validação de disponibilidade com lock antes de confirmar a reserva | Dois usuários selecionam o mesmo assento ao mesmo tempo |
| Falha no envio de e-mail de confirmação | Média | Médio | Exibir confirmação visual na tela como fallback; reenvio manual disponível | Timeout ou erro no serviço de e-mail |
| Fraude com ingresso duplicado | Baixa | Alto | Geração de código único por compra com validação no check-in | Tentativa de uso do mesmo código em dois acessos |
| Indisponibilidade do banco de dados | Baixa | Alto | Health check periódico e retry automático na reconexão | Erro de conexão retornado pelo Dapper |
| Expiração de cupom não verificada | Média | Médio | Validar `DataExpiracao` e `valorMinimoregra` antes de aplicar desconto | Cliente tenta usar cupom vencido ou abaixo do valor mínimo |
