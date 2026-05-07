# 🔍 RELATÓRIO DE VALIDAÇÃO RIGOROSA - PROJETO TICKETPRIME AV1

**Data da Validação:** 06/05/2026  
**Avaliador:** Kiro AI  
**Projeto:** Sistema TicketPrime - Bilheteria Virtual

---

## ⚠️ RESUMO EXECUTIVO

**NOTA ESTIMADA: 6.0/10.0**

### ❌ PROBLEMAS CRÍTICOS IDENTIFICADOS

1. **ESTRUTURA DE PASTAS INCORRETA** - Código está em `BilheteriaAPI/` ao invés de `/src`
2. **BANCO DE DADOS NÃO CONFORME** - Tabelas e colunas não seguem a especificação exata
3. **ROTAS HTTP INCORRETAS** - Faltam rotas obrigatórias da AV1
4. **SQL INJECTION PARCIAL** - Uso de interpolação de string em query crítica

---

## 📋 AVALIAÇÃO DETALHADA POR ITEM

### ✅ ITEM 1: Histórias de Usuário (1.0 ponto)
**STATUS: APROVADO**

**Localização:** `/docs/requisitos.md`

**Histórias Encontradas:**
- ✅ US-01: Como cliente, Quero comprar ingressos para um evento escolhendo setor e assento, Para garantir meu lugar com antecedência.
- ✅ US-02: Como organizador, Quero cadastrar eventos com nome, data, capacidade e preço, Para disponibilizá-los na plataforma de vendas.
- ✅ US-03: Como cliente, Quero aplicar um cupom de desconto na compra do ingresso, Para pagar um valor reduzido.
- ✅ US-04: Como administrador, Quero visualizar relatórios de vendas por evento, Para acompanhar a receita e ocupação em tempo real.

**Formato:** Todas seguem o padrão exato "Como [ator], Quero [ação], Para [motivo]"

---

### ✅ ITEM 2: Critérios BDD (1.0 ponto)
**STATUS: APROVADO**

**Localização:** `/docs/requisitos.md`

**Critérios Encontrados:**
- ✅ US-01 — Compra de Ingresso (formato Dado que... Quando... Então...)
- ✅ US-02 — Cadastro de Evento (formato Dado que... Quando... Então...)
- ✅ US-03 — Aplicação de Cupom de Desconto (formato Dado que... Quando... Então...)
- ✅ US-04 — Relatório de Vendas (formato Dado que... Quando... Então...)

**Formato:** Todos seguem o padrão BDD correto

---

### ✅ ITEM 3: README Executável (1.0 ponto)
**STATUS: APROVADO**

**Localização:** `/README.md`

**Comandos Encontrados:**
```bash
git clone https://github.com/andreyabrantes/engenharia-software.git
cd engenharia-software
cd src
dotnet run
cd BilheteriaVirtualBlazor
dotnet run
cd tests/BilheteriaAPI.Tests
dotnet test
dotnet build
dotnet clean
dotnet publish -c Release
```

**Formato:** Todos os comandos estão em blocos de código markdown

---

### ✅ ITEM 4: Script do Banco (1.0 ponto)
**STATUS: APROVADO COM RESSALVAS**

**Localização:** `/db/script.sql`

**Tabelas Criadas:**
- ✅ Usuarios (Cpf VARCHAR(14) PK, Nome VARCHAR(100), Email VARCHAR(100))
- ✅ Eventos (Id INTEGER PK AUTOINCREMENT, Nome VARCHAR(100), CapacidadeTotal INTEGER, DataEvento DATETIME, PrecoPadrao NUMERIC(10,2))
- ✅ Cupons (Codigo VARCHAR(50) PK, PorcentagemDesconto NUMERIC(5,2), ValorMinimoRegra NUMERIC(10,2))
- ✅ Reservas (Id INTEGER PK AUTOINCREMENT, UsuarioCpf VARCHAR(14) FK, EventoId INTEGER FK, CupomUtilizado VARCHAR(50) FK NULL, ValorFinalPago NUMERIC(10,2))

