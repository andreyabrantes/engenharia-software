# 🎟️ Sistema de Bilheteria Virtual

Plataforma de comercialização e gestão de ingressos online desenvolvida com **Blazor WebAssembly** e C#, como projeto da disciplina de Engenharia de Software — 5° Período de Ciência da Computação, Unifeso.

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

### API (Backend)

```bash
# 1. Clone o repositório
git clone https://github.com/andreyabrantes/engenharia-software.git
cd engenharia-software

# 2. Entre na pasta da API
cd src

# 3. Execute
dotnet run
```

Swagger disponível em: **http://localhost:5047/swagger**

### Frontend (Blazor)

```bash
cd BilheteriaVirtualBlazor
dotnet run
```

Acesse no navegador: **http://localhost:5000**

Para desenvolvimento com hot reload:

```bash
dotnet watch
```

### Comandos úteis

```bash
dotnet build        # Compilar
dotnet clean        # Limpar build
dotnet publish -c Release  # Publicar para produção
```

---

## 🔐 Contas de Teste

| Tipo | E-mail |
|---|---|
| Administrador | admin@bilheteria.com |
| Cliente | cliente@email.com |

---

## 📁 Estrutura do Projeto

```
engenharia-software-3/
├── src/                            # API (Minimal API + EF Core)
│   ├── Controllers/                # Controllers auxiliares
│   ├── Data/                       # AppDbContext
│   ├── Models/                     # Modelos de dados
│   ├── Repositories/               # Repositórios
│   ├── Services/                   # Serviços de negócio
│   └── Program.cs
├── BilheteriaVirtualBlazor/        # Frontend Blazor WebAssembly
├── db/                             # Scripts SQL
├── docs/                           # Documentação
└── tests/                          # Testes xUnit
```

---

## 📋 Histórias de Usuário Implementadas

| ID | Papel | Descrição | Status |
|---|---|---|---|
| US-01 | Cliente | Comprar ingressos escolhendo setor e assento | ✅ |
| US-02 | Cliente | Receber ingresso digital com código único | ✅ |
| US-03 | Organizador | Cadastrar eventos com setores e preços | ✅ |
| US-04 | Organizador | Acompanhar relatórios de vendas em tempo real | ✅ |

---

## ⚙️ Tecnologias

- **Blazor WebAssembly** — SPA com C# no frontend
- **.NET 8.0 / C# 12**
- **CSS3** — Design system próprio com variáveis, gradientes e responsividade
- **Dependency Injection** nativa do .NET

---

## 🔄 Metodologia

Modelo **Incremental e Iterativo (Scrum/Ágil)**, com entregas por Sprint para validação antecipada e mitigação de riscos como race condition em reservas simultâneas.

Veja mais em [`docs/rituais-scrum.md`](docs/rituais-scrum.md).

---

## ⚠️ Riscos Identificados

| Risco | Mitigação |
|---|---|
| Race condition (dois usuários no mesmo assento) | Validação de disponibilidade no serviço antes de confirmar |
| Falha no envio de e-mail | Simulação com fallback visual de confirmação |
| Fraude de ingresso | Código único por compra |

---

## 🗺️ Próximos Passos

- [ ] Banco de dados SQL
