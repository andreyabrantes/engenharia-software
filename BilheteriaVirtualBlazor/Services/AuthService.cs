using System.Net.Http.Json;
using System.Text.Json;
using BilheteriaVirtualBlazor.Models;
using Microsoft.JSInterop;

namespace BilheteriaVirtualBlazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private Usuario? _usuarioLogado;
    private const string StorageKey = "bilheteria_usuario";

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js   = js;
    }

    /// <summary>
    /// Restaura a sessão do localStorage. Deve ser chamado uma vez no startup (App.razor).
    /// </summary>
    public async Task RestaurarSessaoAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorageHelper.get", StorageKey);
            if (string.IsNullOrWhiteSpace(json)) return;
            _usuarioLogado = JsonSerializer.Deserialize<Usuario>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { _usuarioLogado = null; }
    }

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

            await _js.InvokeVoidAsync("localStorageHelper.set", StorageKey,
                JsonSerializer.Serialize(_usuarioLogado));

            OnAuthStateChanged?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

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

    public async Task LogoutAsync()
    {
        _usuarioLogado = null;
        await _js.InvokeVoidAsync("localStorageHelper.remove", StorageKey);
        OnAuthStateChanged?.Invoke();
    }

    // Mantém wrapper síncrono para compatibilidade
    public void Logout() => LogoutAsync().GetAwaiter().GetResult();

    public Usuario? ObterUsuarioLogado() => _usuarioLogado;
    public bool EstaAutenticado()        => _usuarioLogado != null;
    public bool EhAdmin()                => _usuarioLogado?.Tipo == TipoUsuario.Admin;
    public bool EhCliente()              => _usuarioLogado?.Tipo == TipoUsuario.Cliente;

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
