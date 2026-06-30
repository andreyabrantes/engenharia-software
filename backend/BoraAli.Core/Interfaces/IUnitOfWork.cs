using System.Data;

namespace BoraAli.Core.Interfaces;

/// <summary>
/// Interface para Unit of Work - gerencia transações e repositórios
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Entities.User> Users { get; }
    IEventRepository Events { get; }
    IGenericRepository<Entities.Category> Categories { get; }
    IGenericRepository<Entities.TicketType> TicketTypes { get; }
    IGenericRepository<Entities.Order> Orders { get; }
    IGenericRepository<Entities.OrderItem> OrderItems { get; }
    IGenericRepository<Entities.Coupon> Coupons { get; }
    IGenericRepository<Entities.EventFavorite> EventFavorites { get; }
    IGenericRepository<Entities.OrganizerFollow> OrganizerFollows { get; }

    /// <summary>
    /// Conexão direta com o banco para queries Dapper personalizadas
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Transação ativa atual (pode ser nula se não houver transação)
    /// </summary>
    IDbTransaction? Transaction { get; }

    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

