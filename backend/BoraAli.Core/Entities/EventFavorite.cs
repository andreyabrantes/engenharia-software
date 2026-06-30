namespace BoraAli.Core.Entities;

/// <summary>
/// Representa a relação de favorito entre um usuário e um evento
/// </summary>
public class EventFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public Event? Event { get; set; }
}
