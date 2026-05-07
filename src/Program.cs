using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ticketprime.db";

// ── Inicializar banco de dados conforme especificação oficial ────────────────
using (var conn = new SqliteConnection(connStr))
{
    conn.Open();

    // Criar tabelas EXATAMENTE como especificado no /db/script.sql
    conn.Execute(@"
        CREATE TABLE IF NOT EXISTS Usuarios (
            Cpf   VARCHAR(14)  PRIMARY KEY,
            Nome  VARCHAR(100) NOT NULL,
            Email VARCHAR(100) NOT NULL UNIQUE
        );

        CREATE TABLE IF NOT EXISTS Eventos (
            Id              INTEGER      PRIMARY KEY AUTOINCREMENT,
            Nome            VARCHAR(100) NOT NULL,
            CapacidadeTotal INTEGER      NOT NULL,
            DataEvento      DATETIME     NOT NULL,
            PrecoPadrao     NUMERIC(10,2) NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Cupons (
            Codigo                VARCHAR(50)   PRIMARY KEY,
            PorcentagemDesconto   NUMERIC(5,2)  NOT NULL,
            ValorMinimoRegra      NUMERIC(10,2) NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Reservas (
            Id              INTEGER       PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf      VARCHAR(14)   NOT NULL,
            EventoId        INTEGER       NOT NULL,
            CupomUtilizado  VARCHAR(50)   NULL,
            ValorFinalPago  NUMERIC(10,2) NOT NULL,
            FOREIGN KEY (UsuarioCpf)     REFERENCES Usuarios(Cpf),
            FOREIGN KEY (EventoId)       REFERENCES Eventos(Id),
            FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo)
        );
    ");
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// ══════════════════════════════════════════════════════════════════════════════
// ENDPOINTS OBRIGATÓRIOS DA AV1
// ══════════════════════════════════════════════════════════════════════════════

// ── POST /api/eventos ─────────────────────────────────────────────────────────
app.MapPost("/api/eventos", async (CriarEventoRequest req) =>
{
    // Validação Fail-Fast
    if (string.IsNullOrWhiteSpace(req.Nome))
        return Results.BadRequest(new { erro = "Nome é obrigatório." });
    
    if (req.CapacidadeTotal <= 0)
        return Results.BadRequest(new { erro = "CapacidadeTotal deve ser maior que zero." });
    
    if (req.DataEvento == default)
        return Results.BadRequest(new { erro = "DataEvento é obrigatório." });
    
    if (req.PrecoPadrao <= 0)
        return Results.BadRequest(new { erro = "PrecoPadrao deve ser maior que zero." });

    using var conn = new SqliteConnection(connStr);
    
    // Segurança: Uso de parâmetros @ (sem concatenação ou interpolação)
    var eventoId = await conn.ExecuteScalarAsync<int>(
        "INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao) VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao); SELECT last_insert_rowid();",
        new { 
            Nome = req.Nome, 
            CapacidadeTotal = req.CapacidadeTotal, 
            DataEvento = req.DataEvento.ToString("yyyy-MM-dd HH:mm:ss"), 
            PrecoPadrao = req.PrecoPadrao 
        });

    return Results.Created($"/api/eventos/{eventoId}", new { 
        Id = eventoId, 
        req.Nome, 
        req.CapacidadeTotal, 
        DataEvento = req.DataEvento, 
        req.PrecoPadrao 
    });
});

// ── GET /api/eventos ──────────────────────────────────────────────────────────
app.MapGet("/api/eventos", async () =>
{
    using var conn = new SqliteConnection(connStr);
    
    // Segurança: Consulta sem parâmetros dinâmicos
    var eventos = await conn.QueryAsync<EventoResponse>(
        "SELECT Id, Nome, CapacidadeTotal, DataEvento, PrecoPadrao FROM Eventos");
    
    return Results.Ok(eventos);
});

// ── POST /api/cupons ──────────────────────────────────────────────────────────
app.MapPost("/api/cupons", async (CriarCupomRequest req) =>
{
    // Validação Fail-Fast
    if (string.IsNullOrWhiteSpace(req.Codigo))
        return Results.BadRequest(new { erro = "Código do cupom é obrigatório." });
    
    if (req.PorcentagemDesconto <= 0)
        return Results.BadRequest(new { erro = "PorcentagemDesconto deve ser maior que zero." });
    
    if (req.ValorMinimoRegra < 0)
        return Results.BadRequest(new { erro = "ValorMinimoRegra não pode ser negativo." });

    using var conn = new SqliteConnection(connStr);
    
    // Segurança: Uso de parâmetros @ para verificar duplicação
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo", 
        new { Codigo = req.Codigo });
    
    if (existe > 0)
        return Results.BadRequest(new { erro = "Cupom já cadastrado." });

    // Segurança: Uso de parâmetros @ (sem concatenação ou interpolação)
    await conn.ExecuteAsync(
        "INSERT INTO Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra) VALUES (@Codigo, @PorcentagemDesconto, @ValorMinimoRegra)",
        new { 
            Codigo = req.Codigo, 
            PorcentagemDesconto = req.PorcentagemDesconto, 
            ValorMinimoRegra = req.ValorMinimoRegra 
        });

    return Results.Created($"/api/cupons/{req.Codigo}", new { 
        req.Codigo, 
        req.PorcentagemDesconto, 
        req.ValorMinimoRegra 
    });
});

// ── POST /api/usuarios ────────────────────────────────────────────────────────
app.MapPost("/api/usuarios", async (CriarUsuarioRequest req) =>
{
    // Validação Fail-Fast
    if (string.IsNullOrWhiteSpace(req.Nome))
        return Results.BadRequest(new { erro = "Nome é obrigatório." });
    
    if (string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new { erro = "Email é obrigatório." });
    
    if (string.IsNullOrWhiteSpace(req.Cpf))
        return Results.BadRequest(new { erro = "CPF é obrigatório." });

    using var conn = new SqliteConnection(connStr);
    
    // Regra Anti-Cambista: Verificar CPF duplicado
    // Segurança: Uso de parâmetros @
    var existe = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(1) FROM Usuarios WHERE Cpf = @Cpf", 
        new { Cpf = req.Cpf });
    
    if (existe > 0)
        return Results.BadRequest(new { erro = "CPF já cadastrado." });

    // Segurança: Uso de parâmetros @ (sem concatenação ou interpolação)
    await conn.ExecuteAsync(
        "INSERT INTO Usuarios (Cpf, Nome, Email) VALUES (@Cpf, @Nome, @Email)",
        new { 
            Cpf = req.Cpf, 
            Nome = req.Nome, 
            Email = req.Email 
        });

    return Results.Created($"/api/usuarios/{req.Cpf}", new { 
        req.Cpf, 
        req.Nome, 
        req.Email 
    });
});

app.Run();

// ══════════════════════════════════════════════════════════════════════════════
// RECORDS (DTOs)
// ══════════════════════════════════════════════════════════════════════════════

public record CriarEventoRequest(
    string Nome, 
    int CapacidadeTotal, 
    DateTime DataEvento, 
    decimal PrecoPadrao);

public record EventoResponse(
    int Id, 
    string Nome, 
    int CapacidadeTotal, 
    string DataEvento, 
    decimal PrecoPadrao);

public record CriarCupomRequest(
    string Codigo, 
    decimal PorcentagemDesconto, 
    decimal ValorMinimoRegra);

public record CriarUsuarioRequest(
    string Nome, 
    string Email, 
    string Cpf);
