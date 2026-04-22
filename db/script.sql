-- TicketPrime — Script de criação do banco de dados
-- Estrutura exata conforme especificação do professor

CREATE TABLE Usuarios (
    Cpf   VARCHAR(14)  PRIMARY KEY,
    Nome  VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Eventos (
    Id              INTEGER      PRIMARY KEY AUTOINCREMENT,
    Nome            VARCHAR(100) NOT NULL,
    CapacidadeTotal INTEGER      NOT NULL,
    DataEvento      DATETIME     NOT NULL,
    PrecoPadrao     NUMERIC(10,2) NOT NULL
);

CREATE TABLE Cupons (
    Codigo                VARCHAR(50)   PRIMARY KEY,
    PorcentagemDesconto   NUMERIC(5,2)  NOT NULL,
    ValorMinimoRegra      NUMERIC(10,2) NOT NULL
);

CREATE TABLE Reservas (
    Id              INTEGER       PRIMARY KEY AUTOINCREMENT,
    UsuarioCpf      VARCHAR(14)   NOT NULL,
    EventoId        INTEGER       NOT NULL,
    CupomUtilizado  VARCHAR(50)   NULL,
    ValorFinalPago  NUMERIC(10,2) NOT NULL,
    FOREIGN KEY (UsuarioCpf)     REFERENCES Usuarios(Cpf),
    FOREIGN KEY (EventoId)       REFERENCES Eventos(Id),
    FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo)
);
