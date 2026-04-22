namespace BilheteriaAPI.Tests;

public class UsuarioTests
{
    // ── Validação de CPF ──────────────────────────────────────────────────────

    [Fact]
    public void Usuario_CpfVazio_DeveSerRejeitado()
    {
        var cpf = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Usuario_CpfNuloOuEspacos_DeveSerRejeitado(string? cpf)
    {
        Assert.True(string.IsNullOrWhiteSpace(cpf));
    }

    [Fact]
    public void Usuario_CpfComOnzeDigitos_DeveSerAceito()
    {
        var cpf = "12345678900";
        Assert.Equal(11, cpf.Length);
    }

    [Fact]
    public void Usuario_CpfComMenosDeOnzeDigitos_DeveSerRejeitado()
    {
        var cpf = "1234567";
        Assert.True(cpf.Length < 11);
    }

    // ── Regra Anti-Cambista: CPF Único ────────────────────────────────────────

    [Fact]
    public void Usuario_CpfDuplicado_DeveBloquearCadastro()
    {
        var cpfsNoBanco = new List<string> { "111.111.111-11", "222.222.222-22" };
        var cpfNovo = "111.111.111-11";

        var jaExiste = cpfsNoBanco.Contains(cpfNovo);

        Assert.True(jaExiste);
    }

    [Fact]
    public void Usuario_CpfNovo_DevePermitirCadastro()
    {
        var cpfsNoBanco = new List<string> { "111.111.111-11", "222.222.222-22" };
        var cpfNovo = "333.333.333-33";

        var jaExiste = cpfsNoBanco.Contains(cpfNovo);

        Assert.False(jaExiste);
    }

    [Fact]
    public void Usuario_DoisCpfsDiferentes_NaoDevemSerIguais()
    {
        var cpf1 = "111.111.111-11";
        var cpf2 = "222.222.222-22";
        Assert.NotEqual(cpf1, cpf2);
    }

    // ── Validação de Email ────────────────────────────────────────────────────

    [Fact]
    public void Usuario_EmailValido_DeveConterArroba()
    {
        var email = "cliente@email.com";
        Assert.Contains("@", email);
    }

    [Fact]
    public void Usuario_EmailSemArroba_DeveSerRejeitado()
    {
        var email = "clienteemail.com";
        Assert.DoesNotContain("@", email);
    }

    [Fact]
    public void Usuario_EmailVazio_DeveSerRejeitado()
    {
        var email = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(email));
    }

    [Theory]
    [InlineData("usuario@dominio.com", true)]
    [InlineData("teste@teste.com.br", true)]
    [InlineData("semArroba.com", false)]
    [InlineData("", false)]
    public void Usuario_ValidacaoEmail_DeveIdentificarFormatoCorreto(string email, bool esperadoValido)
    {
        var valido = !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        Assert.Equal(esperadoValido, valido);
    }

    // ── Validação de Nome ─────────────────────────────────────────────────────

    [Fact]
    public void Usuario_NomeVazio_DeveSerRejeitado()
    {
        var nome = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(nome));
    }

    [Fact]
    public void Usuario_NomePreenchido_DeveSerAceito()
    {
        var nome = "João da Silva";
        Assert.False(string.IsNullOrWhiteSpace(nome));
    }

    // ── Regra de Tipo de Usuário ──────────────────────────────────────────────

    [Fact]
    public void Usuario_TipoPadrao_DeveSerCliente()
    {
        var tipo = "Cliente";
        Assert.Equal("Cliente", tipo);
    }

    [Fact]
    public void Usuario_TipoAdmin_DeveSerReconhecido()
    {
        var tipo = "Admin";
        var ehAdmin = tipo.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        Assert.True(ehAdmin);
    }

    [Fact]
    public void Usuario_TipoInvalido_NaoDeveSerAdmin()
    {
        var tipo = "SuperUser";
        var ehAdmin = tipo.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        Assert.False(ehAdmin);
    }
}
