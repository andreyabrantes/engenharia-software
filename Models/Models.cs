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
}

public class Setor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeTotal { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public List<Assento> Assentos { get; set; } = new();
}

public class Assento
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public bool Ocupado { get; set; }
    public bool Selecionado { get; set; }
}

public class Compra
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Evento Evento { get; set; } = new();
    public Setor Setor { get; set; } = new();
    public List<Assento> AssentosSelecionados { get; set; } = new();
    public decimal ValorTotal { get; set; }
    public DateTime DataCompra { get; set; }
    public string CodigoIngresso { get; set; } = string.Empty;
}

public class RelatorioVendas
{
    public int TotalIngressosVendidos { get; set; }
    public decimal ReceitaTotal { get; set; }
    public int EventosAtivos { get; set; }
    public decimal TicketMedio => TotalIngressosVendidos > 0 ? ReceitaTotal / TotalIngressosVendidos : 0;
    public List<VendaPorEvento> VendasPorEvento { get; set; } = new();
}

public class VendaPorEvento
{
    public string NomeEvento { get; set; } = string.Empty;
    public int IngressosVendidos { get; set; }
    public decimal Receita { get; set; }
    public int CapacidadeTotal { get; set; }
    public double Ocupacao => CapacidadeTotal > 0 ? (double)IngressosVendidos / CapacidadeTotal * 100 : 0;
}
