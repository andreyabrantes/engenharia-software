-- TicketPrime — Script de criação do banco de dados
-- Nomes de colunas conforme manual do aluno (case-sensitive)

CREATE TABLE Usuarios (
    Id            INTEGER      PRIMARY KEY AUTOINCREMENT,
    Nome          TEXT         NOT NULL,
    Email         TEXT         NOT NULL UNIQUE,
    Cpf           TEXT         NOT NULL UNIQUE,
    SenhaHash     TEXT         NOT NULL,
    Tipo          TEXT         NOT NULL DEFAULT 'Cliente'
);

CREATE TABLE Eventos (
    Id               INTEGER  PRIMARY KEY AUTOINCREMENT,
    Nome             TEXT     NOT NULL,
    Descricao        TEXT     NOT NULL,
    DataEvento       TEXT     NOT NULL,
    Local            TEXT     NOT NULL,
    CapacidadeTotal  INTEGER  NOT NULL,
    PrecoPadrao      REAL     NOT NULL,
    ImagemUrl        TEXT
);

CREATE TABLE Cupons (
    Id               INTEGER  PRIMARY KEY AUTOINCREMENT,
    Codigo           TEXT     NOT NULL UNIQUE,
    Desconto         REAL     NOT NULL,
    valorMinimoregra REAL     NOT NULL,
    DataExpiracao    TEXT     NOT NULL
);

CREATE TABLE Reservas (
    Id          INTEGER  PRIMARY KEY AUTOINCREMENT,
    UsuarioId   INTEGER  NOT NULL,
    EventoId    INTEGER  NOT NULL,
    CupomId     INTEGER,
    DataReserva TEXT     NOT NULL DEFAULT (datetime('now')),
    ValorTotal  REAL     NOT NULL,
    Status      TEXT     NOT NULL DEFAULT 'Ativa',
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (EventoId)  REFERENCES Eventos(Id),
    FOREIGN KEY (CupomId)   REFERENCES Cupons(Id)
);
