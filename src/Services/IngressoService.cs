using BilheteriaAPI.Models;
using Dapper;
using Microsoft.Data.Sqlite;

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

public class IngressoService(IConfiguration config, EmailService emailService)
{
    private static readonly object _lock = new();
    private string ConnStr => config.GetConnectionString("DefaultConnection") ?? "Data Source=bilheteria.db";

    public async Task<CompraResultado> ComprarAsync(ComprarIngressoRequest request)
    {
        using var conn = new SqliteConnection(ConnStr);

        // Busca ou cria usuário
        var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Id, Nome, Email FROM Usuarios WHERE Email = @Email",
            new { Email = request.Email });

        int usuarioId;
        if (usuario is null)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
            usuarioId = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO Usuarios (Nome, Email, Cpf, SenhaHash, Tipo) VALUES (@Nome, @Email, @Cpf, @Hash, 'Cliente'); SELECT last_insert_rowid();",
                new { Nome = request.NomeCliente, Email = request.Email, Cpf = $"TEMP-{Guid.NewGuid():N}"[..14], Hash = hash });
        }
        else
        {
            usuarioId = (int)usuario.Id;
        }

        // Busca evento e setor
        var evento = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Id, Nome, Local, DataEvento AS Data, ImagemUrl FROM Eventos WHERE Id = @Id",
            new { Id = request.EventoId });
        if (evento is null)
            return new CompraResultado { Sucesso = false, Mensagem = "Evento não encontrado." };

        var setor = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Id, Nome, Preco, QuantidadeDisponivel FROM Setores WHERE Id = @Id AND EventoId = @EventoId",
            new { Id = request.SetorId, EventoId = request.EventoId });
        if (setor is null)
            return new CompraResultado { Sucesso = false, Mensagem = "Setor não encontrado." };

        List<dynamic> assentos;
        lock (_lock)
        {
            var ids = string.Join(",", request.AssentoIds);
            assentos = conn.Query<dynamic>(
                $"SELECT Id, Numero, Status FROM Assentos WHERE Id IN ({ids}) AND SetorId = @SetorId",
                new { SetorId = request.SetorId }).ToList();

            if (assentos.Count != request.AssentoIds.Count)
                return new CompraResultado { Sucesso = false, Mensagem = "Um ou mais assentos não encontrados." };

            var ocupados = assentos.Where(a => (int)a.Status != 0).Select(a => (string)a.Numero).ToList();
            if (ocupados.Any())
                return new CompraResultado { Sucesso = false, Mensagem = $"Assentos já ocupados: {string.Join(", ", ocupados)}" };

            foreach (var a in assentos)
                conn.Execute("UPDATE Assentos SET Status = 2 WHERE Id = @Id", new { Id = (int)a.Id });

            conn.Execute(
                "UPDATE Setores SET QuantidadeDisponivel = QuantidadeDisponivel - @Qtd WHERE Id = @Id",
                new { Qtd = assentos.Count, Id = request.SetorId });
        }

        var codigo = $"ING-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var dataCompra = DateTime.UtcNow;

        foreach (var a in assentos)
        {
            await conn.ExecuteAsync(
                "INSERT INTO Ingressos (UsuarioId, AssentoId, CodigoUnico, Status, DataCompra) VALUES (@UsuarioId, @AssentoId, @Codigo, 0, @DataCompra)",
                new { UsuarioId = usuarioId, AssentoId = (int)a.Id, Codigo = $"{codigo}-{(string)a.Numero}", DataCompra = dataCompra });
        }

        var resultado = new CompraResultado
        {
            Sucesso = true,
            Mensagem = "Compra realizada com sucesso!",
            CodigoIngresso = codigo,
            NomeCliente = request.NomeCliente,
            EmailCliente = request.Email,
            EventoId = (int)evento.Id,
            EventoNome = (string)evento.Nome,
            EventoLocal = (string)evento.Local,
            EventoData = DateTime.Parse((string)evento.Data),
            SetorId = (int)setor.Id,
            SetorNome = (string)setor.Nome,
            NumerosAssentos = assentos.Select(a => (string)a.Numero).ToList(),
            ValorTotal = (decimal)setor.Preco * assentos.Count,
            DataCompra = dataCompra
        };

        _ = emailService.EnviarIngressoAsync(resultado);
        return resultado;
    }

    public async Task<IEnumerable<object>> ListarPorEmailAsync(string email)
    {
        using var conn = new SqliteConnection(ConnStr);
        var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Id FROM Usuarios WHERE Email = @Email", new { Email = email });
        if (usuario is null) return [];
        return await ListarPorUsuarioAsync((int)usuario.Id);
    }

    public async Task<IEnumerable<object>> ListarPorUsuarioAsync(int usuarioId)
    {
        using var conn = new SqliteConnection(ConnStr);
        var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Nome, Email FROM Usuarios WHERE Id = @Id", new { Id = usuarioId });

        var ingressos = await conn.QueryAsync<dynamic>(
            @"SELECT i.Id, i.CodigoUnico, i.Status, i.DataCompra,
                     a.Numero AS AssentoNumero,
                     s.Id AS SetorId, s.Nome AS SetorNome, s.Preco,
                     e.Id AS EventoId, e.Nome AS EventoNome, e.Local AS EventoLocal, e.DataEvento AS EventoData
              FROM Ingressos i
              JOIN Assentos a ON a.Id = i.AssentoId
              JOIN Setores s ON s.Id = a.SetorId
              JOIN Eventos e ON e.Id = s.EventoId
              WHERE i.UsuarioId = @UsuarioId AND i.Status = 0",
            new { UsuarioId = usuarioId });

        return ingressos
            .GroupBy(i => ((string)i.CodigoUnico).Length > 20 ? ((string)i.CodigoUnico)[..20] : (string)i.CodigoUnico)
            .Select(g =>
            {
                var p = g.First();
                return (object)new
                {
                    Id = (int)p.Id,
                    NomeCliente = usuario?.Nome ?? "",
                    Email = usuario?.Email ?? "",
                    EventoId = (int)p.EventoId,
                    EventoNome = (string)p.EventoNome,
                    EventoLocal = (string)p.EventoLocal,
                    EventoData = DateTime.Parse((string)p.EventoData),
                    SetorId = (int)p.SetorId,
                    SetorNome = (string)p.SetorNome,
                    NumerosAssentos = g.Select(i => (string)i.AssentoNumero).ToList(),
                    ValorTotal = (decimal)p.Preco * g.Count(),
                    DataCompra = (DateTime)p.DataCompra,
                    CodigoIngresso = g.Key
                };
            });
    }

    public async Task<(bool Sucesso, string Mensagem)> CancelarAsync(int ingressoId)
    {
        using var conn = new SqliteConnection(ConnStr);
        var rows = await conn.ExecuteAsync(
            "UPDATE Ingressos SET Status = 1 WHERE Id = @Id AND Status = 0",
            new { Id = ingressoId });
        return rows > 0 ? (true, "Ingresso cancelado.") : (false, "Ingresso não encontrado ou já cancelado.");
    }

    public async Task<(bool Sucesso, string Mensagem)> ReembolsarAsync(string codigoIngresso)
    {
        using var conn = new SqliteConnection(ConnStr);
        var ingressos = (await conn.QueryAsync<dynamic>(
            @"SELECT i.Id, a.Id AS AssentoId, a.SetorId
              FROM Ingressos i JOIN Assentos a ON a.Id = i.AssentoId
              WHERE i.CodigoUnico LIKE @Prefixo AND i.Status = 0",
            new { Prefixo = $"{codigoIngresso.Trim()}%" })).ToList();

        if (!ingressos.Any())
            return (false, "Ingresso não encontrado ou já reembolsado.");

        foreach (var i in ingressos)
        {
            await conn.ExecuteAsync("UPDATE Ingressos SET Status = 1 WHERE Id = @Id", new { Id = (int)i.Id });
            await conn.ExecuteAsync("UPDATE Assentos SET Status = 0 WHERE Id = @Id", new { Id = (int)i.AssentoId });
            await conn.ExecuteAsync("UPDATE Setores SET QuantidadeDisponivel = QuantidadeDisponivel + 1 WHERE Id = @Id", new { Id = (int)i.SetorId });
        }

        return (true, "Reembolso processado com sucesso.");
    }
}
