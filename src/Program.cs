using BilheteriaAPI.Data;
using BilheteriaAPI.Models;
using BilheteriaAPI.Repositories;
using BilheteriaAPI.Repositories.Interfaces;
using BilheteriaAPI.Services;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIngressoRepository, IngressoRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IngressoService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    db.Database.ExecuteSqlRaw("UPDATE Eventos SET ImagemUrl = 'images/rock.jpg'       WHERE Id = 1 AND ImagemUrl != 'images/rock.jpg'");
    db.Database.ExecuteSqlRaw("UPDATE Eventos SET ImagemUrl = 'images/eletronico.jpg' WHERE Id = 2 AND ImagemUrl != 'images/eletronico.jpg'");
    db.Database.ExecuteSqlRaw("UPDATE Eventos SET ImagemUrl = 'images/teatro.jpg'     WHERE Id = 3 AND ImagemUrl != 'images/teatro.jpg'");
    db.Database.ExecuteSqlRaw("DELETE FROM Ingressos WHERE AssentoId IN (SELECT a.Id FROM Assentos a JOIN Setores s ON a.SetorId = s.Id WHERE s.EventoId = 4)");
    db.Database.ExecuteSqlRaw("DELETE FROM Assentos WHERE SetorId IN (SELECT Id FROM Setores WHERE EventoId = 4)");
    db.Database.ExecuteSqlRaw("DELETE FROM Setores WHERE EventoId = 4");
    db.Database.ExecuteSqlRaw("DELETE FROM Eventos WHERE Id = 4");
    db.Database.ExecuteSqlRaw("UPDATE Eventos SET ImagemUrl = 'images/show-andre.jpg' WHERE Nome = 'Show do André' AND ImagemUrl != 'images/show-andre.jpg'");
    db.SaveChanges();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseStaticFiles();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=bilheteria.db";

// ── AV1: Endpoints obrigatórios — Dapper com parâmetros @ ────────────────────

app.MapGet("/api/eventos", async () =>
{
    using var conn = new SqliteConnection(connStr);
    var eventos = await conn.QueryAsync(
        "SELECT Id, Nome, CapacidadeTotal, DataEvento, PrecoPadrao FROM Eventos");
    return Results.Ok(eventos);
});

app.MapPost("/api/eventos", async (CriarEventoRequest req, EventoService eventoService) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Local) || req.Data == default)
        return Results.BadRequest(new { erro = "Nome, Local e Data são obrigatórios." });
    if (req.Setores == null || req.Setores.Count == 0)
        return Results.BadRequest(new { erro = "Ao menos um setor é obrigatório." });

    using var conn = new SqliteConnection(connStr);
    var id = await conn.ExecuteScalarAsync<int>(
        "INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao) " +
        "VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao); " +
        "SELECT last_insert_rowid();",
        new
        {
            Nome            = req.Nome,
            CapacidadeTotal = req.Setores.Sum(s => s.QuantidadeTotal),
            DataEvento      = req.Data,
            PrecoPadrao     = req.Setores.Min(s => s.Preco)
        });

    return Results.Created($"/api/eventos/{id}", new { id, req.Nome });
});

app.MapPost("/api/cupons", async (CriarCupomRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Codigo))
        return Results.BadRequest(new { erro = "Código do cupom é obrigatório." });
    if (req.Desconto <= 0)
        return Results.BadRequest(new { erro = "Desconto deve ser maior que zero." });

    using var conn = new SqliteConnection(connStr);
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo",
        new { Codigo = req.Codigo });
    if (existe > 0)
        return Results.BadRequest(new { erro = "Cupom já cadastrado." });

    await conn.ExecuteAsync(
        "INSERT INTO Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra) " +
        "VALUES (@Codigo, @PorcentagemDesconto, @ValorMinimoRegra)",
        new
        {
            Codigo              = req.Codigo,
            PorcentagemDesconto = req.Desconto,
            ValorMinimoRegra    = req.valorMinimoregra
        });

    return Results.Created($"/api/cupons/{req.Codigo}", new { req.Codigo });
});

app.MapPost("/api/usuarios", async (CriarUsuarioRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Cpf))
        return Results.BadRequest(new { erro = "Nome, Email e CPF são obrigatórios." });

    using var conn = new SqliteConnection(connStr);
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf",
        new { Cpf = req.Cpf });
    if (existe > 0)
        return Results.BadRequest(new { erro = "CPF já cadastrado." });

    await conn.ExecuteAsync(
        "INSERT INTO Usuarios (Cpf, Nome, Email) VALUES (@Cpf, @Nome, @Email)",
        new { Cpf = req.Cpf, Nome = req.Nome, Email = req.Email });

    return Results.Created($"/api/usuarios/{req.Cpf}", new { req.Cpf, req.Nome, req.Email });
});

