using BilheteriaAPI.Data;
using BilheteriaAPI.Models;
using BilheteriaAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BilheteriaAPI.Repositories;

public class EventoRepository(AppDbContext db) : IEventoRepository
{
    public async Task<IEnumerable<Evento>> GetAllAsync() =>
        await db.Eventos.Include(e => e.Setores).ToListAsync();

    public async Task<Evento?> GetByIdWithSetoresAsync(int id) =>
        await db.Eventos
            .Include(e => e.Setores).ThenInclude(s => s.Assentos)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Evento> CreateAsync(Evento evento)
    {
        db.Eventos.Add(evento);
        await db.SaveChangesAsync();
        return evento;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var evento = await db.Eventos.FindAsync(id);
        if (evento is null) return false;
        db.Eventos.Remove(evento);
        await db.SaveChangesAsync();
        return true;
    }
}
