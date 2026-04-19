namespace BilheteriaAPI.Models;

public enum StatusAssento { Disponivel, Reservado, Ocupado }

public class Assento
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public StatusAssento Status { get; set; } = StatusAssento.Disponivel;
    public int SetorId { get; set; }
    public Setor Setor { get; set; } = null!;
}
