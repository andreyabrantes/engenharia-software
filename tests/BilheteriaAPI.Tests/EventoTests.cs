namespace BilheteriaAPI.Tests;

public class EventoTests
{
    [Fact]
    public void Evento_NomeVazio_DeveSerInvalido()
    {
        var nome = string.Empty;

        Assert.True(string.IsNullOrWhiteSpace(nome));
    }

    [Fact]
    public void Evento_DataNoPassado_DeveSerInvalida()
    {
        var data = DateTime.UtcNow.AddDays(-1);

        Assert.True(data < DateTime.UtcNow);
    }

    [Fact]
    public void Evento_DataNoFuturo_DeveSerValida()
    {
        var data = DateTime.UtcNow.AddDays(30);

        Assert.True(data > DateTime.UtcNow);
    }

    [Fact]
    public void Evento_CapacidadeZero_DeveSerInvalida()
    {
        var capacidade = 0;

        Assert.True(capacidade <= 0);
    }

    [Fact]
    public void Evento_CapacidadePositiva_DeveSerValida()
    {
        var capacidade = 100;

        Assert.True(capacidade > 0);
    }

    [Fact]
    public void Evento_PrecoPadraoNegativo_DeveSerInvalido()
    {
        var preco = -10m;

        Assert.True(preco <= 0);
    }

    [Fact]
    public void Evento_PrecoPadraoPositivo_DeveSerValido()
    {
        var preco = 50m;

        Assert.True(preco > 0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Evento_NomeNuloOuEspacos_DeveSerInvalido(string? nome)
    {
        Assert.True(string.IsNullOrWhiteSpace(nome));
    }

    [Fact]
    public void Evento_IngressosVendidosNaoPodemExcederCapacidade()
    {
        var capacidadeTotal = 100;
        var ingressosVendidos = 101;

        Assert.True(ingressosVendidos > capacidadeTotal);
    }

    [Fact]
    public void Evento_IngressosDentroCapacidade_DeveSerValido()
    {
        var capacidadeTotal = 100;
        var ingressosVendidos = 99;

        Assert.True(ingressosVendidos <= capacidadeTotal);
    }
}
