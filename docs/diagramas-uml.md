# Diagramas UML — BoraAli

> **Ferramenta:** Mermaid (renderizado nativamente no GitHub e VS Code)
> **Notação:** UML 2.5 — Diagramas de Sequência e Casos de Uso

---

## 1. Diagrama de Sequência: Check-in com QR Code

Este diagrama mostra o ciclo de vida completo da validação de um ingresso via QR Code na entrada do evento.

```mermaid
sequenceDiagram
    actor Organizador as 👤 Organizador
    participant CheckIn as 🖥️ Next.js (app/checkin)
    participant API as 🔧 .NET 8 API
    participant Service as 📦 OrderService
    participant DB as 🗄️ SQLite

    Note over Organizador,DB: Fluxo de Check-in via QR Code

    Organizador->>CheckIn: Digita/Escaneia código do pedido
    CheckIn->>CheckIn: Valida formato do código (não vazio)

    CheckIn->>API: POST /api/orders/public-checkin<br/>{ "orderCode": "BA-20260615-A1B2C3D4" }
    Note over CheckIn,API: HTTP Request (AllowAnonymous)

    API->>Service: CheckInOrderAsync("BA-20260615-A1B2C3D4")
    Service->>DB: SELECT o.*, e.Title, e.Location<br/>FROM Orders o<br/>INNER JOIN Events e ON e.Id = o.EventId<br/>WHERE o.OrderCode = @OrderCode

    alt Pedido não encontrado
        DB-->>Service: null
        Service-->>API: Fail("QR Code inválido. Pedido não encontrado.")
        API-->>CheckIn: 400 Bad Request
        CheckIn-->>Organizador: ❌ Acesso Negado
    else Pedido já utilizado
        DB-->>Service: order.Status = "Used"
        Service-->>API: Fail("Este ingresso já foi utilizado.")
        API-->>CheckIn: 400 Bad Request
        CheckIn-->>Organizador: ❌ Acesso Negado
    else Pedido não confirmado
        DB-->>Service: order.Status = "Pending"
        Service-->>API: Fail("Pagamento precisa estar confirmado.")
        API-->>CheckIn: 400 Bad Request
        CheckIn-->>Organizador: ❌ Acesso Negado
    else Pedido confirmado (Confirmed)
        DB-->>Service: order.Status = "Confirmed"
        Service->>DB: UPDATE Orders SET Status = 'Used'<br/>WHERE Id = @Id AND Status = 'Confirmed'
        Note over Service,DB: UPDATE atômico — previne race condition
        DB-->>Service: 1 row affected
        Service-->>API: Ok(order, "✅ Entrada Liberada!")
        API-->>CheckIn: 200 OK + dados do pedido
        CheckIn-->>Organizador: ✅ Entrada Liberada!<br/>Pedido: #BA-20260615-A1B2C3D4<br/>Evento: Rock in Rio 2024<br/>Local: Parque Olímpico
    end
```

### Validações do Backend (Check-in)

| # | Validação | Código | HTTP Status |
|---|-----------|--------|-------------|
| 1 | Código do pedido existe? | `order == null` | 400 |
| 2 | Ingresso já foi utilizado? | `order.Status == "Used"` | 400 |
| 3 | Pagamento está confirmado? | `order.Status != "Confirmed"` | 400 |
| 4 | UPDATE atômico bem-sucedido? | `affected == 0` (race condition) | 400 |

---

## 2. Diagrama de Sequência: Validação de Cupons de Desconto

Este diagrama mostra o fluxo de validação de um cupom durante o checkout.

