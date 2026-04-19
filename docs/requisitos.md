# Requisitos — TicketPrime

## Histórias de Usuário

**US-01 — Compra de Ingresso**
Como cliente, quero comprar ingressos para um evento escolhendo setor e assento, para garantir meu lugar com antecedência.

**US-02 — Cadastro de Evento**
Como organizador, quero cadastrar eventos com setores e preços diferenciados, para disponibilizá-los na plataforma.

**US-03 — Aplicação de Cupom de Desconto**
Como cliente, quero aplicar um cupom de desconto na compra, para pagar um valor reduzido no ingresso.

---

## Critérios de Aceitação BDD

**US-01 — Compra de Ingresso**

**Dado que** o cliente está autenticado e seleciona um evento com assentos disponíveis,
**Quando** ele escolher um setor, um assento disponível e confirmar a compra,
**Então** o sistema deve registrar o ingresso com um código único e marcar o assento como ocupado.

---

**US-02 — Cadastro de Evento**

**Dado que** o organizador está autenticado como administrador,
**Quando** ele preencher nome, data, local e ao menos um setor com preço e capacidade,
**Então** o sistema deve salvar o evento e disponibilizá-lo na listagem pública.

---

**US-03 — Aplicação de Cupom de Desconto**

**Dado que** o cliente possui um cupom válido com código `PROMO10` e valor mínimo de R$ 50,00,
**Quando** ele aplicar o cupom em uma compra de R$ 80,00,
**Então** o sistema deve aplicar o desconto e exibir o novo valor total com o desconto calculado.
