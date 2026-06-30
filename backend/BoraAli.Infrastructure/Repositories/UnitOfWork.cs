using System.Data;
using BoraAli.Core.Entities;
using BoraAli.Core.Interfaces;
using BoraAli.Infrastructure.Data;

namespace BoraAli.Infrastructure.Repositories;

/// <summary>
/// Implementação do Unit of Work para gerenciar transações e repositórios (SQLite)
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbSession _session;
    private IDbTransaction? _transaction;
    private bool _disposed;

    private IGenericRepository<User>? _users;
    private IEventRepository? _events;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<TicketType>? _ticketTypes;
    private IGenericRepository<Order>? _orders;
    private IGenericRepository<OrderItem>? _orderItems;
    private IGenericRepository<Coupon>? _coupons;
    private IGenericRepository<EventFavorite>? _eventFavorites;
    private IGenericRepository<OrganizerFollow>? _organizerFollows;

    public UnitOfWork(DbSession session)
    {
        _session = session;
    }

    public IDbConnection Connection => _session.Connection;

    public IDbTransaction? Transaction => _transaction;

    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_session);

    public IEventRepository Events =>
        _events ??= new EventRepository(_session);

    public IGenericRepository<Category> Categories =>
        _categories ??= new GenericRepository<Category>(_session);

    public IGenericRepository<TicketType> TicketTypes =>
        _ticketTypes ??= new GenericRepository<TicketType>(_session);

    public IGenericRepository<Order> Orders =>
        _orders ??= new GenericRepository<Order>(_session);

    public IGenericRepository<OrderItem> OrderItems =>
        _orderItems ??= new GenericRepository<OrderItem>(_session);

    public IGenericRepository<Coupon> Coupons =>
        _coupons ??= new GenericRepository<Coupon>(_session);

    public IGenericRepository<EventFavorite> EventFavorites =>
        _eventFavorites ??= new GenericRepository<EventFavorite>(_session);

    public IGenericRepository<OrganizerFollow> OrganizerFollows =>
        _organizerFollows ??= new GenericRepository<OrganizerFollow>(_session);

    public async Task<int> CompleteAsync()
    {
        return await Task.FromResult(1);
    }

    public async Task BeginTransactionAsync()
    {
        if (_session.Connection.State != ConnectionState.Open)
        {
            _session.Connection.Open();
        }
        _transaction = _session.Connection.BeginTransaction();
        await Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
        await Task.CompletedTask;
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _session?.Dispose();
        }
        _disposed = true;
    }
}

