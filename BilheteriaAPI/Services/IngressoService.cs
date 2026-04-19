using BilheteriaAPI.Data;
using BilheteriaAPI.Models;
using BilheteriaAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Services;

public record ComprarIngressoRequest(
    string NomeCliente,
    string Email,
    int EventoId,
    int SetorId,
    List<int> AssentoIds);

public class CompraResultado
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string CodigoIngresso { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string EventoNome { get; set; } = string.Empty;
    public string EventoLocal { get; set; } = string.Empty;
    public DateTime EventoData { get; set; }
    public int SetorId { get; set; }
    public string SetorNome { get; set; } = string.Empty;
    public List<string> NumerosAssentos { get; set; } = [];
    public decimal ValorTotal { get; set; }
    public DateTime DataCompra { get; set; }
}

public class IngressoService(IIngressoRepository ingressoRepo, AppDbContext db, EmailService emailService)
{
    private static readonly object _lock = new();

    public async Task<CompraResultado> ComprarAsync(ComprarIngressoRequest request)
    {
        // Busca ou cria usuário pelo email
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario is null)
        {
            usuario = new Usuario
            {
                Nome = request.NomeCliente,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Tipo = "Cliente"
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        var evento = await db.Eventos
            .Include(e => e.Setores).ThenInclude(s => s.Assentos)
            .FirstOrDefaultAsync(e => e.Id == request.EventoId);

        if (evento is null)
            return new CompraResultado { Sucesso = false, Mensagem = "Evento não encontrado." };

        var setor = evento.Setores.FirstOrDefault(s => s.Id == request.SetorId);
        if (setor is null)
            return new CompraResultado { Sucesso = false, Mensagem = "Setor não encontrado." };

        List<Assento> assentos;
        lock (_lock)
        {
            assentos = setor.Assentos
                .Where(a => request.AssentoIds.Contains(a.Id))
                .ToList();

            if (assentos.Count != request.AssentoIds.Count)
                return new CompraResultado { Sucesso = false, Mensagem = "Um ou mais assentos não encontrados." };

            var jaOcupados = assentos.Where(a => a.Status != StatusAssento.Disponivel).Select(a => a.Numero).ToList();
            if (jaOcupados.Any())
                return new CompraResultado { Sucesso = false, Mensagem = $"Assentos já ocupados: {string.Join(", ", jaOcupados)}" };

            foreach (var a in assentos)
                a.Status = StatusAssento.Ocupado;

            setor.QuantidadeDisponivel -= assentos.Count;
        }

        var codigo = $"ING-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var dataCompra = DateTime.UtcNow;

        foreach (var assento in assentos)
        {
            db.Ingressos.Add(new Ingresso
            {
                UsuarioId = usuario.Id,
                AssentoId = assento.Id,
                CodigoUnico = $"{codigo}-{assento.Numero}",
                Status = StatusIngresso.Ativo,
                DataCompra = dataCompra
            });
        }

        await db.SaveChangesAsync();

        return new CompraResultado
        {
            Sucesso = true,
            Mensagem = "Compra realizada com sucesso!",
            CodigoIngresso = codigo,
            NomeCliente = request.NomeCliente,
            EmailCliente = request.Email,
            EventoId = evento.Id,
            EventoNome = evento.Nome,
            EventoLocal = evento.Local,
            EventoData = evento.Data,
            SetorId = setor.Id,
            SetorNome = setor.Nome,
            NumerosAssentos = assentos.Select(a => a.Numero).ToList(),
            ValorTotal = setor.Preco * assentos.Count,
            DataCompra = dataCompra
        };
    }

    public async Task<IEnumerable<object>> ListarPorEmailAsync(string email)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (usuario is null) return [];
        return await ListarPorUsuarioAsync(usuario.Id);
    }

    public async Task<IEnumerable<object>> ListarPorUsuarioAsync(int usuarioId)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        var ingressos = await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
            .Where(i => i.UsuarioId == usuarioId && i.Status == StatusIngresso.Ativo)
            .ToListAsync();

        return ingressos
            .GroupBy(i => i.CodigoUnico.Contains('-', StringComparison.Ordinal)
                ? string.Join("-", i.CodigoUnico.Split('-').Take(3))
                : i.CodigoUnico)
            .Select(g =>
            {
                var primeiro = g.First();
                var evento = primeiro.Assento.Setor.Evento;
                var setor = primeiro.Assento.Setor;
                return (object)new
                {
                    Id = primeiro.Id,
                    NomeCliente = usuario?.Nome ?? "",
                    Email = usuario?.Email ?? "",
                    EventoId = evento.Id,
                    EventoNome = evento.Nome,
                    EventoLocal = evento.Local,
                    EventoData = evento.Data,
                    SetorId = setor.Id,
                    SetorNome = setor.Nome,
                    NumerosAssentos = g.Select(i => i.Assento.Numero).ToList(),
                    ValorTotal = setor.Preco * g.Count(),
                    DataCompra = primeiro.DataCompra,
                    CodigoIngresso = g.Key
                };
            });
    }

    public async Task<(bool Sucesso, string Mensagem)> CancelarAsync(int ingressoId)
    {
        var cancelado = await ingressoRepo.CancelarAsync(ingressoId);
        return cancelado
            ? (true, "Ingresso cancelado.")
            : (false, "Ingresso não encontrado ou já cancelado.");
    }

    public async Task<(bool Sucesso, string Mensagem, decimal ValorReembolsado)> ReembolsarAsync(string codigoIngresso)
    {
        // O codigoIngresso recebido é no formato ING-yyyyMMdd-XXXXXXXX
        // Os CodigoUnico no banco são ING-yyyyMMdd-XXXXXXXX-A1, ING-yyyyMMdd-XXXXXXXX-B2, etc.
        var prefixo = codigoIngresso.Trim();

        var ingressos = await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
            .Include(i => i.Usuario)
            .Where(i => i.CodigoUnico.StartsWith(prefixo) && i.Status == StatusIngresso.Ativo)
            .ToListAsync();

        if (!ingressos.Any())
            return (false, "Ingresso não encontrado ou já reembolsado.", 0);

        var setor = ingressos.First().Assento.Setor;
        var valorReembolsado = setor.Preco * ingressos.Count;

        foreach (var ingresso in ingressos)
        {
            ingresso.Status = StatusIngresso.Cancelado;
            ingresso.Assento.Status = StatusAssento.Disponivel;
            setor.QuantidadeDisponivel++;
        }

        await db.SaveChangesAsync();

        var usuario = ingressos.First().Usuario ?? await db.Usuarios.FindAsync(ingressos.First().UsuarioId);
        var numerosAssentos = ingressos.Select(i => i.Assento.Numero).ToList();

        _ = emailService.EnviarReembolsoAsync(
            emailCliente: usuario?.Email ?? "",
            nomeCliente: usuario?.Nome ?? "",
            eventoNome: ingressos.First().Assento.Setor.Evento?.Nome ?? "",
            codigoIngresso: codigoIngresso,
            assentos: numerosAssentos,
            valorReembolsado: valorReembolsado,
            dataReembolso: DateTime.UtcNow);

        return (true, $"Reembolso de {valorReembolsado:C2} processado com sucesso. Os assentos foram liberados.", valorReembolsado);
    }
}
