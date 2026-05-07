namespace BilheteriaAPI.Models;

/// <summary>
/// Representa uma reserva de ingresso.
/// Tabela: Reservas
/// PK: Id (INTEGER AUTOINCREMENT)
/// FKs: UsuarioCpf → Usuarios.Cpf, EventoId → Eventos.Id, CupomUtilizado → Cupons.Codigo
/// </summary>
public class Reserva
{
    public int Id { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string? CupomUtilizado { get; set; }
    public decimal ValorFinalPago { get; set; }
}
