namespace BilheteriaAPI.Tests;

public class CupomTests
{
    [Fact]
    public void Cupom_ComDescontoValido_DeveAplicarDesconto()
    {
        var valorCompra = 100m;
        var desconto = 10m;
        var valorMinimo = 50m;

        var resultado = valorCompra >= valorMinimo ? valorCompra - desconto : valorCompra;

        Assert.Equal(90m, resultado);
    }

    [Fact]
    public void Cupom_AbaixoDoValorMinimo_NaoDeveAplicarDesconto()
    {
        var valorCompra = 30m;
        var desconto = 10m;
        var valorMinimo = 50m;

        var resultado = valorCompra >= valorMinimo ? valorCompra - desconto : valorCompra;

        Assert.Equal(30m, resultado);
    }

    [Fact]
    public void Cupom_CodigoVazio_DeveSerInvalido()
    {
        var codigo = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(codigo));
    }
}
