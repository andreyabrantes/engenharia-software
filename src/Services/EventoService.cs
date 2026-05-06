using BilheteriaAPI.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BilheteriaAPI.Services;

public record CriarEventoRequest(
    string Nome,
    string Descricao,
    DateTime Data,
    string Local,
    string ImagemUrl,
    List<CriarSetorRequest> Setores);

public record CriarSetorRequest(string Nome, decimal Preco, int QuantidadeTotal);

public class EventoService(IConfiguration config)
{
    private string ConnStr => config.GetConnectionString("DefaultConnection") ?? "Data Source=bilheteria.db";

    public async Task<IEnumerable<Evento>> ListarTodosAsync()
    {
        using var conn = new SqliteConnection(ConnStr);
        var eventos = (await conn.QueryAsync<Evento>(
            "SELECT Id, Nome, Descricao, DataEvento AS Data, Local, ImagemUrl, Destaque FROM Eventos")).ToList();

        foreach (var e in eventos)
        {
            e.Setores = (await conn.QueryAsync<Setor>(
                "SELECT Id, Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId FROM Setores WHERE EventoId = @Id",
                new { Id = e.Id })).ToList();
        }
        return eventos;
    }

    public async Task<Evento?> ObterComSetoresAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        var evento = await conn.QueryFirstOrDefaultAsync<Evento>(
            "SELECT Id, Nome, Descricao, DataEvento AS Data, Local, ImagemUrl, Destaque FROM Eventos WHERE Id = @Id",
            new { Id = id });
        if (evento is null) return null;

        var setores = (await conn.QueryAsync<Setor>(
            "SELECT Id, Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId FROM Setores WHERE EventoId = @Id",
            new { Id = id })).ToList();

        foreach (var s in setores)
        {
            s.Assentos = (await conn.QueryAsync<Assento>(
                "SELECT Id, Numero, Status, SetorId FROM Assentos WHERE SetorId = @Id",
                new { Id = s.Id })).ToList();
        }

        evento.Setores = setores;
        return evento;
    }

    public async Task<Evento> CriarAsync(CriarEventoRequest request)
    {
        using var conn = new SqliteConnection(ConnStr);

        var eventoId = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Eventos (Nome, Descricao, DataEvento, Local, ImagemUrl) VALUES (@Nome, @Descricao, @Data, @Local, @ImagemUrl); SELECT last_insert_rowid();",
            new { request.Nome, request.Descricao, Data = request.Data.ToString("o"), request.Local, request.ImagemUrl });

        foreach (var s in request.Setores)
        {
            var setorId = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO Setores (Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId) VALUES (@Nome, @Preco, @Qtd, @Qtd, @EventoId); SELECT last_insert_rowid();",
                new { s.Nome, s.Preco, Qtd = s.QuantidadeTotal, EventoId = eventoId });

            for (int i = 1; i <= s.QuantidadeTotal; i++)
            {
                var numero = $"{(char)('A' + (i - 1) / 10)}{((i - 1) % 10) + 1}";
                await conn.ExecuteAsync(
                    "INSERT INTO Assentos (Numero, Status, SetorId) VALUES (@Numero, 0, @SetorId)",
                    new { Numero = numero, SetorId = setorId });
            }
        }

        return (await ObterComSetoresAsync(eventoId))!;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        using var conn = new SqliteConnection(ConnStr);
        var existe = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Eventos WHERE Id = @Id", new { Id = id });
        if (existe == 0) return false;

        await conn.ExecuteAsync(
            "DELETE FROM Ingressos WHERE AssentoId IN (SELECT a.Id FROM Assentos a JOIN Setores s ON a.SetorId = s.Id WHERE s.EventoId = @Id)",
            new { Id = id });
        await conn.ExecuteAsync(
            "DELETE FROM Assentos WHERE SetorId IN (SELECT Id FROM Setores WHERE EventoId = @Id)",
            new { Id = id });
        await conn.ExecuteAsync("DELETE FROM Setores WHERE EventoId = @Id", new { Id = id });
        await conn.ExecuteAsync("DELETE FROM Eventos WHERE Id = @Id", new { Id = id });
        return true;
    }
}