```mermaid
sequenceDiagram
    actor Cliente as 👤 Cliente (Comprador)
    participant Checkout as 🖥️ Next.js (checkout-form)
    participant API as 🔧 .NET 8 API
    participant Service as 📦 OrderService
    participant DB as 🗄️ SQLite

    Note over Cliente,DB: Fluxo de Validação de Cupom

    Cliente->>Checkout: Digita código do cupom "POO100"
    Checkout->>Checkout: Valida campo não vazio

    Checkout->>API: POST /api/orders/validate-coupon<br/>{ "code": "POO100", "eventId": 1 }<br/>Authorization: Bearer {JWT}

    API->>Service: ValidateCouponAsync("POO100", 1)
    Service->>DB: SELECT * FROM Coupons<br/>WHERE Code = "POO100"

    alt Cupom não encontrado
        DB-->>Service: null
        Service-->>API: Fail("Cupom inválido")
        API-->>Checkout: 400 Bad Request
        Checkout-->>Cliente: Toast: "Cupom inválido"
    else Cupom inativo
        DB-->>Service: IsActive = false
        Service-->>API: Fail("Este cupom não está mais ativo")
        API-->>Checkout: 400 Bad Request
        Checkout-->>Cliente: Toast: "Este cupom não está mais ativo"
    else Cupom expirado
        DB-->>Service: ValidUntil < DateTime.UtcNow
        Service-->>API: Fail("Este cupom expirou")
        API-->>Checkout: 400 Bad Request
        Checkout-->>Cliente: Toast: "Este cupom expirou"
    else Limite de usos atingido
        DB-->>Service: CurrentUses >= MaxUses
        Service-->>API: Fail("Cupom já atingiu o limite máximo")
        API-->>Checkout: 400 Bad Request
        Checkout-->>Cliente: Toast: "Cupom já atingiu o limite"
    else Cupom vinculado a evento errado
        DB-->>Service: EventId != 1
        Service-->>API: Fail("Cupom não é válido para este evento")
        API-->>Checkout: 400 Bad Request
        Checkout-->>Cliente: Toast: "Cupom não é válido para este evento"
    else Cupom válido (POO100 = 100%)
        DB-->>Service: Coupon válido
        Service-->>API: Ok({ discountPercent: 100, isValid: true })
        API-->>Checkout: 200 OK
        Checkout->>Checkout: Recalcula total com desconto
        Checkout-->>Cliente: ✅ "Cupom POO100 aplicado! 100% OFF"
        Note over Checkout: Total recalculado: R$ 0,00
    end
```

### Regras de Negócio do Cupom

| # | Regra | Campo/Validação |
|---|-------|-----------------|
| 1 | Cupom deve existir no banco | `Code == code.ToUpper().Trim()` |
| 2 | Cupom deve estar ativo | `IsActive == true` |
| 3 | Cupom não pode estar expirado | `ValidUntil == null \|\| ValidUntil >= DateTime.UtcNow` |
| 4 | Cupom não pode ter atingido limite | `CurrentUses < MaxUses` |
| 5 | Cupom deve ser global ou do evento | `EventId == null \|\| EventId == eventId` |

---

## 3. Diagrama de Casos de Uso

```mermaid
graph TB
    subgraph "BoraAli - Plataforma de Eventos"
        %% Atores
        Visitante(👤 Visitante)
        Cliente(👤 Cliente)
        Organizador(👤 Organizador)
        Admin(👤 Admin)

        %% Casos de Uso - Visitante
        subgraph "Visitante (não autenticado)"
            UC_VerEventos[Ver Eventos]
            UC_Filtrar[Filtrar/Buscar Eventos]
            UC_VerDetalhes[Ver Detalhes do Evento]
            UC_Cadastrar[Cadastrar Conta]
            UC_Login[Fazer Login]
            UC_CheckIn[Check-in QR Code]
        end

        %% Casos de Uso - Cliente
        subgraph "Cliente (autenticado)"
            UC_Comprar[Comprar Ingressos]
            UC_Mapa[Selecionar Assentos no Mapa]
            UC_Cupom[Aplicar Cupom de Desconto]
            UC_MeusPedidos[Ver Meus Pedidos]
            UC_Reembolso[Solicitar Reembolso]
            UC_Cancelar[Cancelar Pedido]
            UC_QRCode[Ver QR Code do Ingresso]
        end

        %% Casos de Uso - Organizador
        subgraph "Organizador (autenticado)"
            UC_CriarEvento[Criar Evento]
            UC_EditarEvento[Editar Evento]
            UC_ExcluirEvento[Excluir Evento]
            UC_GerenciarAssentos[Gerenciar Assentos]
            UC_Dashboard[Ver Dashboard de Vendas]
            UC_ValidarIngresso[Validar Ingresso na Entrada]
            UC_GerenciarCupons[Gerenciar Cupons]
        end

        %% Relacionamentos
        Visitante --> UC_VerEventos
        Visitante --> UC_Filtrar
        Visitante --> UC_VerDetalhes
        Visitante --> UC_Cadastrar
        Visitante --> UC_Login
        Visitante --> UC_CheckIn

        Cliente --> UC_Comprar
        Cliente --> UC_Mapa
        Cliente --> UC_Cupom
        Cliente --> UC_MeusPedidos
        Cliente --> UC_Reembolso
        Cliente --> UC_Cancelar
        Cliente --> UC_QRCode

        Organizador --> UC_CriarEvento
        Organizador --> UC_EditarEvento
        Organizador --> UC_ExcluirEvento
        Organizador --> UC_GerenciarAssentos
        Organizador --> UC_Dashboard
        Organizador --> UC_ValidarIngresso
        Organizador --> UC_GerenciarCupons

        %% Include / Extend
        UC_Comprar -.->|include| UC_Mapa
        UC_Comprar -.->|extend| UC_Cupom
        UC_Cadastrar -.->|include| UC_Login
        UC_QRCode -.->|include| UC_CheckIn
    end
```

