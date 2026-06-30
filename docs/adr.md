# Architecture Decision Records (ADR) - BoraAli

## Introdução

Este documento registra as principais decisões arquiteturais tomadas durante o desenvolvimento do **BoraAli**. Cada ADR documenta o contexto, a decisão, as alternativas consideradas e as consequências.

---

## ADR-001: Uso de SQLite como Banco de Dados

### Status
✅ **Aceito**

### Contexto
Precisávamos de um banco de dados relacional para o MVP que fosse simples de configurar, não exigisse servidor externo e permitisse desenvolvimento rápido sem infraestrutura complexa.

### Decisão
Utilizar **SQLite** como banco de dados principal, com o pacote `Microsoft.Data.Sqlite`.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **PostgreSQL** | Exigiria servidor externo, configuração de Docker ou serviço cloud, aumentando a complexidade do setup |
| **SQL Server** | Mesmo problema do PostgreSQL, além de ser mais pesado para desenvolvimento local |
| **MySQL** | Similar aos anteriores, com configuração adicional |

### Consequências
- ✅ Setup zero: apenas um arquivo `.db` é criado automaticamente
- ✅ Portabilidade: o banco viaja com o projeto
- ✅ Performance excelente para o volume esperado (centenas a milhares de eventos)
- ❌ Limitação de concorrência (escrita simultânea) - aceitável para MVP
- ❌ Sem recursos avançados como stored procedures ou replicação

---

## ADR-002: Dapper como ORM (em vez de Entity Framework)

### Status
✅ **Aceito**

### Contexto
Precisávamos de um ORM leve e performático para consultas SQL. Entity Framework Core é o padrão no ecossistema .NET, mas adiciona overhead significativo.

### Decisão
Utilizar **Dapper** (micro-ORM) para acesso a dados, combinado com o padrão Repository.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Entity Framework Core** | Overhead de performance, mais complexidade de configuração, migrations pesadas |
| **ADO.NET puro** | Muito trabalho manual para mapeamento objeto-relacional |
| **NHibernate** | Complexidade excessiva para o escopo do projeto |

### Consequências
- ✅ Performance superior (Dapper é um dos ORMs mais rápidos do .NET)
- ✅ Controle total sobre as queries SQL
- ✅ Facilidade para consultas complexas com JOINs
- ❌ Mais trabalho manual para escrever SQL
- ❌ Sem migrations automáticas (scripts SQL manuais)

---

## ADR-003: Arquitetura de 3 Camadas (Clean Architecture)

### Status
✅ **Aceito**

### Contexto
Precisávamos de uma arquitetura que separasse responsabilidades, facilitasse testes e permitisse evolução independente das camadas.

### Decisão
Adotar **arquitetura em 3 camadas** com separação em projetos:
- `BoraAli.Core` - Domínio e interfaces
- `BoraAli.Infrastructure` - Implementações técnicas
- `BoraAli.Api` - Apresentação (API)

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Monólito em projeto único** | Viola o princípio da separação de responsabilidades, dificulta testes |
| **Microservices** | Complexidade excessiva para o porte do projeto |
| **Vertical Slices** | Boa alternativa, mas menos familiar para a equipe |

### Consequências
- ✅ Separação clara de responsabilidades
- ✅ Facilidade para testes unitários (interfaces podem ser mockadas)
- ✅ Possibilidade de substituir implementações (ex: trocar SQLite por PostgreSQL)
- ❌ Maior número de projetos e arquivos para gerenciar

---

## ADR-004: Next.js com App Router (em vez de Pages Router)

### Status
✅ **Aceito**

### Contexto
O frontend precisava de um framework React moderno com suporte a Server Components, roteamento baseado em arquivos e boa integração com o ecossistema React 19.

### Decisão
Utilizar **Next.js 16 com App Router** (diretório `app/`).

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Next.js Pages Router** | Legado, sem suporte a Server Components |
| **Vite + React** | Sem SSR/SSG nativo, exigiria configuração adicional |
| **Remix** | Curva de aprendizado maior, ecossistema menor |

### Consequências
- ✅ Server Components para páginas estáticas (performance)
- ✅ Client Components para páginas interativas (evento, checkout)
- ✅ Roteamento baseado em arquivos (produtivo)
- ✅ Suporte nativo a React 19
- ❌ App Router ainda tem algumas limitações de maturidade

---

## ADR-005: shadcn/ui como Design System

### Status
✅ **Aceito**

### Contexto
Precisávamos de componentes de UI acessíveis, customizáveis e com boa aparência sem reinventar a roda.

### Decisão
Utilizar **shadcn/ui** (baseado em Radix UI + Tailwind CSS) como design system.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Material UI** | Pesado, difícil de customizar, estilo "Material" engessado |
| **Ant Design** | Estilo muito característico, difícil de adaptar |
| **Chakra UI** | Bom, mas menos integrado com Tailwind |
| **Componentes próprios** | Tempo de desenvolvimento muito maior |

### Consequências
- ✅ Componentes acessíveis (Radix UI cuida da acessibilidade)
- ✅ Código-fonte dos componentes no projeto (customização total)
- ✅ Estilização consistente com Tailwind CSS
- ✅ 40+ componentes prontos (button, card, dialog, dropdown, etc.)
- ❌ Dependência de múltiplos pacotes Radix UI

---

## ADR-006: Autenticação JWT com BCrypt

### Status
✅ **Aceito**

### Contexto
Precisávamos de um sistema de autenticação seguro, stateless e compatível com APIs REST.

