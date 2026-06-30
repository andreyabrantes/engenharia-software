namespace BoraAli.Core.Entities;

/// <summary>
/// Representa a relação de seguir entre um usuário (follower) e um organizador
/// </summary>
public class OrganizerFollow
{
    public int Id { get; set; }
    public int FollowerId { get; set; }
    public int OrganizerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Follower { get; set; }
    public User? Organizer { get; set; }
}
