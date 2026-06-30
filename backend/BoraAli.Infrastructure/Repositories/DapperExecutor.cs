using System.Data;
using BoraAli.Core.Interfaces;
using Dapper;

namespace BoraAli.Infrastructure.Repositories;

/// <summary>
/// Implementação concreta do executor Dapper usando a conexão do UnitOfWork
/// </summary>
public class DapperExecutor : IDapperExecutor
{
    private readonly IUnitOfWork _unitOfWork;

    public DapperExecutor(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        return await _unitOfWork.Connection.ExecuteAsync(
            sql,
            parameters,
            transaction ?? _unitOfWork.Transaction);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<T>(
            sql,
            parameters,
            _unitOfWork.Transaction);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        return await _unitOfWork.Connection.QueryAsync<T>(
            sql,
            parameters,
            _unitOfWork.Transaction);
    }

    public async Task<IEnumerable<TReturn>> QueryMultiMapAsync<T1, T2, T3, T4, TReturn>(
        string sql,
        Func<T1, T2, T3, T4, TReturn> map,
        object? parameters = null,
        string splitOn = "Id")
    {
        return await _unitOfWork.Connection.QueryAsync(
            sql,
            map,
            parameters,
            _unitOfWork.Transaction,
            splitOn: splitOn);
    }
}
