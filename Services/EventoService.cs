using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class EventoService
{
    private List<Evento> _eventos = new();
    private List<Compra> _compras = new();

    public EventoService()
    {
        InicializarDados();
    }

    private void InicializarDados()
    {
        _eventos = new List<Evento>
        {
            new Evento
            {
                Id = 1,
                Nome = "Show de Rock 2024",
                Descricao = "O maior show de rock do ano com bandas internacionais!",
                Data = DateTime.Now.AddDays(30),
                Local = "Arena Unifeso",
                ImagemUrl = "🎸",
                Setores = new List<Setor>
                {
                    new Setor { Id = 1, Nome = "Pista", Preco = 80.00m, QuantidadeTotal = 500, QuantidadeDisponivel = 350 },
                    new Setor { Id = 2, Nome = "Camarote", Preco = 200.00m, QuantidadeTotal = 100, QuantidadeDisponivel = 45 }
                }
            },
            new Evento
            {
                Id = 2,
                Nome = "Festival de Música Eletrônica",
                Descricao = "Uma noite inesquecível com os melhores DJs do mundo",
                Data = DateTime.Now.AddDays(45),
                Local = "Clube Teresópolis",
                ImagemUrl = "🎧",
                Setores = new List<Setor>
                {
                    new Setor { Id = 3, Nome = "Pista", Preco = 100.00m, QuantidadeTotal = 800, QuantidadeDisponivel = 600 },
                    new Setor { Id = 4, Nome = "VIP", Preco = 250.00m, QuantidadeTotal = 150, QuantidadeDisponivel = 80 }
                }
            },
            new Evento
            {
                Id = 3,
                Nome = "Teatro: A Comédia dos Erros",
                Descricao = "Peça clássica de Shakespeare com elenco renomado",
                Data = DateTime.Now.AddDays(15),
                Local = "Teatro Municipal",
                ImagemUrl = "🎭",
                Setores = new List<Setor>
                {
                    new Setor { Id = 5, Nome = "Plateia", Preco = 60.00m, QuantidadeTotal = 200, QuantidadeDisponivel = 120 },
                    new Setor { Id = 6, Nome = "Balcão", Preco = 40.00m, QuantidadeTotal = 100, QuantidadeDisponivel = 75 }
                }
            }
        };
    }

    public List<Evento> ObterEventos() => _eventos;

    public Evento? ObterEventoPorId(int id) => _eventos.FirstOrDefault(e => e.Id == id);

    public void AdicionarEvento(Evento evento)
    {
        evento.Id = _eventos.Any() ? _eventos.Max(e => e.Id) + 1 : 1;
        _eventos.Add(evento);
    }

    public bool ExcluirEvento(int id)
    {
        var evento = _eventos.FirstOrDefault(e => e.Id == id);
        if (evento != null)
        {
            _eventos.Remove(evento);
            return true;
        }
        return false;
    }

    public List<Assento> GerarAssentos(int quantidade)
    {
        var assentos = new List<Assento>();
        var random = new Random();
        
        for (int i = 1; i <= quantidade; i++)
        {
            assentos.Add(new Assento
            {
                Id = i,
                Numero = $"A{i}",
                Ocupado = random.Next(0, 100) < 30
            });
        }
        
        return assentos;
    }

    public string FinalizarCompra(Compra compra)
    {
        compra.Id = _compras.Any() ? _compras.Max(c => c.Id) + 1 : 1;
        compra.DataCompra = DateTime.Now;
        compra.CodigoIngresso = $"ING-{DateTime.Now.Ticks.ToString().Substring(8)}";
        
        _compras.Add(compra);
        
        return compra.CodigoIngresso;
    }

    public RelatorioVendas ObterRelatorio()
    {
        var vendas = _compras.GroupBy(c => c.Evento.Id)
            .Select(g => new VendaPorEvento
            {
                NomeEvento = g.First().Evento.Nome,
                IngressosVendidos = g.Sum(c => c.AssentosSelecionados.Count),
                Receita = g.Sum(c => c.ValorTotal),
                CapacidadeTotal = g.First().Evento.Setores.Sum(s => s.QuantidadeTotal)
            }).ToList();

        return new RelatorioVendas
        {
            TotalIngressosVendidos = _compras.Sum(c => c.AssentosSelecionados.Count),
            ReceitaTotal = _compras.Sum(c => c.ValorTotal),
            EventosAtivos = _eventos.Count,
            VendasPorEvento = vendas
        };
    }

    public void SimularVendas()
    {
        var random = new Random();
        foreach (var venda in ObterRelatorio().VendasPorEvento)
        {
            venda.IngressosVendidos += random.Next(1, 10);
            venda.Receita += random.Next(100, 1000);
        }
    }
}
