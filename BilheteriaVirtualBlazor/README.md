# 🎫 Bilheteria Virtual - Blazor WebAssembly

Sistema completo de bilheteria virtual desenvolvido com **Blazor WebAssembly**, C#, HTML e CSS.

## 🔐 Sistema de Autenticação

O sistema possui dois tipos de usuários com permissões diferentes:

### 👨💼 Administrador
- **E-mail:** admin@bilheteria.com
- **Senha:** admin123
- **Permissões:**
  - Visualizar todos os eventos
  - Cadastrar novos eventos
  - Excluir eventos
  - Visualizar relatórios de vendas
  - Comprar ingressos

### 👤 Cliente
- **E-mail:** cliente@email.com
- **Senha:** cliente123
- **Permissões:**
  - Visualizar eventos disponíveis
  - Comprar ingressos
  - Selecionar setores e assentos

## 📋 User Stories Implementadas

### ✅ História 1 - Compra de Ingressos
**Como cliente**, eu quero comprar ingressos online escolhendo setor e assento para que eu garanta minha participação no evento com comodidade e segurança.

**Implementação:** 
- Página inicial com lista de eventos disponíveis
- Página de compra com seleção interativa de eventos, setores e assentos
- Validação de disponibilidade em tempo real
- Interface responsiva e intuitiva

### ✅ História 2 - Ingresso Digital
**Como cliente**, eu quero receber meu ingresso digital por e-mail após a compra para que eu possa acessar o evento de forma rápida e sem precisar imprimir nada.

**Implementação:** 
- Confirmação de compra com código único de ingresso
- Simulação de envio por e-mail
- Exibição de todos os detalhes do ingresso
- Código para apresentação na entrada

### ✅ História 3 - Cadastro de Eventos
**Como organizador do evento**, eu quero cadastrar eventos com datas, setores, preços e quantidade de ingressos disponíveis para que eu possa vender ingressos de forma estruturada e controlada.

**Implementação:** 
- Formulário completo de cadastro de eventos
- Suporte para múltiplos setores por evento
- Configuração de preços e quantidades
- Validação de dados
- Cálculo automático de capacidade e receita potencial

### ✅ História 4 - Relatórios de Vendas
**Como organizador do evento**, eu quero acompanhar relatórios de vendas em tempo real para que eu possa monitorar o desempenho do evento e tomar decisões estratégicas.

**Implementação:** 
- Dashboard com estatísticas em tempo real
- Cards com métricas principais (ingressos vendidos, receita, ticket médio)
- Tabela detalhada de vendas por evento
- Indicadores de ocupação com barra de progresso
- Insights automáticos (evento mais vendido, maior receita)
- Atualização em tempo real

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK (já instalado)

### Executar o Projeto

```bash
cd BilheteriaVirtualBlazor
export PATH="$HOME/.dotnet:$PATH"
dotnet run
```

Ou com hot reload para desenvolvimento:

```bash
dotnet watch
```

Acesse no navegador: **https://localhost:5001** ou **http://localhost:5000**

## 📁 Estrutura do Projeto

```
BilheteriaVirtualBlazor/
├── Models/
│   └── Models.cs              # Modelos de dados (Evento, Setor, Assento, Compra, Relatório)
├── Services/
│   └── EventoService.cs       # Serviço de gerenciamento de eventos e vendas
├── Pages/
│   ├── Home.razor             # Página inicial com eventos (User Story 1)
│   ├── ComprarIngresso.razor  # Compra de ingressos (User Stories 1 e 2)
│   ├── CadastrarEvento.razor  # Cadastro de eventos (User Story 3)
│   └── Relatorios.razor       # Dashboard de relatórios (User Story 4)
├── Layout/
│   └── MainLayout.razor       # Layout principal com navegação
├── wwwroot/
│   └── css/
│       └── app.css            # Estilos CSS customizados
├── Program.cs                 # Configuração da aplicação
└── App.razor                  # Componente raiz

```

## 🎨 Características do Design

- ✨ Design moderno e profissional com Blazor WebAssembly
- 🎨 Gradientes vibrantes e animações suaves
- 📱 Totalmente responsivo (mobile-first)
- 🎯 Interface intuitiva e amigável
- 🌈 Paleta de cores vibrante e consistente
- ⚡ Transições e efeitos visuais aprimorados
- 🔄 Atualizações em tempo real
- 💫 Componentes interativos e reativos

## 🛠️ Tecnologias Utilizadas

- **Blazor WebAssembly** - Framework SPA da Microsoft com C#
- **C# 12** - Linguagem de programação
- **.NET 8.0** - Plataforma de desenvolvimento
- **HTML5** - Estrutura semântica
- **CSS3** - Estilização moderna com gradientes, flexbox e grid
- **Dependency Injection** - Injeção de dependências nativa
- **Component-based Architecture** - Arquitetura baseada em componentes

## 📱 Funcionalidades

### Para Clientes:
- ✅ Visualização de eventos disponíveis com detalhes
- ✅ Seleção interativa de setores com preços
- ✅ Seleção visual de assentos (até 4 por compra)
- ✅ Resumo da compra em tempo real
- ✅ Recebimento de código de ingresso digital
- ✅ Interface responsiva para mobile e desktop

### Para Organizadores:
- ✅ Cadastro completo de eventos
- ✅ Configuração de múltiplos setores e preços
- ✅ Definição de capacidade por setor
- ✅ Dashboard com estatísticas em tempo real
- ✅ Relatórios detalhados de vendas por evento
- ✅ Análise de ocupação e receita
- ✅ Insights automáticos para tomada de decisão

## 🎯 Diferenciais do Blazor WebAssembly

- 🚀 **Performance**: Execução no lado do cliente com WebAssembly
- 💻 **C# Full Stack**: Mesma linguagem no front e back-end
- 🔄 **Reatividade**: Atualização automática da UI
- 🧩 **Componentização**: Reutilização de componentes
- 🔒 **Type Safety**: Tipagem forte em todo o código
- 🛠️ **Tooling**: Suporte completo do Visual Studio e VS Code
- 📦 **Sem JavaScript**: Desenvolvimento 100% em C#

## 🔄 Próximos Passos (Melhorias Futuras)

- [ ] Integração com API REST backend
- [ ] Autenticação e autorização (Identity)
- [ ] Banco de dados real (SQL Server/PostgreSQL)
- [ ] Gateway de pagamento (Stripe/PagSeguro)
- [ ] Envio real de e-mails (SendGrid)
- [ ] QR Code para ingressos
- [ ] Sistema de check-in
- [ ] Notificações push
- [ ] Exportação de relatórios (PDF/Excel)
- [ ] Painel administrativo avançado

## 👨‍💻 Desenvolvimento

Projeto desenvolvido para a disciplina de **Engenharia de Software - 6° Período - Unifeso**

### Comandos Úteis

```bash
# Compilar o projeto
dotnet build

# Executar testes
dotnet test

# Publicar para produção
dotnet publish -c Release

# Limpar build
dotnet clean
```

---

**Feito com ❤️ e Blazor WebAssembly**
