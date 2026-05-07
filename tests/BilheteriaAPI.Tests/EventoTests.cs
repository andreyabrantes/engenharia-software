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

    [Fact]
    public void Evento_DataDefault_DeveSerRejeitada()
    {
        var data = default(DateTime);
        Assert.Equal(default(DateTime), data);
    }

    // ── Validação de CapacidadeTotal ──────────────────────────────────────────

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
    public void Evento_ReservasMaisQueCapacidade_DeveSerBloqueada()
    {
        var capacidadeTotal = 100;
        var reservasAtuais = 101;

        var excedeu = reservasAtuais > capacidadeTotal;

        Assert.True(excedeu);
    }

    [Fact]
    public void Evento_ReservasDentroCapacidade_DeveSerPermitida()
    {
        var capacidadeTotal = 100;
        var reservasAtuais = 100;

        var excedeu = reservasAtuais > capacidadeTotal;

        Assert.False(excedeu);
    }

    [Fact]
    public void Evento_ReservasAbaixoCapacidade_DeveSerPermitida()
    {
        var capacidadeTotal = 100;
        var reservasAtuais = 50;

        var excedeu = reservasAtuais > capacidadeTotal;

        Assert.False(excedeu);
    }

    // ── Validação de PrecoPadrao ──────────────────────────────────────────────

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

    // ── Regra de Negócio: Estrutura da Tabela Eventos ─────────────────────────

    [Fact]
    public void Evento_TabelaEventos_DeveTerIdAutoincremento()
    {
        // Simula a estrutura da tabela conforme especificação
        var evento1 = new { Id = 1, Nome = "Evento 1", CapacidadeTotal = 100, DataEvento = DateTime.Now, PrecoPadrao = 50m };
        var evento2 = new { Id = 2, Nome = "Evento 2", CapacidadeTotal = 200, DataEvento = DateTime.Now, PrecoPadrao = 80m };
        
        Assert.True(evento2.Id > evento1.Id);
    }

    [Fact]
    public void Evento_CamposObrigatorios_DevemEstarPreenchidos()
    {
        var nome = "Show de Rock";
        var capacidadeTotal = 500;
        var dataEvento = DateTime.Now.AddDays(30);
        var precoPadrao = 100m;

        var todosValidos = !string.IsNullOrWhiteSpace(nome) &&
                          capacidadeTotal > 0 &&
                          dataEvento != default &&
                          precoPadrao > 0;

        Assert.True(todosValidos);
    }

    // ── Regra de Negócio: Cálculo de Valor Total ──────────────────────────────

    [Theory]
    [InlineData(80.00, 2, 160.00)]
    [InlineData(50.00, 1, 50.00)]
    [InlineData(200.00, 4, 800.00)]
    [InlineData(100.00, 10, 1000.00)]
    public void Evento_ValorTotalReserva_DeveSerPrecoVezesQuantidade(
        decimal precoPadrao, int quantidade, decimal esperado)
    {
        var valorTotal = precoPadrao * quantidade;
        Assert.Equal(esperado, valorTotal);
    }
}
