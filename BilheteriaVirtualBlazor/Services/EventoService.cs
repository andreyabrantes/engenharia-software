using BilheteriaVirtualBlazor.Models;

namespace BilheteriaVirtualBlazor.Services;

public class EventoService
{
    private readonly List<Evento> _eventos = new();
    private readonly List<Compra> _compras = new();
    private readonly object _lock = new();

    public event Action? OnEstadoAlterado;

    public EventoService() => InicializarDados();

    private void InicializarDados()
    {
        _eventos.AddRange(new[]
        {
            CriarEvento(1, "Show de Rock 2024",
                "O maior show de rock do ano com bandas internacionais!", 30,
                "Arena Unifeso", "🎸",
                new[] { (1, "Pista", 80.00m, 50), (2, "Camarote", 200.00m, 20) }),

            CriarEvento(2, "Festival de Música Eletrônica",
                "Uma noite inesquecível com os melhores DJs do mundo", 45,
                "Clube Teresópolis", "🎧",
                new[] { (3, "Pista", 100.00m, 60), (4, "VIP", 250.00m, 15) }),

            CriarEvento(3, "Teatro: A Comédia dos Erros",
                "Peça clássica de Shakespeare com elenco renomado", 15,
                "Teatro Municipal", "🎭",
                new[] { (5, "Plateia", 60.00m, 40), (6, "Balcão", 40.00m, 20) })
        });
    }

    private static Evento CriarEvento(int id, string nome, string descricao,
        int diasParaEvento, string local, string emoji,
        (int id, string nome, decimal preco, int qtd)[] setores)
    {
        return new Evento
        {
            Id = id,
            Nome = nome,
            Descricao = descricao,
            Data = DateTime.Now.AddDays(diasParaEvento),
            Local = local,
            ImagemUrl = emoji,
            Setores = setores.Select(s => new Setor
            {
                Id = s.id,
                Nome = s.nome,
                Preco = s.preco,
                QuantidadeTotal = s.qtd,
                QuantidadeDisponivel = s.qtd,
                Assentos = GerarAssentosIniciais(s.qtd)
            }).ToList()
        };
    }

    private static List<Assento> GerarAssentosIniciais(int quantidade)
    {
        return Enumerable.Range(1, quantidade).Select(i => new Assento
        {
            Id = i,
            Numero = $"{(char)('A' + (i - 1) / 10)}{((i - 1) % 10) + 1}",
            Status = StatusAssento.Disponivel
        }).ToList();
    }

    public List<Evento> ObterEventos()
    {
        lock (_lock) return _eventos.ToList();
    }

    public Evento? ObterEventoPorId(int id)
    {
        lock (_lock) return _eventos.FirstOrDefault(e => e.Id == id);
    }

    // Retorna cópia dos assentos para evitar mutação externa sem lock
    public List<Assento> ObterAssentos(int eventoId, int setorId)
    {
        lock (_lock)
        {
            var setor = _eventos
                .FirstOrDefault(e => e.Id == eventoId)?
                .Setores.FirstOrDefault(s => s.Id == setorId);

            return setor?.Assentos.Select(a => new Assento
            {
                Id = a.Id,
                Numero = a.Numero,
                Status = a.Status,
                Selecionado = false
            }).ToList() ?? new List<Assento>();
        }
    }

    public (bool sucesso, string erro) ValidarEvento(Evento evento)
    {
        if (string.IsNullOrWhiteSpace(evento.Nome))
            return (false, "Nome do evento é obrigatório.");
        if (string.IsNullOrWhiteSpace(evento.Local))
            return (false, "Local do evento é obrigatório.");
        if (string.IsNullOrWhiteSpace(evento.Descricao))
            return (false, "Descrição do evento é obrigatória.");
        if (evento.Data <= DateTime.Now)
            return (false, "A data do evento deve ser no futuro.");
        if (!evento.Setores.Any())
            return (false, "Adicione pelo menos um setor.");

        foreach (var s in evento.Setores)
        {
            if (string.IsNullOrWhiteSpace(s.Nome))
                return (false, "Todos os setores precisam de um nome.");
            if (s.Preco <= 0)
                return (false, $"O preço do setor '{s.Nome}' deve ser maior que zero.");
            if (s.QuantidadeTotal <= 0)
                return (false, $"A quantidade do setor '{s.Nome}' deve ser maior que zero.");
        }

        return (true, string.Empty);
    }

