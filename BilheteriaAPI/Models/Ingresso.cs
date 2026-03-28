namespace BilheteriaAPI.Models;

public enum StatusIngresso { Ativo, Cancelado, Utilizado }

public class Ingresso
{
    public int Id { get; set; }
    public string CodigoUnico { get; set; } = string.Empty;
    public StatusIngresso Status { get; set; } = StatusIngresso.Ativo;
    public DateTime DataCompra { get; set; } = DateTime.UtcNow;
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int AssentoId { get; set; }
    public Assento Assento { get; set; } = null!;
}
