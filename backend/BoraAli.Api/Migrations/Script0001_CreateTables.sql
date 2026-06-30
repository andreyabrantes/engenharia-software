-- ============================================================
-- Migration 0001: Criação das tabelas principais
-- Database: SQLite
-- ============================================================

-- TABELA: Users
CREATE TABLE IF NOT EXISTS Users (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    Name            TEXT        NOT NULL,
    Email           TEXT        NOT NULL,
    Cpf             TEXT        NOT NULL UNIQUE,
    PasswordHash    TEXT        NOT NULL,
    Phone           TEXT        NULL,
    AvatarUrl       TEXT        NULL,
    Role            TEXT        NOT NULL DEFAULT 'User',
    IsActive        INTEGER     NOT NULL DEFAULT 1,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),
    UpdatedAt       TEXT        NULL,

    UNIQUE (Email)
);

-- TABELA: Categories
CREATE TABLE IF NOT EXISTS Categories (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    Name            TEXT        NOT NULL,
    Slug            TEXT        NOT NULL,
    Icon            TEXT        NULL,
    IsActive        INTEGER     NOT NULL DEFAULT 1,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),

    UNIQUE (Slug)
);

-- TABELA: Events
CREATE TABLE IF NOT EXISTS Events (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    Title           TEXT        NOT NULL,
    Description     TEXT        NOT NULL,
    FullDescription TEXT        NULL,
    EventDate       TEXT        NOT NULL,
    Time            TEXT        NOT NULL,
    Location        TEXT        NOT NULL,
    Address         TEXT        NOT NULL,
    City            TEXT        NOT NULL,
    Cep             TEXT        NULL,
    Street          TEXT        NULL,
    Neighborhood    TEXT        NULL,
    State           TEXT        NULL,
    AddressNumber   TEXT        NULL,
    ImageUrl        TEXT        NULL,
    IsFeatured      INTEGER     NOT NULL DEFAULT 0,
    Status          TEXT        NOT NULL DEFAULT 'Draft',
    CategoryId      INTEGER     NOT NULL,
    OrganizerId     INTEGER     NOT NULL,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),
    UpdatedAt       TEXT        NULL,
    PublishedAt     TEXT        NULL,

    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (OrganizerId) REFERENCES Users(Id),
    CHECK (Status IN ('Draft', 'Published', 'Cancelled', 'Finished'))
);

CREATE INDEX IF NOT EXISTS IX_Events_CategoryId ON Events(CategoryId);
CREATE INDEX IF NOT EXISTS IX_Events_OrganizerId ON Events(OrganizerId);
CREATE INDEX IF NOT EXISTS IX_Events_City ON Events(City);
CREATE INDEX IF NOT EXISTS IX_Events_Status_EventDate ON Events(Status, EventDate);

-- TABELA: TicketTypes
CREATE TABLE IF NOT EXISTS TicketTypes (
    Id                INTEGER     PRIMARY KEY AUTOINCREMENT,
    EventId           INTEGER     NOT NULL,
    Name              TEXT        NOT NULL,
    Price             REAL        NOT NULL,
    TotalQuantity     INTEGER     NOT NULL,
    AvailableQuantity INTEGER     NOT NULL,
    Description       TEXT        NULL,
    SaleStartDate     TEXT        NULL,
    SaleEndDate       TEXT        NULL,
    IsActive          INTEGER     NOT NULL DEFAULT 1,
    CreatedAt         TEXT        NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE,
    CHECK (Price >= 0),
    CHECK (TotalQuantity > 0),
    CHECK (AvailableQuantity >= 0)
);

CREATE INDEX IF NOT EXISTS IX_TicketTypes_EventId ON TicketTypes(EventId);

-- TABELA: Orders
CREATE TABLE IF NOT EXISTS Orders (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER     NOT NULL,
    EventId         INTEGER     NOT NULL,
    OrderCode       TEXT        NOT NULL,
    TotalAmount     REAL        NOT NULL,
    Status          TEXT        NOT NULL DEFAULT 'Pending',
    PaymentMethod   TEXT        NULL,
    PaymentId       TEXT        NULL,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),
    ConfirmedAt     TEXT        NULL,
    UpdatedAt       TEXT        NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (EventId) REFERENCES Events(Id),
    UNIQUE (OrderCode),
    CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Refunded')),
    CHECK (TotalAmount >= 0)
);

CREATE INDEX IF NOT EXISTS IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IF NOT EXISTS IX_Orders_EventId ON Orders(EventId);

-- TABELA: OrderItems
CREATE TABLE IF NOT EXISTS OrderItems (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    OrderId         INTEGER     NOT NULL,
    TicketTypeId    INTEGER     NOT NULL,
    Quantity        INTEGER     NOT NULL,
    UnitPrice       REAL        NOT NULL,
    Subtotal        REAL        NOT NULL,

    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (TicketTypeId) REFERENCES TicketTypes(Id),
    CHECK (Quantity > 0),
    CHECK (UnitPrice >= 0),
    CHECK (Subtotal >= 0)
);

CREATE INDEX IF NOT EXISTS IX_OrderItems_OrderId ON OrderItems(OrderId);
