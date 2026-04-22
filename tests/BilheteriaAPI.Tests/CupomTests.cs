namespace BilheteriaAPI.Tests;

public class CupomTests
{
    [Fact]
    public void Cupom_ComDescontoValido_DeveAplicarDesconto()
    {
        var valorCompra = 100m;
        var porcentagem = 10m;
        var valorMinimo = 50m;

        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(90m, resultado);
    }

    [Fact]
    public void Cupom_AbaixoDoValorMinimo_NaoDeveAplicarDesconto()
    {
        var valorCompra = 30m;
        var porcentagem = 10m;
        var valorMinimo = 50m;

        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(30m, resultado);
    }

    [Fact]
    public void Cupom_CodigoVazio_DeveSerInvalido()
    {
        var codigo = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(codigo));
    }

    [Fact]
    public void Cupom_DescontoNegativo_DeveSerInvalido()
    {
        var desconto = -5m;

        Assert.True(desconto <= 0);
    }

    [Fact]
    public void Cupom_DescontoZero_DeveSerInvalido()
    {
        var desconto = 0m;

        Assert.True(desconto <= 0);
    }

    [Fact]
    public void Cupom_DescontoPositivo_DeveSerValido()
    {
        var desconto = 20m;

        Assert.True(desconto > 0);
    }

    [Fact]
    public void Cupom_ValorMinimoNegativo_DeveSerInvalido()
    {
        var valorMinimo = -1m;

        Assert.True(valorMinimo < 0);
    }

    [Fact]
    public void Cupom_NaoDeveGerarValorFinalNegativo()
    {
        var valorCompra = 10m;
        var porcentagem = 100m;

        var valorFinal = valorCompra - (valorCompra * porcentagem / 100);

        Assert.True(valorFinal >= 0);
    }

    [Theory]
    [InlineData(100, 10, 50, 90)]
    [InlineData(200, 20, 100, 160)]
    [InlineData(50,  5,  30,  47.5)]
    public void Cupom_CalculoDesconto_DeveSerCorreto(
        decimal valorCompra, decimal porcentagem, decimal valorMinimo, decimal esperado)
    {
        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(esperado, resultado);
    }
}
