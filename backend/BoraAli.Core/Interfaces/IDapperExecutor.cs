using System.Data;

namespace BoraAli.Core.Interfaces;

/// <summary>
/// Abstração para operações Dapper, permitindo mock em testes unitários
/// </summary>
public interface IDapperExecutor
{
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);

    /// <summary>
    /// Executa query Dapper com multi-mapping para 4 tipos de entidade.
    /// Usado para consultas com JOINs que retornam múltiplas entidades relacionadas.
    /// </summary>
    Task<IEnumerable<TReturn>> QueryMultiMapAsync<T1, T2, T3, T4, TReturn>(
        string sql,
        Func<T1, T2, T3, T4, TReturn> map,
        object? parameters = null,
        string splitOn = "Id");
}
