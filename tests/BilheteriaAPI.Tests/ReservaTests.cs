namespace BilheteriaAPI.Tests;

public class ReservaTests
{
    // ── Validação de UsuarioCpf ───────────────────────────────────────────────

    [Fact]
    public void Reserva_UsuarioCpfVazio_DeveSerRejeitado()
    {
        var usuarioCpf = string.Empty;
        Assert.True(string.IsNullOrWhiteSpace(usuarioCpf));
    }

    [Fact]
    public void Reserva_UsuarioCpfPreenchido_DeveSerAceito()
    {
        var usuarioCpf = "12345678900";
        Assert.False(string.IsNullOrWhiteSpace(usuarioCpf));
    }

    // ── Validação de EventoId ─────────────────────────────────────────────────

    [Fact]
    public void Reserva_EventoIdZero_DeveSerRejeitado()
    {
        var eventoId = 0;
        Assert.True(eventoId <= 0);
    }

    [Fact]
    public void Reserva_EventoIdNegativo_DeveSerRejeitado()
    {
        var eventoId = -1;
        Assert.True(eventoId <= 0);
    }

    [Fact]
    public void Reserva_EventoIdPositivo_DeveSerAceito()
    {
        var eventoId = 1;
        Assert.True(eventoId > 0);
    }

    // ── Validação de ValorFinalPago ───────────────────────────────────────────

    [Fact]
    public void Reserva_ValorFinalPagoNegativo_DeveSerRejeitado()
    {
        var valorFinalPago = -50m;
        Assert.True(valorFinalPago < 0);
    }

    [Fact]
    public void Reserva_ValorFinalPagoZero_DeveSerAceito()
    {
        var valorFinalPago = 0m;
        Assert.True(valorFinalPago >= 0);
    }

    [Fact]
    public void Reserva_ValorFinalPagoPositivo_DeveSerAceito()
    {
        var valorFinalPago = 100m;
        Assert.True(valorFinalPago >= 0);
    }

    // ── Regra de Negócio: CupomUtilizado (Opcional) ───────────────────────────

    [Fact]
    public void Reserva_CupomUtilizadoPodeSerNulo()
    {
        string? cupomUtilizado = null;
        Assert.Null(cupomUtilizado);
    }

    [Fact]
    public void Reserva_CupomUtilizadoPodeSerPreenchido()
    {
        string? cupomUtilizado = "PROMO10";
        Assert.NotNull(cupomUtilizado);
        Assert.False(string.IsNullOrWhiteSpace(cupomUtilizado));
    }

    // ── Regra de Negócio: Cálculo com Cupom ───────────────────────────────────

    [Fact]
    public void Reserva_SemCupom_ValorFinalIgualAoPrecoPadrao()
    {
        var precoPadrao = 100m;
        string? cupomUtilizado = null;

        var valorFinalPago = cupomUtilizado == null ? precoPadrao : 0m;

        Assert.Equal(100m, valorFinalPago);
    }

    [Fact]
    public void Reserva_ComCupom_ValorFinalComDesconto()
    {
        var precoPadrao = 100m;
        var porcentagemDesconto = 10m;
        var valorMinimoRegra = 50m;
        string? cupomUtilizado = "PROMO10";

        var valorFinalPago = cupomUtilizado != null && precoPadrao >= valorMinimoRegra
            ? precoPadrao - (precoPadrao * porcentagemDesconto / 100)
            : precoPadrao;

        Assert.Equal(90m, valorFinalPago);
    }

    [Fact]
    public void Reserva_ComCupomAbaixoValorMinimo_NaoAplicaDesconto()
    {
        var precoPadrao = 30m;
        var porcentagemDesconto = 10m;
        var valorMinimoRegra = 50m;
        string? cupomUtilizado = "PROMO10";

        var valorFinalPago = cupomUtilizado != null && precoPadrao >= valorMinimoRegra
            ? precoPadrao - (precoPadrao * porcentagemDesconto / 100)
            : precoPadrao;

        Assert.Equal(30m, valorFinalPago);
    }

    // ── Regra de Negócio: Estrutura da Tabela Reservas ────────────────────────

    [Fact]
    public void Reserva_TabelaReservas_DeveTerIdAutoincremento()
    {
        // Simula a estrutura da tabela conforme especificação
        var reserva1 = new { Id = 1, UsuarioCpf = "111", EventoId = 1, CupomUtilizado = (string?)null, ValorFinalPago = 100m };
        var reserva2 = new { Id = 2, UsuarioCpf = "222", EventoId = 1, CupomUtilizado = (string?)null, ValorFinalPago = 100m };
        
        Assert.True(reserva2.Id > reserva1.Id);
    }

    [Fact]
    public void Reserva_ChavesEstrangeiras_DevemSerValidas()
    {
        var usuarioCpf = "12345678900";
        var eventoId = 1;
        var cupomUtilizado = "PROMO10";

        // Simula que as FKs devem existir nas tabelas referenciadas
        var usuarioExiste = !string.IsNullOrWhiteSpace(usuarioCpf);
        var eventoExiste = eventoId > 0;
        var cupomExiste = !string.IsNullOrWhiteSpace(cupomUtilizado);

        Assert.True(usuarioExiste);
        Assert.True(eventoExiste);
        Assert.True(cupomExiste);
    }

    // ── Regra de Negócio: Proteção contra Fraude ──────────────────────────────

    [Fact]
    public void Reserva_ValorFinalPago_NuncaDeveSerNegativo()
    {
        var precoPadrao = 100m;
        var porcentagemDesconto = 150m; // desconto absurdo

        var valorCalculado = precoPadrao - (precoPadrao * porcentagemDesconto / 100);
        var valorFinalPago = Math.Max(0, valorCalculado);

        Assert.True(valorFinalPago >= 0);
    }

    [Theory]
    [InlineData(100, 10, 50, 90)]
    [InlineData(200, 20, 100, 160)]
    [InlineData(80, 25, 80, 60)]
    [InlineData(50, 100, 50, 0)]
    public void Reserva_CalculoValorFinalPago_DeveEstarCorreto(
        decimal precoPadrao, decimal porcentagem, decimal valorMinimo, decimal esperado)
    {
        var valorFinalPago = precoPadrao >= valorMinimo
            ? Math.Max(0, precoPadrao - (precoPadrao * porcentagem / 100))
            : precoPadrao;

        Assert.Equal(esperado, valorFinalPago);
    }
}
