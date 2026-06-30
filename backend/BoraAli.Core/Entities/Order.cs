namespace BoraAli.Core.Entities;

/// <summary>
/// Representa um pedido de compra de ingressos
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Refunded, Used
    public string? PaymentMethod { get; set; }
    public string? PaymentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
    public Event? Event { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
