# Documento de Visão - BoraAli

## 1. Introdução

### 1.1 Propósito do Documento
Este documento apresenta a visão geral do produto **BoraAli**, uma plataforma digital para descoberta, criação e venda de ingressos para eventos. Ele descreve o problema que o produto resolve, o público-alvo, as funcionalidades principais e o valor entregue aos usuários.

### 1.2 Escopo
O BoraAli é uma plataforma web completa que conecta organizadores de eventos a participantes, oferecendo ferramentas para:
- **Criação e gerenciamento de eventos** por organizadores
- **Descoberta de eventos** por categorias, cidades e busca textual
- **Compra de ingressos** com seleção de tipos e assentos
- **Processamento de pagamentos** via cartão de crédito, Pix e boleto
- **Autenticação e controle de acesso** com perfis de usuário (Cliente, Organizador, Admin)

---

## 2. Problema e Oportunidade

### 2.1 Problema
Atualmente, organizar eventos e vender ingressos envolve múltiplas ferramentas desconectadas: divulgação em redes sociais, venda de ingressos em plataformas separadas, controle manual de capacidade e pagamentos fragmentados. Para o público, encontrar eventos relevantes próximos a si exige consultar várias fontes diferentes.

### 2.2 Oportunidade
Existe uma demanda crescente por plataformas integradas que simplifiquem tanto a vida do organizador quanto do participante. O mercado brasileiro de eventos movimenta bilhões de reais anualmente, e uma solução moderna, acessível e intuitiva pode capturar uma fatia significativa desse mercado.

---

## 3. Descrição do Produto

### 3.1 O que é o BoraAli?
O **BoraAli** é uma plataforma web de ponta a ponta para o ecossistema de eventos. Ela permite que organizadores criem, publiquem e gerenciem eventos com venda de ingressos, enquanto participantes descobrem eventos filtrando por categoria, cidade ou termo de busca, selecionam seus ingressos e realizam a compra de forma segura.

### 3.2 Valor Entregue

| Para | Valor |
|------|-------|
| **Organizadores** | Ferramenta completa para criar eventos, configurar tipos de ingressos (lotes, preços), gerenciar mapa de assentos, publicar e acompanhar vendas em tempo real |
| **Participantes (Clientes)** | Descoberta facilitada de eventos com busca e filtros, seleção intuitiva de ingressos, checkout seguro com múltiplas formas de pagamento (cartão, Pix, boleto) |
| **Administradores** | Visão geral da plataforma, gerenciamento de usuários e moderação de conteúdo |

### 3.3 Benefícios Principais

1. **Integração Completa** - Uma única plataforma para criar, divulgar e vender ingressos
2. **Experiência Moderna** - Interface responsiva e intuitiva construída com Next.js e Tailwind CSS
3. **Segurança** - Autenticação JWT, senhas hash com BCrypt, validação de dados com FluentValidation
4. **Mapa de Assentos** - Seleção visual de assentos para eventos com lugares marcados
5. **Múltiplos Pagamentos** - Suporte a cartão de crédito, Pix e boleto bancário
6. **Performance** - Arquitetura otimizada com Dapper para consultas rápidas ao banco SQLite

---

## 4. Público-Alvo

### 4.1 Personas

| Persona | Perfil | Necessidades |
|---------|--------|-------------|
| **Carlos (Organizador)** | Produtor de eventos, 35-50 anos, precisa de ferramenta prática para vender ingressos | Criar eventos rapidamente, configurar lotes de ingressos, gerenciar vendas |
| **Ana (Cliente)** | Profissional 25-40 anos, busca eventos culturais e de entretenimento | Descobrir eventos próximos, comparar preços, comprar com segurança |
| **João (Admin)** | Administrador da plataforma | Moderar conteúdo, gerenciar usuários, acompanhar métricas |

### 4.2 Mercado-Alvo
- Organizadores de eventos independentes e produtoras de médio porte
- Público geral interessado em shows, festivais, teatro, esportes, gastronomia, tecnologia e workshops
- Regiões metropolitanas brasileiras (São Paulo, Rio de Janeiro, Belo Horizonte, Curitiba, etc.)

---

## 5. Funcionalidades do Produto

### 5.1 Funcionalidades Implementadas

