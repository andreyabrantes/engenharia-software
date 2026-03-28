using System.Net.Http.Json;
using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private Usuario? _usuarioLogado;

    public event Action? OnAuthStateChanged;

    // IHttpClientFactory não existe em WASM — recebe o HttpClient via DI manual
    public AuthService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Envia POST api/auth/login e guarda o utilizador em memória.
    /// Retorna true em caso de sucesso, false se credenciais inválidas.
    /// </summary>
    public async Task<bool> LoginAsync(string email, string senha)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest
            {
                Email = email,
                Senha = senha
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var resultado = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (resultado is null)
                return false;

            _usuarioLogado = new Usuario
            {
                Id    = resultado.Id,
                Nome  = resultado.Nome,
                Email = resultado.Email,
                Tipo  = resultado.Tipo.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                            ? TipoUsuario.Admin
                            : TipoUsuario.Cliente
            };

            OnAuthStateChanged?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Mantém assinatura síncrona usada nas páginas existentes como wrapper
    public bool Login(string email, string senha)
        => LoginAsync(email, senha).GetAwaiter().GetResult();

    public async Task<(bool sucesso, string erro)> RegisterAsync(
        string nome, string email, string cpf, string senha, string tipo)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new
            {
                Nome  = nome,
                Email = email,
                Cpf   = cpf,
                Senha = senha,
                Tipo  = tipo
            });

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var erro = await response.Content.ReadFromJsonAsync<ErroResponse>();
            return (false, erro?.Mensagem ?? "Erro ao cadastrar.");
        }
        catch
        {
            return (false, "Erro de conexão com o servidor.");
        }
    }

    public void Logout()
    {
        _usuarioLogado = null;
        OnAuthStateChanged?.Invoke();
    }

    public Usuario? ObterUsuarioLogado() => _usuarioLogado;
    public bool EstaAutenticado()        => _usuarioLogado != null;
    public bool EhAdmin()                => _usuarioLogado?.Tipo == TipoUsuario.Admin;
    public bool EhCliente()              => _usuarioLogado?.Tipo == TipoUsuario.Cliente;

    // DTO interno para deserializar a resposta da API
    private sealed class LoginResponse
    {
        public int    Id    { get; set; }
        public string Nome  { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Tipo  { get; set; } = "Cliente";
    }

    private sealed class ErroResponse
    {
        public string Mensagem { get; set; } = string.Empty;
    }
}
