using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using BoraAli.Core.Interfaces;
using BoraAli.Infrastructure.Data;
using Dapper;

namespace BoraAli.Infrastructure.Repositories;

/// <summary>
/// Implementação genérica do repositório usando Dapper com SQLite
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbSession _session;
    protected readonly string _tableName;
    protected readonly string _keyColumn = "Id";

    /// <summary>
    /// Cache de propriedades que são colunas reais do banco (ignora navigation properties)
    /// </summary>
    private static readonly Lazy<List<PropertyInfo>> _columnProperties = new(GetColumnProperties);

    public GenericRepository(DbSession session)
    {
        _session = session;
        _tableName = typeof(T).Name + "s"; // Pluralização simples
        if (_tableName.EndsWith("ys")) _tableName = _tableName.Replace("ys", "ies"); // Category -> Categories
        if (_tableName == "Userss") _tableName = "Users";
    }

    /// <summary>
    /// Retorna apenas as propriedades que representam colunas reais no banco,
    /// ignorando navigation properties (ICollection, List, etc.) e propriedades sem setter.
    /// </summary>
    private static List<PropertyInfo> GetColumnProperties()
    {
        // Tipos considerados como colunas SQL válidas
        var validTypes = new HashSet<Type>
        {
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(int?), typeof(long?), typeof(short?), typeof(byte?),
            typeof(string), typeof(char), typeof(char?),
            typeof(bool), typeof(bool?),
            typeof(decimal), typeof(double), typeof(float),
            typeof(decimal?), typeof(double?), typeof(float?),
            typeof(DateTime), typeof(DateTime?),
            typeof(Guid), typeof(Guid?),
            typeof(byte[]), typeof(TimeSpan), typeof(TimeSpan?)
        };

        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
            {
                // Ignora Id (tratado separadamente)
                if (p.Name == "Id") return false;

                // Ignora navigation properties (tipos que não são colunas SQL)
                if (!validTypes.Contains(p.PropertyType)) return false;

                // Ignora propriedades sem setter público
                return p.CanWrite && p.GetSetMethod(true)?.IsPublic == true;
            })
            .ToList();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var sql = string.Concat("SELECT * FROM [", _tableName, "] WHERE Id = @Id");
        return await _session.Connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = string.Concat("SELECT * FROM [", _tableName, "]");
        return await _session.Connection.QueryAsync<T>(sql);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // Dapper não suporta Expression diretamente, então buscamos todos e filtramos em memória
        var all = await GetAllAsync();
        return all.AsQueryable().Where(predicate).ToList();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var properties = _columnProperties.Value;

        var columns = string.Join(", ", properties.Select(p => string.Concat("[", p.Name, "]")));
        var parameters = string.Join(", ", properties.Select(p => string.Concat("@", p.Name)));

        var sql = string.Concat(
            "INSERT INTO [", _tableName, "] (", columns, ") VALUES (", parameters, "); SELECT last_insert_rowid();");

        // Cria DynamicParameters apenas com as propriedades de coluna (evita navigation properties)
        var dp = new DynamicParameters();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);
            dp.Add(string.Concat("@", prop.Name), value);
        }

        var id = await _session.Connection.ExecuteScalarAsync<int>(sql, dp);
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, id);
        }

        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        var properties = _columnProperties.Value;

        var setClause = string.Join(", ", properties.Select(p => string.Concat("[", p.Name, "] = @", p.Name)));
        var sql = string.Concat("UPDATE [", _tableName, "] SET ", setClause, " WHERE Id = @Id");

        // Cria DynamicParameters apenas com as propriedades de coluna + Id
        var dp = new DynamicParameters();
        foreach (var prop in properties)
        {
            dp.Add(string.Concat("@", prop.Name), prop.GetValue(entity));
        }
        var idProp = typeof(T).GetProperty("Id");
        if (idProp != null)
        {
            dp.Add("@Id", idProp.GetValue(entity));
        }

        await _session.Connection.ExecuteAsync(sql, dp);
    }

    public virtual async Task DeleteAsync(T entity)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(entity);
        var sql = string.Concat("DELETE FROM [", _tableName, "] WHERE Id = @Id");
        await _session.Connection.ExecuteAsync(sql, new { Id = id });
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var sql = string.Concat("SELECT COUNT(1) FROM [", _tableName, "] WHERE Id = @Id");
        return await _session.Connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            var sql = string.Concat("SELECT COUNT(1) FROM [", _tableName, "]");
            return await _session.Connection.ExecuteScalarAsync<int>(sql);
        }

        // Quando ha predicado, carrega todos e filtra em memoria
        // (mesmo padrao do FindAsync — Dapper nao suporta Expression diretamente)
        var all = await GetAllAsync();
        return all.AsQueryable().Count(predicate);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? orderBy = null,
        bool ascending = true)
    {
        // Count total com ou sem filtro
        int totalCount;
        if (filter != null)
        {
            var all = await GetAllAsync();
            totalCount = all.AsQueryable().Count(filter);
        }
        else
        {
            var countSql = string.Concat("SELECT COUNT(1) FROM [", _tableName, "]");
            totalCount = await _session.Connection.ExecuteScalarAsync<int>(countSql);
        }

        // Sanitização: valida orderBy contra propriedades reais da entidade para prevenir SQL injection
        var validColumns = _columnProperties.Value.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var orderClause = "Id";
        if (!string.IsNullOrEmpty(orderBy) && validColumns.Contains(orderBy))
        {
            orderClause = orderBy;
        }
        var direction = ascending ? "ASC" : "DESC";
        var offset = (pageNumber - 1) * pageSize;

        var sql = new StringBuilder();
        sql.Append(string.Concat("SELECT * FROM [", _tableName, "] "));
        sql.Append(string.Concat("ORDER BY [", orderClause, "] ", direction, " "));
        sql.Append("LIMIT @PageSize OFFSET @Offset");

        var items = await _session.Connection.QueryAsync<T>(sql.ToString(),
            new { Offset = offset, PageSize = pageSize });

        return (items, totalCount);
    }
}
