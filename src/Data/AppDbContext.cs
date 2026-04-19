using BilheteriaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<Assento> Assentos => Set<Assento>();
    public DbSet<Ingresso> Ingressos => Set<Ingresso>();
    public DbSet<Cupom> Cupons => Set<Cupom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Cpf).IsUnique();

        modelBuilder.Entity<Setor>()
            .Property(s => s.Preco).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Ingresso>()
            .HasIndex(i => i.CodigoUnico).IsUnique();

        modelBuilder.Entity<Ingresso>()
            .HasOne(i => i.Assento)
            .WithMany()
            .HasForeignKey(i => i.AssentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed: usuários padrão (hashes pré-computados de admin123 e cliente123)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nome = "Administrador", Email = "admin@bilheteria.com", Cpf = "00000000001", SenhaHash = "$2a$11$vp68k5RW6BWznVfj3PVwT.ayxWVVjVoerC9FODZQZMrVDwuylavJC", Tipo = "Admin" },
            new Usuario { Id = 2, Nome = "Cliente Teste", Email = "cliente@email.com", Cpf = "00000000002", SenhaHash = "$2a$11$Je0gQ8uXDwFuxos4jfyRUe85JOLX8d84VqXss5fAe1ptz7cXmBcZ2", Tipo = "Cliente" }
        );

        // Seed: eventos iniciais
        modelBuilder.Entity<Evento>().HasData(
            new Evento { Id = 1, Nome = "Show de Rock 2026", Descricao = "O maior show de rock do ano com bandas internacionais!", Data = new DateTime(2026, 8, 15, 20, 0, 0), Local = "Arena Unifeso", ImagemUrl = "🎸" },
            new Evento { Id = 2, Nome = "Festival de Música Eletrônica", Descricao = "Uma noite inesquecível com os melhores DJs do mundo", Data = new DateTime(2026, 9, 5, 22, 0, 0), Local = "Clube Teresópolis", ImagemUrl = "🎧" },
            new Evento { Id = 3, Nome = "Teatro: A Comédia dos Erros", Descricao = "Peça clássica de Shakespeare com elenco renomado", Data = new DateTime(2026, 7, 20, 19, 0, 0), Local = "Teatro Municipal", ImagemUrl = "🎭" }
        );

        // Seed: setores
        modelBuilder.Entity<Setor>().HasData(
            new Setor { Id = 1, Nome = "Pista", Preco = 80.00m, QuantidadeTotal = 50, QuantidadeDisponivel = 50, EventoId = 1 },
            new Setor { Id = 2, Nome = "Camarote", Preco = 200.00m, QuantidadeTotal = 20, QuantidadeDisponivel = 20, EventoId = 1 },
            new Setor { Id = 3, Nome = "Pista", Preco = 100.00m, QuantidadeTotal = 60, QuantidadeDisponivel = 60, EventoId = 2 },
            new Setor { Id = 4, Nome = "VIP", Preco = 250.00m, QuantidadeTotal = 15, QuantidadeDisponivel = 15, EventoId = 2 },
            new Setor { Id = 5, Nome = "Plateia", Preco = 60.00m, QuantidadeTotal = 40, QuantidadeDisponivel = 40, EventoId = 3 },
            new Setor { Id = 6, Nome = "Balcão", Preco = 40.00m, QuantidadeTotal = 20, QuantidadeDisponivel = 20, EventoId = 3 }
        );

        // Seed: assentos gerados para cada setor
        var assentos = new List<Assento>();
        var assentoId = 1;
        var setorQtds = new[] { (1, 50), (2, 20), (3, 60), (4, 15), (5, 40), (6, 20) };
        foreach (var (setorId, qtd) in setorQtds)
        {
            for (int i = 1; i <= qtd; i++)
            {
                var numero = $"{(char)('A' + (i - 1) / 10)}{((i - 1) % 10) + 1}";
                assentos.Add(new Assento { Id = assentoId++, Numero = numero, Status = StatusAssento.Disponivel, SetorId = setorId });
            }
        }
        modelBuilder.Entity<Assento>().HasData(assentos);
    }
}
