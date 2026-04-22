# Requisitos — TicketPrime

## Histórias de Usuário

**US-01**
Como cliente, Quero comprar ingressos para um evento escolhendo setor e assento, Para garantir meu lugar com antecedência.

**US-02**
Como organizador, Quero cadastrar eventos com nome, data, capacidade e preço, Para disponibilizá-los na plataforma de vendas.

**US-03**
Como cliente, Quero aplicar um cupom de desconto na compra do ingresso, Para pagar um valor reduzido.

**US-04**
Como administrador, Quero visualizar relatórios de vendas por evento, Para acompanhar a receita e ocupação em tempo real.

---

## Critérios de Aceitação (BDD)

**US-01 — Compra de Ingresso**

Dado que o cliente está autenticado e existe um evento com assentos disponíveis,
Quando ele selecionar um setor, um assento disponível e confirmar a compra,
Então o sistema deve registrar o ingresso com código único e marcar o assento como ocupado.

---

**US-02 — Cadastro de Evento**

Dado que o organizador está autenticado como administrador,
Quando ele preencher nome, data, capacidade total e preço padrão do evento,
Então o sistema deve salvar o evento e disponibilizá-lo na listagem pública.

---

**US-03 — Aplicação de Cupom de Desconto**

Dado que o cliente possui um cupom válido com código PROMO10 e valor mínimo de R$ 50,00,
Quando ele aplicar o cupom em uma compra de R$ 80,00,
Então o sistema deve calcular o desconto e exibir o valor final reduzido ao cliente.

---

**US-04 — Relatório de Vendas**

Dado que o administrador está autenticado no sistema,
Quando ele acessar a seção de relatórios,
Então o sistema deve exibir o total de ingressos vendidos, receita total e ocupação por evento.
