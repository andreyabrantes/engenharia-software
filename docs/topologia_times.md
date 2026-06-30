# Topologia de Times — BoraAli

## Modelo: Team Topologies (Matthew Skelton e Manuel Pais)

O ecossistema BoraAli é organizado em 4 tipos de times fundamentais, cada um com responsabilidades, limites e modos de interação bem definidos.

---

### 1. Stream-Aligned Team: Squad de Ingressos e Eventos

**Responsabilidade:** Dono completo do fluxo de valor "Descoberta e Compra de Ingressos". Este time é responsável por todas as features visíveis ao usuário final: listagem de eventos, busca, detalhes do evento, carrinho de compras, checkout, geração de QR Code e histórico de pedidos.

**Foco cognitivo:** Regras de negócio do domínio de eventos (publicação, validação de dados, tipos de ingresso, categorias, cupons de desconto) e experiência do comprador (UX de compra, e-mails transacionais, ingressos).

**Interfaces:** Consome APIs do time de Plataforma para autenticação e storage de imagens. Reporta métricas de negócio (taxa de conversão, ingressos vendidos) para o time de Platform.

**Stack:** ASP.NET Core 8 (Controllers), Dapper, SQLite, FluentValidation, BCrypt.Net.

---

### 2. Platform Team: Plataforma de Infraestrutura e DevOps

**Responsabilidade:** Fornecer e manter a plataforma interna de desenvolvimento que acelera os times Stream-Aligned. Isso inclui pipelines de CI/CD, ambiente de staging/produção, monitoramento, logging centralizado (Serilog + Elasticsearch), gestão de secrets (Azure Key Vault / GitHub Secrets) e health checks.

**Foco cognitivo:** Infraestrutura como código (IaC), orquestração de containers (Docker/Kubernetes), observabilidade (logs, métricas, tracing), segurança de rede (firewall, CORS, HTTPS, WAF).

**Interfaces:** Expõe APIs de plataforma interna: sistema de logging centralizado, serviço de feature toggles, painel de error budget. Serviço ao time Stream-Aligned via modelo X-as-a-Service.

**Stack:** Docker, GitHub Actions, Terraform, Serilog + Elasticsearch + Kibana, Prometheus + Grafana, Azure/AWS.

---

### 3. Enabling Team: Qualidade e Engenharia de Software

**Responsabilidade:** Capacitar os times Stream-Aligned a entregarem com qualidade e velocidade. Este time não entrega features diretamente, mas ajuda a remover impedimentos técnicos: introduz práticas de TDD, revisa arquitetura, define padrões de código (AAA para testes, convenção de nomes), e conduz root cause analysis pós-incidentes.

**Foco cognitivo:** Padrões de arquitetura (Repository, Unit of Work, CQRS), qualidade de código (SonarQube, análise estática), estratégias de teste (pirâmide de testes, testes de integração, smoke tests), documentação de ADRs.

**Interfaces:** Colaboração temporária com times Stream-Aligned (modelo de Facilitation). Não possui serviços próprios expostos. Entrega bibliotecas internas, templates de projeto e documentação.

**Stack:** SonarQube, xUnit + Moq + FluentAssertions, Markdown para ADRs, draw.io para diagramas C4.

---

### 4. Complicated-Subsystem Team: Segurança e Conformidade

**Responsabilidade:** Gerenciar subsistemas de alta complexidade que exigem conhecimento especializado além da capacidade dos times Stream-Aligned: criptografia (JWT, BCrypt, TLS), conformidade LGPD (anonimização de dados, política de retenção, relatórios de dados), proteção contra fraudes em pagamentos e modelagem de ameaças (STRIDE).

**Foco cognitivo:** Criptografia aplicada (assinatura JWT RS256, hashing BCrypt, encriptação AES de dados sensíveis), conformidade regulatória (LGPD art. 7º, 18º), análise de vulnerabilidades (OWASP Top 10, CVEs), PCI-DSS para pagamentos.

**Interfaces:** Expõe bibliotecas de segurança (ex: `BoraAli.Security` NuGet package com validadores de token, middleware de criptografia). Audita os times Stream-Aligned trimestralmente para conformidade. Modelo de colaboração: X-as-a-Service para ferramentas de scan, Colaboração para threat modeling.

**Stack:** OWASP ZAP, Burp Suite, `git-secrets`, `detect-secrets`, Vault (Hashicorp), ferramentas de SAST/DAST.
