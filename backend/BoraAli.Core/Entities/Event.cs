namespace BoraAli.Core.Entities;

/// <summary>
/// Representa um evento criado por um organizador
/// </summary>
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Cep { get; set; }
    public string? Street { get; set; }
    public string? Neighborhood { get; set; }
    public string? State { get; set; }
    public string? AddressNumber { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Published, Cancelled, Finished
    public int CategoryId { get; set; }
    public int OrganizerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public User? Organizer { get; set; }
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
