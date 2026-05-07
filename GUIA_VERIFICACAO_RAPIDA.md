# ⚡ GUIA DE VERIFICAÇÃO RÁPIDA - AV1

Use este guia para verificar rapidamente se o projeto está conforme antes da entrega.

---

## ✅ CHECKLIST DE 2 MINUTOS

### 1. Estrutura de Pastas (30 segundos)
```bash
ls
```
**Deve mostrar:**
- ✅ `/docs`
- ✅ `/db`
- ✅ `/src`
- ✅ `/tests`
- ✅ `README.md`

---

### 2. Script SQL (15 segundos)
```bash
cat db/script.sql
```
**Deve conter exatamente 4 tabelas:**
- ✅ `CREATE TABLE Usuarios` (Cpf PK)
- ✅ `CREATE TABLE Eventos` (Id PK AUTOINCREMENT)
- ✅ `CREATE TABLE Cupons` (Codigo PK)
- ✅ `CREATE TABLE Reservas` (Id PK AUTOINCREMENT, 3 FKs)

---

### 3. Compilação (15 segundos)
```bash
cd src
dotnet build
```
**Deve mostrar:**
- ✅ `Construir êxito` ou `Build succeeded`

---

### 4. Testes (30 segundos)
```bash
cd tests/BilheteriaAPI.Tests
dotnet test
```
**Deve mostrar:**
- ✅ `total: 88; falhou: 0; bem-sucedido: 88`

---

### 5. Endpoints da API (30 segundos)
```bash
cd src
dotnet run
```
Abra: http://localhost:5047/swagger

**Deve mostrar exatamente 4 endpoints:**
- ✅ `POST /api/eventos`
- ✅ `GET /api/eventos`
- ✅ `POST /api/cupons`
- ✅ `POST /api/usuarios`

---

## 🔍 VERIFICAÇÃO DE SEGURANÇA (1 minuto)

### Buscar por SQL Injection
```bash
grep -r '\$"' src/Program.cs
grep -r ' + ' src/Program.cs
```
**Deve retornar:** NADA (zero resultados)

### Verificar uso de parâmetros @
```bash
grep -r '@' src/Program.cs | grep -i 'select\|insert\|update\|delete'
```
**Deve retornar:** Várias linhas com `@Cpf`, `@Nome`, `@Email`, etc.

---

## 📋 VERIFICAÇÃO DE DOCUMENTAÇÃO (30 segundos)

### Histórias de Usuário
```bash
grep -c "Como" docs/requisitos.md
```
**Deve retornar:** 4 ou mais

### Critérios BDD
```bash
grep -c "Dado que" docs/requisitos.md
```
**Deve retornar:** 4 ou mais

---

## 🎯 TESTE FUNCIONAL RÁPIDO (2 minutos)

### 1. Iniciar a API
```bash
cd src
dotnet run
```

### 2. Testar POST /api/usuarios (Anti-Cambista)
```bash
curl -X POST http://localhost:5047/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{"nome":"João","email":"joao@email.com","cpf":"12345678900"}'
```
**Deve retornar:** Status 201 Created

### 3. Testar CPF Duplicado
```bash
curl -X POST http://localhost:5047/api/usuarios \
  -H "Content-Type: application/json" \
  -d '{"nome":"Maria","email":"maria@email.com","cpf":"12345678900"}'
```
**Deve retornar:** Status 400 Bad Request com mensagem "CPF já cadastrado."

### 4. Testar POST /api/eventos
```bash
curl -X POST http://localhost:5047/api/eventos \
  -H "Content-Type: application/json" \
  -d '{"nome":"Show de Rock","capacidadeTotal":500,"dataEvento":"2026-08-15T20:00:00","precoPadrao":100.00}'
```
**Deve retornar:** Status 201 Created

### 5. Testar GET /api/eventos
```bash
curl http://localhost:5047/api/eventos
```
**Deve retornar:** Array JSON com eventos

---

## 🚨 SINAIS DE ALERTA

### ❌ REPROVAÇÃO AUTOMÁTICA se encontrar:
- Pasta `/src` vazia ou inexistente
- Mais de 4 endpoints na API
- Tabelas além de Usuarios, Eventos, Cupons, Reservas
- Uso de `$"..."` em queries SQL
- Uso de `+` para concatenar SQL
- Testes sem `Assert`
- CPF duplicado sendo aceito

### ⚠️ PONTOS PERDIDOS se encontrar:
- README sem comandos em blocos de código
- Menos de 3 histórias de usuário
- Histórias sem formato "Como... Quero... Para..."
- Critérios sem formato "Dado que... Quando... Então..."
- Validações que não retornam BadRequest

---

## 📊 PONTUAÇÃO ESPERADA

Se todos os itens acima estiverem ✅:
- **Nota Estimada: 10.0/10.0**

Se algum item estiver ❌:
- Consulte `VALIDACAO_AV1.md` para detalhes

---

## 🔗 LINKS ÚTEIS

- [Relatório de Validação Completo](VALIDACAO_AV1.md)
- [Relatório de Correções](CORRECOES_REALIZADAS.md)
- [README Principal](README.md)
- [Requisitos](docs/requisitos.md)

---

## 💡 DICA FINAL

Antes de fazer o commit final:
```bash
# 1. Limpar builds antigos
dotnet clean

# 2. Recompilar do zero
cd src
dotnet build

# 3. Executar todos os testes
cd ../tests/BilheteriaAPI.Tests
dotnet test

# 4. Se tudo passar, fazer commit
git add .
git commit -m "feat: projeto AV1 conforme especificação oficial"
git push
```

---

**Boa sorte na avaliação! 🎓**
