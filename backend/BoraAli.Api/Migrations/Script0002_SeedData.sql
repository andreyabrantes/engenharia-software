-- ============================================================
-- Migration 0002: Dados de seed (usuários, categorias, eventos, ingressos, pedidos)
-- Database: SQLite
-- ============================================================

-- SEED: Users
INSERT OR IGNORE INTO Users (Id, Name, Email, Cpf, PasswordHash, Phone, AvatarUrl, Role, IsActive)
VALUES
    (1, 'Admin BoraAli', 'admin@boraali.com.br', '529.982.247-25',
     '$2a$11$NiN5ZgZRo1oCj7/db.x2b.7l1iCmcIECiZhRHyJdScvdJ7uv1pa7q', -- senha: Admin@123
     '(11) 99999-0001', NULL, 'Admin', 1),

    (2, 'João Silva', 'joao@email.com', '123.456.789-09',
     '$2a$11$MZMLCXZjDjc0FSjRo8j5peogULFfYo4aXI9g.9T00YDegwJIzyg4u', -- senha: 123456
     '(11) 99999-0002', NULL, 'Cliente', 1),

    (3, 'Maria Santos', 'maria@email.com', '987.654.321-00',
     '$2a$11$MZMLCXZjDjc0FSjRo8j5peogULFfYo4aXI9g.9T00YDegwJIzyg4u', -- senha: 123456
     '(11) 99999-0003', NULL, 'Cliente', 1),

    (4, 'Carlos Eventos', 'carlos@eventos.com', '111.222.333-44',
     '$2a$11$MZMLCXZjDjc0FSjRo8j5peogULFfYo4aXI9g.9T00YDegwJIzyg4u', -- senha: 123456
     '(21) 99999-0004', NULL, 'Organizador', 1),

    (5, 'Ana Produções', 'ana@producoes.com', '555.666.777-88',
     '$2a$11$MZMLCXZjDjc0FSjRo8j5peogULFfYo4aXI9g.9T00YDegwJIzyg4u', -- senha: 123456
     '(31) 99999-0005', NULL, 'Organizador', 1);

-- SEED: Categories
INSERT OR IGNORE INTO Categories (Id, Name, Slug, Icon)
VALUES
    (1, 'Shows', 'shows', 'music'),
    (2, 'Teatro', 'teatro', 'theater'),
    (3, 'Esportes', 'esportes', 'sports'),
    (4, 'Festivais', 'festivais', 'festival'),
    (5, 'Cursos', 'cursos', 'education'),
    (6, 'Gastronomia', 'gastronomia', 'food'),
    (7, 'Tecnologia', 'tecnologia', 'tech'),
    (8, 'Infantil', 'infantil', 'kids');

-- SEED: Events
-- Limpeza de dados dependentes antes de recriar eventos (idempotência)
DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE EventId IN (1,2,3,4));
DELETE FROM Orders WHERE EventId IN (1,2,3,4);
DELETE FROM TicketTypes WHERE EventId IN (1,2,3,4);
DELETE FROM Events WHERE Id IN (1,2,3,4);

INSERT INTO Events (Id, Title, Description, FullDescription, EventDate, Time, Location, Address, Cep, Street, Neighborhood, State, AddressNumber, City, ImageUrl, IsFeatured, Status, CategoryId, OrganizerId, PublishedAt)
VALUES
    -- Evento 1: Tecnologia / Acadêmico (CategoryId=7)
    (1, 'Hackathon CCOMP 2026',
     'Maior maratona de programação da Serra.',
     'Um final de semana inteiro de muito código, palestras e networking. Traga sua equipe e resolva desafios reais da indústria com foco em inovação e IA.',
     '2026-10-15', '08:00', 'Campus Unifeso', 'Av. Alberto Torres, 111',
     '25964004', 'Av. Alberto Torres', 'Alto', 'RJ', '111', 'Teresópolis',
     'https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?w=800&q=80', 1, 'Published', 7, 4, datetime('now')),

    -- Evento 2: Festival / Gastronomia (CategoryId=4)
    (2, 'Oktoberfest na Serra',
     'Cultura cervejeira, gastronomia típica e muita música.',
     'Venha celebrar a tradição alemã com as melhores cervejas artesanais, pratos típicos e bandas ao vivo em um ambiente familiar e acolhedor.',
     '2026-10-24', '18:00', 'Vila St Gallen', 'Rua Augusto do Amaral Peixoto, 166',
     '25960110', 'Rua Augusto do Amaral Peixoto', 'Alto', 'RJ', '166', 'Teresópolis',
     'https://images.unsplash.com/photo-1514933651103-005eec06c04b?w=800&q=80', 1, 'Published', 4, 4, datetime('now')),

    -- Evento 3: Show / Música (CategoryId=1)
    (3, 'Turnê Jão - Super',
     'O maior espetáculo pop do ano em grande arena.',
     'A turnê nacional chega à cidade com um palco monumental, sucessos de todos os álbuns e uma experiência visual inesquecível para os fãs.',
     '2026-11-12', '21:30', 'Jeunesse Arena', 'Av. Embaixador Abelardo Bueno, 3401',
     '22775040', 'Av. Embaixador Abelardo Bueno', 'Barra da Tijuca', 'RJ', '3401', 'Rio de Janeiro',
     'https://picsum.photos/seed/evento3/800/400', 1, 'Published', 1, 4, datetime('now')),

    -- Evento 4: Esportes (CategoryId=3)
    (4, 'Final Regional de Futsal',
     'Disputa do campeonato universitário de futsal.',
     'A grande final regional universitária. Venha torcer pela sua atlética e acompanhar de perto quem leva a taça de 2026.',
     '2026-11-28', '10:00', 'Ginásio Pedrão', 'R. Ten. Luiz Meirelles, 211',
     '25953200', 'R. Ten. Luiz Meirelles', 'Várzea', 'RJ', '211', 'Teresópolis',
     'https://images.unsplash.com/photo-1574629810360-7efbbe195018?w=800&q=80', 1, 'Published', 3, 4, datetime('now'));

