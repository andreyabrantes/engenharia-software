# Casos de Uso - BoraAli

## Introdução

Este documento descreve os casos de uso do sistema **BoraAli**, detalhando as interações entre os atores e o sistema para cada funcionalidade. Cada caso de uso inclui fluxo principal, fluxos alternativos, pré-condições e pós-condições.

---

## Atores

| Ator | Descrição |
|------|-----------|
| **Cliente** | Usuário não autenticado ou autenticado com perfil "Cliente". Pode navegar, buscar eventos e comprar ingressos. |
| **Organizador** | Usuário autenticado com perfil "Organizador". Pode criar e gerenciar eventos. |
| **Admin** | Usuário autenticado com perfil "Admin". Pode gerenciar usuários, categorias e moderar conteúdo. |
| **Sistema** | Ator secundário que representa processos automáticos (envio de e-mail, processamento de pagamento, logging). |

---

## Diagrama de Casos de Uso (ASCII)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        BoraAli - Sistema de Eventos                  │
│                                                                      │
│  ┌──────────────┐                                                    │
│  │              │────── UC-01: Navegar Eventos ──────────────────►   │
│  │              │────── UC-02: Visualizar Evento ───────────────►   │
│  │   Cliente    │────── UC-03: Buscar Eventos ──────────────────►   │
│  │  (não aut.)  │────── UC-04: Registrar-se ────────────────────►   │
│  │              │                                                    │
│  └──────┬───────┘                                                    │
│         │                                                            │
│         ▼                                                            │
│  ┌──────────────┐                                                    │
│  │              │────── UC-05: Autenticar-se ───────────────────►   │
│  │   Cliente    │────── UC-06: Comprar Ingresso ────────────────►   │
│  │ (autenticado)│────── UC-07: Visualizar Pedidos ──────────────►   │
│  │              │────── UC-08: Gerenciar Perfil ────────────────►   │
│  └──────┬───────┘                                                    │
│         │                                                            │
│         ▼                                                            │
│  ┌──────────────┐                                                    │
│  │              │────── UC-09: Criar Evento ────────────────────►   │
│  │ Organizador  │────── UC-10: Gerenciar Evento ────────────────►   │
│  │              │────── UC-11: Gerenciar Ingressos ─────────────►   │
│  └──────┬───────┘                                                    │
│         │                                                            │
│         ▼                                                            │
│  ┌──────────────┐                                                    │
│  │              │────── UC-12: Gerenciar Usuários ──────────────►   │
│  │    Admin     │────── UC-13: Moderar Conteúdo ────────────────►   │
│  │              │                                                    │
│  └──────────────┘                                                    │
│                                                                      │
│  ┌──────────────┐                                                    │
│  │              │◄────── UC-14: Enviar Notificação ─────────────     │
│  │   Sistema    │◄────── UC-15: Processar Pagamento ────────────     │
│  │              │◄────── UC-16: Registrar Log ──────────────────     │
│  └──────────────┘                                                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## UC-01: Navegar Eventos

| Campo | Valor |
|-------|-------|
| **ID** | UC-01 |
| **Nome** | Navegar Eventos |
| **Ator Principal** | Cliente (não autenticado) |
| **Pré-condições** | Nenhuma |
| **Pós-condições** | O sistema exibe a lista de eventos disponíveis |

### Fluxo Principal
1. O usuário acessa a página inicial (`/`)
2. O sistema carrega os eventos em destaque no carrossel (HeroSection)
3. O sistema carrega o grid de eventos paginado
4. O usuário visualiza os cards com imagem, título, data, local e preço
5. O caso de uso termina

### Fluxos Alternativos
**FA01 - Nenhum evento encontrado:**
1. O sistema exibe mensagem "Nenhum evento encontrado"
2. O caso de uso termina

**FA02 - Erro de carregamento:**
1. O sistema tenta carregar da API real
2. Se a API falhar, o sistema utiliza dados mockados como fallback
3. O caso de uso continua no passo 3

---

## UC-02: Visualizar Evento

| Campo | Valor |
|-------|-------|
| **ID** | UC-02 |
| **Nome** | Visualizar Detalhes do Evento |
| **Ator Principal** | Cliente (não autenticado) |
| **Pré-condições** | O evento deve existir no sistema |
| **Pós-condições** | O sistema exibe os detalhes completos do evento |

