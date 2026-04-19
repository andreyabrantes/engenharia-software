using BilheteriaAPI.Data;
using BilheteriaAPI.Repositories;
using BilheteriaAPI.Repositories.Interfaces;
using BilheteriaAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIngressoRepository, IngressoRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IngressoService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

app.MapPost("/api/ingressos/reembolso", async (string codigoIngresso, AppDbContext db, EmailService emailService) =>
{
    var ingresso = await db.Ingressos
        .Include(i => i.Assento)
        .FirstOrDefaultAsync(i => i.CodigoUnico == codigoIngresso);

    if (ingresso == null)
        return Results.BadRequest(new { Mensagem = "Ingresso não encontrado." });

    if (ingresso.Status == 1) // 1 = Cancelado
        return Results.BadRequest(new { Mensagem = "Este ingresso já foi reembolsado." });

    // Altera o status do ingresso para Cancelado (1) e libera o assento (0 = Disponivel)
    ingresso.Status = 1;
    
    decimal valorReembolsado = 0;
    if (ingresso.Assento != null)
    {
        ingresso.Assento.Status = 0;
        
        var setor = await db.Setores.FindAsync(ingresso.Assento.SetorId);
        if (setor != null) valorReembolsado = setor.Preco;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new { Mensagem = "Reembolso processado com sucesso.", ValorReembolsado = valorReembolsado });
});

app.Run();