// ── Upload de Imagem ──────────────────────────────────────────────────────────

app.MapPost("/api/eventos/upload-imagem", async (UploadImagemRequest req, IWebHostEnvironment env) =>
{
    if (string.IsNullOrWhiteSpace(req.Base64)) return Results.BadRequest();
    var ext = string.IsNullOrWhiteSpace(req.Extensao) ? ".jpg" : req.Extensao;
    var nomeArquivo = $"{Guid.NewGuid()}{ext}";
    var pasta = Path.Combine(env.WebRootPath ?? "wwwroot", "images");
    Directory.CreateDirectory(pasta);
    var caminho = Path.Combine(pasta, nomeArquivo);
    await File.WriteAllBytesAsync(caminho, Convert.FromBase64String(req.Base64));
    return Results.Ok(new { Caminho = $"images/{nomeArquivo}" });
});

// ── Eventos (detalhes / assentos / exclusão) ──────────────────────────────────

app.MapGet("/api/eventos/{id:int}", async (int id, EventoService eventoService) =>
{
    var evento = await eventoService.ObterComSetoresAsync(id);
    return evento is null ? Results.NotFound(new { mensagem = "Evento não encontrado." }) : Results.Ok(evento);
});

app.MapGet("/api/eventos/{eventoId:int}/setores/{setorId:int}/assentos",
    async (int eventoId, int setorId, EventoService eventoService) =>
    {
        var evento = await eventoService.ObterComSetoresAsync(eventoId);
        if (evento is null) return Results.NotFound(new { mensagem = "Evento não encontrado." });
        var setor = evento.Setores.FirstOrDefault(s => s.Id == setorId);
        return setor is null ? Results.NotFound(new { mensagem = "Setor não encontrado." }) : Results.Ok(setor.Assentos);
    });

app.MapDelete("/api/eventos/{id:int}", async (int id, EventoService eventoService) =>
{
    var removido = await eventoService.ExcluirAsync(id);
    return removido ? Results.Ok(new { mensagem = "Evento excluído com sucesso." }) : Results.NotFound(new { mensagem = "Evento não encontrado." });
});

// ── Ingressos ─────────────────────────────────────────────────────────────────

app.MapPost("/api/ingressos", async (ComprarIngressoRequest req, IngressoService ingressoService) =>
{
    var resultado = await ingressoService.ComprarAsync(req);
    return resultado.Sucesso ? Results.Ok(resultado) : Results.Conflict(new { mensagem = resultado.Mensagem });
});

app.MapGet("/api/ingressos/usuario/{usuarioId:int}", async (int usuarioId, IngressoService ingressoService) =>
    Results.Ok(await ingressoService.ListarPorUsuarioAsync(usuarioId)));

app.MapGet("/api/ingressos/email/{email}", async (string email, IngressoService ingressoService) =>
    Results.Ok(await ingressoService.ListarPorEmailAsync(email)));

app.MapDelete("/api/ingressos/{id:int}", async (int id, IngressoService ingressoService) =>
{
    var (sucesso, mensagem) = await ingressoService.CancelarAsync(id);
    return sucesso ? Results.Ok(new { mensagem }) : Results.NotFound(new { mensagem });
});

app.MapPost("/api/ingressos/reembolso", async (string codigoIngresso, IngressoService ingressoService) =>
{
    try
    {
        var (sucesso, mensagem) = await ingressoService.ReembolsarAsync(codigoIngresso);
        return sucesso ? Results.Ok(new { mensagem, valorReembolsado = 0m }) : Results.NotFound(new { mensagem });
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Erro interno.", detail: ex.Message, statusCode: 500);
    }
});

// ── Auth ──────────────────────────────────────────────────────────────────────

app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
        return Results.BadRequest(new { mensagem = "E-mail e senha são obrigatórios." });
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email);
    if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Senha, usuario.SenhaHash))
        return Results.Unauthorized();
    return Results.Ok(new { usuario.Id, usuario.Nome, usuario.Email, Tipo = usuario.Tipo });
});

