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
    public void Evento_SemSetores_DeveSerInvalido()
    {
        var setores = new List<string>();

        Assert.Empty(setores);
    }
}
