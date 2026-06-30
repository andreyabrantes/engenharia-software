using System.Linq.Expressions;

namespace BoraAli.Core.Interfaces;

/// <summary>
/// Interface genérica para operações de repositório
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Métodos com paginação e ordenação
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? orderBy = null,
        bool ascending = true);
}
