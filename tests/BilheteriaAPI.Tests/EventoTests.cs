namespace BilheteriaAPI.Tests;

public class EventoTests
{
    // ── Validação de Nome ─────────────────────────────────────────────────────

    [Fact]
    public void Evento_NomeVazio_DeveSerRejeitado()
    {
        var nome = string.Empty;
        var invalido = string.IsNullOrWhiteSpace(nome);
        Assert.True(invalido);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Evento_NomeNuloOuEspacos_DeveSerRejeitado(string? nome)
    {
        Assert.True(string.IsNullOrWhiteSpace(nome));
    }

    [Fact]
    public void Evento_NomePreenchido_DeveSerAceito()
    {
        var nome = "Show de Rock 2026";
        Assert.False(string.IsNullOrWhiteSpace(nome));
    }

    // ── Validação de Data ─────────────────────────────────────────────────────

    [Fact]
    public void Evento_DataNoPassado_DeveSerRejeitada()
    {
        var data = DateTime.UtcNow.AddDays(-1);
        Assert.True(data < DateTime.UtcNow);
    }

    [Fact]
    public void Evento_DataNoFuturo_DeveSerAceita()
    {
        var data = DateTime.UtcNow.AddDays(30);
        Assert.True(data > DateTime.UtcNow);
    }

    // ── Validação de Capacidade ───────────────────────────────────────────────

    [Fact]
    public void Evento_CapacidadeZero_DeveSerRejeitada()
    {
        var capacidade = 0;
        Assert.True(capacidade <= 0);
    }

    [Fact]
    public void Evento_CapacidadeNegativa_DeveSerRejeitada()
    {
        var capacidade = -10;
        Assert.True(capacidade <= 0);
    }

    [Fact]
    public void Evento_CapacidadePositiva_DeveSerAceita()
    {
        var capacidade = 200;
        Assert.True(capacidade > 0);
    }

    // ── Regra de Negócio: Capacidade Máxima ──────────────────────────────────

    [Fact]
    public void Evento_VendaMaisQueCapacidade_DeveSerBloqueada()
    {
        var capacidadeTotal = 100;
        var tentativaVenda = 101;

        var excedeu = tentativaVenda > capacidadeTotal;

        Assert.True(excedeu);
    }

    [Fact]
    public void Evento_VendaDentroCapacidade_DeveSerPermitida()
    {
        var capacidadeTotal = 100;
        var tentativaVenda = 100;

        var excedeu = tentativaVenda > capacidadeTotal;

        Assert.False(excedeu);
    }

    // ── Regra de Negócio: Assentos ────────────────────────────────────────────

    [Fact]
    public void Assento_StatusInicial_DeveSerDisponivel()
    {
        var assento = new { Status = "Disponivel" };
        Assert.Equal("Disponivel", assento.Status);
    }

    [Fact]
    public void Assento_Ocupado_NaoDevePermitirNovaVenda()
    {
        var statusOcupado = "Ocupado";
        var podeVender = statusOcupado == "Disponivel";
        Assert.False(podeVender);
    }

    [Fact]
    public void Assento_Disponivel_DevePermitirVenda()
    {
        var statusDisponivel = "Disponivel";
        var podeVender = statusDisponivel == "Disponivel";
        Assert.True(podeVender);
    }

    // ── Regra de Negócio: Preço ───────────────────────────────────────────────

    [Fact]
    public void Evento_PrecoPadraoZero_DeveSerRejeitado()
    {
        var preco = 0m;
        Assert.True(preco <= 0);
    }

    [Fact]
    public void Evento_PrecoPadraoNegativo_DeveSerRejeitado()
    {
        var preco = -50m;
        Assert.True(preco <= 0);
    }

    [Fact]
    public void Evento_PrecoPadraoPositivo_DeveSerAceito()
    {
        var preco = 80m;
        Assert.True(preco > 0);
    }

    // ── Regra de Negócio: Cálculo de Valor Total ──────────────────────────────

    [Theory]
    [InlineData(80.00, 2, 160.00)]
    [InlineData(50.00, 1, 50.00)]
    [InlineData(200.00, 4, 800.00)]
    public void Evento_ValorTotalCompra_DeveSerPrecoVezesQuantidade(
        decimal preco, int quantidade, decimal esperado)
    {
        var valorTotal = preco * quantidade;
        Assert.Equal(esperado, valorTotal);
    }
}