### Fluxo Principal
1. O usuário clica em um card de evento na página inicial
2. O sistema redireciona para `/evento/{id}`
3. O sistema carrega os detalhes do evento (banner, data, horário, local, descrição)
4. O sistema carrega os tipos de ingresso disponíveis com preços
5. O sistema exibe o card do organizador
6. O usuário visualiza as informações
7. O caso de uso termina

### Fluxos Alternativos
**FA01 - Evento não encontrado:**
1. O sistema exibe mensagem "Evento não encontrado"
2. O caso de uso termina

---

## UC-03: Buscar Eventos

| Campo | Valor |
|-------|-------|
| **ID** | UC-03 |
| **Nome** | Buscar Eventos |
| **Ator Principal** | Cliente (não autenticado) |
| **Pré-condições** | Nenhuma |
| **Pós-condições** | O sistema exibe os resultados da busca |

### Fluxo Principal
1. O usuário digita um termo de busca no campo de pesquisa do header
2. O sistema filtra eventos por título, descrição, cidade ou categoria
3. O sistema exibe os resultados no grid
4. O caso de uso termina

### Fluxos Alternativos
**FA01 - Nenhum resultado:**
1. O sistema exibe mensagem "Nenhum evento encontrado para sua busca"
2. O caso de uso termina

**FA02 - Filtro por categoria:**
1. O usuário seleciona uma categoria no dropdown de filtros
2. O sistema filtra eventos pela categoria selecionada
3. O caso de uso continua no passo 3

**FA03 - Filtro por cidade:**
1. O usuário seleciona uma cidade no dropdown de filtros
2. O sistema filtra eventos pela cidade selecionada
3. O caso de uso continua no passo 3

---

## UC-04: Registrar-se

| Campo | Valor |
|-------|-------|
| **ID** | UC-04 |
| **Nome** | Registrar Novo Usuário |
| **Ator Principal** | Cliente (não autenticado) |
| **Pré-condições** | O usuário não deve possuir cadastro prévio com o mesmo e-mail |
| **Pós-condições** | Uma nova conta de usuário é criada no sistema |

### Fluxo Principal
1. O usuário acessa a página de login (`/login`)
2. O sistema exibe o formulário de login
3. O usuário clica em "Criar conta"
4. O sistema alterna para o modo de cadastro
5. O usuário preenche: nome, CPF (com formatação automática), e-mail, senha (com indicador de força), confirmação de senha
6. O usuário seleciona o perfil (Cliente ou Organizador) através de cards visuais
7. O usuário clica em "Criar conta"
8. O sistema valida os dados (frontend com Zod + backend com FluentValidation)
9. O sistema envia requisição POST `/api/auth/register`
10. O sistema retorna token JWT e dados do usuário
11. O sistema armazena o token no localStorage
12. O sistema redireciona para a página inicial
13. O caso de uso termina

### Fluxos Alternativos
**FA01 - E-mail já cadastrado:**
1. O sistema exibe toast de erro "E-mail já cadastrado"
2. O caso de uso retorna ao passo 5

**FA02 - CPF inválido:**
1. O sistema exibe erro de validação "CPF inválido"
2. O caso de uso retorna ao passo 5

**FA03 - Senha fraca:**
1. O sistema exibe erro "Senha deve conter no mínimo 8 caracteres, maiúsculas, minúsculas e números"
2. O caso de uso retorna ao passo 5

---

## UC-05: Autenticar-se

| Campo | Valor |
|-------|-------|
| **ID** | UC-05 |
| **Nome** | Autenticar Usuário |
| **Ator Principal** | Cliente (não autenticado) |
| **Pré-condições** | O usuário deve possuir cadastro no sistema |
| **Pós-condições** | O usuário recebe um token JWT válido |

### Fluxo Principal
1. O usuário acessa a página de login (`/login`)
2. O sistema exibe o formulário de login (e-mail + senha)
3. O usuário preenche e-mail e senha
4. O usuário clica em "Entrar"
5. O sistema valida os campos
6. O sistema envia requisição POST `/api/auth/login`
7. O sistema valida as credenciais (BCrypt)
8. O sistema gera um token JWT com claims (Id, Name, Email, Role)
9. O sistema retorna o token e dados do usuário
10. O sistema armazena o token no localStorage
11. O sistema redireciona para a página inicial
12. O caso de uso termina

