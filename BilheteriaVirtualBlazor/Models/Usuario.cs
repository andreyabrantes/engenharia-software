namespace BilheteriaVirtualBlazor.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario Tipo { get; set; }
}

public enum TipoUsuario
{
    Cliente,
    Admin
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
