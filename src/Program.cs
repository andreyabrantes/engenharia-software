using BilheteriaAPI.Models;
using BilheteriaAPI.Services;
using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<IngressoService>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=bilheteria.db";

// ── Inicializar banco de dados ────────────────────────────────────────────────
using (var conn = new SqliteConnection(connStr))
{
    conn.Open();
    conn.Execute(@"
        CREATE TABLE IF NOT EXISTS Usuarios (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome      TEXT NOT NULL,
            Email     TEXT NOT NULL UNIQUE,
            Cpf       TEXT NOT NULL UNIQUE,
            SenhaHash TEXT NOT NULL,
            Tipo      TEXT NOT NULL DEFAULT 'Cliente'
        );
        CREATE TABLE IF NOT EXISTS Eventos (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome      TEXT NOT NULL,
            Descricao TEXT NOT NULL DEFAULT '',
            Data      TEXT NOT NULL,
            Local     TEXT NOT NULL DEFAULT '',
            ImagemUrl TEXT NOT NULL DEFAULT '🎉'
        );
        CREATE TABLE IF NOT EXISTS Setores (
            Id                   INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome                 TEXT NOT NULL,
            Preco                REAL NOT NULL,
            QuantidadeTotal      INTEGER NOT NULL,
            QuantidadeDisponivel INTEGER NOT NULL,
            EventoId             INTEGER NOT NULL,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id)
        );
        CREATE TABLE IF NOT EXISTS Assentos (
            Id      INTEGER PRIMARY KEY AUTOINCREMENT,
            Numero  TEXT NOT NULL,
            Status  INTEGER NOT NULL DEFAULT 0,
            SetorId INTEGER NOT NULL,
            FOREIGN KEY (SetorId) REFERENCES Setores(Id)
        );
        CREATE TABLE IF NOT EXISTS Ingressos (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            CodigoUnico TEXT NOT NULL UNIQUE,
            Status      INTEGER NOT NULL DEFAULT 0,
            DataCompra  TEXT NOT NULL,
            UsuarioId   INTEGER NOT NULL,
            AssentoId   INTEGER NOT NULL,
            FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
            FOREIGN KEY (AssentoId) REFERENCES Assentos(Id)
        );
    ");

    // Seed usuários padrão
    conn.Execute(@"
        INSERT OR IGNORE INTO Usuarios (Id, Nome, Email, Cpf, SenhaHash, Tipo)
        VALUES (1, 'Administrador', 'admin@bilheteria.com', '00000000001', '$2a$11$vp68k5RW6BWznVfj3PVwT.ayxWVVjVoerC9FODZQZMrVDwuylavJC', 'Admin');
        INSERT OR IGNORE INTO Usuarios (Id, Nome, Email, Cpf, SenhaHash, Tipo)
        VALUES (2, 'Cliente Teste', 'cliente@email.com', '00000000002', '$2a$11$Je0gQ8uXDwFuxos4jfyRUe85JOLX8d84VqXss5fAe1ptz7cXmBcZ2', 'Cliente');
    ");

    // Seed eventos iniciais
    conn.Execute(@"
        INSERT OR IGNORE INTO Eventos (Id, Nome, Descricao, Data, Local, ImagemUrl)
        VALUES (1, 'Show de Rock 2026', 'O maior show de rock do ano com bandas internacionais!', '2026-08-15T20:00:00', 'Arena Unifeso', 'images/rock.jpg');
        INSERT OR IGNORE INTO Eventos (Id, Nome, Descricao, Data, Local, ImagemUrl)
        VALUES (2, 'Festival de Música Eletrônica', 'Uma noite inesquecível com os melhores DJs do mundo', '2026-09-05T22:00:00', 'Clube Teresópolis', 'images/eletronico.jpg');
        INSERT OR IGNORE INTO Eventos (Id, Nome, Descricao, Data, Local, ImagemUrl)
        VALUES (3, 'Teatro: A Comédia dos Erros', 'Peça clássica de Shakespeare com elenco renomado', '2026-07-20T19:00:00', 'Teatro Municipal', 'images/teatro.jpg');
    ");

    // Seed setores
    var setoresSeed = new[]
    {
        (1, "Pista",    80.00m, 50, 1),
        (2, "Camarote", 200.00m, 20, 1),
        (3, "Pista",    100.00m, 60, 2),
        (4, "VIP",      250.00m, 15, 2),
        (5, "Plateia",  60.00m,  40, 3),
        (6, "Balcão",   40.00m,  20, 3),
    };
    foreach (var (sid, nome, preco, qtd, eid) in setoresSeed)
    {
        var existe = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Setores WHERE Id = @Id", new { Id = sid });
        if (existe == 0)
        {
            conn.Execute("INSERT INTO Setores (Id, Nome, Preco, QuantidadeTotal, QuantidadeDisponivel, EventoId) VALUES (@Id, @Nome, @Preco, @Qtd, @Qtd, @EventoId)",
                new { Id = sid, Nome = nome, Preco = preco, Qtd = qtd, EventoId = eid });

            var assentoId = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(Id), 0) FROM Assentos") + 1;
            for (int i = 1; i <= qtd; i++)
            {
                var numero = $"{(char)('A' + (i - 1) / 10)}{((i - 1) % 10) + 1}";
                conn.Execute("INSERT INTO Assentos (Numero, Status, SetorId) VALUES (@Numero, 0, @SetorId)",
                    new { Numero = numero, SetorId = sid });
            }
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseStaticFiles();

// ── AV1: Endpoints obrigatórios — Dapper com parâmetros @ ────────────────────

app.MapGet("/api/eventos", async (EventoService eventoService) =>
    Results.Ok(await eventoService.ListarTodosAsync()));

app.MapPost("/api/eventos", async (CriarEventoRequest req, EventoService eventoService) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Local) || req.Data == default)
        return Results.BadRequest(new { erro = "Nome, Local e Data são obrigatórios." });
    if (req.Setores == null || req.Setores.Count == 0)
        return Results.BadRequest(new { erro = "Ao menos um setor é obrigatório." });

    var evento = await eventoService.CriarAsync(req);
    return Results.Created($"/api/eventos/{evento.Id}", evento);
});

app.MapPost("/api/cupons", async (CriarCupomRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Codigo))
        return Results.BadRequest(new { erro = "Código do cupom é obrigatório." });
    if (req.Desconto <= 0)
        return Results.BadRequest(new { erro = "Desconto deve ser maior que zero." });

    using var conn = new SqliteConnection(connStr);
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo", new { Codigo = req.Codigo });
    if (existe > 0)
        return Results.BadRequest(new { erro = "Cupom já cadastrado." });

    await conn.ExecuteAsync(
        "INSERT INTO Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra) VALUES (@Codigo, @PorcentagemDesconto, @ValorMinimoRegra)",
        new { Codigo = req.Codigo, PorcentagemDesconto = req.Desconto, ValorMinimoRegra = req.valorMinimoregra });

    return Results.Created($"/api/cupons/{req.Codigo}", new { req.Codigo });
});

