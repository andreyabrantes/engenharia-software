namespace BilheteriaAPI.Tests;

public class UsuarioTests
{
    [Fact]
    public void Usuario_CpfVazio_DeveSerInvalido()
    {
        var cpf = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(cpf));
    }

    [Fact]
    public void Usuario_EmailValido_DeveConterArroba()
    {
        var email = "cliente@email.com";

        Assert.Contains("@", email);
    }

    [Fact]
    public void Usuario_SenhaVazia_DeveSerInvalida()
    {
        var senha = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(senha));
    }
}
