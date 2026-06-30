# Guia Rápido de Apresentação — BoraAli

> **Objetivo:** Garantir que a demo ao vivo rode sem imprevistos.
> **Público:** Banca avaliadora da disciplina de POO.

---

## 1. Pré-requisitos

```bash
# Verifique se estão instalados:
node --version     # >= 18
dotnet --version   # >= 8.0
```

---

## 2. Comandos para Rodar o Projeto

### 2.1. Backend (.NET 8 API)

Abra um terminal e execute:

```bash
cd backend/BoraAli.Api

# Restaura pacotes NuGet
dotnet restore

# Roda a API (com hot reload)
dotnet run
```

A API estará disponível em:
- **URL Base:** `http://localhost:5188`
- **Swagger UI:** `http://localhost:5188/swagger`
- **Health Check:** `http://localhost:5188/health`

> **Nota:** O banco SQLite (`BoraAli.db`) é criado automaticamente na primeira execução, com tabelas e dados de seed. Não é necessário rodar migrations manualmente.

### 2.2. Frontend (Next.js)

Abra **outro** terminal e execute:

```bash
# Na raiz do projeto
cd engenharia-software-main

# Instala dependências (apenas na primeira vez)
npm install

# Roda o frontend
npm run dev
```

O frontend estará disponível em:
- **URL:** `http://localhost:3000`

---

## 3. Credenciais de Teste

> **Senha padrão para todos os usuários de teste: `123456`**

### Organizador (cria e gerencia eventos)

| Nome | E-mail | Senha | Role |
|------|--------|-------|------|
| Carlos Eventos | `carlos@eventos.com` | `123456` | Organizador |
| Ana Produções | `ana@producoes.com` | `123456` | Organizador |

**Use `carlos@eventos.com` na demonstração** — ele já tem 5 eventos criados (Rock in Rio, Hamlet, Parque da Criança, Festival de Verão, DevConf).

### Cliente / Comprador (compra ingressos)

| Nome | E-mail | Senha | Role |
|------|--------|-------|------|
| João Silva | `joao@email.com` | `123456` | Cliente |
| Maria Santos | `maria@email.com` | `123456` | Cliente |

**Use `joao@email.com` na demonstração** — ele já tem pedidos criados:
- Pedido #BA-20240101-A1B2C3D4 (Rock in Rio — **Confirmed**)
- Pedido #BA-20240103-I9J0K1L2 (Maratona BH — **Pending**)

### Admin

| Nome | E-mail | Senha | Role |
|------|--------|-------|------|
| Admin BoraAli | `admin@boraali.com.br` | `Admin@123` | Admin |

---

## 4. Cupons de Desconto para Demonstração

| Código | Desconto | Limite | Descrição |
|--------|----------|--------|-----------|
| **`POO100`** | **100%** | 5 usos | ⭐ **Use este!** Cupom de 100% de desconto — ideal para demonstrar a compra sem custo. |
| `BORA50` | 50% | 20 usos | 50% de desconto na primeira compra |
| `ALUNO20` | 20% | 50 usos | 20% de desconto para estudantes |

---

## 5. Roteiro de Demonstração (15-20 min)

### Ato 1 — Visitante (3 min)

1. Acesse `http://localhost:3000`
2. Mostre a **home page** com eventos em destaque
3. Use a **busca** e os **filtros** por categoria
4. Clique em um evento para ver a **página de detalhes**

### Ato 2 — Cadastro e Login (2 min)

5. Clique em "Entrar" no header
6. Mostre o **formulário de login/cadastro**
7. Faça login como **organizador**: `carlos@eventos.com` / `123456`

### Ato 3 — Criar Evento (3 min)

8. Vá para **"Meus Eventos"** → **"Criar Evento"**
9. Preencha o formulário:
   - Nome, descrição, categoria
   - **CEP** (ex: `01001-000` — Praça da Sé, SP) — mostre o autocompletar
   - Data e horário
   - Tipos de ingresso (nome, preço, quantidade)
10. Clique em **"Publicar evento"**

### Ato 4 — Comprar Ingresso com Cupom (4 min)

11. Faça logout e login como **cliente**: `joao@email.com` / `123456`
12. Navegue até um evento publicado (ex: Rock in Rio)
13. Selecione ingressos e vá para o checkout
14. No campo de cupom, digite **`POO100`** e aplique
15. Mostre o **desconto de 100%** aplicado
16. Finalize a compra e confirme o pagamento (Pix simulado)

### Ato 5 — Check-in QR Code (2 min)

17. Vá para **"Meus Pedidos"** e encontre o pedido confirmado
18. Copie o **código do pedido** (formato `BA-YYYYMMDD-XXXXXXXX`)
19. Acesse `http://localhost:3000/checkin`
20. Cole o código e clique em **"Validar"**
21. Mostre a tela de **"✅ Entrada Liberada!"**

### Ato 6 — Dashboard do Organizador (2 min)

22. Faça logout e login como **organizador**: `carlos@eventos.com` / `123456`
23. Vá para **"Meus Eventos"**
24. Mostre o **dashboard analítico** com:
    - Receita total
    - Ingressos vendidos
    - Gráfico de receita por evento
    - Gráfico de receita por tipo de ingresso

---

## 6. Troubleshooting Rápido

### "Erro ao conectar com o servidor"

- Verifique se o backend está rodando em `http://localhost:5188`
- Teste: `curl http://localhost:5188/health`

### "Não foi possível inicializar o banco"

```bash
cd backend/BoraAli.Api
rm -f BoraAli.db                  # Remove o banco corrompido
dotnet run                         # Recria com seed data
```

### Porta 3000 ou 5188 em uso

```bash
# Mata processos nas portas (Windows)
netstat -ano | findstr :3000
netstat -ano | findstr :5188
taskkill /PID <PID> /F

# Ou mude a porta do frontend:
npm run dev -- -p 3001
```

### Login não funciona

- Os usuários de seed têm senha **`123456`** (hash BCrypt)
- O admin tem senha **`Admin@123`**
- Se criou usuários novos em execuções anteriores, delete o arquivo `BoraAli.db` e reinicie o backend

---

## 7. Checklist Pré-Apresentação

- [ ] Backend rodando em `http://localhost:5188`
- [ ] Frontend rodando em `http://localhost:3000`
- [ ] Login como organizador funciona (`carlos@eventos.com` / `123456`)
- [ ] Login como cliente funciona (`joao@email.com` / `123456`)
- [ ] Cupom `POO100` está ativo (verifique no Swagger: `GET /api/orders/validate-coupon`)
- [ ] Página de check-in acessível em `/checkin`
- [ ] Swagger acessível em `http://localhost:5188/swagger` (fallback para demonstrar a API)
- [ ] Conexão com internet (para buscar CEP na API ViaCEP e carregar imagens do Unsplash)