app.MapPost("/api/usuarios", async (CriarUsuarioRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Cpf))
        return Results.BadRequest(new { erro = "Nome, Email e CPF são obrigatórios." });

    using var conn = new SqliteConnection(connStr);
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf", new { Cpf = req.Cpf });
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
    await File.WriteAllBytesAsync(Path.Combine(pasta, nomeArquivo), Convert.FromBase64String(req.Base64));
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
    var (sucesso, mensagem) = await ingressoService.ReembolsarAsync(codigoIngresso);
    return sucesso ? Results.Ok(new { mensagem, valorReembolsado = 0m }) : Results.NotFound(new { mensagem });
});

// ── Auth ──────────────────────────────────────────────────────────────────────

app.MapPost("/api/auth/login", async (LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
        return Results.BadRequest(new { mensagem = "E-mail e senha são obrigatórios." });

    using var conn = new SqliteConnection(connStr);
    var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
        "SELECT Id, Nome, Email, SenhaHash, Tipo FROM Usuarios WHERE Email = @Email",
        new { Email = req.Email });

    if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Senha, (string)usuario.SenhaHash))
        return Results.Unauthorized();

    return Results.Ok(new { Id = (int)usuario.Id, Nome = (string)usuario.Nome, Email = (string)usuario.Email, Tipo = (string)usuario.Tipo });
});