### Decisão
Implementar autenticação via **JWT (JSON Web Tokens)** com senhas hash usando **BCrypt** (11 rounds de salt).

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **ASP.NET Core Identity** | Muito acoplado ao EF Core, pesado para o escopo |
| **Auth0 / Firebase Auth** | Dependência externa, custo, lock-in |
| **Session-based auth** | Stateful, não escala bem para APIs |

### Consequências
- ✅ Stateless (escalável horizontalmente)
- ✅ Seguro (BCrypt + JWT assinado)
- ✅ Controle total sobre claims e roles
- ❌ Gerenciamento manual de refresh tokens (planejado para futura versão)
- ❌ Token armazenado no localStorage (vulnerável a XSS - mitigado por boas práticas)

---

## ADR-007: Rate Limiting para Proteção da API

### Status
✅ **Aceito**

### Contexto
A API precisava de proteção contra abusos, ataques de força bruta e sobrecarga.

### Decisão
Implementar **Rate Limiting** com políticas de janela fixa usando o middleware nativo do .NET 8.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Cloudflare / API Gateway** | Dependência externa, custo adicional |
| **Middleware customizado** | Mais trabalho, menos confiável |
| **Sem rate limiting** | Inaceitável para produção |

### Consequências
- ✅ Proteção contra ataques de força bruta (5 tentativas/min no login)
- ✅ Prevenção de sobrecarga (100 req/min global)
- ✅ Configuração granular por endpoint
- ❌ Pode afetar usuários legítimos em casos extremos (mitigado com limites generosos)

---

## ADR-008: Serilog para Logging Estruturado

### Status
✅ **Aceito**

### Contexto
Precisávamos de um sistema de logging que permitisse rastrear erros, monitorar a aplicação e facilitar debugging.

### Decisão
Utilizar **Serilog** com sinks de console e arquivo com rotação diária.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **NLog** | Similar, mas Serilog tem melhor integração com .NET 8 |
| **log4net** | Legado, menos flexível |
| **Console.WriteLine** | Inaceitável para produção |

### Consequências
- ✅ Logging estruturado (JSON) para análise
- ✅ Rotação diária de arquivos (sem acumular logs)
- ✅ Configuração declarativa no appsettings.json
- ✅ Múltiplos sinks (console + arquivo)
- ❌ Sem sink cloud (planejado: Application Insights ou Seq)

---

## ADR-009: FluentValidation para Validação

### Status
✅ **Aceito**

### Contexto
Precisávamos de validação declarativa e reutilizável para as requisições da API.

### Decisão
Utilizar **FluentValidation** com validação automática via `AddFluentValidationAutoValidation()`.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Data Annotations** | Limitado, mistura validação com modelo |
| **Validação manual** | Repetitivo, propenso a erros |
| **Sem validação** | Inaceitável |

### Consequências
- ✅ Validação declarativa e fluente
- ✅ Separação da lógica de validação dos modelos
- ✅ Validação automática no pipeline do ASP.NET Core
- ✅ Mensagens de erro customizadas em português
- ❌ Mais classes para gerenciar

---

## ADR-010: AutoMapper para Mapeamento DTO ↔ Entidade

### Status
✅ **Aceito**

### Contexto
Precisávamos mapear entidades de domínio para DTOs de resposta da API sem código repetitivo.

### Decisão
Utilizar **AutoMapper** com perfil de mapeamento centralizado.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **Mapeamento manual** | Repetitivo, propenso a erros em propriedades esquecidas |
| **Mapster** | Menos conhecido, ecossistema menor |
| **Implicit operators** | Viola princípios de design |

### Consequências
- ✅ Mapeamento centralizado e reutilizável
- ✅ Redução de código boilerplate
- ✅ Facilidade para evoluir os DTOs
- ❌ Performance overhead (aceitável para o volume)
- ❌ Complexidade adicional em mapeamentos não triviais

---

## ADR-011: Tailwind CSS 4 para Estilização

### Status
✅ **Aceito**

### Contexto
Precisávamos de uma abordagem de estilização rápida, consistente e responsiva.

### Decisão
Utilizar **Tailwind CSS 4** com PostCSS.

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **CSS Modules** | Mais verboso, sem design system integrado |
| **Styled Components** | Overhead de runtime, bundle maior |
| **Sass/SCSS** | Sem utility-first, mais arquivos |

### Consequências
- ✅ Desenvolvimento rápido com classes utilitárias
- ✅ Bundle pequeno (purge de CSS não utilizado)
- ✅ Design responsivo facilitado
- ✅ Integração nativa com shadcn/ui
- ❌ HTML pode ficar verboso com muitas classes

---

## ADR-012: Uso de Dados Mockados como Fallback

### Status
✅ **Aceito**

### Contexto
Durante o desenvolvimento, a API pode não estar disponível. Precisávamos de uma forma de desenvolver e testar o frontend independentemente.

### Decisão
Manter dados mockados em [`lib/mock-data.ts`](lib/mock-data.ts) como fallback, com transição gradual para API real via [`lib/api-types.ts`](lib/api-types.ts).

### Alternativas Consideradas
| Alternativa | Motivo da Rejeição |
|-------------|-------------------|
| **MSW (Mock Service Worker)** | Mais complexo, overhead de configuração |
| **JSON Server** | Dependência externa, mais um serviço para rodar |
| **Sem mock, apenas API** | Impede desenvolvimento frontend independente |

### Consequências
- ✅ Desenvolvimento frontend independente do backend
- ✅ Facilidade para testes e demonstrações
- ✅ Transição gradual para API real
- ❌ Código legado de mock precisa ser mantido
- ❌ Possível divergência entre mock e API real