### Fluxos Alternativos
**FA01 - Credenciais inválidas:**
1. O sistema exibe toast de erro "E-mail ou senha inválidos"
2. O caso de uso retorna ao passo 3

**FA02 - Conta inativa:**
1. O sistema exibe toast de erro "Conta desativada. Contate o administrador"
2. O caso de uso termina

**FA03 - Rate limit excedido:**
1. O sistema exibe toast de erro "Muitas tentativas. Tente novamente em 1 minuto"
2. O caso de uso termina

---

## UC-06: Comprar Ingresso

| Campo | Valor |
|-------|-------|
| **ID** | UC-06 |
| **Nome** | Comprar Ingresso |
| **Ator Principal** | Cliente (autenticado) |
| **Atores Secundários** | Sistema (processamento de pagamento) |
| **Pré-condições** | O usuário deve estar autenticado. O evento deve ter ingressos disponíveis. |
| **Pós-condições** | Um pedido é criado com status "Pending". A quantidade disponível do ingresso é reduzida. |

### Fluxo Principal
1. O usuário está na página do evento (`/evento/{id}`)
2. O sistema exibe o seletor de ingressos (TicketSelector) com tipos, preços e quantidades disponíveis
3. O usuário seleciona os tipos e quantidades desejadas
4. O sistema calcula o subtotal, taxa de serviço e total
5. O usuário clica em "Continuar para Pagamento"
6. O sistema verifica se o usuário está autenticado (useRequireAuth)
7. O sistema redireciona para `/checkout/{id}`
8. O sistema exibe o formulário de checkout (CheckoutForm)
9. O usuário preenche dados pessoais (nome, e-mail, CPF)
10. O usuário seleciona o método de pagamento (cartão, Pix, boleto)
11. Se cartão: o usuário preenche número, validade, CVV e nome no cartão
12. O usuário confirma a compra
13. O sistema valida os dados (frontend + backend)
14. O sistema envia requisição POST `/api/orders` com token JWT
15. O backend inicia uma transação (UnitOfWork.BeginTransactionAsync)
16. O backend cria o pedido (Order) com status "Pending"
17. O backend cria os itens do pedido (OrderItem)
18. O backend atualiza a quantidade disponível dos ingressos (TicketType.AvailableQuantity)
19. O backend confirma a transação (UnitOfWork.CommitTransactionAsync)
20. O sistema retorna confirmação com código do pedido
21. O sistema exibe toast de sucesso
22. O caso de uso termina

### Fluxos Alternativos
**FA01 - Usuário não autenticado:**
1. O sistema redireciona para `/login`
2. Após autenticação, o sistema redireciona de volta para o checkout
3. O caso de uso continua no passo 8

**FA02 - Ingresso esgotado durante a compra:**
1. O backend detecta que a quantidade disponível é insuficiente
2. O backend faz rollback da transação (UnitOfWork.RollbackTransactionAsync)
3. O sistema exibe toast de erro "Ingressos esgotados"
4. O caso de uso retorna ao passo 2

**FA03 - Dados de pagamento inválidos:**
1. O sistema exibe erro de validação no campo específico
2. O caso de uso retorna ao passo 10

**FA04 - Erro de validação no backend:**
1. O backend retorna erros de validação (FluentValidation)
2. O sistema exibe os erros no formulário
3. O caso de uso retorna ao passo 9

---

## UC-07: Visualizar Pedidos

| Campo | Valor |
|-------|-------|
| **ID** | UC-07 |
| **Nome** | Visualizar Meus Pedidos |
| **Ator Principal** | Cliente (autenticado) |
| **Pré-condições** | O usuário deve estar autenticado |
| **Pós-condições** | O sistema exibe a lista de pedidos do usuário |

### Fluxo Principal
1. O usuário acessa a seção "Meus Pedidos" (via menu do usuário)
2. O sistema envia requisição GET `/api/orders/my` com token JWT
3. O sistema exibe a lista de pedidos com código, data, evento, valor e status
4. O usuário visualiza os pedidos
5. O caso de uso termina

### Fluxos Alternativos
**FA01 - Nenhum pedido encontrado:**
1. O sistema exibe mensagem "Você ainda não possui pedidos"
2. O caso de uso termina