---

## 4. Diagrama de Sequência: Criação de Pedido com Assentos

Fluxo completo da compra com seleção de assentos, demonstrando concorrência segura.

```mermaid
sequenceDiagram
    actor Cliente as 👤 Cliente
    participant Next as 🖥️ Next.js
    participant API as 🔧 .NET API
    participant Service as 📦 OrderService
    participant UoW as 🔄 UnitOfWork
    participant DB as 🗄️ SQLite

    Cliente->>Next: Seleciona assentos e clica "Comprar"
    Next->>API: POST /api/orders<br/>{ eventId, items: [{ ticketTypeId, seatId }] }

    API->>Service: CreateOrderAsync(createDto, userId)
    Service->>DB: Verifica limite de 5 ingressos/evento
    Service->>Service: Detecta hasSeats = true

    Note over Service,DB: ⚡ Início da Transação

    Service->>UoW: BeginTransactionAsync()
    UoW->>DB: BEGIN TRANSACTION

    loop Para cada assento
        Service->>DB: UPDATE Seats SET Status = 'Reserved'<br/>WHERE Id = @SeatId AND Status = 'Available'
        alt Assento já vendido/reservado
            DB-->>Service: 0 rows affected
            Service->>UoW: RollbackTransactionAsync()
            UoW->>DB: ROLLBACK
            Service-->>API: Fail("Assento já vendido")
            API-->>Next: 400
            Next-->>Cliente: Toast: "Assento não disponível"
        end
    end

    Service->>DB: INSERT INTO Orders (...)
    Service->>DB: INSERT INTO OrderItems (...)
    Service->>DB: UPDATE TicketTypes SET AvailableQuantity -= Qty
    Service->>DB: UPDATE Seats SET Status = 'Sold' WHERE Status = 'Reserved'

    Service->>UoW: CommitTransactionAsync()
    UoW->>DB: COMMIT

    Service->>Service: Envia e-mail com QR Code (MailKit)
    Service-->>API: Ok(orderDto)
    API-->>Next: 201 Created
    Next-->>Cliente: ✅ Compra realizada!<br/>Redireciona para tela de pagamento (Pix)
```

---

## 5. Diagrama de Estados: Ciclo de Vida do Pedido

```mermaid
stateDiagram-v2
    [*] --> Pending: Cliente cria pedido

    Pending --> Confirmed: Pagamento confirmado (Pix)
    Pending --> Cancelled: Cliente cancela

    Confirmed --> Used: Check-in na entrada (QR Code)
    Confirmed --> Refunded: Reembolso solicitado<br/>(antes do evento)
    Confirmed --> Cancelled: Cliente cancela<br/>(antes do evento)

    Cancelled --> [*]
    Refunded --> [*]
    Used --> [*]

    note right of Pending
        Status inicial.
        Ingressos reservados,
        estoque debitado.
    end note

    note right of Confirmed
        Pagamento confirmado.
        QR Code enviado por e-mail.
        Pode dar check-in.
    end note

    note right of Used
        Check-in realizado.
        Ingresso consumido.
        Estado terminal.
    end note
```

---

## Resumo das Validações Críticas do Backend

| Fluxo | Validação | Objetivo |
|-------|-----------|----------|
| **Check-in** | `UPDATE ... WHERE Status = 'Confirmed'` | Evita dupla entrada (idempotência) |
| **Compra com assentos** | `UPDATE Seats ... WHERE Status = 'Available'` | Evita venda do mesmo assento para 2 pessoas |
| **Compra com quantidade** | `UPDATE TicketTypes ... WHERE AvailableQuantity >= @Qty` | Evita overselling |
| **Cupom** | 5 verificações em sequência | Cupom válido, ativo, dentro do prazo, com usos restantes, compatível com o evento |
| **Reembolso** | `eventDate > DateTime.UtcNow` | Só reembolsa antes do evento acontecer |
| **Limite de ingressos** | `userTotalTickets + requested <= 5` | Máximo 5 ingressos por pessoa por evento |
