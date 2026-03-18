using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class AuthService
{
    private List<Usuario> _usuarios = new();
    private Usuario? _usuarioLogado;

    public event Action? OnAuthStateChanged;

    public AuthService()
    {
        InicializarUsuarios();
    }

    private void InicializarUsuarios()
    {
        _usuarios = new List<Usuario>
        {
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@bilheteria.com",
                Senha = "admin123",
                Tipo = TipoUsuario.Admin
            },
            new Usuario
            {
                Id = 2,
                Nome = "Cliente Teste",
                Email = "cliente@email.com",
                Senha = "cliente123",
                Tipo = TipoUsuario.Cliente
            }
        };
    }

    public bool Login(string email, string senha)
    {
        var usuario = _usuarios.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
            u.Senha == senha);

        if (usuario != null)
        {
            _usuarioLogado = usuario;
            OnAuthStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public void Logout()
    {
        _usuarioLogado = null;
        OnAuthStateChanged?.Invoke();
    }

    public Usuario? ObterUsuarioLogado() => _usuarioLogado;

    public bool EstaAutenticado() => _usuarioLogado != null;

    public bool EhAdmin() => _usuarioLogado?.Tipo == TipoUsuario.Admin;

    public bool EhCliente() => _usuarioLogado?.Tipo == TipoUsuario.Cliente;
}