---

## UC-08: Gerenciar Perfil

| Campo | Valor |
|-------|-------|
| **ID** | UC-08 |
| **Nome** | Gerenciar Perfil do Usuário |
| **Ator Principal** | Cliente ou Organizador (autenticado) |
| **Pré-condições** | O usuário deve estar autenticado |
| **Pós-condições** | Os dados do perfil são atualizados |

### Fluxo Principal
1. O usuário acessa o menu do usuário no header
2. O sistema exibe as opções: "Meus Pedidos" (cliente), "Criar Evento" (organizador), "Sair"
3. O usuário seleciona uma opção
4. O sistema executa a ação correspondente
5. O caso de uso termina

### Fluxos Alternativos
**FA01 - Logout:**
1. O usuário clica em "Sair"
2. O sistema remove o token do localStorage
3. O sistema redireciona para a página inicial
4. O caso de uso termina

---

## UC-09: Criar Evento

| Campo | Valor |
|-------|-------|
| **ID** | UC-09 |
| **Nome** | Criar Novo Evento |
| **Ator Principal** | Organizador (autenticado) |
| **Pré-condições** | O usuário deve estar autenticado com perfil "Organizador" |
| **Pós-condições** | Um novo evento é criado com status "Draft" |

### Fluxo Principal
1. O usuário acessa a página de criação (`/criar-evento`)
2. O sistema verifica se o usuário é organizador (useRequireAuth)
3. O sistema exibe o formulário dividido em 4 cards:
   - Informações Básicas (título, descrição, categoria)
   - Imagem (upload com preview)
   - Data e Local (data, horário, endereço, cidade)
   - Ingressos (tipos com nome, preço, quantidade)
4. O usuário preenche as informações básicas e seleciona uma categoria
5. O usuário faz upload da imagem (com preview)
6. O usuário preenche data, horário e endereço
7. O usuário adiciona tipos de ingresso (mínimo 1)
8. O usuário clica em "Criar Evento"
9. O sistema valida os campos obrigatórios
10. O sistema envia requisição POST `/api/events` com token JWT
11. O sistema exibe toast de sucesso
12. O sistema redireciona para a página do evento criado
13. O caso de uso termina

### Fluxos Alternativos
**FA01 - Usuário não é organizador:**
1. O sistema redireciona para a página inicial
2. O caso de uso termina

**FA02 - Upload de imagem muito grande:**
1. O sistema exibe erro "Imagem muito grande. Tamanho máximo: 5MB"
2. O caso de uso retorna ao passo 5

**FA03 - Nenhum tipo de ingresso adicionado:**
1. O sistema exibe erro "Adicione pelo menos um tipo de ingresso"
2. O caso de uso retorna ao passo 7

---

## UC-10: Gerenciar Evento

| Campo | Valor |
|-------|-------|
| **ID** | UC-10 |
| **Nome** | Gerenciar Evento Existente |
| **Ator Principal** | Organizador (autenticado) |
| **Pré-condições** | O evento deve pertencer ao organizador |
| **Pós-condições** | O evento é atualizado |

### Fluxo Principal
1. O organizador acessa a página de gerenciamento do evento
2. O sistema exibe as opções: editar, publicar, cancelar
3. O organizador seleciona uma ação
4. O sistema executa a ação (PUT `/api/events/{id}` ou alteração de status)
5. O sistema exibe toast de confirmação
6. O caso de uso termina

---

## UC-11: Gerenciar Ingressos

| Campo | Valor |
|-------|-------|
| **ID** | UC-11 |
| **Nome** | Gerenciar Tipos de Ingresso |
| **Ator Principal** | Organizador (autenticado) |
| **Pré-condições** | O evento deve pertencer ao organizador |
| **Pós-condições** | Os tipos de ingresso são atualizados |

### Fluxo Principal
1. O organizador acessa a seção de ingressos do evento
2. O sistema exibe os tipos de ingresso existentes
3. O organizador pode adicionar, remover ou editar tipos
4. O sistema valida as alterações
5. O sistema persiste as alterações
6. O caso de uso termina

---

## UC-12: Gerenciar Usuários

| Campo | Valor |
|-------|-------|
| **ID** | UC-12 |
| **Nome** | Gerenciar Usuários do Sistema |
| **Ator Principal** | Admin |
| **Pré-condições** | O usuário deve estar autenticado com perfil "Admin" |
| **Pós-condições** | O sistema de usuários é atualizado |

