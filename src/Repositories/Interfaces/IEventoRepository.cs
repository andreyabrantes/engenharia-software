using BilheteriaAPI.Models;

namespace BilheteriaAPI.Repositories.Interfaces;

public interface IEventoRepository
{
    Task<IEnumerable<Evento>> GetAllAsync();
    Task<Evento?> GetByIdWithSetoresAsync(int id);
    Task<Evento> CreateAsync(Evento evento);
    Task<bool> DeleteAsync(int id);
}
