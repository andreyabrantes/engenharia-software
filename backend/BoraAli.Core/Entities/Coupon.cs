namespace BoraAli.Core.Entities;

/// <summary>
/// Representa um cupom de desconto para eventos
/// </summary>
public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; } // 0 a 100
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public bool IsActive { get; set; } = true;
    public int? EventId { get; set; } // null = cupom global, valor = específico para um evento
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event? Event { get; set; }
}
