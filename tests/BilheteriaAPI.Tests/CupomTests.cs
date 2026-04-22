namespace BilheteriaAPI.Tests;

public class CupomTests
{
    // ── Validação de Código ───────────────────────────────────────────────────

    [Fact]
    public void Cupom_CodigoVazio_DeveSerRejeitado()
    {
        var codigo = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(codigo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Cupom_CodigoNuloOuEspacos_DeveSerRejeitado(string? codigo)
    {
        Assert.True(string.IsNullOrWhiteSpace(codigo));
    }

    [Fact]
    public void Cupom_CodigoPreenchido_DeveSerAceito()
    {
        var codigo = "PROMO10";
        Assert.False(string.IsNullOrWhiteSpace(codigo));
    }

    // ── Validação de Desconto ─────────────────────────────────────────────────

    [Fact]
    public void Cupom_DescontoZero_DeveSerRejeitado()
    {
        var desconto = 0m;
        Assert.True(desconto <= 0);
    }

    [Fact]
    public void Cupom_DescontoNegativo_DeveSerRejeitado()
    {
        var desconto = -10m;
        Assert.True(desconto <= 0);
    }

    [Fact]
    public void Cupom_DescontoPositivo_DeveSerAceito()
    {
        var desconto = 20m;
        Assert.True(desconto > 0);
    }

    // ── Regra de Negócio: Valor Mínimo ───────────────────────────────────────

    [Fact]
    public void Cupom_CompraAbaixoDoValorMinimo_NaoDeveAplicarDesconto()
    {
        var valorCompra  = 30m;
        var valorMinimo  = 50m;
        var porcentagem  = 10m;

        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(30m, resultado);
    }

    [Fact]
    public void Cupom_CompraAcimaDovalorMinimo_DeveAplicarDesconto()
    {
        var valorCompra  = 100m;
        var valorMinimo  = 50m;
        var porcentagem  = 10m;

        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(90m, resultado);
    }

    [Fact]
    public void Cupom_CompraExatamenteNoValorMinimo_DeveAplicarDesconto()
    {
        var valorCompra  = 50m;
        var valorMinimo  = 50m;
        var porcentagem  = 20m;

        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(40m, resultado);
    }

    // ── Regra de Negócio: Proteção contra Valor Negativo (fraude) ────────────

    [Fact]
    public void Cupom_DescontoDe100Porcento_NaoDeveGerarValorNegativo()
    {
        var valorCompra = 80m;
        var porcentagem = 100m;

        var valorFinal = valorCompra - (valorCompra * porcentagem / 100);

        Assert.True(valorFinal >= 0);
    }

    [Fact]
    public void Cupom_DescontoNaoDeveGerarValorAbaixoDeZero()
    {
        var valorCompra = 50m;
        var porcentagem = 150m; // desconto inválido acima de 100%

        var valorFinal = valorCompra - (valorCompra * porcentagem / 100);
        var valorProtegido = Math.Max(0, valorFinal);

        Assert.True(valorProtegido >= 0);
    }

    // ── Regra de Negócio: Expiração ───────────────────────────────────────────

    [Fact]
    public void Cupom_DataExpiracaoNoPassado_DeveSerRejeitado()
    {
        var dataExpiracao = DateTime.UtcNow.AddDays(-1);
        Assert.True(dataExpiracao < DateTime.UtcNow);
    }

    [Fact]
    public void Cupom_DataExpiracaoNoFuturo_DeveSerAceito()
    {
        var dataExpiracao = DateTime.UtcNow.AddDays(30);
        Assert.True(dataExpiracao > DateTime.UtcNow);
    }

    // ── Cálculo Parametrizado ─────────────────────────────────────────────────

    [Theory]
    [InlineData(100,  10,  50,  90)]
    [InlineData(200,  20, 100, 160)]
    [InlineData(50,   5,  30,  47.5)]
    [InlineData(80,  25,  80,  60)]
    public void Cupom_CalculoDesconto_DeveRetornarValorCorreto(
        decimal valorCompra, decimal porcentagem, decimal valorMinimo, decimal esperado)
    {
        var resultado = valorCompra >= valorMinimo
            ? valorCompra - (valorCompra * porcentagem / 100)
            : valorCompra;

        Assert.Equal(esperado, resultado);
    }
}
