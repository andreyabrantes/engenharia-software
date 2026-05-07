# 🎟️ TicketPrime — Sistema de Bilheteria Virtual

Plataforma de comercialização e gestão de ingressos desenvolvida com **Minimal API C#** e **Dapper**, como projeto da disciplina de Engenharia de Software — 5° Período de Ciência da Computação, Unifeso.

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

### 3. Execute os Testes

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

## 📁 Estrutura do Projeto

```
engenharia-software/
├── src/          # Minimal API em C# com Dapper
├── db/           # Scripts SQL (CREATE TABLE)
├── docs/         # Requisitos, arquitetura e operação
└── tests/        # Testes automatizados xUnit
```

---

## 🗄️ Banco de Dados

O sistema utiliza **SQLite** com as seguintes tabelas:

### Usuarios
- **Cpf** (VARCHAR(14), PK)
- Nome (VARCHAR(100))
- Email (VARCHAR(100), UNIQUE)

### Eventos
- **Id** (INTEGER, PK, AUTOINCREMENT)
- Nome (VARCHAR(100))
- CapacidadeTotal (INTEGER)
- DataEvento (DATETIME)
- PrecoPadrao (NUMERIC(10,2))

### Cupons
- **Codigo** (VARCHAR(50), PK)
- PorcentagemDesconto (NUMERIC(5,2))
- ValorMinimoRegra (NUMERIC(10,2))

### Reservas
- **Id** (INTEGER, PK, AUTOINCREMENT)
- UsuarioCpf (VARCHAR(14), FK → Usuarios.Cpf)
- EventoId (INTEGER, FK → Eventos.Id)
- CupomUtilizado (VARCHAR(50), FK → Cupons.Codigo, NULL)
- ValorFinalPago (NUMERIC(10,2))

---

## 🔌 Endpoints da API (AV1)

### POST /api/eventos
Cadastra um novo evento no sistema.

**Request Body:**
```json
{
  "nome": "Show de Rock 2026",
  "capacidadeTotal": 500,
  "dataEvento": "2026-08-15T20:00:00",
  "precoPadrao": 100.00
}
```

### GET /api/eventos
Lista todos os eventos disponíveis.

### POST /api/cupons
Cadastra um novo cupom de desconto.

**Request Body:**
```json
{
  "codigo": "PROMO10",
  "porcentagemDesconto": 10.0,
  "valorMinimoRegra": 50.00
}
```

### POST /api/usuarios
Cadastra um novo usuário.

**Request Body:**
```json
{
  "nome": "João da Silva",
  "email": "joao@email.com",
  "cpf": "12345678900"
}
```

**Retorna erro 400 se o CPF já existir** (proteção anti-cambista).

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
- **SQLite** — banco de dados relacional
- **xUnit** — testes automatizados

---

## 🔒 Segurança

- ✅ **Zero SQL Injection**: Todas as consultas usam parâmetros `@`
- ✅ **Validação Fail-Fast**: Retorna `BadRequest` (400) para dados inválidos
- ✅ **Proteção Anti-Cambista**: CPF único por usuário
- ✅ **Proteção contra Fraude**: Validação de valor mínimo em cupons

---

## 🧪 Testes

O projeto possui **60+ testes automatizados** cobrindo:

- ✅ Validação de campos obrigatórios
- ✅ Regras de negócio (capacidade, cupons, CPF único)
- ✅ Cálculos de desconto e valor final
- ✅ Proteção contra valores negativos
- ✅ Estrutura das tabelas do banco

Execute os testes com:
```bash
cd tests/BilheteriaAPI.Tests
dotnet test
```

---

## 🔄 Metodologia

Modelo **Incremental e Iterativo (Scrum/Ágil)**, com entregas por Sprint para validação antecipada e mitigação de riscos.

Veja mais em [`docs/rituais-scrum.md`](docs/rituais-scrum.md).

---

## ⚠️ Riscos Identificados

| Risco | Mitigação |
|---|---|
| Venda além da capacidade | Validação de CapacidadeTotal antes de criar reserva |
| Fraude com cupons negativos | Validação de PorcentagemDesconto > 0 e ValorMinimoRegra >= 0 |
| CPF duplicado (cambistas) | Retorno 400 com verificação via `WHERE Cpf = @Cpf` |
| SQL Injection | Uso exclusivo de parâmetros `@` em todas as queries |

---

## 📚 Documentação Adicional

- [Requisitos e Histórias de Usuário](docs/requisitos.md)
- [Script SQL do Banco de Dados](db/script.sql)
- [Relatório de Validação AV1](VALIDACAO_AV1.md)

---

**Desenvolvido com 💙 pela equipe TicketPrime**