**Chaves Estrangeiras:** ✅ Todas implementadas corretamente

**⚠️ OBSERVAÇÃO:** O script está correto, mas o código da API usa um schema diferente (com tabelas Setores, Assentos, Ingressos) que NÃO está no script oficial.

---

### ❌ ITEM 5: Contrato da API (0.0 pontos)
**STATUS: REPROVADO**

**Problema:** A API não está mapeando as rotas na pasta `/src` conforme exigido.

**Localização Real:** `/src/Program.cs` existe, mas o código principal está em `BilheteriaAPI/`

**Rotas Exigidas para AV1:**
- ✅ `POST /api/eventos` - ENCONTRADO
- ✅ `GET /api/eventos` - ENCONTRADO
- ✅ `POST /api/cupons` - ENCONTRADO
- ✅ `POST /api/usuarios` - ENCONTRADO

**⚠️ PROBLEMA CRÍTICO:** Embora as rotas existam, elas estão implementadas em um schema de banco diferente do especificado. A API usa tabelas `Setores`, `Assentos`, `Ingressos` que NÃO estão no script `/db/script.sql`.

**INCONSISTÊNCIA GRAVE:** O código não está usando o banco de dados definido no script oficial.

---

### ⚠️ ITEM 6: Fail-Fast (Validação) (0.5 pontos)
**STATUS: PARCIALMENTE APROVADO**

**Validações Encontradas:**

**POST /api/eventos:**
```csharp
if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Local) || req.Data == default)
    return Results.BadRequest(new { erro = "Nome, Local e Data são obrigatórios." });
```
✅ Retorna `BadRequest` corretamente

**POST /api/cupons:**
```csharp
if (string.IsNullOrWhiteSpace(req.Codigo))
    return Results.BadRequest(new { erro = "Código do cupom é obrigatório." });
if (req.Desconto <= 0)
    return Results.BadRequest(new { erro = "Desconto deve ser maior que zero." });
```
✅ Retorna `BadRequest` corretamente

**POST /api/usuarios:**
```csharp
if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Cpf))
    return Results.BadRequest(new { erro = "Nome, Email e CPF são obrigatórios." });

var existe = await conn.ExecuteScalarAsync<int>(
    "SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf", new { Cpf = req.Cpf });
if (existe > 0)
    return Results.BadRequest(new { erro = "CPF já cadastrado." });
```
✅ Retorna `BadRequest` para CPF duplicado (anti-cambista)

**⚠️ PROBLEMA:** As validações estão corretas, mas o schema do banco não corresponde ao especificado.

---

### ⚠️ ITEM 7: Segurança no Dapper (0.5 pontos)
**STATUS: PARCIALMENTE APROVADO**

**Consultas Analisadas:**

**✅ CORRETO - EventoService.cs:**
```csharp
"SELECT Id, Nome, Descricao, DataEvento AS Data, Local, ImagemUrl, Destaque FROM Eventos WHERE Id = @Id"
new { Id = id }
```

**✅ CORRETO - IngressoService.cs:**
```csharp
"SELECT Id FROM Usuarios WHERE Email = @Email"
new { Email = email }
```

**✅ CORRETO - Program.cs:**
```csharp
"SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf"
new { Cpf = req.Cpf }
```

**Maioria das consultas usa parâmetros `@` corretamente.**

---

### ❌ ITEM 8: Zero SQL Injection (0.0 pontos)
**STATUS: REPROVADO - FALHA CRÍTICA DE SEGURANÇA**

**🚨 VULNERABILIDADE ENCONTRADA em IngressoService.cs (linha ~70):**

```csharp
var inClause = string.Join(",", request.AssentoIds.Select((id, idx) =>
{
    parametros.Add($"id{idx}", id);
    return $"@id{idx}";
}));

assentos = conn.Query<dynamic>(
    $"SELECT Id, Numero, Status FROM Assentos WHERE Id IN ({inClause}) AND SetorId = @SetorId",
    parametros).ToList();
```

