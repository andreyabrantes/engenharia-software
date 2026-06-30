# Especificação de Requisitos Não-Funcionais - BoraAli

## Introdução

Este documento especifica os requisitos não-funcionais (RNFs) do sistema **BoraAli**. Diferentemente dos requisitos funcionais (que descrevem **o que** o sistema faz), os requisitos não-funcionais descrevem **como** o sistema se comporta em termos de performance, segurança, usabilidade, disponibilidade e manutenibilidade.

---

## 1. Performance

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-01 | Tempo de carregamento da página inicial | < 2 segundos (3G simulado) | Alta |
| RNF-02 | Tempo de resposta da API (p95) | < 500ms para 95% das requisições | Alta |
| RNF-03 | Tempo de resposta da API de login | < 1 segundo (incluindo hash BCrypt) | Alta |
| RNF-04 | Tempo de carregamento de página de evento | < 1.5 segundos | Média |
| RNF-05 | Tempo de processamento de pedido | < 3 segundos (incluindo transação) | Alta |
| RNF-06 | Tamanho do bundle JavaScript inicial | < 200KB (gzip) | Média |
| RNF-07 | Número de requisições simultâneas suportadas | > 100 usuários concorrentes | Média |
| RNF-08 | Tempo de resposta para consultas com filtro | < 800ms | Média |

### Estratégias de Performance

| Estratégia | Onde se aplica |
|------------|----------------|
| **Server Components (Next.js)** | Páginas estáticas (home, specs) — renderização no servidor |
| **Client Components** | Páginas interativas (evento, checkout, criar evento) |
| **Dapper (micro-ORM)** | Consultas SQL otimizadas sem overhead de ORM completo |
| **Índices SQL** | Colunas de busca (City, CategoryId, Status, EventDate) |
| **Rate Limiting** | Prevenção de sobrecarga (100 req/min global) |
| **Lazy loading de imagens** | Cards de eventos na página inicial |
| **Paginação** | Listagem de eventos com offset/limit |

---

## 2. Segurança

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-09 | Senhas armazenadas com hash | BCrypt com 11 rounds de salt | Crítica |
| RNF-10 | Autenticação via tokens | JWT com assinatura HMAC-SHA256 | Crítica |
| RNF-11 | Expiração de token | 8 horas (configurável) | Alta |
| RNF-12 | Proteção contra força bruta | Rate limiting: 5 tentativas/min no login | Alta |
| RNF-13 | Controle de acesso por perfil | Role-based (Admin, Cliente, Organizador) | Crítica |
| RNF-14 | Validação de entrada | Frontend (Zod) + Backend (FluentValidation) | Crítica |
| RNF-15 | CORS restrito | Apenas origens configuradas (localhost:3000, etc.) | Alta |
| RNF-16 | Proteção contra XSS | React com escape automático de HTML | Alta |
| RNF-17 | Proteção contra SQL Injection | Dapper com parâmetros tipados | Crítica |
| RNF-18 | Headers de segurança | Content-Security-Policy, X-Content-Type-Options | Média |

### Políticas de Autorização

| Perfil | Acesso |
|--------|--------|
| **Admin** | Gerenciamento de usuários, moderação de conteúdo, acesso total |
| **Organizador** | Criar/editar/gerenciar próprios eventos, upload de imagens |
| **Cliente** | Comprar ingressos, visualizar próprios pedidos |
| **Não autenticado** | Navegar eventos, visualizar detalhes, registrar-se |

---

## 3. Usabilidade

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-19 | Design responsivo | Funcional em dispositivos mobile (320px+) e desktop | Alta |
| RNF-20 | Suporte a tema claro/escuro | Alternância via next-themes | Média |
| RNF-21 | Feedback visual para ações | Toast notifications (Sonner) para sucesso/erro | Alta |
| RNF-22 | Formatação automática de campos | CPF, telefone, cartão de crédito, validade | Média |
| RNF-23 | Indicador de força de senha | Barra visual com níveis (fraca, média, forte) | Média |
| RNF-24 | Navegação por teclado | Componentes Radix UI com acessibilidade WAI-ARIA | Média |
| RNF-25 | Mensagens de erro em português | FluentValidation com mensagens customizadas | Alta |
| RNF-26 | Tempo máximo para tarefa do usuário | Criar evento < 5 minutos | Média |

### Compatibilidade de Navegadores

| Navegador | Versão Mínima |
|-----------|:-------------:|
| Google Chrome | 120+ |
| Mozilla Firefox | 115+ |
| Microsoft Edge | 120+ |
| Safari | 17+ |
| Opera | 100+ |

---

## 4. Disponibilidade e Confiabilidade

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-27 | Disponibilidade da API | 99.5% (exceto manutenção programada) | Alta |
| RNF-28 | Tratamento de erros global | ExceptionMiddleware com respostas padronizadas | Crítica |
| RNF-29 | Fallback de dados mockados | Frontend funcional mesmo sem API | Alta |
| RNF-30 | Logging de erros | Serilog com rotação diária de arquivos | Alta |
| RNF-31 | Health check | Endpoint GET /health | Média |
| RNF-32 | Recuperação de falhas | Transações com rollback automático em caso de erro | Crítica |

