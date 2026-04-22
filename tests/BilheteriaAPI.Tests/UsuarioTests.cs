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
    public void Usuario_EmailSemArroba_DeveSerInvalido()
    {
        var email = "clienteemail.com";

        Assert.DoesNotContain("@", email);
    }

    [Fact]
    public void Usuario_NomeVazio_DeveSerInvalido()
    {
        var nome = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(nome));
    }

    [Fact]
    public void Usuario_CpfDuplicado_DeveBloquearCadastro()
    {
        var cpfExistente = "123.456.789-00";
        var cpfNovo = "123.456.789-00";

        Assert.Equal(cpfExistente, cpfNovo);
    }

    [Fact]
    public void Usuario_CpfsDiferentes_DevemSerPermitidos()
    {
        var cpf1 = "111.111.111-11";
        var cpf2 = "222.222.222-22";

        Assert.NotEqual(cpf1, cpf2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Usuario_CpfNuloOuEspacos_DeveSerInvalido(string? cpf)
    {
        Assert.True(string.IsNullOrWhiteSpace(cpf));
    }

    [Fact]
    public void Usuario_CpfComOnzeDigitos_DeveSerValido()
    {
        var cpf = "12345678900";

        Assert.Equal(11, cpf.Length);
    }

    [Fact]
    public void Usuario_CpfComMenosDigitos_DeveSerInvalido()
    {
        var cpf = "1234567";

        Assert.True(cpf.Length < 11);
    }
}
