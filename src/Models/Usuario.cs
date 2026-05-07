namespace BilheteriaAPI.Models;

/// <summary>
/// Representa um usuário do sistema TicketPrime.
/// Tabela: Usuarios
/// PK: Cpf (VARCHAR(14))
/// </summary>
public class Usuario
{
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
