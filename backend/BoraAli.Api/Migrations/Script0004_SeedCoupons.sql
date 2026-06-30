-- ============================================================
-- Migration 0004: Cupons de seed
-- Database: SQLite
-- ============================================================

INSERT OR IGNORE INTO Coupons (Code, Description, DiscountPercent, MaxUses, CurrentUses, IsActive, ValidUntil)
VALUES
    ('POO100', 'Cupom de 100% de desconto - apenas 5 usos', 100, 5, 0, 1, '2026-12-31'),
    ('BORA50', '50% de desconto na primeira compra', 50, 20, 0, 1, '2026-12-31'),
    ('ALUNO20', '20% de desconto para estudantes', 20, 50, 0, 1, '2026-12-31');
