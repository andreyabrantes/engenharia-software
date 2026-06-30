-- ============================================================
-- Migration 0003: Criação da tabela Coupons
-- Database: SQLite
-- ============================================================

CREATE TABLE IF NOT EXISTS Coupons (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    Code            TEXT        NOT NULL UNIQUE,
    Description     TEXT        NULL,
    DiscountPercent REAL        NOT NULL DEFAULT 0,
    MaxUses         INTEGER     NOT NULL DEFAULT 1,
    CurrentUses     INTEGER     NOT NULL DEFAULT 0,
    IsActive        INTEGER     NOT NULL DEFAULT 1,
    EventId         INTEGER     NULL,
    ValidUntil      TEXT        NULL,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (EventId) REFERENCES Events(Id),
    CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100)
);

CREATE INDEX IF NOT EXISTS IX_Coupons_Code ON Coupons(Code);
CREATE INDEX IF NOT EXISTS IX_Coupons_EventId ON Coupons(EventId);