app.MapPost("/api/auth/register", async (RegistroRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email) ||
        string.IsNullOrWhiteSpace(req.Cpf)  || string.IsNullOrWhiteSpace(req.Senha))
        return Results.BadRequest(new { mensagem = "Nome, Email, CPF e Senha são obrigatórios." });
    if (await db.Usuarios.AnyAsync(u => u.Email == req.Email))
        return Results.BadRequest(new { mensagem = "E-mail já cadastrado." });
    if (await db.Usuarios.AnyAsync(u => u.Cpf == req.Cpf))
        return Results.BadRequest(new { mensagem = "CPF já cadastrado." });
    var usuario = new Usuario
    {
        Nome      = req.Nome,
        Email     = req.Email,
        Cpf       = req.Cpf,
        SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.Senha),
        Tipo      = req.Tipo == "Admin" ? "Admin" : "Cliente"
    };
    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();
    return Results.Created($"/api/usuarios/{usuario.Id}", new { usuario.Id, usuario.Nome, usuario.Email, usuario.Cpf });
});

// ── Relatórios ────────────────────────────────────────────────────────────────

app.MapGet("/api/relatorios", async (AppDbContext db) =>
{
    var eventos = await db.Eventos
        .Include(e => e.Setores).ThenInclude(s => s.Assentos)
        .ToListAsync();

    var ingressos = await db.Ingressos
        .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
        .Where(i => i.Status == StatusIngresso.Ativo)
        .ToListAsync();

    var vendasPorEvento = ingressos
        .GroupBy(i => i.Assento.Setor.EventoId)
        .Select(g =>
        {
            var evento = eventos.FirstOrDefault(e => e.Id == g.Key);
            var receita = g.Sum(i => i.Assento.Setor.Preco);
            return new
            {
                NomeEvento        = evento?.Nome ?? "",
                IngressosVendidos = g.Count(),
                Receita           = receita,
                CapacidadeTotal   = evento?.Setores.Sum(s => s.QuantidadeTotal) ?? 0
            };
        }).ToList();

    var totalVendidos = ingressos.Count;
    var receitaTotal  = ingressos.Sum(i => i.Assento.Setor.Preco);

    return Results.Ok(new
    {
        TotalIngressosVendidos = totalVendidos,
        ReceitaTotal           = receitaTotal,
        EventosAtivos          = eventos.Count,
        TicketMedio            = totalVendidos > 0 ? receitaTotal / totalVendidos : 0,
        VendasPorEvento        = vendasPorEvento
    });
});

// ── Pagamentos ────────────────────────────────────────────────────────────────

app.MapPost("/api/pagamentos/checkout", (CheckoutRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.TipoPagamento))
        return Results.BadRequest(new { Mensagem = "Tipo de pagamento é obrigatório." });

    var pagamentoService = new PagamentoService();
    Pagamento pagamento = req.TipoPagamento.ToUpper() switch
    {
        "PIX"      => new PagamentoPix     { ValorTotal = req.Valor, ChavePixOrigem = req.DadosPagamento },
        "CARTAO"   => new PagamentoCartao  { ValorTotal = req.Valor, NumeroCartao = req.DadosPagamento, Titular = "Cliente" },
        "BOLETO"   => new PagamentoBoleto  { ValorTotal = req.Valor },
        "DINHEIRO" => new PagamentoDinheiro{ ValorTotal = req.Valor, ValorEntregue = req.Valor },
        _          => null!
    };

    if (pagamento is null)
        return Results.BadRequest(new { Mensagem = "Tipo de pagamento inválido. Use: PIX, CARTAO, BOLETO ou DINHEIRO." });

    var resultado = pagamentoService.RealizarCheckout(pagamento);
    return Results.Ok(new { Status = pagamento.Status, Mensagem = resultado, DataHora = pagamento.DataPagamento });
});

app.Run();

// ── Request records ───────────────────────────────────────────────────────────

public record LoginRequest(string Email, string Senha);
public record RegistroRequest(string Nome, string Email, string Cpf, string Senha, string Tipo = "Cliente");

public record CriarCupomRequest(
    string Codigo,
    decimal Desconto,
    decimal valorMinimoregra,
    DateTime DataExpiracao);

public record CriarUsuarioRequest(
    string Nome,
    string Email,
    string Cpf,
    string Senha);

public class CheckoutRequest
{
    public decimal Valor          { get; set; }
    public string TipoPagamento   { get; set; } = string.Empty;
    public string DadosPagamento  { get; set; } = string.Empty;
}

public record UploadImagemRequest(string Base64, string Extensao);
