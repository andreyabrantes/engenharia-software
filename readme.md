# 🎟️ TicketPrime — Sistema de Bilheteria Virtual

Plataforma de comercialização e gestão de ingressos desenvolvida com **Blazor WebAssembly** e **Minimal API C#**, como projeto da disciplina de Engenharia de Software — 5° Período de Ciência da Computação, Unifeso.

## 👥 Equipe

| Nome | GitHub |
|---|---|
| Andrey Campos | [@andreyabrantes](https://github.com/andreyabrantes) |
| Gustavo Ramos | [@GustaRD02](https://github.com/GustaRD02) |
| Nathan Salles | [@Shelby1311](https://github.com/shelby1311) |
| Cristiano Cordeiro | [@CristianoCSantos23](https://github.com/CristianoCSantos23) |
| Lucas Gabriel | [@Lucas-zip](https://github.com/Lucas-zip) |
| Julia Scarpi | [@juscarpi](https://github.com/juscarpi) |

> Projeto orientado pelo professor André Campos.

---

## 🚀 Como Executar

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 1. Clone o repositório

```bash
git clone https://github.com/andreyabrantes/engenharia-software.git
cd engenharia-software
```

### 2. Execute a API (Backend)

```bash
cd src
dotnet run
```

Swagger disponível em: **http://localhost:5047/swagger**

### 3. Execute o Frontend (Blazor)

```bash
cd BilheteriaVirtualBlazor
dotnet run
```

Acesse no navegador: **http://localhost:5000**

### 4. Execute os Testes

```bash
cd tests/BilheteriaAPI.Tests
dotnet test
```

### Comandos úteis

```bash
dotnet build              # Compilar
dotnet clean              # Limpar build
dotnet publish -c Release # Publicar para produção
```

---

## 🔐 Contas de Teste

| Tipo | E-mail | Senha |
|---|---|---|
| Administrador | admin@bilheteria.com | admin123 |
| Cliente | cliente@email.com | cliente123 |

---

## 📁 Estrutura do Projeto

```
engenharia-software/
├── src/          # Minimal API em C# com Dapper
├── db/           # Scripts SQL (CREATE TABLE)
├── docs/         # Requisitos, arquitetura e operação
├── tests/        # Testes automatizados xUnit
└── BilheteriaVirtualBlazor/  # Frontend Blazor WebAssembly
```

---

## 📋 Histórias de Usuário

**US-01**
Como cliente, Quero comprar ingressos para um evento escolhendo setor e assento, Para garantir meu lugar com antecedência.

**US-02**
Como organizador, Quero cadastrar eventos com nome, data, capacidade e preço, Para disponibilizá-los na plataforma de vendas.

**US-03**
Como cliente, Quero aplicar um cupom de desconto na compra do ingresso, Para pagar um valor reduzido.

**US-04**
Como administrador, Quero visualizar relatórios de vendas por evento, Para acompanhar a receita e ocupação em tempo real.

---

## ✅ Critérios de Aceitação (BDD)

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

---

## ⚙️ Tecnologias

- **Minimal API** — .NET 9.0 / C# 13
- **Dapper** — acesso ao banco com parâmetros `@` (sem ORM)
- **SQL** — banco de dados relacional
- **xUnit** — testes automatizados
- **Blazor WebAssembly** — frontend SPA em C#
- **CSS3** — design system próprio com variáveis e responsividade

---

## 🔄 Metodologia

Modelo **Incremental e Iterativo (Scrum/Ágil)**, com entregas por Sprint para validação antecipada e mitigação de riscos.

Veja mais em [`docs/rituais-scrum.md`](docs/rituais-scrum.md).

---

## ⚠️ Riscos Identificados

| Risco | Mitigação |
|---|---|
| Race condition em reservas simultâneas | Validação de disponibilidade no serviço antes de confirmar |
| Fraude com cupons negativos | Validação de valor mínimo e desconto positivo |
| CPF duplicado (cambistas) | Retorno 400 com verificação via `WHERE Cpf = @Cpf` |