app.MapPost("/api/auth/register", async (RegistroRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email) ||
        string.IsNullOrWhiteSpace(req.Cpf)  || string.IsNullOrWhiteSpace(req.Senha))
        return Results.BadRequest(new { mensagem = "Nome, Email, CPF e Senha são obrigatórios." });

    using var conn = new SqliteConnection(connStr);
    if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Usuarios WHERE Email = @Email", new { Email = req.Email }) > 0)
        return Results.BadRequest(new { mensagem = "E-mail já cadastrado." });
    if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf", new { Cpf = req.Cpf }) > 0)
        return Results.BadRequest(new { mensagem = "CPF já cadastrado." });

    var hash = BCrypt.Net.BCrypt.HashPassword(req.Senha);
    var tipo = req.Tipo == "Admin" ? "Admin" : "Cliente";
    var id = await conn.ExecuteScalarAsync<int>(
        "INSERT INTO Usuarios (Nome, Email, Cpf, SenhaHash, Tipo) VALUES (@Nome, @Email, @Cpf, @Hash, @Tipo); SELECT last_insert_rowid();",
        new { req.Nome, req.Email, req.Cpf, Hash = hash, Tipo = tipo });

    return Results.Created($"/api/usuarios/{id}", new { Id = id, req.Nome, req.Email, req.Cpf });
});

// ── Relatórios ────────────────────────────────────────────────────────────────

app.MapGet("/api/relatorios", async () =>
{
    using var conn = new SqliteConnection(connStr);
    var eventos = (await conn.QueryAsync<dynamic>("SELECT Id, Nome, Descricao, Data, Local, ImagemUrl FROM Eventos")).ToList();
    var setores = (await conn.QueryAsync<dynamic>("SELECT Id, EventoId, QuantidadeTotal FROM Setores")).ToList();

    var ingressos = (await conn.QueryAsync<dynamic>(
        @"SELECT i.Id, s.Preco, s.EventoId
          FROM Ingressos i
          JOIN Assentos a ON a.Id = i.AssentoId
          JOIN Setores s ON s.Id = a.SetorId
          WHERE i.Status = 0")).ToList();

    var vendasPorEvento = ingressos
        .GroupBy(i => (int)i.EventoId)
        .Select(g =>
        {
            var ev = eventos.FirstOrDefault(e => (int)e.Id == g.Key);
            var cap = setores.Where(s => (int)s.EventoId == g.Key).Sum(s => (int)s.QuantidadeTotal);
            return new
            {
                NomeEvento = ev is null ? "" : (string)ev.Nome,
                IngressosVendidos = g.Count(),
                Receita = g.Sum(i => (decimal)i.Preco),
                CapacidadeTotal = cap,
                Ocupacao = cap > 0 ? (double)g.Count() / cap * 100 : 0
            };
        }).ToList();

    var totalVendidos = ingressos.Count;
    var receitaTotal = ingressos.Sum(i => (decimal)i.Preco);

    return Results.Ok(new
    {
        TotalIngressosVendidos = totalVendidos,
        ReceitaTotal = receitaTotal,
        EventosAtivos = eventos.Count,
        TicketMedio = totalVendidos > 0 ? receitaTotal / totalVendidos : 0,
        VendasPorEvento = vendasPorEvento
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
        return Results.BadRequest(new { Mensagem = "Tipo de pagamento inválido." });

    var resultado = pagamentoService.RealizarCheckout(pagamento);
    return Results.Ok(new { Status = pagamento.Status, Mensagem = resultado, DataHora = pagamento.DataPagamento });
});

app.Run();

// ── Records ───────────────────────────────────────────────────────────────────

public record LoginRequest(string Email, string Senha);
public record RegistroRequest(string Nome, string Email, string Cpf, string Senha, string Tipo = "Cliente");
public record CriarCupomRequest(string Codigo, decimal Desconto, decimal valorMinimoregra, DateTime DataExpiracao);
public record CriarUsuarioRequest(string Nome, string Email, string Cpf, string Senha);
public record UploadImagemRequest(string Base64, string Extensao);

public class CheckoutRequest
{
    public decimal Valor         { get; set; }
    public string TipoPagamento  { get; set; } = string.Empty;
    public string DadosPagamento { get; set; } = string.Empty;
}
