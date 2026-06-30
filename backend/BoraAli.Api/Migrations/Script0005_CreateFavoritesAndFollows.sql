-- ============================================================
-- Migration 0005: Tabelas de favoritos e seguir organizadores
-- Database: SQLite
-- ============================================================

-- TABELA: EventFavorites (relação N:N entre Usuário e Evento)
CREATE TABLE IF NOT EXISTS EventFavorites (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER     NOT NULL,
    EventId         INTEGER     NOT NULL,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE,
    UNIQUE (UserId, EventId)
);

CREATE INDEX IF NOT EXISTS IX_EventFavorites_UserId ON EventFavorites(UserId);
CREATE INDEX IF NOT EXISTS IX_EventFavorites_EventId ON EventFavorites(EventId);

-- TABELA: OrganizerFollows (relação N:N entre Usuário e Organizador/User)
CREATE TABLE IF NOT EXISTS OrganizerFollows (
    Id              INTEGER     PRIMARY KEY AUTOINCREMENT,
    FollowerId      INTEGER     NOT NULL,
    OrganizerId     INTEGER     NOT NULL,
    CreatedAt       TEXT        NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (FollowerId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrganizerId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE (FollowerId, OrganizerId)
);

CREATE INDEX IF NOT EXISTS IX_OrganizerFollows_FollowerId ON OrganizerFollows(FollowerId);
CREATE INDEX IF NOT EXISTS IX_OrganizerFollows_OrganizerId ON OrganizerFollows(OrganizerId);
