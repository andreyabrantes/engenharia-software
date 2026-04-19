using BilheteriaAPI.Data;
using BilheteriaAPI.Models;
using BilheteriaAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Repositories;

public class IngressoRepository(AppDbContext db) : IIngressoRepository
{
    public async Task<Ingresso?> GetByIdAsync(int id) =>
        await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Ingresso>> GetByUsuarioIdAsync(int usuarioId) =>
        await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor).ThenInclude(s => s.Evento)
            .Where(i => i.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task<bool> AssentoJaReservadoAsync(int assentoId) =>
        await db.Ingressos.AnyAsync(i => i.AssentoId == assentoId && i.Status == StatusIngresso.Ativo);

    public async Task<Ingresso> CreateAsync(Ingresso ingresso)
    {
        db.Ingressos.Add(ingresso);
        await db.SaveChangesAsync();
        return ingresso;
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var ingresso = await db.Ingressos
            .Include(i => i.Assento).ThenInclude(a => a.Setor)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingresso is null || ingresso.Status != StatusIngresso.Ativo) return false;

        ingresso.Status = StatusIngresso.Cancelado;
        ingresso.Assento.Status = StatusAssento.Disponivel;
        ingresso.Assento.Setor.QuantidadeDisponivel++;

        await db.SaveChangesAsync();
        return true;
    }
}
