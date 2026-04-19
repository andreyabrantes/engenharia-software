using BilheteriaAPI.Models;

namespace BilheteriaAPI.Repositories.Interfaces;

public interface IIngressoRepository
{
    Task<Ingresso?> GetByIdAsync(int id);
    Task<IEnumerable<Ingresso>> GetByUsuarioIdAsync(int usuarioId);
    Task<bool> AssentoJaReservadoAsync(int assentoId);
    Task<Ingresso> CreateAsync(Ingresso ingresso);
    Task<bool> CancelarAsync(int id);
}