-- SEED: TicketTypes
-- Ingressos para Evento 1: Hackathon CCOMP 2026
INSERT INTO TicketTypes (Id, EventId, Name, Price, TotalQuantity, AvailableQuantity, Description)
VALUES
    (1, 1, 'Equipe (até 4 pessoas)', 50.00, 100, 95, 'Inscrição para equipe de até 4 integrantes'),
    (2, 1, 'Individual', 15.00, 200, 190, 'Inscrição individual com acesso a todas as palestras');

-- Ingressos para Evento 2: Oktoberfest na Serra
INSERT INTO TicketTypes (Id, EventId, Name, Price, TotalQuantity, AvailableQuantity, Description)
VALUES
    (3, 2, 'Entrada', 30.00, 2000, 1950, 'Acesso ao evento com caneca comemorativa'),
    (4, 2, 'VIP Open Bar', 120.00, 300, 280, 'Acesso VIP com open bar de cervejas artesanais');

-- Ingressos para Evento 3: Turnê Jão - Super
INSERT INTO TicketTypes (Id, EventId, Name, Price, TotalQuantity, AvailableQuantity, Description)
VALUES
    (5, 3, 'Pista', 80.00, 5000, 4900, 'Pista comum'),
    (6, 3, 'Pista Premium', 180.00, 2000, 1950, 'Pista premium com acesso a área exclusiva'),
    (7, 3, 'Camarote', 350.00, 500, 480, 'Camarote open bar com vista panorâmica');

-- Ingressos para Evento 4: Final Regional de Futsal
INSERT INTO TicketTypes (Id, EventId, Name, Price, TotalQuantity, AvailableQuantity, Description)
VALUES
    (8, 4, 'Arquibancada', 20.00, 1000, 970, 'Arquibancada geral'),
    (9, 4, 'Cadeira', 40.00, 500, 480, 'Cadeira numerada coberta');

-- SEED: Orders (pedidos de exemplo)
-- Pedido 1: João comprou ingressos para o Hackathon
INSERT OR IGNORE INTO Orders (Id, UserId, EventId, OrderCode, TotalAmount, Status, PaymentMethod, CreatedAt, ConfirmedAt)
VALUES
    (1, 2, 1, 'BA-20240601-A1B2C3D4', 100.00, 'Confirmed', 'Pix', datetime('now', '-3 days'), datetime('now', '-3 days'));

INSERT OR IGNORE INTO OrderItems (Id, OrderId, TicketTypeId, Quantity, UnitPrice, Subtotal)
VALUES
    (1, 1, 1, 2, 50.00, 100.00);

-- Pedido 2: Maria comprou ingresso para o show do Jão
INSERT OR IGNORE INTO Orders (Id, UserId, EventId, OrderCode, TotalAmount, Status, PaymentMethod, CreatedAt, ConfirmedAt)
VALUES
    (2, 3, 3, 'BA-20240602-E5F6G7H8', 360.00, 'Confirmed', 'CreditCard', datetime('now', '-1 days'), datetime('now', '-1 days'));

INSERT OR IGNORE INTO OrderItems (Id, OrderId, TicketTypeId, Quantity, UnitPrice, Subtotal)
VALUES
    (2, 2, 6, 2, 180.00, 360.00);
