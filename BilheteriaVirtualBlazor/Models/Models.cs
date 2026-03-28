namespace BilheteriaVirtualBlazor.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Local { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public List<Setor> Setores { get; set; } = new();

    // Corrigido: só esgotado se TEM setores e todos estão sem vagas
    public bool Esgotado => Setores.Any() && Setores.All(s => s.QuantidadeDisponivel == 0);
    public decimal PrecoMinimo => Setores.Any() ? Setores.Min(s => s.Preco) : 0;
    public int TotalDisponivel => Setores.Sum(s => s.QuantidadeDisponivel);
    public int TotalCapacidade => Setores.Sum(s => s.QuantidadeTotal);
    public double PorcentagemOcupacao => TotalCapacidade > 0
        ? (double)(TotalCapacidade - TotalDisponivel) / TotalCapacidade * 100
        : 0;
}

public class Setor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeTotal { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public List<Assento> Assentos { get; set; } = new();
    public bool Esgotado => QuantidadeDisponivel == 0;
    public double PorcentagemOcupacao => QuantidadeTotal > 0
        ? (double)(QuantidadeTotal - QuantidadeDisponivel) / QuantidadeTotal * 100
        : 0;
}

public enum StatusAssento
{
    Disponivel,
    Reservado,
    Ocupado
}

public class Assento
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public StatusAssento Status { get; set; } = StatusAssento.Disponivel;
    public bool Selecionado { get; set; }
    public bool Ocupado => Status != StatusAssento.Disponivel;
}

public class Compra
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string EventoNome { get; set; } = string.Empty;
    public string EventoLocal { get; set; } = string.Empty;
    public DateTime EventoData { get; set; }
    public int SetorId { get; set; }
    public string SetorNome { get; set; } = string.Empty;
    public List<string> NumerosAssentos { get; set; } = new();
    public decimal ValorTotal { get; set; }
    public DateTime DataCompra { get; set; }
    public string CodigoIngresso { get; set; } = string.Empty;
}

public class RelatorioVendas
{
    public int TotalIngressosVendidos { get; set; }
    public decimal ReceitaTotal { get; set; }
    public int EventosAtivos { get; set; }
    public decimal TicketMedio => TotalIngressosVendidos > 0
        ? ReceitaTotal / TotalIngressosVendidos
        : 0;
    public List<VendaPorEvento> VendasPorEvento { get; set; } = new();
}

public class VendaPorEvento
{
    public string NomeEvento { get; set; } = string.Empty;
    public int IngressosVendidos { get; set; }
    public decimal Receita { get; set; }
    public int CapacidadeTotal { get; set; }
    public double Ocupacao => CapacidadeTotal > 0
        ? (double)IngressosVendidos / CapacidadeTotal * 100
        : 0;
}