| ID | Funcionalidade | Descrição | Visão Relacionada |
|:--:|---------------|-----------|:-----------------:|
| F1 | Autenticação e Cadastro | Registro e login com validação de CPF, força de senha, seleção de perfil (Cliente/Organizador) | Segurança |
| F2 | Catálogo de Eventos | Listagem com grid responsivo, filtros por categoria e cidade, busca textual | Descoberta |
| F3 | Página do Evento | Detalhes completos: data, local, descrição, organizador, mapa de localização | Informação |
| F4 | Seletor de Ingressos | Escolha de tipos/lotes com controle de quantidade, cálculo automático de total e taxas | Compra |
| F5 | Checkout | Formulário de pagamento com cartão, Pix e boleto, resumo do pedido | Transação |
| F6 | Criação de Eventos | Formulário completo para organizadores: informações, imagem, data/local, tipos de ingresso | Criação |
| F7 | Mapa de Assentos | Assentos individuais por setor/fileira com status (disponível, vendido, reservado) | Seleção Visual |
| F8 | Gerenciamento de Pedidos | Criação de pedidos com itens, cálculo de totais, status (pendente, confirmado, cancelado) | Gestão |
| F9 | Categorias | Organização de eventos por categorias (Shows, Teatro, Esportes, Gastronomia, etc.) | Organização |
| F10 | Destaques | Seção de eventos em destaque com carrossel automático na página inicial | Promoção |

### 5.2 Funcionalidades Planejadas

| ID | Funcionalidade | Descrição | Prioridade |
|:--:|---------------|-----------|:----------:|
| FP1 | Dashboard do Organizador | Painel com métricas de vendas, ingressos vendidos, faturamento | Alta |
| FP2 | Notificações por E-mail | Confirmação de compra, lembretes de evento usando MailKit | Alta |
| FP3 | Upload de Imagens | Sistema de upload para imagens de eventos com QRCoder para QR Codes | Média |
| FP4 | Avaliações e Comentários | Participantes avaliam eventos após a realização | Média |
| FP5 | Wishlist/Favoritos | Participantes salvam eventos favoritos | Média |
| FP6 | App Mobile | Versão mobile nativa (React Native ou Flutter) | Baixa |
| FP7 | Integração com Redes Sociais | Compartilhamento de eventos e login social | Baixa |
| FP8 | Relatórios Avançados | Exportação de dados de vendas em CSV/Excel | Baixa |

---

## 6. Tecnologias Utilizadas

| Camada | Tecnologia | Finalidade |
|--------|-----------|------------|
| **Frontend** | Next.js 16 + React 19 | Framework web moderno com SSR/CSR |
| **Estilização** | Tailwind CSS 4 + shadcn/ui | Design system responsivo e acessível |
| **Backend** | .NET 8 (C#) | API RESTful robusta e performática |
| **ORM/Query** | Dapper 2.1 | Micro-ORM para consultas SQL rápidas |
| **Banco** | SQLite | Banco de dados relacional embarcado |
| **Autenticação** | JWT + BCrypt | Tokens seguros e hash de senhas |
| **Validação** | FluentValidation | Validação declarativa de dados |
| **Logging** | Serilog | Logging estruturado em arquivo e console |
| **Documentação** | Swagger/OpenAPI | Documentação interativa da API |
| **Paginação** | Rate Limiting | Proteção contra abusos (100 req/min global) |

---

## 7. Métricas de Sucesso

| Indicador | Meta |
|-----------|------|
| Tempo de carregamento da página inicial | < 2 segundos |
| Disponibilidade da API | 99.5% |
| Taxa de conversão (visita → compra) | > 3% |
| Satisfação do usuário (NPS) | > 70 |
| Tempo médio para criar um evento | < 5 minutos |

---

## 8. Conclusão

O **BoraAli** é uma plataforma completa e moderna para o mercado de eventos brasileiro. Com uma arquitetura robusta (Next.js + .NET 8 + SQLite), interface intuitiva e funcionalidades que cobrem todo o ciclo de vida de um evento — da criação à venda de ingressos — o produto entrega valor real tanto para organizadores quanto para participantes. A combinação de performance, segurança e experiência do usuário posiciona o BoraAli como uma solução competitiva no mercado de ticketing digital.
