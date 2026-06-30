# Roadmap - BoraAli

## Introdução

Este documento apresenta o roadmap completo do projeto **BoraAli**, listando todas as especificações (specs) planejadas, em desenvolvimento e já executadas. Cada spec está referenciada aos documentos de **Visão** e **Arquitetura** para demonstrar o valor entregue e a base técnica utilizada.

---

## Legenda

| Ícone | Significado |
|:-----:|-------------|
| ✅ | Concluído |
| 🔄 | Em desenvolvimento |
| 📋 | Planejado |

---

## Fase 1: Fundação (MVP) ✅

### S1 - Configuração do Projeto
**Status:** ✅ Concluído  
**Descrição:** Inicialização dos projetos frontend (Next.js) e backend (.NET 8) com todas as configurações base.

| Referência | Link |
|:----------:|------|
| **Visão** | [V1 - Infraestrutura Tecnológica](visao.md#6-tecnologias-utilizadas) |
| **Arquitetura** | [A1 - Stack Tecnológica](arquitetura.md#11-frontend) |

**Entregáveis:**
- [x] Projeto Next.js 16 com TypeScript e Tailwind CSS 4
- [x] Solution .NET 8 com 3 projetos (Core, Infrastructure, Api)
- [x] Configuração de pacotes NuGet (Dapper, BCrypt, JWT, Serilog, FluentValidation, AutoMapper)
- [x] Configuração de dependências npm (shadcn/ui, Radix UI, Lucide, React Hook Form, Zod)
- [x] Estrutura de diretórios organizada

---

### S2 - Modelagem de Dados
**Status:** ✅ Concluído  
**Descrição:** Criação do esquema de banco de dados SQLite com todas as tabelas e relacionamentos.

| Referência | Link |
|:----------:|------|
| **Visão** | [V2 - Modelo de Dados](visao.md#5-funcionalidades-do-produto) |
| **Arquitetura** | [A2 - Modelo de Dados ER](arquitetura.md#25-modelo-de-dados-entidade-relacionamento) |

**Entregáveis:**
- [x] Script `01-create-tables.sql` com 7 tabelas (Users, Categories, Events, TicketTypes, Orders, OrderItems, Seats)
- [x] Índices para otimização de consultas
- [x] Constraints de integridade referencial (FOREIGN KEY, CHECK, UNIQUE)
- [x] Script `02-seed-data.sql` com dados iniciais (5 usuários, 8 categorias, 10 eventos, 31 tipos de ingresso, 55 assentos, 3 pedidos)

---

### S3 - Entidades de Domínio
**Status:** ✅ Concluído  
**Descrição:** Implementação das classes de entidade no projeto Core.

| Referência | Link |
|:----------:|------|
| **Visão** | [V2 - Modelo de Dados](visao.md#5-funcionalidades-do-produto) |
| **Arquitetura** | [A2.2.1 - BoraAli.Core](arquitetura.md#221-boraalicode-camada-de-domínio) |

**Entregáveis:**
- [x] `User` - Id, Name, Email, Cpf, PasswordHash, Phone, AvatarUrl, Role, IsActive
- [x] `Category` - Id, Name, Slug, Icon, IsActive
- [x] `Event` - Id, Title, Description, FullDescription, EventDate, Time, Location, Address, City, ImageUrl, IsFeatured, Status, CategoryId, OrganizerId
- [x] `TicketType` - Id, EventId, Name, Price, TotalQuantity, AvailableQuantity, Description, SaleStartDate, SaleEndDate
- [x] `Order` - Id, UserId, EventId, OrderCode, TotalAmount, Status, PaymentMethod, PaymentId
- [x] `OrderItem` - Id, OrderId, TicketTypeId, SeatId, Quantity, UnitPrice, Subtotal
- [x] `Seat` - Id, EventId, TicketTypeId, Row, Number, Section, Price, Status

---

### S4 - Interfaces e Contratos
**Status:** ✅ Concluído  
**Descrição:** Definição das interfaces para repositórios, Unit of Work e executor Dapper.

| Referência | Link |
|:----------:|------|
| **Arquitetura** | [A2.4 - Padrões Arquiteturais](arquitetura.md#24-padrões-arquiteturais) |

**Entregáveis:**
- [x] `IGenericRepository<T>` - CRUD genérico com paginação
- [x] `IEventRepository` - Consultas específicas (featured, by category, by city, search, by organizer, with details)
- [x] `IUnitOfWork` - Gerenciamento de transações e exposição de repositórios
- [x] `IDapperExecutor` - Abstração para queries Dapper (mockável em testes)

---

### S5 - Camada de Infraestrutura
**Status:** ✅ Concluído  
**Descrição:** Implementação dos repositórios, DbSession, UnitOfWork e middleware.

| Referência | Link |
|:----------:|------|
| **Arquitetura** | [A2.2.2 - BoraAli.Infrastructure](arquitetura.md#222-boraaliinfrastructure-camada-de-infraestrutura) |

**Entregáveis:**
- [x] `DbSession` - Gerenciamento de conexão SQLite
- [x] `GenericRepository<T>` - Implementação genérica com Dapper
- [x] `EventRepository` - Consultas especializadas com JOINs
- [x] `UnitOfWork` - Coordenação de transações e repositórios
- [x] `DapperExecutor` - Execução de queries SQL arbitrárias
- [x] `ExceptionMiddleware` - Tratamento global de erros com respostas padronizadas

---

### S6 - API Controllers e Services
**Status:** ✅ Concluído  
**Descrição:** Implementação dos controllers REST e serviços de aplicação.

| Referência | Link |
|:----------:|------|
| **Visão** | [V5.1 - Funcionalidades Implementadas](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.8 - API Endpoints](arquitetura.md#28-api-endpoints) |

**Entregáveis:**
- [x] `EventsController` - CRUD de eventos, busca, filtros, categorias
- [x] `AuthController` - Registro e login com JWT
- [x] `OrdersController` - Criação e consulta de pedidos
- [x] `EventService` - Lógica de negócio de eventos
- [x] `AuthService` - Lógica de autenticação (BCrypt + JWT)
- [x] `OrderService` - Lógica de pedidos com transações
- [x] `AutoMapperProfile` - Mapeamento Entidade → DTO
- [x] Validators FluentValidation (CreateEvent, UpdateEvent, RegisterUser, Login, CreateOrder)

---

### S7 - Frontend: Layout e Navegação
**Status:** ✅ Concluído  
**Descrição:** Estrutura base do frontend com layout responsivo, header, footer e navegação.

| Referência | Link |
|:----------:|------|
| **Visão** | [V3.2 - Experiência Moderna](visao.md#33-benefícios-principais) |
| **Arquitetura** | [A2.3 - Estrutura do Frontend](arquitetura.md#23-estrutura-do-frontend) |

**Entregáveis:**
- [x] Layout raiz com fonte Poppins e metadata
- [x] Header com logo, busca, categorias, menu do usuário
- [x] Footer com links e informações
- [x] Tema claro/escuro com next-themes
- [x] Navegação mobile com Sheet (drawer)
- [x] 40+ componentes shadcn/ui configurados

---

### S8 - Frontend: Home Page
**Status:** ✅ Concluído  
**Descrição:** Página inicial com hero section (carrossel) e grid de eventos com filtros.

| Referência | Link |
|:----------:|------|
| **Visão** | [F2 - Catálogo de Eventos](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.6 - Fluxo de Dados](arquitetura.md#26-fluxo-de-dados---compra-de-ingresso) |

**Entregáveis:**
- [x] `HeroSection` - Carrossel de eventos em destaque com navegação e autoplay
- [x] `EventsGrid` - Grid responsivo (1-4 colunas) com filtros por categoria e cidade
- [x] `EventCard` - Card com imagem, data, local, preço e link para detalhes
- [x] Seção de newsletter
- [x] Integração com API real (fallback silencioso)

---

### S9 - Frontend: Autenticação
**Status:** ✅ Concluído  
**Descrição:** Página de login/cadastro com validação de CPF, força de senha e seleção de perfil.

| Referência | Link |
|:----------:|------|
| **Visão** | [F1 - Autenticação e Cadastro](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.7 - Segurança](arquitetura.md#27-segurança) |

**Entregáveis:**
- [x] Tela de login com e-mail e senha
- [x] Tela de cadastro com nome, CPF (formatado), e-mail, senha (força), confirmação
- [x] Seleção de perfil (Cliente/Organizador) com cards visuais
- [x] `useAuth` hook - gerenciamento de token no localStorage
- [x] `useRequireAuth` hook - proteção de rotas
- [x] Integração com API de autenticação (register + login)
- [x] Toast de feedback (sucesso/erro)

---

### S10 - Frontend: Página do Evento
**Status:** ✅ Concluído  
**Descrição:** Página de detalhes do evento com seletor de ingressos.

| Referência | Link |
|:----------:|------|
| **Visão** | [F3 - Página do Evento](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.6 - Fluxo de Dados](arquitetura.md#26-fluxo-de-dados---compra-de-ingresso) |

**Entregáveis:**
- [x] Banner do evento com gradiente e badge de categoria
- [x] Informações rápidas (data, horário, local)
- [x] Descrição completa com formatação
- [x] Card do organizador com avatar e seguidores
- [x] Placeholder de mapa de localização
- [x] `TicketSelector` - Seleção de tipos/quantidade com cálculo de total e taxa
- [x] Botão "Continuar para Pagamento"

---

### S11 - Frontend: Checkout
**Status:** ✅ Concluído  
**Descrição:** Página de checkout com formulário de pagamento e resumo do pedido.

| Referência | Link |
|:----------:|------|
| **Visão** | [F5 - Checkout](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.6 - Fluxo de Dados](arquitetura.md#26-fluxo-de-dados---compra-de-ingresso) |

**Entregáveis:**
- [x] Formulário de informações pessoais (nome, e-mail, CPF)
- [x] Seleção de método de pagamento (cartão, Pix, boleto)
- [x] Campos de cartão de crédito com formatação (número, validade, CVV)
- [x] Instruções condicionais para Pix e boleto
- [x] Resumo do pedido com itens, subtotal, taxa e total
- [x] Integração com API de pedidos (POST /api/orders)
- [x] Indicador de compra segura

---

### S12 - Frontend: Criação de Eventos
**Status:** ✅ Concluído  
**Descrição:** Página para organizadores criarem eventos com formulário completo.

| Referência | Link |
|:----------:|------|
| **Visão** | [F6 - Criação de Eventos](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.8 - API Endpoints](arquitetura.md#28-api-endpoints) |

**Entregáveis:**
- [x] Formulário dividido em cards (Informações Básicas, Imagem, Data/Local, Ingressos)
- [x] Upload de imagem com preview
- [x] Seleção de categoria via API
- [x] Adição/remoção dinâmica de tipos de ingresso
- [x] Validação de campos obrigatórios
- [x] Proteção de rota (apenas organizadores autenticados)
- [x] Integração com API de criação de eventos

---

## Fase 2: Aprimoramentos 🔄

### S13 - Dashboard do Organizador
**Status:** 📋 Planejado  
**Descrição:** Painel administrativo para organizadores acompanharem vendas e gerenciarem eventos.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP1 - Dashboard do Organizador](visao.md#52-funcionalidades-planejadas) |
| **Arquitetura** | [A1.1 - Recharts](arquitetura.md#11-frontend) |

**Entregáveis Planejados:**
- [ ] Gráficos de vendas (Recharts)
- [ ] Tabela de ingressos vendidos por evento
- [ ] Faturamento total e por período
- [ ] Gerenciamento de eventos (editar, cancelar, finalizar)
- [ ] Exportação de relatórios

---

### S14 - Notificações por E-mail
**Status:** 📋 Planejado  
**Descrição:** Envio de e-mails transacionais usando MailKit.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP2 - Notificações por E-mail](visao.md#52-funcionalidades-planejadas) |
| **Arquitetura** | [A1.2 - MailKit](arquitetura.md#12-backend) |

**Entregáveis Planejados:**
- [ ] E-mail de confirmação de cadastro
- [ ] E-mail de confirmação de compra com QR Code (QRCoder)
- [ ] E-mail de lembrete de evento (24h antes)
- [ ] Templates de e-mail responsivos

---

### S15 - Mapa de Assentos Interativo
**Status:** 📋 Planejado  
**Descrição:** Interface visual para seleção de assentos no mapa do evento.

| Referência | Link |
|:----------:|------|
| **Visão** | [F7 - Mapa de Assentos](visao.md#51-funcionalidades-implementadas) |
| **Arquitetura** | [A2.5 - Seats](arquitetura.md#25-modelo-de-dados-entidade-relacionamento) |

**Entregáveis Planejados:**
- [ ] Componente visual de mapa de assentos
- [ ] Cores por status (disponível, reservado, vendido)
- [ ] Seleção de múltiplos assentos
- [ ] Integração com TicketSelector

---

### S16 - Upload de Imagens com Preview
**Status:** 📋 Planejado  
**Descrição:** Sistema completo de upload com redimensionamento e cache.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP3 - Upload de Imagens](visao.md#52-funcionalidades-planejadas) |
| **Arquitetura** | [A2.8 - Upload Endpoint](arquitetura.md#28-api-endpoints) |

**Entregáveis Planejados:**
- [ ] Upload com drag & drop
- [ ] Redimensionamento automático
- [ ] Preview antes do envio
- [ ] Limpeza de imagens não utilizadas

---

## Fase 3: Expansão 📋

### S17 - Avaliações e Comentários
**Status:** 📋 Planejado  
**Descrição:** Sistema de avaliação de eventos pós-realização.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP4 - Avaliações e Comentários](visao.md#52-funcionalidades-planejadas) |

**Entregáveis Planejados:**
- [ ] Avaliação por estrelas (1-5)
- [ ] Comentários textuais
- [ ] Moderação de conteúdo
- [ ] Exibição na página do evento

---

### S18 - Wishlist e Favoritos
**Status:** 📋 Planejado  
**Descrição:** Participantes salvam eventos para acompanhar.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP5 - Wishlist/Favoritos](visao.md#52-funcionalidades-planejadas) |

**Entregáveis Planejados:**
- [ ] Botão de favoritar na página do evento e no card
- [ ] Página de favoritos do usuário
- [ ] Notificação de mudança de preço/disponibilidade

---

### S19 - App Mobile
**Status:** 📋 Planejado  
**Descrição:** Versão mobile nativa do BoraAli.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP6 - App Mobile](visao.md#52-funcionalidades-planejadas) |

**Entregáveis Planejados:**
- [ ] React Native ou Flutter
- [ ] Push notifications
- [ ] Offline mode
- [ ] Deep links

---

### S20 - Integração com Redes Sociais
**Status:** 📋 Planejado  
**Descrição:** Login social e compartilhamento de eventos.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP7 - Integração com Redes Sociais](visao.md#52-funcionalidades-planejadas) |

**Entregáveis Planejados:**
- [ ] Login com Google, Facebook, Apple
- [ ] Compartilhamento de eventos (WhatsApp, Instagram, Twitter)
- [ ] Open Graph tags para preview em redes sociais

---

### S21 - Relatórios Avançados
**Status:** 📋 Planejado  
**Descrição:** Exportação de dados e relatórios gerenciais.

| Referência | Link |
|:----------:|------|
| **Visão** | [FP8 - Relatórios Avançados](visao.md#52-funcionalidades-planejadas) |

**Entregáveis Planejados:**
- [ ] Exportação CSV/Excel
- [ ] Relatórios por período
- [ ] Dashboard administrativo global
- [ ] Métricas de conversão

---

## Resumo do Roadmap

| Fase | Specs | Status |
|:----:|:-----:|:------:|
| **Fase 1: Fundação (MVP)** | S1 a S12 | ✅ 12/12 concluídas |
| **Fase 2: Aprimoramentos** | S13 a S16 | 🔄 0/4 concluídas |
| **Fase 3: Expansão** | S17 a S21 | 📋 0/5 concluídas |
| **Total** | **21 specs** | **12 concluídas (57%)** |

---

## Apontamentos para Documentos

### Mapeamento Spec → Visão

| Spec | Seção da Visão | Valor Entregue |
|:----:|:--------------:|----------------|
| S1 | [V6 - Tecnologias](visao.md#6-tecnologias-utilizadas) | Infraestrutura moderna e escalável |
| S2 | [V5 - Funcionalidades](visao.md#5-funcionalidades-do-produto) | Base de dados relacional completa |
| S3 | [V5 - Funcionalidades](visao.md#5-funcionalidades-do-produto) | Modelagem de domínio rica |
| S4 | [V3.1 - Integração Completa](visao.md#33-benefícios-principais) | Padrões de design robustos |
| S5 | [V3.4 - Segurança](visao.md#33-benefícios-principais) | Infraestrutura confiável |
| S6 | [V5.1 - Funcionalidades](visao.md#51-funcionalidades-implementadas) | API completa e documentada |
| S7 | [V3.2 - Experiência Moderna](visao.md#33-benefícios-principais) | Interface responsiva e acessível |
| S8 | [F2 - Catálogo de Eventos](visao.md#51-funcionalidades-implementadas) | Descoberta facilitada de eventos |
| S9 | [F1 - Autenticação](visao.md#51-funcionalidades-implementadas) | Acesso seguro com perfis |
| S10 | [F3/F4 - Evento + Ingressos](visao.md#51-funcionalidades-implementadas) | Informação completa + seleção |
| S11 | [F5 - Checkout](visao.md#51-funcionalidades-implementadas) | Compra segura e intuitiva |
| S12 | [F6 - Criação de Eventos](visao.md#51-funcionalidades-implementadas) | Autonomia para organizadores |

### Mapeamento Spec → Arquitetura

| Spec | Seção da Arquitetura | Componente Técnico |
|:----:|:--------------------:|--------------------|
| S1 | [A1 - Stack](arquitetura.md#11-frontend) | Next.js + .NET 8 + SQLite |
| S2 | [A2.5 - Modelo ER](arquitetura.md#25-modelo-de-dados-entidade-relacionamento) | 7 tabelas relacionadas |
| S3 | [A2.2.1 - Core](arquitetura.md#221-boraalicode-camada-de-domínio) | Entidades de domínio |
| S4 | [A2.4 - Padrões](arquitetura.md#24-padrões-arquiteturais) | Repository + Unit of Work |
| S5 | [A2.2.2 - Infrastructure](arquitetura.md#222-boraaliinfrastructure-camada-de-infraestrutura) | Dapper + SQLite + Middleware |
| S6 | [A2.8 - Endpoints](arquitetura.md#28-api-endpoints) | Controllers REST |
| S7 | [A2.3 - Frontend](arquitetura.md#23-estrutura-do-frontend) | App Router + shadcn/ui |
| S8 | [A2.3 - Componentes](arquitetura.md#23-estrutura-do-frontend) | HeroSection + EventsGrid |
| S9 | [A2.7 - Segurança](arquitetura.md#27-segurança) | JWT + BCrypt |
| S10 | [A2.6 - Fluxo](arquitetura.md#26-fluxo-de-dados---compra-de-ingresso) | TicketSelector + API |
| S11 | [A2.6 - Fluxo](arquitetura.md#26-fluxo-de-dados---compra-de-ingresso) | CheckoutForm + POST /api/orders |
| S12 | [A2.8 - Endpoints](arquitetura.md#28-api-endpoints) | POST /api/events |
