namespace BoraAli.Core.Entities;

/// <summary>
/// Representa um item individual dentro de um pedido
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public TicketType? TicketType { get; set; }
}