**PROBLEMA:** Uso de **interpolação de string** (`$"SELECT ... IN ({inClause})"`) na construção da query SQL.

**JUSTIFICATIVA DA REPROVAÇÃO:**
Embora os valores individuais estejam parametrizados, a **construção dinâmica da query usando interpolação de string** viola a regra:

> "É proibido usar concatenação ( + ) ou interpolação ( $"{}" ) nas consultas SQL."

**CORREÇÃO NECESSÁRIA:** Usar `string.Format` ou construir a query sem interpolação `$`.

---

### ✅ ITEM 9: Infraestrutura de Testes (1.0 ponto)
**STATUS: APROVADO**

**Localização:** `/tests/BilheteriaAPI.Tests/`

**Arquivos de Teste:**
- ✅ `CupomTests.cs` - 15 testes com `[Fact]` e `[Theory]`
- ✅ `EventoTests.cs` - 15 testes com `[Fact]` e `[Theory]`
- ✅ `UsuarioTests.cs` - 15 testes com `[Fact]` e `[Theory]`

**Projeto xUnit:** ✅ `BilheteriaAPI.Tests.csproj` configurado corretamente

---

### ✅ ITEM 10: Testes com Oráculo (1.0 ponto)
**STATUS: APROVADO**

**Todos os testes possuem cláusulas `Assert`:**

**Exemplos de CupomTests.cs:**
```csharp
Assert.True(string.IsNullOrWhiteSpace(codigo));
Assert.Equal(30m, resultado);
Assert.True(valorProtegido >= 0);
```

**Exemplos de EventoTests.cs:**
```csharp
Assert.True(invalido);
Assert.False(excedeu);
Assert.Equal(esperado, valorTotal);
```

**Exemplos de UsuarioTests.cs:**
```csharp
Assert.Equal(11, cpf.Length);
Assert.Contains("@", email);
Assert.NotEqual(cpf1, cpf2);
```

**✅ TODOS os 45 testes possuem Assert válido**

---

## 🎯 PONTUAÇÃO FINAL

| Item | Descrição | Pontos | Status |
|------|-----------|--------|--------|
| 1 | Histórias de Usuário | 1.0 | ✅ |
| 2 | Critérios BDD | 1.0 | ✅ |
| 3 | README Executável | 1.0 | ✅ |
| 4 | Script do Banco | 1.0 | ✅ |
| 5 | Contrato da API | 0.0 | ❌ |
| 6 | Fail-Fast (Validação) | 0.5 | ⚠️ |
| 7 | Segurança no Dapper | 0.5 | ⚠️ |
| 8 | Zero SQL Injection | 0.0 | ❌ |
| 9 | Infraestrutura de Testes | 1.0 | ✅ |
| 10 | Testes com Oráculo | 1.0 | ✅ |
| **TOTAL** | | **6.0/10.0** | |

---

## 🔴 PROBLEMAS CRÍTICOS QUE IMPEDEM NOTA MÁXIMA

### 1. INCONSISTÊNCIA ENTRE SCRIPT SQL E CÓDIGO DA API

**Problema:** O arquivo `/db/script.sql` define as tabelas:
- Usuarios (Cpf, Nome, Email)
- Eventos (Id, Nome, CapacidadeTotal, DataEvento, PrecoPadrao)
- Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra)
- Reservas (Id, UsuarioCpf, EventoId, CupomUtilizado, ValorFinalPago)

**Mas o código da API usa tabelas completamente diferentes:**
- Usuarios (Id, Nome, Email, Cpf, SenhaHash, Tipo)
- Eventos (Id, Nome, Descricao, DataEvento, Local, ImagemUrl, Destaque)
- Setores (Id, Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId)
- Assentos (Id, Numero, Status, SetorId)
- Ingressos (Id, CodigoUnico, Status, DataCompra, UsuarioId, AssentoId)
- Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra)