### Fluxo Principal
1. O admin acessa o painel administrativo
2. O sistema exibe a lista de usuários
3. O admin pode ativar/desativar contas, alterar perfis
4. O sistema persiste as alterações
5. O caso de uso termina

---

## UC-13: Moderar Conteúdo

| Campo | Valor |
|-------|-------|
| **ID** | UC-13 |
| **Nome** | Moderar Conteúdo de Eventos |
| **Ator Principal** | Admin |
| **Pré-condições** | O usuário deve estar autenticado com perfil "Admin" |
| **Pós-condições** | O conteúdo é moderado |

### Fluxo Principal
1. O admin acessa o painel de moderação
2. O sistema exibe eventos pendentes de revisão
3. O admin aprova ou rejeita eventos
4. O sistema atualiza o status do evento
5. O caso de uso termina

---

## UC-14: Enviar Notificação

| Campo | Valor |
|-------|-------|
| **ID** | UC-14 |
| **Nome** | Enviar Notificação por E-mail |
| **Ator Principal** | Sistema |
| **Pré-condições** | Um evento que dispara notificação deve ocorrer (ex: compra confirmada) |
| **Pós-condições** | Um e-mail é enviado ao destinatário |

### Fluxo Principal
1. Um evento de domínio ocorre (ex: pedido confirmado)
2. O sistema dispara o serviço de e-mail (MailKit)
3. O sistema envia o e-mail com template apropriado
4. O sistema registra o envio no log (Serilog)
5. O caso de uso termina

---

## UC-15: Processar Pagamento

| Campo | Valor |
|-------|-------|
| **ID** | UC-15 |
| **Nome** | Processar Pagamento |
| **Ator Principal** | Sistema |
| **Pré-condições** | Um pedido com status "Pending" deve existir |
| **Pós-condições** | O pedido é confirmado ou cancelado |

### Fluxo Principal
1. O sistema recebe a confirmação de pagamento do gateway
2. O sistema atualiza o status do pedido para "Confirmed"
3. O sistema atualiza a data de confirmação (ConfirmedAt)
4. O sistema registra o ID do pagamento (PaymentId)
5. O caso de uso termina

---

## UC-16: Registrar Log

| Campo | Valor |
|-------|-------|
| **ID** | UC-16 |
| **Nome** | Registrar Log do Sistema |
| **Ator Principal** | Sistema |
| **Pré-condições** | Uma operação relevante deve ocorrer |
| **Pós-condições** | Um log é registrado no arquivo e console |

### Fluxo Principal
1. Uma operação ocorre (requisição, erro, alteração de dados)
2. O sistema registra o log via Serilog
3. O log é escrito no console e no arquivo com rotação diária
4. O caso de uso termina

---

## Matriz de Rastreabilidade

| Caso de Uso | Spec (Roadmap) | Funcionalidade (Visão) | Controller/Endpoint |
|:-----------:|:--------------:|:----------------------:|:-------------------:|
| UC-01 | S8 | F2 | GET /api/events, GET /api/events/featured |
| UC-02 | S10 | F3 | GET /api/events/{id} |
| UC-03 | S8 | F2 | GET /api/events (query params) |
| UC-04 | S9 | F1 | POST /api/auth/register |
| UC-05 | S9 | F1 | POST /api/auth/login |
| UC-06 | S11 | F5 | POST /api/orders |
| UC-07 | S11 | F8 | GET /api/orders/my |
| UC-08 | S9 | F1 | — (client-side) |
| UC-09 | S12 | F6 | POST /api/events |
| UC-10 | S12 | F6 | PUT /api/events/{id} |
| UC-11 | S12 | F4 | PUT /api/events/{id} |
| UC-12 | — | — | — (planejado) |
| UC-13 | — | — | — (planejado) |
| UC-14 | S14 | FP2 | — (serviço interno) |
| UC-15 | S11 | F5 | — (serviço interno) |
| UC-16 | S6 | — | — (middleware) |

---

## Especificação de Requisitos Não-Funcionais

*(Os requisitos não-funcionais detalhados estão no documento [`docs/requisitos-nao-funcionais.md`](docs/requisitos-nao-funcionais.md))*