    public (bool sucesso, string erro) AdicionarEvento(Evento evento)
    {
        var (valido, erro) = ValidarEvento(evento);
        if (!valido) return (false, erro);

        bool notificar;
        lock (_lock)
        {
            evento.Id = _eventos.Any() ? _eventos.Max(e => e.Id) + 1 : 1;
            var proximoSetorId = _eventos.SelectMany(e => e.Setores).Any()
                ? _eventos.SelectMany(e => e.Setores).Max(s => s.Id) + 1
                : 1;

            foreach (var setor in evento.Setores)
            {
                setor.Id = proximoSetorId++;
                setor.QuantidadeDisponivel = setor.QuantidadeTotal;
                setor.Assentos = GerarAssentosIniciais(setor.QuantidadeTotal);
            }

            _eventos.Add(evento);
            notificar = true;
        }

        // Notifica FORA do lock para evitar deadlock
        if (notificar) OnEstadoAlterado?.Invoke();
        return (true, string.Empty);
    }

    public bool ExcluirEvento(int id)
    {
        bool removido;
        lock (_lock)
        {
            var evento = _eventos.FirstOrDefault(e => e.Id == id);
            removido = evento != null && _eventos.Remove(evento);
        }

        if (removido) OnEstadoAlterado?.Invoke();
        return removido;
    }

    /// <summary>
    /// Reserva assentos atomicamente. Protege contra race condition com lock.
    /// </summary>
    public (bool sucesso, string mensagem, string codigoIngresso) FinalizarCompra(
        string nomeCliente, string email, int eventoId, int setorId, List<int> assentoIds)
    {
        Compra? novaCompra = null;

        lock (_lock)
        {
            var evento = _eventos.FirstOrDefault(e => e.Id == eventoId);
            if (evento == null)
                return (false, "Evento não encontrado.", string.Empty);

            var setor = evento.Setores.FirstOrDefault(s => s.Id == setorId);
            if (setor == null)
                return (false, "Setor não encontrado.", string.Empty);

            var assentos = setor.Assentos.Where(a => assentoIds.Contains(a.Id)).ToList();

            if (assentos.Count != assentoIds.Count)
                return (false, "Um ou mais assentos selecionados não foram encontrados.", string.Empty);

            // Verificação atômica — algum assento foi ocupado entre a seleção e o clique em comprar?
            var jaOcupados = assentos
                .Where(a => a.Status != StatusAssento.Disponivel)
                .Select(a => a.Numero)
                .ToList();

            if (jaOcupados.Any())
                return (false,
                    $"Os assentos {string.Join(", ", jaOcupados)} foram ocupados por outro usuário. Selecione outros.",
                    string.Empty);

            foreach (var assento in assentos)
                assento.Status = StatusAssento.Ocupado;

            setor.QuantidadeDisponivel -= assentos.Count;

            novaCompra = new Compra
            {
                Id = _compras.Any() ? _compras.Max(c => c.Id) + 1 : 1,
                NomeCliente = nomeCliente,
                Email = email,
                EventoId = evento.Id,
                EventoNome = evento.Nome,
                EventoLocal = evento.Local,
                EventoData = evento.Data,
                SetorId = setor.Id,
                SetorNome = setor.Nome,
                NumerosAssentos = assentos.Select(a => a.Numero).ToList(),
                ValorTotal = setor.Preco * assentos.Count,
                DataCompra = DateTime.Now,
                CodigoIngresso = GerarCodigoIngresso()
            };

            _compras.Add(novaCompra);
        }

        // Notifica FORA do lock
        OnEstadoAlterado?.Invoke();
        return (true, "Compra realizada com sucesso!", novaCompra.CodigoIngresso);
    }

    public List<Compra> ObterComprasPorEmail(string email)
    {
        lock (_lock)
            return _compras
                .Where(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .ToList();
    }

    public RelatorioVendas ObterRelatorio()
    {
        lock (_lock)
        {
            var vendas = _compras
                .GroupBy(c => c.EventoId)
                .Select(g =>
                {
                    var evento = _eventos.FirstOrDefault(e => e.Id == g.Key);
                    return new VendaPorEvento
                    {
                        NomeEvento = g.First().EventoNome,
                        IngressosVendidos = g.Sum(c => c.NumerosAssentos.Count),
                        Receita = g.Sum(c => c.ValorTotal),
                        CapacidadeTotal = evento?.Setores.Sum(s => s.QuantidadeTotal) ?? 0
                    };
                }).ToList();

            return new RelatorioVendas
            {
                TotalIngressosVendidos = _compras.Sum(c => c.NumerosAssentos.Count),
                ReceitaTotal = _compras.Sum(c => c.ValorTotal),
                EventosAtivos = _eventos.Count,
                VendasPorEvento = vendas
            };
        }
    }

    private static string GerarCodigoIngresso() =>
        $"ING-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