**Impacto:** A API NÃO funciona com o banco de dados especificado no documento.

---

### 2. VULNERABILIDADE DE SQL INJECTION

**Arquivo:** `src/Services/IngressoService.cs` (linha ~70)

**Código Problemático:**
```csharp
assentos = conn.Query<dynamic>(
    $"SELECT Id, Numero, Status FROM Assentos WHERE Id IN ({inClause}) AND SetorId = @SetorId",
    parametros).ToList();
```

**Violação:** Uso de interpolação de string `$"..."` na construção da query SQL.

**Correção Necessária:**
```csharp
var query = string.Format(
    "SELECT Id, Numero, Status FROM Assentos WHERE Id IN ({0}) AND SetorId = @SetorId",
    inClause);
assentos = conn.Query<dynamic>(query, parametros).ToList();
```

Ou melhor ainda, usar uma abordagem sem interpolação:
```csharp
var query = "SELECT Id, Numero, Status FROM Assentos WHERE Id IN (" + inClause + ") AND SetorId = @SetorId";
```

---

### 3. ESTRUTURA DE PASTAS NÃO CONFORME

**Esperado pelo documento:**
```
/docs    ✅ (existe)
/db      ✅ (existe)
/src     ⚠️ (existe mas código real está em BilheteriaAPI/)
/tests   ✅ (existe)
```

**Problema:** O código principal da API está em `BilheteriaAPI/` ao invés de `/src`.

---

## 📝 RECOMENDAÇÕES PARA CORREÇÃO

### PRIORIDADE ALTA (Obrigatório para aprovação)

1. **Alinhar o código da API com o script SQL oficial**
   - Remover tabelas extras (Setores, Assentos, Ingressos)
   - Usar exatamente as 4 tabelas especificadas: Usuarios, Eventos, Cupons, Reservas
   - Ajustar os endpoints para trabalhar com o schema correto

2. **Corrigir a vulnerabilidade de SQL Injection**
   - Eliminar o uso de interpolação `$"..."` em `IngressoService.cs`
   - Usar concatenação simples ou `string.Format`

3. **Reorganizar a estrutura de pastas**
   - Mover todo o código de `BilheteriaAPI/` para `/src`
   - Garantir que `dotnet run` funcione a partir de `/src`

### PRIORIDADE MÉDIA

4. **Simplificar a API para focar apenas nos 4 endpoints da AV1**
   - Remover endpoints extras que não são exigidos na AV1
   - Focar em: POST /api/eventos, GET /api/eventos, POST /api/cupons, POST /api/usuarios

5. **Atualizar o README.md**
   - Corrigir os comandos para refletir a estrutura correta
   - Remover referências ao frontend Blazor (não é parte da AV1)

---

## ✅ PONTOS FORTES DO PROJETO

1. **Documentação Excelente** - Histórias de usuário e critérios BDD muito bem escritos
2. **Testes Abrangentes** - 45 testes unitários cobrindo validações e regras de negócio
3. **Validações Robustas** - Fail-fast implementado corretamente nos endpoints
4. **Uso Correto do Dapper** - Maioria das queries usa parâmetros `@` adequadamente
5. **README Completo** - Comandos executáveis bem documentados

---

## 🎓 CONCLUSÃO

O projeto demonstra **conhecimento sólido de Engenharia de Software**, com documentação exemplar e testes bem estruturados. No entanto, **falhas críticas de conformidade** com a especificação (schema do banco diferente, SQL injection, estrutura de pastas) impedem uma nota mais alta.

**Com as correções recomendadas, o projeto tem potencial para alcançar 9.0-10.0 pontos.**

---

**Validação realizada por:** Kiro AI  
**Baseada em:** Especificação oficial do Projeto TicketPrime - Prof. Dr. André Campos  
**Data:** 06/05/2026
