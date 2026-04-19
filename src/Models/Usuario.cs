namespace BilheteriaAPI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Cliente"; // "Admin" ou "Cliente"
    public ICollection<Ingresso> Ingressos { get; set; } = [];
}