### Estrutura de Resposta de Erro (API)

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "Um ou mais campos inválidos.",
  "errors": {
    "Title": ["O título é obrigatório"],
    "EventDate": ["A data do evento deve ser futura"]
  }
}
```

---

## 5. Manutenibilidade

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-33 | Separação em camadas | 3 projetos .NET (Core, Infrastructure, Api) | Alta |
| RNF-34 | Código tipado | TypeScript no frontend, C# no backend | Alta |
| RNF-35 | Testes unitários | Cobertura mínima de 30% (serviços) | Média |
| RNF-36 | Documentação de API | Swagger/OpenAPI disponível em /swagger | Alta |
| RNF-37 | Logging estruturado | Serilog com formato JSON para análise | Média |
| RNF-38 | Configuração externalizada | appsettings.json para strings de conexão, JWT, CORS | Alta |

### Estrutura de Diretórios

```
backend/
├── BoraAli.Core/              # Domínio (entidades, interfaces, exceções)
├── BoraAli.Infrastructure/    # Infraestrutura (repositórios, DbSession, middleware)
├── BoraAli.Api/               # Apresentação (controllers, services, DTOs)
└── BoraAli.Tests/             # Testes unitários

frontend/
├── app/                       # Next.js App Router (páginas)
├── components/                # Componentes React (shadcn/ui + custom)
├── hooks/                     # Custom hooks (useAuth, useToast)
└── lib/                       # Utilitários e tipos (api-types, mock-data)
```

---

## 6. Portabilidade

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-39 | Backend cross-platform | .NET 8 Runtime (Windows, Linux, macOS) | Alta |
| RNF-40 | Banco de dados portátil | SQLite (arquivo único .db) | Alta |
| RNF-41 | Frontend sem dependência de servidor | Next.js static export ou Vercel deploy | Alta |
| RNF-42 | Containerização | Dockerfile (planejado) | Baixa |

---

## 7. Escalabilidade

| ID | Requisito | Métrica | Prioridade |
|:--:|-----------|---------|:----------:|
| RNF-43 | Escalabilidade horizontal | API stateless (JWT) permite múltiplas instâncias | Média |
| RNF-44 | Cache de consultas frequentes | Redis ou MemoryCache (planejado) | Baixa |
| RNF-45 | Migração de banco | Troca de SQLite para PostgreSQL via Dapper | Média |

---

## 8. Restrições Técnicas

| ID | Restrição | Descrição |
|:--:|-----------|-----------|
| RT-01 | Framework frontend | Next.js 16 (App Router) — obrigatório para o projeto |
| RT-02 | Framework backend | .NET 8 (C# 12) — obrigatório para o projeto |
| RT-03 | Banco de dados | SQLite — definido para o MVP |
| RT-04 | ORM | Dapper (micro-ORM) — definido na arquitetura |
| RT-05 | Design system | shadcn/ui + Tailwind CSS 4 |
| RT-06 | Autenticação | JWT + BCrypt — sem provedor externo |
| RT-07 | Hospedagem frontend | Vercel (otimizado para Next.js) |
| RT-08 | Sistema operacional | Windows 11 (desenvolvimento), qualquer SO com .NET 8 (produção) |

---

## Matriz de Rastreabilidade RNF → ADR

| RNF | ADR Relacionado | Decisão |
|:---:|:---------------:|---------|
| RNF-01 a RNF-08 | [ADR-002](adr.md#adr-002-dapper-como-orm-em-vez-de-entity-framework), [ADR-004](adr.md#adr-004-nextjs-com-app-router-em-vez-de-pages-router), [ADR-011](adr.md#adr-011-tailwind-css-4-para-estilização) | Dapper, Next.js App Router, Tailwind CSS |
| RNF-09 a RNF-18 | [ADR-006](adr.md#adr-006-autenticação-jwt-com-bcrypt), [ADR-007](adr.md#adr-007-rate-limiting-para-proteção-da-api), [ADR-009](adr.md#adr-009-fluentvalidation-para-validação) | JWT+BCrypt, Rate Limiting, FluentValidation |
| RNF-19 a RNF-26 | [ADR-005](adr.md#adr-005-shadcnui-como-design-system), [ADR-011](adr.md#adr-011-tailwind-css-4-para-estilização) | shadcn/ui, Tailwind CSS |
| RNF-27 a RNF-32 | [ADR-003](adr.md#adr-003-arquitetura-de-3-camadas-clean-architecture), [ADR-008](adr.md#adr-008-serilog-para-logging-estruturado) | 3 camadas, Serilog |
| RNF-33 a RNF-38 | [ADR-003](adr.md#adr-003-arquitetura-de-3-camadas-clean-architecture), [ADR-010](adr.md#adr-010-automapper-para-mapeamento-dto--entidade) | 3 camadas, AutoMapper |
| RNF-39 a RNF-42 | [ADR-001](adr.md#adr-001-uso-de-sqlite-como-banco-de-dados) | SQLite |
| RNF-43 a RNF-45 | [ADR-001](adr.md#adr-001-uso-de-sqlite-como-banco-de-dados), [ADR-002](adr.md#adr-002-dapper-como-orm-em-vez-de-entity-framework) | SQLite, Dapper |