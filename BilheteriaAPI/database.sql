-- Bilheteria Virtual - Script SQL Completo
-- Compatível com SQLite (banco padrão do projeto)
-- Para usar com outros SGBDs, ajuste os tipos conforme necessário
-- ============================================================

PRAGMA foreign_keys = ON;

-- TABELAS
-- ============================================================

CREATE TABLE IF NOT EXISTS Usuarios (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome        TEXT    NOT NULL,
    Email       TEXT    NOT NULL UNIQUE,
    SenhaHash   TEXT    NOT NULL,
    Tipo        TEXT    NOT NULL DEFAULT 'Cliente' -- 'Admin' ou 'Cliente'
);

CREATE TABLE IF NOT EXISTS Eventos (
    Id          INTEGER  PRIMARY KEY AUTOINCREMENT,
    Nome        TEXT     NOT NULL,
    Descricao   TEXT     NOT NULL DEFAULT '',
    Data        DATETIME NOT NULL,
    Local       TEXT     NOT NULL DEFAULT '',
    ImagemUrl   TEXT     NOT NULL DEFAULT '🎉'
);

CREATE TABLE IF NOT EXISTS Setores (
    Id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome                 TEXT    NOT NULL,
    Preco                DECIMAL(10,2) NOT NULL,
    QuantidadeTotal      INTEGER NOT NULL DEFAULT 0,
    QuantidadeDisponivel INTEGER NOT NULL DEFAULT 0,
    EventoId             INTEGER NOT NULL,
    FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Assentos (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Numero   TEXT    NOT NULL,
    Status   INTEGER NOT NULL DEFAULT 0, -- 0=Disponivel, 1=Reservado, 2=Ocupado
    SetorId  INTEGER NOT NULL,
    FOREIGN KEY (SetorId) REFERENCES Setores(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Ingressos (
    Id          INTEGER  PRIMARY KEY AUTOINCREMENT,
    CodigoUnico TEXT     NOT NULL UNIQUE,
    Status      INTEGER  NOT NULL DEFAULT 0, -- 0=Ativo, 1=Cancelado, 2=Utilizado
    DataCompra  DATETIME NOT NULL DEFAULT (datetime('now')),
    UsuarioId   INTEGER  NOT NULL,
    AssentoId   INTEGER  NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssentoId) REFERENCES Assentos(Id) ON DELETE RESTRICT
);

-- Tabela de pagamentos com herança por coluna discriminadora (TPH)
CREATE TABLE IF NOT EXISTS Pagamentos (
    Id              TEXT     PRIMARY KEY, -- GUID
    Tipo            TEXT     NOT NULL,    -- 'Pix' ou 'Cartao'
    ValorTotal      DECIMAL(10,2) NOT NULL,
    DataPagamento   DATETIME NOT NULL DEFAULT (datetime('now')),
    Status          TEXT     NOT NULL DEFAULT 'Pendente',
    -- Campos de PagamentoPix
    ChavePixOrigem  TEXT,
    -- Campos de PagamentoCartao
    NumeroCartao    TEXT,
    Titular         TEXT
);

-- ÍNDICES
-- ============================================================

CREATE UNIQUE INDEX IF NOT EXISTS IX_Usuarios_Email    ON Usuarios(Email);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Ingressos_Codigo  ON Ingressos(CodigoUnico);
CREATE        INDEX IF NOT EXISTS IX_Setores_EventoId  ON Setores(EventoId);
CREATE        INDEX IF NOT EXISTS IX_Assentos_SetorId  ON Assentos(SetorId);
CREATE        INDEX IF NOT EXISTS IX_Ingressos_Usuario ON Ingressos(UsuarioId);
CREATE        INDEX IF NOT EXISTS IX_Ingressos_Assento ON Ingressos(AssentoId);

-- SEED: Usuários
-- Senhas: admin123 e cliente123 (hashes BCrypt)
-- ============================================================

INSERT OR IGNORE INTO Usuarios (Id, Nome, Email, SenhaHash, Tipo) VALUES
(1, 'Administrador', 'admin@bilheteria.com',  '$2a$11$Sj6FFqXRMKbqptoNe8/ESuTzs5q2ar5H2NrCBsw4TBfUka1eGXosm', 'Admin'),
(2, 'Cliente Teste',  'cliente@email.com',     '$2a$11$pUX86BEuYqjBB.AU/12rnOv.u3R7YWB5.e8ndOI8g6noHjnO66Ruq', 'Cliente');

-- SEED: Eventos
-- ============================================================

INSERT OR IGNORE INTO Eventos (Id, Nome, Descricao, Data, Local, ImagemUrl) VALUES
(1, 'Show de Rock 2024',              'O maior show de rock do ano com bandas internacionais!', '2025-08-15 20:00:00', 'Arena Unifeso',       '🎸'),
(2, 'Festival de Música Eletrônica',  'Uma noite inesquecível com os melhores DJs do mundo',   '2025-09-05 22:00:00', 'Clube Teresópolis',   '🎧'),
(3, 'Teatro: A Comédia dos Erros',    'Peça clássica de Shakespeare com elenco renomado',       '2025-07-20 19:00:00', 'Teatro Municipal',    '🎭');

-- SEED: Setores
-- ============================================================

INSERT OR IGNORE INTO Setores (Id, Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId) VALUES
(1, 'Pista',    80.00,  50, 50, 1),
(2, 'Camarote', 200.00, 20, 20, 1),
(3, 'Pista',    100.00, 60, 60, 2),
(4, 'VIP',      250.00, 15, 15, 2),
(5, 'Plateia',  60.00,  40, 40, 3),
(6, 'Balcão',   40.00,  20, 20, 3);

-- SEED: Assentos
-- Gerados dinamicamente conforme a lógica do AppDbContext:
-- Letra = (i-1)/10, Número = (i-1)%10 + 1  →  A1..A10, B1..B10, ...
-- Setor 1: 50 assentos | Setor 2: 20 | Setor 3: 60 | Setor 4: 15 | Setor 5: 40 | Setor 6: 20
-- ============================================================

-- Setor 1 - Pista (Show de Rock) - 50 assentos (A1-E10)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(1,'A1',0,1),(2,'A2',0,1),(3,'A3',0,1),(4,'A4',0,1),(5,'A5',0,1),
(6,'A6',0,1),(7,'A7',0,1),(8,'A8',0,1),(9,'A9',0,1),(10,'A10',0,1),
(11,'B1',0,1),(12,'B2',0,1),(13,'B3',0,1),(14,'B4',0,1),(15,'B5',0,1),
(16,'B6',0,1),(17,'B7',0,1),(18,'B8',0,1),(19,'B9',0,1),(20,'B10',0,1),
(21,'C1',0,1),(22,'C2',0,1),(23,'C3',0,1),(24,'C4',0,1),(25,'C5',0,1),
(26,'C6',0,1),(27,'C7',0,1),(28,'C8',0,1),(29,'C9',0,1),(30,'C10',0,1),
(31,'D1',0,1),(32,'D2',0,1),(33,'D3',0,1),(34,'D4',0,1),(35,'D5',0,1),
(36,'D6',0,1),(37,'D7',0,1),(38,'D8',0,1),(39,'D9',0,1),(40,'D10',0,1),
(41,'E1',0,1),(42,'E2',0,1),(43,'E3',0,1),(44,'E4',0,1),(45,'E5',0,1),
(46,'E6',0,1),(47,'E7',0,1),(48,'E8',0,1),(49,'E9',0,1),(50,'E10',0,1);

-- Setor 2 - Camarote (Show de Rock) - 20 assentos (A1-B10)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(51,'A1',0,2),(52,'A2',0,2),(53,'A3',0,2),(54,'A4',0,2),(55,'A5',0,2),
(56,'A6',0,2),(57,'A7',0,2),(58,'A8',0,2),(59,'A9',0,2),(60,'A10',0,2),
(61,'B1',0,2),(62,'B2',0,2),(63,'B3',0,2),(64,'B4',0,2),(65,'B5',0,2),
(66,'B6',0,2),(67,'B7',0,2),(68,'B8',0,2),(69,'B9',0,2),(70,'B10',0,2);

-- Setor 3 - Pista (Festival Eletrônico) - 60 assentos (A1-F10)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(71,'A1',0,3),(72,'A2',0,3),(73,'A3',0,3),(74,'A4',0,3),(75,'A5',0,3),
(76,'A6',0,3),(77,'A7',0,3),(78,'A8',0,3),(79,'A9',0,3),(80,'A10',0,3),
(81,'B1',0,3),(82,'B2',0,3),(83,'B3',0,3),(84,'B4',0,3),(85,'B5',0,3),
(86,'B6',0,3),(87,'B7',0,3),(88,'B8',0,3),(89,'B9',0,3),(90,'B10',0,3),
(91,'C1',0,3),(92,'C2',0,3),(93,'C3',0,3),(94,'C4',0,3),(95,'C5',0,3),
(96,'C6',0,3),(97,'C7',0,3),(98,'C8',0,3),(99,'C9',0,3),(100,'C10',0,3),
(101,'D1',0,3),(102,'D2',0,3),(103,'D3',0,3),(104,'D4',0,3),(105,'D5',0,3),
(106,'D6',0,3),(107,'D7',0,3),(108,'D8',0,3),(109,'D9',0,3),(110,'D10',0,3),
(111,'E1',0,3),(112,'E2',0,3),(113,'E3',0,3),(114,'E4',0,3),(115,'E5',0,3),
(116,'E6',0,3),(117,'E7',0,3),(118,'E8',0,3),(119,'E9',0,3),(120,'E10',0,3),
(121,'F1',0,3),(122,'F2',0,3),(123,'F3',0,3),(124,'F4',0,3),(125,'F5',0,3),
(126,'F6',0,3),(127,'F7',0,3),(128,'F8',0,3),(129,'F9',0,3),(130,'F10',0,3);

-- Setor 4 - VIP (Festival Eletrônico) - 15 assentos (A1-B5)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(131,'A1',0,4),(132,'A2',0,4),(133,'A3',0,4),(134,'A4',0,4),(135,'A5',0,4),
(136,'A6',0,4),(137,'A7',0,4),(138,'A8',0,4),(139,'A9',0,4),(140,'A10',0,4),
(141,'B1',0,4),(142,'B2',0,4),(143,'B3',0,4),(144,'B4',0,4),(145,'B5',0,4);

-- Setor 5 - Plateia (Teatro) - 40 assentos (A1-D10)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(146,'A1',0,5),(147,'A2',0,5),(148,'A3',0,5),(149,'A4',0,5),(150,'A5',0,5),
(151,'A6',0,5),(152,'A7',0,5),(153,'A8',0,5),(154,'A9',0,5),(155,'A10',0,5),
(156,'B1',0,5),(157,'B2',0,5),(158,'B3',0,5),(159,'B4',0,5),(160,'B5',0,5),
(161,'B6',0,5),(162,'B7',0,5),(163,'B8',0,5),(164,'B9',0,5),(165,'B10',0,5),
(166,'C1',0,5),(167,'C2',0,5),(168,'C3',0,5),(169,'C4',0,5),(170,'C5',0,5),
(171,'C6',0,5),(172,'C7',0,5),(173,'C8',0,5),(174,'C9',0,5),(175,'C10',0,5),
(176,'D1',0,5),(177,'D2',0,5),(178,'D3',0,5),(179,'D4',0,5),(180,'D5',0,5),
(181,'D6',0,5),(182,'D7',0,5),(183,'D8',0,5),(184,'D9',0,5),(185,'D10',0,5);

-- Setor 6 - Balcão (Teatro) - 20 assentos (A1-B10)
INSERT OR IGNORE INTO Assentos (Id, Numero, Status, SetorId) VALUES
(186,'A1',0,6),(187,'A2',0,6),(188,'A3',0,6),(189,'A4',0,6),(190,'A5',0,6),
(191,'A6',0,6),(192,'A7',0,6),(193,'A8',0,6),(194,'A9',0,6),(195,'A10',0,6),
(196,'B1',0,6),(197,'B2',0,6),(198,'B3',0,6),(199,'B4',0,6),(200,'B5',0,6),
(201,'B6',0,6),(202,'B7',0,6),(203,'B8',0,6),(204,'B9',0,6),(205,'B10',0,6);

-- VIEWS ÚTEIS
-- ============================================================

-- Resumo de disponibilidade por evento e setor
CREATE VIEW IF NOT EXISTS vw_DisponibilidadeSetores AS
SELECT
    e.Id        AS EventoId,
    e.Nome      AS Evento,
    e.Data      AS DataEvento,
    e.Local,
    s.Id        AS SetorId,
    s.Nome      AS Setor,
    s.Preco,
    s.QuantidadeTotal,
    s.QuantidadeDisponivel,
    (s.QuantidadeTotal - s.QuantidadeDisponivel) AS Vendidos
FROM Eventos e
JOIN Setores s ON s.EventoId = e.Id;

-- Ingressos com detalhes completos
CREATE VIEW IF NOT EXISTS vw_IngressosDetalhados AS
SELECT
    i.Id            AS IngressoId,
    i.CodigoUnico,
    i.Status        AS StatusIngresso,
    i.DataCompra,
    u.Nome          AS Comprador,
    u.Email,
    e.Nome          AS Evento,
    e.Data          AS DataEvento,
    e.Local,
    s.Nome          AS Setor,
    s.Preco,
    a.Numero        AS Assento
FROM Ingressos i
JOIN Usuarios u  ON u.Id = i.UsuarioId
JOIN Assentos a  ON a.Id = i.AssentoId
JOIN Setores  s  ON s.Id = a.SetorId
JOIN Eventos  e  ON e.Id = s.EventoId;

-- Relatório de vendas por evento
CREATE VIEW IF NOT EXISTS vw_VendasPorEvento AS
SELECT
    e.Id        AS EventoId,
    e.Nome      AS Evento,
    COUNT(i.Id) AS TotalIngressosVendidos,
    SUM(s.Preco) AS ReceitaTotal
FROM Eventos e
LEFT JOIN Setores  s ON s.EventoId = e.Id
LEFT JOIN Assentos a ON a.SetorId  = s.Id
LEFT JOIN Ingressos i ON i.AssentoId = a.Id AND i.Status = 0 -- Ativo
GROUP BY e.Id, e.Nome;
