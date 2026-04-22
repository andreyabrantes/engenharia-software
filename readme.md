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

## ⚙️ Tecnologias

- **Minimal API** — .NET 9.0 / C# 13
- **Dapper** — acesso ao banco com parâmetros `@` (sem ORM)
- **SQLite** — banco de dados relacional
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
